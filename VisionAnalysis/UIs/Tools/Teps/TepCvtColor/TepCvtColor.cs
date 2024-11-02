using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepCvtColor : BaseToolEditParas
    {
        public TepCvtColor(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["code"] = new PInput() { value = ColorConversionCodes.BGR2GRAY };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Cv2.CvtColor(
                (Mat)Inputs["InputImage"].value,
                (Mat)Outputs["Output1"].value,
                TepHelper.getEnum<ColorConversionCodes>(Inputs["code"].value));

            updateUIImage((Mat)Outputs["Output1"].value);
        };
        #endregion
    }
}
