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
    public class TepInputs : IToolEditParas
    {
        private ObservableRangeCollection<Nd> nodes;
        public TepInputs(ObservableRangeCollection<Nd> nodes)
        {
            this.nodes = nodes;
            #region read save paras or default value...
            Inputs["ImageUrl"] = new PInput() { value = "" };
            Outputs["SourceImage"] = new POutput();
            #endregion
        }
        public TepInputs(ObservableRangeCollection<Nd> nodes, JObject jobject) : this(nodes)
        {
            ToolName = (string)jobject["ToolName"];
            JObject inputs = (JObject)jobject["Inputs"];

            Inputs["ImageUrl"] = ParaDictBuilder.JObjectToPInput(inputs["ImageUrl"]);
        }

        #region implement IToolEditParas member
        public string ToolName { get; set; }
        public IMatProperty UIImage { get; set; }
        public Dictionary<string, PInput> Inputs { get; } = new Dictionary<string, PInput>();
        public Dictionary<string, POutput> Outputs { get; } = new Dictionary<string, POutput>();
        public Action actionProcess => () =>
        {
            if (File.Exists(Inputs["ImageUrl"].value.ToString()))
            {
                Outputs["SourceImage"].value = new Mat(Inputs["ImageUrl"].value.ToString());
                updateUIImage((Mat)Outputs["SourceImage"].value);
            }
        };
        public Func<string, JObject> getJObjectAndSaveImg => (imgDirPath) =>
        {
            JObject jobject = new JObject();
            jobject["ToolType"] = GetType().FullName;
            jobject["ToolName"] = ToolName;
            jobject["Inputs"] = PInput.getJObjectAndSaveImg(Inputs, imgDirPath);

            return jobject;
        };
        #endregion

        private void updateUIImage(Mat mat)
        {
            //if (UIImage != null) UIImage.Source = Tools.ToBitmapSource(mat);
            if (UIImage != null) UIImage.Image = mat;
        }
    }
}
