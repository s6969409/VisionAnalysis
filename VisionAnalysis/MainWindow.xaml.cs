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
        private ObservableRangeCollection<Nd> nodes = new ObservableRangeCollection<Nd>();
        private TVImoveByMouse tVImoveByMouse;

        public MainWindow()
        {
            InitializeComponent();

            tVImoveByMouse = new TVImoveByMouse(this, tvl, tvl_Move);

            WindowToolBox window = new WindowToolBox(addTool);
            window.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mat mat = new Mat();
            nodes.Clear();
            
            Nd input = new Nd(new UcParaInputs(nodes) { ToolName = "Inputs" });
            nodes.Add(input);

            tvl.ItemsSource = nodes;
        }

        private void tvl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Nd selectNd = tvl.SelectedItem as Nd;
            if(selectNd.value is IToolEditParas)
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
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string loadPath = PathSelector.getUserSelectPath(PathSelector.PathRequest.ReadFile);
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
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string savePath = PathSelector.getUserSelectPath(PathSelector.PathRequest.ReadFile);
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

        #region events for TreeViewItem move by mouse hold
        private void tvi_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tVImoveByMouse.tvi_MouseDown(sender, e);
        }
        private void tvi_MouseMove(object sender, MouseEventArgs e)
        {
            tVImoveByMouse.tvi_MouseMove(sender, e);
        }
        private void tvl_Move(object odd, object dst)
        {
            Nd cut = odd as Nd;
            Nd past = dst as Nd;
            if (!(past.value is IToolEditParas)) return;

            int cutId = nodes.IndexOf(cut);
            int pastId = nodes.IndexOf(past);
            nodes.Move(cutId, pastId);
        }
        #endregion
        #region show by selected para
        private void tvl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Nd selected = tvl.SelectedItem as Nd;
            if (selected == null) return;
            if(selected.value is Mat)
            {
                img.Source = Tools.ToBitmapSource((Mat)selected.value);
            }
            else if (selected.value is PInput)
            {
                PInput selectedInput = selected.value as PInput;
                if (selectedInput.value is Mat)
                {
                    img.Source = Tools.ToBitmapSource((Mat)selectedInput.value);
                }
                else
                {
                    img.Source = null;
                }
            }
            else if (selected.value is POutput)
            {
                POutput selectedInput = selected.value as POutput;
                if (selectedInput.value is Mat)
                {
                    img.Source = Tools.ToBitmapSource((Mat)selectedInput.value);
                }
                else
                {
                    img.Source = null;
                }
            }
            else
            {
                img.Source = null;
            }


        }
        #endregion
        #region events for TreeViewItem remove by MenuItem mouse click
        private Nd targetItem;
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Nd selectNode = tvl.SelectedItem as Nd;
            if (selectNode == null) return;

            MenuItem menuItem = sender as MenuItem;
            string name = menuItem.Header as string;
            if (name.Equals("Remove"))
            {
                nodes.Remove(selectNode);
            }
            else if (name.Equals("Cut"))
            {
                targetItem = selectNode;
                nodes.Remove(selectNode);
            }
            else if (name.Equals("Copy"))
            {
                targetItem = selectNode;
            }
            else if (name.Equals("Past") && targetItem != null)
            {
                //Nd copyItem = targetItem.depthClone();
                //int idx = nodes.IndexOf(selectNode);
                //nodes.Insert(idx, copyItem);
            }
        }
        private void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            menuItem.IsEnabled = targetItem != null;
        }
        #endregion

        #region window API for other window
        private void addTool(Type type)
        {
            int num = 1;
            string toolName = $"{type.Name}{num}";
            bool hasSameName = true;
            while (hasSameName)
            {
                hasSameName = false;
                foreach (Nd nd in nodes)
                {
                    IToolEditParas toolEditParas = nd.value as IToolEditParas;
                    if (toolEditParas.ToolName.Equals(toolName))
                    {
                        num++;
                        toolName = $"{type.Name}{num}";
                        hasSameName = true;
                        break;
                    }
                }
            }

            Nd ToolThresHold = new Nd(new UcParaThresHold(nodes) { ToolName = toolName });
            nodes.Add(ToolThresHold);
        }
        #endregion

    }

    public interface IPara
    {
        string name { get; }
        object value { get; set; }
    }
    public interface IUITreeViewItem
    {
        #region UI control properties
        ObservableRangeCollection<IUITreeViewItem> childNodes { get; }

        string name { get; }
        bool isExpanded { get;}
        //int FontSize { get; }
        bool CanExpand { get; }
        #endregion
    }
    public class Nd : IUITreeViewItem, INotifyPropertyChanged
    {
        public ObservableRangeCollection<IUITreeViewItem> childNodes { get; set; } = new ObservableRangeCollection<IUITreeViewItem>();

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
