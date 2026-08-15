using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepRotate : BaseToolEditParas
    {
        public TepRotate(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["Rotate"] = new PInput() { value = RotateFlags.Rotate180};

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            RotateFlags rotate = TepHelper.getEnum<RotateFlags>(Inputs["Rotate"].value);

            //process...
            Cv2.Rotate(source, (Mat)Outputs["Output1"].value, rotate);
            updateUIImage((Mat)Outputs["Output1"].value);
        };
        #endregion
    }
}
