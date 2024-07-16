using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepMatchTemplate : BaseToolEditParas
    {
        public TepMatchTemplate(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["TargetImage"] = new PInput() { value = new Mat() };
            Inputs["TemplateImage"] = new PInput() { value = new Mat() };
            Inputs["method"] = new PInput() { value = TemplateMatchModes.CCoeffNormed };
            Inputs["outlierRatio"] = new PInput() { value = 0.7 };
            Inputs["ptNums"] = new PInput() { value = 0 };

            Outputs["OutputResult"] = new POutput() { value = null };
            Outputs["OutputArr"] = new POutput() { value = null };
            Outputs["OutputMatch"] = new POutput() { value = null };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat targetImage = Inputs["TargetImage"].value as Mat;
            Mat templateImage = Inputs["TemplateImage"].value as Mat;
            TemplateMatchModes method = TepHelper.getEnum<TemplateMatchModes>(Inputs["method"].value);
            double outlierRatio = Convert.ToDouble(Inputs["outlierRatio"].value);
            int ptNums = Convert.ToInt32(Inputs["ptNums"].value);
            #endregion

            #region process... & output

            Mat result = new Mat(targetImage.Size(), MatType.CV_32F);

            Cv2.MatchTemplate(targetImage, templateImage, result, method);
            Outputs["OutputResult"].value = result;
            IEnumerable<MatVal> outputArr = ConvertDataSets(result);
            Outputs["OutputArr"].value = outputArr;
            #endregion

            #region find max & min value
            result.MinMaxIdx(out double minValue, out double maxValue);
            result.MinMaxLoc(out Point minLocation, out Point maxLocation);
            #endregion

            #region output match locatiom information
            ptNums = ptNums == 0? (int)Math.Sqrt(outputArr.Count()) : ptNums;
            IEnumerable<MatVal> findPts = findFeaturePt(outputArr.OrderBy(v => v.Value).Take(outputArr.Count()/ ptNums), outlierRatio);

            Mat OutputMatch = targetImage.Clone();
            Cv2.Rectangle(OutputMatch, new Rect(maxLocation, templateImage.Size()), Scalar.Red);
            Cv2.PutText(OutputMatch, $"Max:{maxValue}", maxLocation + new Point(0, 30), HersheyFonts.HersheySimplex, 1, Scalar.Red, 2);
            for (int i = 0; i < findPts.Count(); i++)
            {
                Point pt = new Point(findPts.ElementAt(i).X, findPts.ElementAt(i).Y);
                Cv2.Rectangle(OutputMatch, new Rect(pt, templateImage.Size()), Scalar.Green);
                Cv2.PutText(OutputMatch, $"FindVal{i}:{findPts.ElementAt(i).Value}", pt + new Point(0, 30), HersheyFonts.HersheySimplex, 1, Scalar.Green, 2);
            };
            Cv2.Rectangle(OutputMatch, new Rect(minLocation, templateImage.Size()), Scalar.Blue);
            Cv2.PutText(OutputMatch, $"Min:{minValue}", minLocation + new Point(0, 30), HersheyFonts.HersheySimplex, 1, Scalar.Blue, 2);
            Outputs["OutputMatch"].value = OutputMatch;
            updateUIImage(OutputMatch);
            #endregion

        };
        #endregion

        private IEnumerable<MatVal> ConvertDataSets(Mat mat)
        {
            ConcurrentBag<MatVal> list = new ConcurrentBag<MatVal>();
            Parallel.For(0, mat.Height, y =>
            {
                for (int x = 0; x < mat.Width; x++)
                {
                    list.Add(new MatVal() { X = x, Y = y, Value = mat.Get<double>(y, x) });
                }
            });
            return list;
        }
        private struct MatVal
        {
            public int X;
            public int Y;
            public double Value;
        }
        private IEnumerable<MatVal> findFeaturePt(IEnumerable<MatVal> pts,double outlierRatio, int threshold = 1)
        {
            List<List<MatVal>> groups = new List<List<MatVal>>();
            List<MatVal> list = new List<MatVal>();

            foreach (MatVal pv in pts)
            {
                bool isUnique = true;
                foreach (List<MatVal> group in groups)
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
                    List<MatVal> newG = new List<MatVal>();
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

                return new MatVal() { X = x, Y = y, Value = v };
            });
            return c;
        }
    }
}
