using Emgu.CV;
using Newtonsoft.Json.Linq;
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

            if (File.Exists(Inputs["ImageUrl"].value.ToString()))
            {
                Outputs["SourceImage"].value = new Mat(Inputs["ImageUrl"].value.ToString());
                updateUIImage((Mat)Outputs["SourceImage"].value);
            }

        };
        #endregion
    }
}
