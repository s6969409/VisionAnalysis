using Emgu.CV;
using Emgu.CV.CvEnum;
using Newtonsoft.Json.Linq;
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
            Inputs["code"] = new PInput() { value = ColorConversion.Bgr2Gray };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            CvInvoke.CvtColor(
                (Mat)Inputs["InputImage"].value,
                (Mat)Outputs["Output1"].value,
                TepHelper.getEnum<ColorConversion>(Inputs["code"].value));

            updateUIImage((Mat)Outputs["Output1"].value);
        };
        #endregion
    }
}
