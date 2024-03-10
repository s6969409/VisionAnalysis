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

            Outputs["OutputResult"] = new POutput() { value = new Mat() };
            Outputs["OutputReScale"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int blockSize = (int)Inputs["blockSize"].value;
            int ksize = (int)Inputs["ksize"].value;
            float k = (float)Inputs["k"].value;
            Mat result = new Mat();
            //process...
            Cv2.CornerHarris(source, result, blockSize, ksize, k);
            Outputs["OutputResult"].value = result;

            #region find max & min value
            result.MinMaxIdx(out double minValue, out double maxValue);
            result.MinMaxLoc(out Point minLocation, out Point maxLocation);
            #endregion

            Outputs["OutputReScale"].value = ImageProcess.ConvertGrayImg(result, minValue, maxValue);
            updateUIImage((Mat)Outputs["OutputReScale"].value);
        };
        #endregion
    }
}
