using Emgu.CV;
using Emgu.CV.CvEnum;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepThresHold : BaseToolEditParas
    {
        public TepThresHold(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["threshold"] = new PInput() { value = 100 };
            Inputs["maxValue"] = new PInput() { value = 200 };
            Inputs["thresholdType"] = new PInput() { value = ThresholdType.Binary };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            CvInvoke.Threshold(
                (Mat)Inputs["InputImage"].value,
                (Mat)Outputs["Output1"].value,
                (int)Inputs["threshold"].value,
                (int)Inputs["maxValue"].value,
                TepHelper.getEnum<ThresholdType>(Inputs["thresholdType"].value));
            updateUIImage((Mat)Outputs["Output1"].value);
        };
        #endregion

    }
}
