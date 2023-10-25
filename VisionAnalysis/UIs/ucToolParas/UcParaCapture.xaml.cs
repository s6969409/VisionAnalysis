using Emgu.CV;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace VisionAnalysis.UIs.ucToolParas
{
    /// <summary>
    /// UcParaCapture.xaml 的互動邏輯
    /// </summary>
    public partial class UcParaCapture : UserControl, IToolEditParas
    {
        private ObservableRangeCollection<Nd> nodes;
        public UcParaCapture()
        {
            InitializeComponent();
        }

        #region implement IToolEditParas member
        public string ToolName { get; set; }
        public IMatProperty UIImage { get; set; }
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

            //CvInvoke.Threshold(
            //    (Mat)Inputs["InputImage"].value,
            //    (Mat)Outputs["Output1"].value,
            //    (int)Inputs["threshold"].value,
            //    (int)Inputs["maxValue"].value,
            //    getEnum<ThresholdType>(Inputs["thresholdType"].value));
            //updateUIImage((Mat)Outputs["Output1"].value);
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

    }
}
