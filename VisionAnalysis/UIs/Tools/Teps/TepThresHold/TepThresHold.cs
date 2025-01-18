using Newtonsoft.Json.Linq;
using OpenCvSharp;
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
            Inputs["thresholdType"] = new PInput() { value = ThresholdTypes.Binary };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["threshold"] = new POutput() { value = 0.0 };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Outputs["threshold"].value = Cv2.Threshold(
                (Mat)Inputs["InputImage"].value,
                (Mat)Outputs["Output1"].value,
                (int)Inputs["threshold"].value,
                (int)Inputs["maxValue"].value,
                TepHelper.getEnum<ThresholdTypes>(Inputs["thresholdType"].value));
            updateUIImage((Mat)Outputs["Output1"].value);
            Outputs["Output1"].value = Outputs["Output1"].value;
        };
        #endregion

    }
    
    public class TepAdaptiveThreshold : BaseToolEditParas
    {
        public TepAdaptiveThreshold(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["maxValue"] = new PInput() { value = 255.0 };
            Inputs["adaptiveMethod"] = new PInput() { value = AdaptiveThresholdTypes.GaussianC };
            Inputs["thresholdType"] = new PInput() { value = ThresholdTypes.Binary };
            Inputs["blockSize"] = new PInput() { value = 3 };
            Inputs["c"] = new PInput() { value = 0.0 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Cv2.AdaptiveThreshold(
                (Mat)Inputs["InputImage"].value,
                (Mat)Outputs["Output1"].value,
                (double)Inputs["maxValue"].value,
                TepHelper.getEnum<AdaptiveThresholdTypes>(Inputs["adaptiveMethod"].value),
                TepHelper.getEnum<ThresholdTypes>(Inputs["thresholdType"].value),
                (int)Inputs["blockSize"].value,
                (double)Inputs["c"].value);
            updateUIImage((Mat)Outputs["Output1"].value);
            Outputs["Output1"].value = Outputs["Output1"].value;
        };
        #endregion

    }
}
