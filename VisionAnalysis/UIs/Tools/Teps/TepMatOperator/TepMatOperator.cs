using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepMatOperator : BaseToolEditParas
    {
        public TepMatOperator(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage1"] = new PInput() { value = new Mat() };
            Inputs["InputImage2"] = new PInput() { value = new Mat() };
            Inputs["OptMethod"] = new PInput() { value = OptMethod.Addition };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source1 = Inputs["InputImage1"].value as Mat;
            Mat source2 = Inputs["InputImage2"].value as Mat;
            OptMethod optMethod = TepHelper.getEnum<OptMethod>(Inputs["OptMethod"].value);

            //process...
            Mat newMat;
            if (optMethod == OptMethod.Addition)
                newMat = (Mat)(source1 + source2);
            else if (optMethod == OptMethod.Subtraction)
                newMat = (Mat)(source1 - source2);
            else if (optMethod == OptMethod.Multiplication)
                newMat = (Mat)(source1 * source2);
            else if (optMethod == OptMethod.Division)
                newMat = (Mat)(source1 / source2);
            else if (optMethod == OptMethod.Inverse)
                newMat = ~source1;
            else if (optMethod == OptMethod.ReScaleValue)
            {
                source1.MinMaxIdx(out double minV, out double maxV);
                newMat = TepHelper.ConvertGrayImg<byte>(source1, minV, maxV);
            }
            else if (optMethod == OptMethod.Absdiff)
            {
                newMat = new Mat();
                Cv2.Absdiff(source1, source2, newMat);
            }
            else return;

            Outputs["Output1"].value = newMat;
        };
        #endregion
        public enum OptMethod
        {
            Addition, Subtraction, Multiplication, Division, Inverse, ReScaleValue, Absdiff
        }
    }
}
