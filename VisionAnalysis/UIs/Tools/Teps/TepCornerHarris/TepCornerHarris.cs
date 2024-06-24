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
            Outputs["OutputMaxV"] = new POutput() { value = 0.0 };
            Outputs["OutputMinV"] = new POutput() { value = 0.0 };
            Outputs["Corner"] = new POutput() { value = new Mat() };
            Outputs["CornerDetail"] = new POutput() { value = new Mat() };
            Outputs["Edge"] = new POutput() { value = new Mat() };
            Outputs["EdgeDetail"] = new POutput() { value = new Mat() };
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

            #region find max & min value
            result.MinMaxIdx(out double minValue, out double maxValue);
            result.MinMaxLoc(out Point minLocation, out Point maxLocation);
            Outputs["OutputMaxV"].value = maxValue;
            Outputs["OutputMinV"].value = minValue;
            #endregion

            Mat corner = new Mat(source.Size(), MatType.CV_8UC3, Scalar.Black);
            Mat edge = new Mat(source.Size(), MatType.CV_8UC3, Scalar.Black);
            ConcurrentBag<ImgPtV> imgPtVsCorner = new ConcurrentBag<ImgPtV>();
            ConcurrentBag<ImgPtV> imgPtVsEdge = new ConcurrentBag<ImgPtV>();
            Parallel.For(0, corner.Rows, y =>
            {
                for (int x = 0; x < corner.Cols; x++)
                {
                    float val = result.At<float>(y, x);
                    var f = (byte)((val - minValue) / (maxValue - minValue) * byte.MaxValue);
                    // 如果結果值大於0，繪製紅色(角點)
                    if (val > 0)
                    {
                        corner.Set(y, x, new Vec3b(0, 0, f));
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
                        edge.Set(y, x, new Vec3b(0, f, 0));
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
        };
        #endregion
        private struct ImgPtV
        {
            public int X, Y;
            public double Value;
        }
    }
}
