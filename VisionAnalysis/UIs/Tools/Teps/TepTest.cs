using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepTest : BaseToolEditParas
    {
        public TepTest(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Output2"] = new POutput() { value = new Rectangle(100,123,50,70) };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;

            //process...

            Outputs["Output1"].value = source;
        };
        #endregion
    }
}
