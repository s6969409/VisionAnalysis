using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepBlob : BaseToolEditParas
    {
        public TepBlob(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Count"] = new POutput() { value = 0 };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;

            //process...
            ConnectedComponents cc = Cv2.ConnectedComponentsEx(source);
            Outputs["Count"].value = cc.LabelCount;

            Mat result = source.Clone();
            cc.RenderBlobs(result);

            Outputs["Output1"].value = result;
            updateUIImage(result);
        };
        #endregion
    }
}
