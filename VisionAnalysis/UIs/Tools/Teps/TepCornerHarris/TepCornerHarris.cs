using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
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

            Outputs["OutputResult"] = new POutput() { value = new Mat() };
            Outputs["OutputReScale"] = new POutput() { value = null };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            Mat result = new Mat();
            //process...
            CvInvoke.CornerHarris(source, result, 10);
            Outputs["OutputResult"].value = result;

            #region find max & min value
            result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);
            #endregion

            Outputs["OutputReScale"].value = ImageProcess.ConvertGrayImg(result.ToImage<Emgu.CV.Structure.Gray, double>(), minValues[0], maxValues[0]);
        };
        #endregion
    }
}
