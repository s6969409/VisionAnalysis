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
        public UcParaInputs(List<Nd> nodes)
        {
            InitializeComponent();
            #region read save paras or default value...
            Inputs["ImageUrl"] = new IInput() { value = "" };
            Outputs["SourceImage"] = null;
            #endregion
        }
        public UcParaInputs(List<Nd> nodes, JObject inputs) : this(nodes)
        {
            ucT_ImageUrl.PValue = (string)inputs["ImageUrl"];
        }

        #region implement IToolEditParas member
        public string ToolName { get; set; }
        public Image UIImage { get; set; }
        public Dictionary<string, IInput> Inputs { get; } = new Dictionary<string, IInput>();
        public Dictionary<string, object> Outputs { get; } = new Dictionary<string, object>();
        public Action actionProcess => () =>
        {
            if (File.Exists(ucT_ImageUrl.PValue.ToString()))
            {
                Outputs["SourceImage"] = new Mat(ucT_ImageUrl.PValue.ToString());
                updateUIImage((Mat)Outputs["SourceImage"]);
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

        private void tb_path_TextChanged(object sender, TextChangedEventArgs e)
        {
            Inputs["ImageUrl"].value = ucT_ImageUrl.PValue.ToString();
        }
        private void updateUIImage(Mat mat)
        {
            if (UIImage != null) UIImage.Source = Tools.ToBitmapSource(mat);
        }
    }
}
