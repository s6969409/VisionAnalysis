using Emgu.CV;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepCapture : BaseToolEditParas
    {
        public TepCapture(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["ROI"] = new PInput() { value = ParaDictBuilder<Rectangle>(100, 100, 200, 200) };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat inputImage = Inputs["InputImage"].value as Mat;
            Dictionary<string, PInput> dictROI = (Dictionary<string, PInput>)Inputs["ROI"].value;
            Rectangle roi = toT<Rectangle>(dictROI);

            Outputs["Output1"].value = new Mat(inputImage, roi);
            updateUIImage((Mat)Outputs["Output1"].value);
        };
        #endregion
    }
}
