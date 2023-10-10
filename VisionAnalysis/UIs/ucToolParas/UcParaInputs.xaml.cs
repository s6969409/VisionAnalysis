using Emgu.CV;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisionAnalysis
{
    /// <summary>
    /// UcParaInputs.xaml 的互動邏輯
    /// </summary>
    public partial class UcParaInputs : UserControl, IToolEditParas
    {
        private ObservableRangeCollection<Nd> nodes;
        public UcParaInputs(ObservableRangeCollection<Nd> nodes)
        {
            InitializeComponent();
            this.nodes = nodes;
            #region read save paras or default value...
            Inputs["ImageUrl"] = new PInput() { value = "" };
            Outputs["SourceImage"] = new POutput();
            #endregion
        }
        public UcParaInputs(ObservableRangeCollection<Nd> nodes, JObject inputs) : this(nodes)
        {
            Inputs["ImageUrl"].value = (string)inputs["ImageUrl"];
        }

        #region implement IToolEditParas member
        public string ToolName { get; set; }
        public Image UIImage { get; set; }
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
            jobject["Inputs"] = new JObject();
            jobject["Inputs"]["ImageUrl"] = Inputs["ImageUrl"].value.ToString();
            jobject["Outputs"] = new JObject();
            jobject["Outputs"]["SourceImage"] = 100;

            return jobject;
        };
        #endregion

        private void updateUIImage(Mat mat)
        {
            if (UIImage != null) UIImage.Source = Tools.ToBitmapSource(mat);
        }
    }
}
