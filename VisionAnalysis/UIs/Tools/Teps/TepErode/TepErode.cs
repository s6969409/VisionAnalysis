using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepErode : BaseToolEditParas
    {
        public TepErode(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["element"] = new PInput() { value = new Mat() };
            Inputs["anchor"] = new PInput() { value = ParaDictBuilder<Point>(-1, -1) };
            Inputs["iterations"] = new PInput() { value = 3 };
            Inputs["borderType"] = new PInput() { value = BorderType.Default };
            Inputs["borderValue"] = new PInput() { value = ParaDictBuilder<MCvScalar>(255, 0, 0, 0) };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;

            CvInvoke.Erode(
                source, 
                (Mat)Outputs["Output1"].value, 
                (Mat)Inputs["element"].value, 
                toT<Point>((Dictionary<string, PInput>)Inputs["anchor"].value), 
                (int)Inputs["iterations"].value, 
                TepHelper.getEnum<BorderType>(Inputs["borderType"].value), 
                toT<MCvScalar>((Dictionary<string, PInput>)Inputs["borderValue"].value));
        };
        #endregion
    }
}
