using Emgu.CV;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace VisionAnalysis
{
    /// <summary>
    /// UcParaCapture.xaml 的互動邏輯
    /// </summary>
    public partial class UcParaCapture : UserControl, IToolEditParas
    {
        private ObservableRangeCollection<Nd> nodes;
        public UcParaCapture(ObservableRangeCollection<Nd> nodes)
        {
            InitializeComponent();
            this.nodes = nodes;

            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["ROI"] = new PInput() { value = ParaDictBuilder.Rectangle() };

            Outputs["Output1"] = new POutput() { value = new Mat() };
        }

        #region implement IToolEditParas member
        public string ToolName { get; set; }
        public IMatProperty UIImage { get; set; }
        public Dictionary<string, PInput> Inputs { get; } = new Dictionary<string, PInput>();
        public Dictionary<string, POutput> Outputs { get; } = new Dictionary<string, POutput>();
        public Action actionProcess => () =>
        {
            //read paras
            UcPHelper.readInputs(this, nodes);

            Dictionary<string, PInput> dictROI = (Dictionary<string, PInput>)Inputs["ROI"].value;
            Rectangle roi = ParaDictRead.Rectangle(dictROI);
            Outputs["Output1"].value = new Mat((Mat)Inputs["InputImage"].value, roi);
            updateUIImage((Mat)Outputs["Output1"].value);
        };

        public Func<string, JObject> getJObjectAndSaveImg => (imgDirPath) =>
        {
            JObject jobject = new JObject();
            jobject["ToolType"] = GetType().FullName;
            jobject["ToolName"] = ToolName;
            jobject["Inputs"] = new JObject();
            jobject["Inputs"]["InputImage"] = Inputs["InputImage"].getJObjectAndSaveImg(imgDirPath);
            jobject["Inputs"]["ROI"] = null;//???


            return jobject;
        };
        #endregion

        private void updateUIImage(Mat mat)
        {
            if (UIImage != null) UIImage.Image = mat;
        }
    }
}
