using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepCornerHarris : BaseToolEditParas
    {
        public TepCornerHarris(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["blockSize"] = new PInput() { value = 3 };
            Inputs["ksize"] = new PInput() { value = 3 };
            Inputs["k"] = new PInput() { value = 0.04 };

            Outputs["Output"] = new POutput() { value = new Mat() };
            Outputs["OutputArr"] = new POutput();
            Outputs["OutputMaxV"] = new POutput() { value = 0.0 };
            Outputs["OutputMinV"] = new POutput() { value = 0.0 };
            Outputs["Corner"] = new POutput() { value = new Mat() };
            Outputs["CornerDetail"] = new POutput() { value = new Mat() };
            Outputs["Edge"] = new POutput() { value = new Mat() };
            Outputs["EdgeDetail"] = new POutput() { value = new Mat() };
            Outputs["total"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int blockSize = (int)Inputs["blockSize"].value;
            int ksize = (int)Inputs["ksize"].value;
            double k = Convert.ToDouble(Inputs["k"].value);
            #endregion

            Mat result = new Mat();
            //process...
            Cv2.CornerHarris(source, result, blockSize, ksize, k);
            Outputs["Output"].value = result;
            IEnumerable<ImgPtV> outputArr = ConvertDataSets(result);
            Outputs["OutputArr"].value = outputArr;

            #region find max & min value
            result.MinMaxIdx(out double minValue, out double maxValue);
            result.MinMaxLoc(out Point minLocation, out Point maxLocation);
            Outputs["OutputMaxV"].value = maxValue;
            Outputs["OutputMinV"].value = minValue;
            #endregion
            var a = FindClosestToZero(outputArr.Select(c => c.Value));


            Mat corner = new Mat(source.Size(), MatType.CV_8UC3, Scalar.Black);
            Mat edge = new Mat(source.Size(), MatType.CV_8UC3, Scalar.Black);
            Mat total = new Mat(source.Size(), MatType.CV_8UC3, Scalar.Black);
            ConcurrentBag<ImgPtV> imgPtVsCorner = new ConcurrentBag<ImgPtV>();
            ConcurrentBag<ImgPtV> imgPtVsEdge = new ConcurrentBag<ImgPtV>();
            Parallel.For(0, corner.Rows, y =>
            {
                for (int x = 0; x < corner.Cols; x++)
                {
                    float val = result.At<float>(y, x);
                    // 如果結果值大於0，繪製紅色(角點)
                    if (val > 0)
                    {
                        var f = (byte)((val-a.closestPositive) / (maxValue-a.closestPositive) * byte.MaxValue);
                        if (f == 0) continue;
                        corner.Set(y, x, new Vec3b(0, 0, (byte)(f != 0 ? 255 : 0)));
                        total.Set(y, x, new Vec3b(0, 0, f));
                        imgPtVsCorner.Add(new ImgPtV()
                        {
                            X = x,
                            Y = y,
                            Value = f
                        });
                    }
                    // 如果結果值小於0，繪製綠色(邊)
                    else if (val < 0)
                    {
                        var f = (byte)((a.closestNegative - val) / (a.closestNegative - minValue) * byte.MaxValue);
                        if (f == 0) continue;
                        edge.Set(y, x, new Vec3b(0, (byte)(f!=0?255:0), 0));
                        total.Set(y, x, new Vec3b(0, f, 0));
                        imgPtVsEdge.Add(new ImgPtV()
                        {
                            X = x,
                            Y = y,
                            Value = f
                        });
                    }
                }
            });
            Outputs["Corner"].value = corner;
            Outputs["Edge"].value = edge;
            Outputs["CornerDetail"].value = imgPtVsCorner;
            Outputs["EdgeDetail"].value = imgPtVsEdge;
            Outputs["total"].value = total;
        };

        public override Action<IParaValue, UcAnalysis> paraSelect => (p, u) =>
        {

        };
        #endregion
        private struct ImgPtV
        {
            public int X, Y;
            public double Value;
        }
        private static IEnumerable<ImgPtV> ConvertDataSets(Mat mat)
        {
            ConcurrentBag<ImgPtV> list = new ConcurrentBag<ImgPtV>();
            Parallel.For(0, mat.Height, y =>
            {
                for (int x = 0; x < mat.Width; x++)
                {
                    list.Add(new ImgPtV() { X = x, Y = y, Value = mat.Get<float>(y, x) });
                }
            });
            return list;
        }
        private IEnumerable<ImgPtV> findFeaturePt(Mat mat, double outlierRatio, int threshold = 1)
        {
            IEnumerable<ImgPtV> pts = ConvertDataSets(mat);

            List<List<ImgPtV>> groups = new List<List<ImgPtV>>();
            List<ImgPtV> list = new List<ImgPtV>();

            foreach (ImgPtV pv in pts)
            {
                bool isUnique = true;
                foreach (List<ImgPtV> group in groups)
                {
                    int minX = group.Min(p => p.X);
                    int maxX = group.Max(p => p.X);
                    int minY = group.Min(p => p.Y);
                    int maxY = group.Max(p => p.Y);

                    if (minX - threshold <= pv.X && pv.X <= maxX + threshold && minY - threshold <= pv.Y && pv.Y <= maxY + threshold)
                    {
                        isUnique = false;
                        group.Add(pv);
                        break;
                    }
                }

                if (isUnique)
                {
                    List<ImgPtV> newG = new List<ImgPtV>();
                    newG.Add(pv);
                    groups.Add(newG);
                }
            }
            var a = groups.OrderByDescending(g => g.Count);
            int lastCount = 0;
            int noiseIndex = 0;
            for (int i = 0; i < a.Count(); i++)
            {
                if (a.ElementAt(i).Count < lastCount * outlierRatio)
                {
                    noiseIndex = i;
                    break;
                }
                lastCount = a.ElementAt(i).Count;
            }
            var b = a.Take(noiseIndex);
            var c = b.Select(g =>
            {
                int x = (int)g.Average(p => p.X);
                int y = (int)g.Average(p => p.Y);
                double v = g.Average(p => p.Value);

                return new ImgPtV() { X = x, Y = y, Value = v };
            });
            return c;
        }
        private (double? closestPositive, double? closestNegative) FindClosestToZero(IEnumerable<double> numbers)
        {
            var grouped = numbers.Where(n => n != 0).GroupBy(n => n > 0);

            var closestPositive = grouped.FirstOrDefault(g => g.Key)?.Min(Math.Abs);
            var closestNegative = grouped.FirstOrDefault(g => !g.Key)?.Max(n => n);

            return (closestPositive, closestNegative);
        }
    }
}
