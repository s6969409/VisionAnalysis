using Emgu.CV;
using Emgu.CV.CvEnum;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisionAnalysis
{
    /// <summary>
    /// UcParaThresHold.xaml 的互動邏輯
    /// </summary>
    public partial class UcParaThresHold : UserControl, IToolEditParas
    {
        private List<Nd> nodes;
        public UcParaThresHold(List<Nd> nodes)
        {
            InitializeComponent();
            this.nodes = nodes;
            #region read save paras or default value...
            Inputs["InputImage"] = new IInput() {
                ToolName = "Inputs", ParaName = "SourceImage",
                value = new Mat(@"D:\05_Project\HCA-20tmpResult\BackGroung_Image.bmp") };
            Inputs["threshold"] = new IInput() { value = 100 };
            Inputs["maxValue"] = new IInput() { value = 200 };
            Inputs["thresholdType"] = new IInput() { value = ThresholdType.Binary };

            Outputs["Output1"] = new Mat();
            #endregion
        }
        public UcParaThresHold(List<Nd> nodes, JObject inputs): this(nodes)
        {
            Inputs["InputImage"] = new IInput()
            {
                ToolName = (string)inputs["InputImage"]["ToolName"],
                ParaName = (string)inputs["InputImage"]["ParaName"],
                value = new Mat((string)inputs["InputImage"]["value"])
            };
            Inputs["threshold"] = new IInput() { value = (int)inputs["threshold"]};
            Inputs["maxValue"] = new IInput() { value = (int)inputs["maxValue"] };
            Inputs["thresholdType"] = new IInput() { value = Enum.Parse(typeof(ThresholdType), (string)inputs["thresholdType"]) };
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 使用正則表達式確保只允許數值輸入
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private T getEnum<T>(object arg)
        {
            string code = arg.ToString();
            return (T)Enum.Parse(typeof(T), code);
        }

        #region implement IToolEditParas member
        public string ToolName { get; set; }
        public Image UIImage { get; set; }
        public Dictionary<string, IInput> Inputs { get; } = new Dictionary<string, IInput>();
        public Dictionary<string, object> Outputs { get; } = new Dictionary<string, object>();
        public Action actionProcess => () =>
        {
            #region read paras
            foreach (string inputKey in Inputs.Keys)
            {
                foreach (Nd nd in nodes)
                {
                    if (nd.name == Inputs[inputKey].ToolName)
                    {
                        IToolEditParas toolEditParas = nd.value as IToolEditParas;
                        foreach (string pNameKey in toolEditParas.Outputs.Keys)
                        {
                            if (pNameKey == Inputs[inputKey].ParaName && toolEditParas.Outputs[pNameKey] != null)
                            {
                                Mat mat = (Mat)toolEditParas.Outputs[pNameKey];

                                Inputs[inputKey].value = toolEditParas.Outputs[pNameKey];
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            CvInvoke.Threshold(
                (Mat)Inputs["InputImage"].value,
                (Mat)Outputs["Output1"],
                (int)Inputs["threshold"].value,
                (int)Inputs["maxValue"].value,
                getEnum<ThresholdType>(Inputs["thresholdType"].value));
            updateUIImage((Mat)Outputs["Output1"]);
        };

        public Func<string, JObject> getJObjectAndSaveImg => (imgDirPath) =>
        {
            JObject jobject = new JObject();
            jobject["ToolType"] = GetType().FullName;
            jobject["ToolName"] = ToolName;
            jobject["Inputs"] = new JObject();
            jobject["Inputs"]["InputImage"] = Inputs["InputImage"].getJObjectAndSaveImg(imgDirPath);
            jobject["Inputs"]["threshold"] = (int)Inputs["threshold"].value;
            jobject["Inputs"]["maxValue"] = (int)Inputs["maxValue"].value;
            jobject["Inputs"]["thresholdType"] = Inputs["thresholdType"].value.ToString();
            jobject["Outputs"] = new JObject();
            jobject["Outputs"]["Output1"] = 100;


            return jobject;
        };
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Mat mat = (Mat)Inputs["InputImage"].value;

            updateUIImage(mat);
        }
        private void updateUIImage(Mat mat)
        {
            if (UIImage != null) UIImage.Source = Tools.ToBitmapSource(mat);
        }

        
    }

    public interface IToolEditParas
    {
        string ToolName { get; }
        Image UIImage { get; set; }
        Dictionary<string, IInput> Inputs { get; }
        Dictionary<string, object> Outputs { get; }
        Action actionProcess { get; }
        Func<string, JObject> getJObjectAndSaveImg { get; }
    }
    public class IInput
    {
        public string ToolName { get; set; }
        public string ParaName { get; set; }
        public object value { get; set; }
        public JObject getJObjectAndSaveImg(string imgDirPath)
        {
            JObject jobject = new JObject();
            jobject["ToolName"] = ToolName;
            jobject["ParaName"] = ParaName;
            if (value is Mat)
            {
                string imgPath = $@"{imgDirPath}\{Directory.GetFiles(imgDirPath).Length}.bmp";
                ((Mat)value).Save(imgPath);
                jobject["value"] = imgPath;
            }
            else jobject["value"] = value.ToString();

            return jobject;
        }
    }
}
