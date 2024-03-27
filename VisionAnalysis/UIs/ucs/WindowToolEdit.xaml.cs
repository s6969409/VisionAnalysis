using OpenCvSharp;
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
    public partial class WindowToolEdit : System.Windows.Window
    {
        private IToolEditParas toolEditParas;
        private IEnumerable<Nd> nodes;
        public WindowToolEdit(IToolEditParas toolEditParas, IEnumerable<Nd> nodes)
        {
            InitializeComponent();

            #region initial fronSize
            int fontSize = WindowPreference.getCfgValue<int>(
                WindowPreference.fontSize);
            FontSize = fontSize;
            #endregion

            cc_valueUIInit();

            this.toolEditParas = toolEditParas;
            toolEditParas.UIImage = uc_Analysis.img;

            Nd ndInput = new Nd("Inputs", null);
            ndInput.childNodes.AddRange(toolEditParas.Inputs.Select(i => TepHelper.NdBuild(i)));
            tv_inputs.ItemsSource = ndInput.childNodes;

            Nd udOutput = new Nd("Outputs", null);
            udOutput.childNodes.AddRange(toolEditParas.Outputs.Select(i => TepHelper.NdBuild(i)));
            tv_outputs.ItemsSource = udOutput.childNodes;
            
            this.nodes = nodes;

            cb_ToolName.ItemsSource = new string[] { "" }.Concat(nodes.Select(nd => nd.name).Where(name => name != toolEditParas.ToolName));
        }

        private void Run_Click(object sender, RoutedEventArgs e) => toolEditParas.actionProcess();

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
                cb_ParaName.ItemsSource = new string[] { "" }.Concat(selectedTool.Outputs.Select(p => p.Key));
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
                cc_value.Content = comboBox;
                comboBox.ItemsSource = toolEditParas.Inputs[paraName].valueSource;
            }
            else if (val.value is Mat)
            {
                uc_Analysis.update(val);

                textBox.IsEnabled = false;
                cc_value.Content = textBox;
            }
            else
            {
                textBox.IsEnabled = true;
                cc_value.Content = textBox;
            }
        }

        private void tv_outputs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Nd selected = tv_outputs.SelectedItem as Nd;
            if (selected == null) return;
            uc_Analysis.update(selected.value);
        }

        #region ContentControl cc_value used
        private TextBox textBox = new TextBox();
        private ComboBox comboBox = new ComboBox();
        private void cc_valueUIInit()
        {
            Binding binding = new Binding("value");
            textBox.SetBinding(TextBox.TextProperty, binding);
            comboBox.SetBinding(ComboBox.SelectedItemProperty, binding);
        }
        #endregion

    }
}
