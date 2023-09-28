using Emgu.CV;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace VisionAnalysis
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Nd> nodes = new List<Nd>();

        public MainWindow()
        {
            InitializeComponent();

            WindowToolBox window = new WindowToolBox();
            window.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mat mat = new Mat();
            nodes.Clear();
            
            Nd input = new Nd(new UcParaInputs(nodes) { ToolName = "Inputs" });
            nodes.Add(input);
            /*
            Nd ToolThresHold = new Nd(new UcParaThresHold(nodes) { ToolName = "ThresHold1" });
            nodes.Add(ToolThresHold);
            */
            //Nd output = new Nd() { name = "Outputs" };
            //nodes.Add(output);

            tvl.ItemsSource = nodes;
        }

        private void tvl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Nd selectNd = tvl.SelectedItem as Nd;
            new WindowToolEdit((UserControl)selectNd.value) { Title = selectNd.name }.Show();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            foreach (Nd nd in nodes)
            {
                IToolEditParas tool = (IToolEditParas)nd.value;
                tool.actionProcess();
            }
        }
        private static string testPath = @"Test\test.json";
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string loadPath = $@"{Directory.GetCurrentDirectory()}\{testPath}";
            string imgDirPath = $@"{Path.GetDirectoryName(loadPath)}\Images";

            nodes.Clear();
            string str = File.ReadAllText(loadPath);
            JArray jArray = JArray.Parse(str);

            foreach(JObject jobject in jArray)
            {
                string toolType = (string)jobject["ToolType"];
                string toolName = (string)jobject["ToolName"];
                JObject inputs = (JObject)jobject["Inputs"];
                JObject outputs = (JObject)jobject["Outputs"];
                if (typeof(UcParaInputs).FullName.Equals(toolType))
                {
                    Nd input = new Nd(new UcParaInputs(nodes, inputs) { ToolName = toolName });
                    nodes.Add(input);
                }
                else if (typeof(UcParaThresHold).FullName.Equals(toolType))
                {
                    Nd ToolThresHold = new Nd(new UcParaThresHold(nodes, inputs) { ToolName = toolName });
                    nodes.Add(ToolThresHold);
                }
                else throw new Exception($"無法解析ToolType:\n{toolType}\n程式沒寫!?");

            }

            //-------------------------

            tvl.ItemsSource = null;
            tvl.ItemsSource = nodes;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string savePath = $@"{Directory.GetCurrentDirectory()}\{testPath}";
            string imgDirPath = $@"{Path.GetDirectoryName(savePath)}\Images";
            if (!Directory.Exists(imgDirPath)) Directory.CreateDirectory(imgDirPath);
            foreach(string path in Directory.GetFiles(imgDirPath))
            {
                File.Delete(path);
            }


            JArray jArray = new JArray(); 
            foreach(Nd nd in nodes)
            {
                IToolEditParas tool = (IToolEditParas)nd.value;
                jArray.Add(tool.getJObjectAndSaveImg(imgDirPath));
            }

            File.WriteAllText(savePath, jArray.ToString());
        }
    }

    public interface IPara
    {
        string name { get; }
        object value { get; set; }
    }
    public interface IUITreeViewItem
    {
        #region UI control properties
        List<IUITreeViewItem> childNodes { get; }

        string name { get; }
        bool isExpanded { get;}
        //int FontSize { get; }
        bool CanExpand { get; }
        #endregion
    }
    public class Nd : IUITreeViewItem, INotifyPropertyChanged
    {
        public List<IUITreeViewItem> childNodes { get; set; } = new List<IUITreeViewItem>();

        private string _name;
        public string name
        {
            get
            {
                if(value is IToolEditParas)
                {
                    IToolEditParas toolEditParas = value as IToolEditParas;
                    return toolEditParas.ToolName;
                }
                else
                {
                    return _name;
                }
            }
        }
        public object value { get; set; }
        public bool isExpanded { get; set; } = true;

        public Nd(IToolEditParas valueDefault)
        {
            value = valueDefault;
            foreach (var item in valueDefault.Inputs)
            {
                childNodes.Add(new Nd(item.Key, item.Value));
            }
            foreach (var item in valueDefault.Outputs)
            {
                childNodes.Add(new Nd(item.Key, item.Value));
            }
        }
        public Nd(string name, object value) { _name = name; this.value = value; }

        public bool CanExpand => childNodes.Count != 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
