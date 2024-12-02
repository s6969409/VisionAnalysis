using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepResize : BaseToolEditParas
    {
        public TepResize(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["dSize"] = ParaDictBuilder<Size>(1920, 1080);

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            Size dSize = (Size)Inputs["dSize"].value;

            //process...
            Mat resized = new Mat();
            Cv2.Resize(source, resized, dSize);
            Outputs["Output1"].value = resized;
        };
        #endregion
    }
}
