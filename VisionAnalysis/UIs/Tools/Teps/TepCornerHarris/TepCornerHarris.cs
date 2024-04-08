using OpenCvSharp;
using System;
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
            Inputs["ThredHoldLT"] = new PInput() { value = -0.01 };
            Inputs["ThredHoldGT"] = new PInput() { value = 0.01 };

            Outputs["OutputResult"] = new POutput() { value = new Mat() };
            Outputs["OutputResultMaxV"] = new POutput() { value = 0.0 };
            Outputs["OutputResultMinV"] = new POutput() { value = 0.0 };
            Outputs["OutputResult2"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int blockSize = (int)Inputs["blockSize"].value;
            int ksize = (int)Inputs["ksize"].value;
            double k = Convert.ToDouble(Inputs["k"].value);
            double ThredHoldLT = Convert.ToDouble(Inputs["ThredHoldLT"].value);
            double ThredHoldGT = Convert.ToDouble(Inputs["ThredHoldGT"].value);

            Mat result = new Mat();
            //process...
            Cv2.CornerHarris(source, result, blockSize, ksize, k);
            Outputs["OutputResult"].value = result;

            #region find max & min value
            result.MinMaxIdx(out double minValue, out double maxValue);
            result.MinMaxLoc(out Point minLocation, out Point maxLocation);
            Outputs["OutputResultMaxV"].value = maxValue;
            Outputs["OutputResultMinV"].value = minValue;
            #endregion

            Mat output = source.CvtColor(ColorConversionCodes.GRAY2BGR);
            Parallel.For(0, output.Rows, y =>
            {
                for (int x = 0; x < output.Cols; x++)
                {
                    float val = result.At<float>(y, x);
                    var f = (byte)((val - minValue) / (maxValue - minValue) * byte.MaxValue);
                    // 如果結果值大於ThredHoldGT，繪製紅色(角點)
                    if (val > ThredHoldGT)
                    {
                        output.Set(y, x, new Vec3b(0, 0, f));
                    }
                    // 如果結果值小於ThredHoldLT，繪製綠色(邊)
                    else if (val < ThredHoldLT)
                    {
                        output.Set(y, x, new Vec3b(0, f, 0));
                    }
                }
            });
            Outputs["OutputResult2"].value = output;
        };
        #endregion
    }
}
