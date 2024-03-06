using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepCanny : BaseToolEditParas
    {
        public TepCanny(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["threshold1"] = new PInput() { value = 200 };
            Inputs["threshold2"] = new PInput() { value = 255 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Cv2.Canny(
                (Mat)Inputs["InputImage"].value, 
                (Mat)Outputs["Output1"].value, 
                (int)Inputs["threshold1"].value, 
                (int)Inputs["threshold2"].value);

            updateUIImage((Mat)Outputs["Output1"].value);
        };
        #endregion
    }
}
