using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepGetStructuringElement : BaseToolEditParas
    {
        public TepGetStructuringElement(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["shape"] = new PInput() { value = ElementShape.Ellipse };
            Inputs["ksize"] = new PInput() { value = ParaDictBuilder<Size>(3, 3) };
            Inputs["anchor"] = new PInput() { value = ParaDictBuilder<Point>(-1, -1) };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Outputs["Output1"].value = CvInvoke.GetStructuringElement(
                TepHelper.getEnum<ElementShape>(Inputs["shape"].value),
                toT<Size>((Dictionary<string, PInput>)Inputs["ksize"].value),
                toT<Point>((Dictionary<string, PInput>)Inputs["anchor"].value)
                );
        };
        #endregion
    }
}
