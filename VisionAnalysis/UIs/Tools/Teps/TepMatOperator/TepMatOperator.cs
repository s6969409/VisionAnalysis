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
            MatExpr matExpr = null;
            if (optMethod== OptMethod.Addition)
                matExpr = source1 + source2;
            else if (optMethod== OptMethod.Subtraction)
                matExpr = source1 - source2;
            else if (optMethod == OptMethod.Multiplication)
                matExpr = source1 * source2;
            else if (optMethod == OptMethod.Division)
                matExpr = source1 / source2;
            else if (optMethod == OptMethod.Inverse)
                matExpr = ~source1;

            Outputs["Output1"].value = (Mat)matExpr;
        };
        #endregion
        public enum OptMethod
        {
            Addition, Subtraction, Multiplication, Division, Inverse
        }
    }
}
