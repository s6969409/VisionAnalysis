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
        private WindowToolBox toolBox;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region initial fronSize
            int fontSize = WindowPreference.getCfgValue<int>(
                WindowPreference.fontSize);
            if (fontSize == 0)
            {
                fontSize = 16;
                WindowPreference.saveCfgValue(WindowPreference.fontSize, fontSize);
            }
            FontSize = fontSize;
            #endregion

            #region UI setting
            tVImoveByMouse = new TVImoveByMouse(this, tvl, tvl_Move);
            toolBox = new WindowToolBox(addTool);
            #endregion

            Mat mat = new Mat();
            nodes.Clear();
            
            Nd input = new Nd(new TepInputs(nodes) { ToolName = "Inputs" });
            nodes.Add(input);

            tvl.ItemsSource = nodes;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if(e.Key == Key.F2)
            {
                Hide();
                WindowPreference window = new WindowPreference();
                WindowControlHelper.WindowInitial(window, this,
                    WindowControlHelper.WindowLocation.OwnCenter);
                window.ShowDialog();
                Show();
            }
        }

        private void tvl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Nd selectNd = tvl.SelectedItem as Nd;
            if(selectNd != null && selectNd.value is IToolEditParas)
                new WindowToolEdit((IToolEditParas)selectNd.value, nodes) { Title = selectNd.name }.Show();
        }

        #region ToolBar click event
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            #region runException read
            bool runException = WindowPreference.getCfgValue<bool>(
                WindowPreference.runException);
            #endregion

            foreach (Nd nd in nodes)
            {
                IToolEditParas tool = (IToolEditParas)nd.value;
                if(runException) tool.actionProcess();
                else
                {
                    try { tool.actionProcess(); }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), $"{tool.ToolName} Exception!");
                        break;
                    }
                }
            }
        }
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string loadPath = PathSelector.getUserSelectPath(PathSelector.PathRequest.ReadFile);
            if (!File.Exists(loadPath)) return;

            nodes.Clear();
            string str = File.ReadAllText(loadPath);
            JArray jArray = JArray.Parse(str);

            foreach(JObject jobject in jArray)
            {
                string toolType = (string)jobject["ToolType"];
                Type type = Type.GetType(toolType);
                if (type == null) throw new Exception($"無法解析ToolType:\n{toolType}\n程式沒寫!?");

                IToolEditParas input = Activator.CreateInstance(type, new object[] { nodes }) as IToolEditParas;
                input.loadParas(jobject);
                nodes.Add(new Nd(input));
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string savePath = PathSelector.getUserSelectPath(PathSelector.PathRequest.SaveFile);
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
        private void ToolBox_Click(object sender, RoutedEventArgs e)
        {
            WindowControlHelper.WindowInitialShow(toolBox, this,
                    WindowControlHelper.WindowLocation.Left);
        }
        #endregion

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
            loadImg(null);
            Nd selected = tvl.SelectedItem as Nd;
            if (selected == null) return; 
            if (selected.value is Mat)
            {
                loadImg((Mat)selected.value);
            }
            else if (selected.value is PInput)
            {
                PInput selectedInput = selected.value as PInput;
                if (selectedInput.value is Mat)
                {
                    loadImg((Mat)selectedInput.value);
                }
            }
            else if (selected.value is POutput)
            {
                POutput selectedInput = selected.value as POutput;
                if (selectedInput.value is Mat)
                {
                    loadImg((Mat)selectedInput.value);
                }
                else if (selectedInput.value is IEnumerable<object>)
                {
                    table.update((IEnumerable<object>)selectedInput.value);
                }
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
            IToolEditParas iTool = Activator.CreateInstance(type, new object[] { nodes }) as IToolEditParas;
            iTool.ToolName = toolNameGenerate(type);
            Nd addNd = new Nd(iTool);
            nodes.Add(addNd);
        }
        private string toolNameGenerate(Type type)
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
            return toolName;
        }
        #endregion

        private void loadImg(Mat mat) => img.Image = mat;

        private void tvl_Drop(object sender, DragEventArgs e)
        {
            string[] paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths.Length != 1) return;

            IToolEditParas iTool = new TepInputs(nodes);
            iTool.ToolName = toolNameGenerate(typeof(TepInputs));
            iTool.Inputs["ImageUrl"].value = paths[0];
            iTool.actionProcess();
            Nd addNd = new Nd(iTool);
            nodes.Add(addNd);
        }

        private void tvl_DragEnter(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else e.Effects = DragDropEffects.None;
        }
    }

    public interface IUIPara
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
                if (value is IToolEditParas)
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
        public string tip
        {
            get
            {
                if(value is IParaValue)
                {
                    IParaValue paraVal = value as IParaValue;
                    return $"type: {paraVal.value.GetType()}\nvalue: {paraVal.value}";
                }
                else if (value is IToolEditParas)
                {
                    IToolEditParas val = value as IToolEditParas;
                    return $"type: {val.GetType()}\nname: {val.ToolName}";
                }
                else
                {
                    throw new Exception("UI Node value is not {IParaValue} or {IToolEditParas}");
                }
            }
        }

        public Nd(IToolEditParas valueDefault)
        {
            value = valueDefault;
            foreach (var item in valueDefault.Inputs)
            {
                childNodes.Add(buildParaNd(item));
            }
            foreach (var item in valueDefault.Outputs)
            {
                childNodes.Add(buildParaNd(item));
            }
        }
        public Nd(string name, object value) { _name = name; this.value = value; }
        private Nd buildParaNd<T>(KeyValuePair<string, T> item)where T: IParaValue
        {
            Nd nd = new Nd(item.Key, item.Value);
            if (item.Value.value is Dictionary<string, T>)
            {
                foreach (KeyValuePair<string, T> i in (Dictionary<string, T>)item.Value.value)
                {
                    nd.childNodes.Add(buildParaNd(i));
                }
            }
            return nd;
        }

        public bool CanExpand => childNodes.Count != 0;
        public int FontSize { get => WindowPreference.getCfgValue<int>(WindowPreference.fontSize); }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
