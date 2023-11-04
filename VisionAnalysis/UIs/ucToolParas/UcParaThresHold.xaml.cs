using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
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
            Inputs["threshold"] = ParaDictBuilder.JObjectToPInput(inputs["threshold"]);
            Inputs["maxValue"] = ParaDictBuilder.JObjectToPInput(inputs["maxValue"]);
            Inputs["thresholdType"] = ParaDictBuilder.JObjectToPInput<ThresholdType>(inputs["thresholdType"]);
        }

        private T getEnum<T>(object arg)
        {
            string code = arg.ToString();
            return (T)Enum.Parse(typeof(T), code);
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
            jobject["Inputs"] = PInput.getJObjectAndSaveImg(Inputs, imgDirPath);

            return jobject;
        };
        #endregion

        private void updateUIImage(Mat mat)
        {
            if (UIImage != null) UIImage.Image = mat;
        }

        
    }

    public interface IToolEditParas
    {
        string ToolName { get; set; }
        IMatProperty UIImage { get; set; }
        Dictionary<string, PInput> Inputs { get; }
        Dictionary<string, POutput> Outputs { get; }
        Action actionProcess { get; }
        Func<string, JObject> getJObjectAndSaveImg { get; }
    }
    public interface IParaValue
    {
        object value { get; }
    }
    public class PInput: IParaValue, INotifyPropertyChanged
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
        public static JObject getJObjectAndSaveImg(Dictionary<string, PInput> inputs, string imgDirPath)
        {
            JObject jobject = new JObject();
            foreach(string key in inputs.Keys)
            {
                jobject[key] = getJObjectAndSaveImg(inputs[key], imgDirPath);
            }
            return jobject;
        }
        public static JObject getJObjectAndSaveImg(PInput pi, string imgDirPath)
        {
            JObject jobject = new JObject();
            jobject["ToolName"] = pi.ToolName;
            jobject["ParaName"] = pi.ParaName;
            if (pi.value is PInput)
            {
                PInput input = pi.value as PInput;
                jobject["value"] = getJObjectAndSaveImg(input, imgDirPath);
            }
            else if (pi.value is Dictionary<string, PInput>)
            {
                jobject["value"] = new JObject();

                Dictionary<string, PInput> paras = pi.value as Dictionary<string, PInput>;
                foreach(var p in paras)
                {
                    jobject["value"][p.Key] = getJObjectAndSaveImg(p.Value, imgDirPath);
                }
            }
            else if (pi.value is Mat)
            {
                string imgPath = $@"{imgDirPath}\{Directory.GetFiles(imgDirPath).Length}.bmp";
                ((Mat)pi.value).Save(imgPath);
                jobject["value"] = imgPath;
            }
            else if (pi.value is int) jobject["value"] = (int)pi.value;
            else if (pi.value is float) jobject["value"] = (float)pi.value;
            else jobject["value"] = pi.value == null ? null : pi.value.ToString();

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
    public class POutput: IParaValue, INotifyPropertyChanged
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
