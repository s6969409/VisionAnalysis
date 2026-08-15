using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepWarpAffine : BaseToolEditParas
    {
        public TepWarpAffine(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["RotateCenter"] = ParaDictBuilder<Point>(0, 0);
            Inputs["Angle"] = new PInput() { value = 0.0 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Output2"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            Point rotateCenter = toT<Point>((Dictionary<string, PInput>)Inputs["RotateCenter"].value);
            double angle = Convert.ToDouble(Inputs["Angle"].value);
            double scale = 1.0;

            //process...
            Mat rotationMatrix = Cv2.GetRotationMatrix2D(rotateCenter, angle, scale);
            Cv2.WarpAffine(source, (Mat)Outputs["Output1"].value, rotationMatrix, source.Size(), InterpolationFlags.Linear, BorderTypes.Constant, Scalar.Black);
            Outputs["Output2"].value = rotationMatrix;
        };
        #endregion
    }

}
