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
using System.Windows.Shapes;

namespace VisionAnalysis
{
    /// <summary>
    /// WindowToolEdit.xaml 的互動邏輯
    /// </summary>
    public partial class WindowToolEdit : Window
    {
        private UserControl ucPara;
        private IEnumerable<Nd> nodes;
        public WindowToolEdit(UserControl ucPara, IEnumerable<Nd> nodes)
        {
            InitializeComponent();

            #region initial fronSize
            int fontSize = WindowPreference.getCfgValue<int>(
                WindowPreference.fontSize);
            FontSize = fontSize;
            #endregion

            IToolEditParas toolEditParas = ucPara as IToolEditParas;
            toolEditParas.UIImage = img;
            this.ucPara = ucPara;

            Nd nd1 = new Nd("Inputs", null);
            nd1.childNodes.AddRange(toolEditParas.Inputs.Select(i => UcPHelper.NdBuild(i)));
            tv_inputs.ItemsSource = nd1.childNodes;

            this.nodes = nodes;

            cb_ToolName.ItemsSource = new string[] { "" }.Concat(nodes.Select(nd => nd.name).Where(name => name != toolEditParas.ToolName));
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            ((IToolEditParas)ucPara).actionProcess();
        }

        private void cb_ToolName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            cb_ParaName.IsEnabled = comboBox.SelectedItem != null;
            if (!cb_ParaName.IsEnabled) return;
            if(comboBox.SelectedItem.ToString() == "")
            {
                cb_ParaName.ItemsSource = null;
            }
            else
            {
                Nd selectedNd = nodes.First(nd => nd.name == comboBox.SelectedItem.ToString());
                IToolEditParas selectedTool = selectedNd.value as IToolEditParas;
                cb_ParaName.ItemsSource = new string[] { "" }.Concat(selectedTool.Outputs.Keys);
            }
        }

        private void tv_inputs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            cb_ToolName.IsEnabled = true;
            Nd selectedNd = tv_inputs.SelectedItem as Nd;
            string paraName = selectedNd.name;
            PInput val = selectedNd.value as PInput;
            DataContext = val;
            if (val.value is Enum)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.ItemsSource = val.valueSource;
                Binding binding = new Binding("value");
                comboBox.SetBinding(ComboBox.SelectedItemProperty, binding);

                cc_value.Content = comboBox;
            }
            else
            {
                TextBox textBox = new TextBox();
                Binding binding = new Binding("value");
                textBox.SetBinding(TextBox.TextProperty, binding);

                cc_value.Content = textBox;
            }
        }
    }
}
