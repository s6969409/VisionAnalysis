using Emgu.CV;
using Emgu.CV.CvEnum;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private ObservableRangeCollection<Nd> nodes;
        public UcParaThresHold(ObservableRangeCollection<Nd> nodes)
        {
            InitializeComponent();
            this.nodes = nodes;
            #region read save paras or default value...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["threshold"] = new PInput() { value = 100 };
            Inputs["maxValue"] = new PInput() { value = 200 };
            Inputs["thresholdType"] = new PInput() { value = ThresholdType.Binary };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        public UcParaThresHold(ObservableRangeCollection<Nd> nodes, JObject inputs): this(nodes)
        {
            string InputImageUrl = (string)inputs["InputImage"]["value"];

            Inputs["InputImage"] = new PInput()
            {
                ToolName = (string)inputs["InputImage"]["ToolName"],
                ParaName = (string)inputs["InputImage"]["ParaName"],
                value = File.Exists(InputImageUrl) ? new Mat(InputImageUrl) : null
            };
            Inputs["threshold"] = new PInput() { value = (int)inputs["threshold"]};
            Inputs["maxValue"] = new PInput() { value = (int)inputs["maxValue"] };
            Inputs["thresholdType"] = new PInput() { value = Enum.Parse(typeof(ThresholdType), (string)inputs["thresholdType"]) };
        }

        private T getEnum<T>(object arg)
        {
            string code = arg.ToString();
            return (T)Enum.Parse(typeof(T), code);
        }

        #region implement IToolEditParas member
        public string ToolName { get; set; }
        public Image UIImage { get; set; }
        public Dictionary<string, PInput> Inputs { get; } = new Dictionary<string, PInput>();
        public Dictionary<string, POutput> Outputs { get; } = new Dictionary<string, POutput>();
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
                            if (pNameKey == Inputs[inputKey].ParaName && toolEditParas.Outputs[pNameKey].value != null)
                            {
                                Inputs[inputKey].value = toolEditParas.Outputs[pNameKey].value;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            CvInvoke.Threshold(
                (Mat)Inputs["InputImage"].value,
                (Mat)Outputs["Output1"].value,
                (int)Inputs["threshold"].value,
                (int)Inputs["maxValue"].value,
                getEnum<ThresholdType>(Inputs["thresholdType"].value));
            updateUIImage((Mat)Outputs["Output1"].value);
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

        private void updateUIImage(Mat mat)
        {
            if (UIImage != null) UIImage.Source = Tools.ToBitmapSource(mat);
        }

        
    }

    public interface IToolEditParas
    {
        string ToolName { get; set; }
        Image UIImage { get; set; }
        Dictionary<string, PInput> Inputs { get; }
        Dictionary<string, POutput> Outputs { get; }
        Action actionProcess { get; }
        Func<string, JObject> getJObjectAndSaveImg { get; }
    }
    public class PInput: INotifyPropertyChanged
    {
        private string _ToolName;
        public string ToolName
        {
            get => _ToolName;
            set
            {
                _ToolName = value;
                onPropertyChanged(nameof(ToolName));
            }
        }
        private string _ParaName;
        public string ParaName
        {
            get => _ParaName;
            set
            {
                _ParaName = value;
                onPropertyChanged(nameof(ParaName));
            }
        }
        private object _value;
        public object value { 
            get => _value; 
            set
            {
                if(_value == null)
                {
                    _value = value;
                }
                else
                {
                    _value = Convert.ChangeType(value, _value.GetType());
                }

                onPropertyChanged(nameof(this.value));
            } 
        }
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
            else jobject["value"] = value == null ? null : value.ToString();

            return jobject;
        }
        public Type Type { get => _value == null ? null : _value.GetType(); }
        public Array valueSource
        {
            get => _value == null ? null : Enum.GetValues(_value.GetType());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class POutput: INotifyPropertyChanged
    {
        private object _value;
        public object value
        {
            get => _value;
            set
            {
                _value = value;
                onPropertyChanged(nameof(value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
