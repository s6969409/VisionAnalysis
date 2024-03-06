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
    public class TepDilate : TepErode
    {
        public TepDilate(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;

            CvInvoke.Dilate(
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
