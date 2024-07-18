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
    public class TepInputs : BaseToolEditParas
    {
        private int index = 0;

        public TepInputs(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["ImageUrl"] = new PInput() { value = "" };
            Outputs["SourceImage"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            string ImageUrl = Inputs["ImageUrl"].value.ToString();

            if (Directory.Exists(ImageUrl))
            {
                string[] fs = Directory.GetFiles(ImageUrl);
                if (++index >= fs.Length) index = 0;
                ImageUrl = fs[index];
            }
            if (File.Exists(ImageUrl))
            {
                Outputs["SourceImage"].value = new Mat(ImageUrl);
                updateUIImage((Mat)Outputs["SourceImage"].value);
            }
        };
        #endregion
    }
}
