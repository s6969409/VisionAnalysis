using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
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
            Inputs["ROI"] = ParaDictBuilder<Rect>(100, 100, 200, 200);

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat inputImage = Inputs["InputImage"].value as Mat;
            Dictionary<string, PInput> dictROI = (Dictionary<string, PInput>)Inputs["ROI"].value;
            Rect roi = toT<Rect>(dictROI);

            Outputs["Output1"].value = new Mat(inputImage, roi);
            updateUIImage((Mat)Outputs["Output1"].value);
        };
        #endregion
    }
}
