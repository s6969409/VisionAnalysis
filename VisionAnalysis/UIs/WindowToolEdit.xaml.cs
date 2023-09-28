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
        public UserControl ucPara;
        public WindowToolEdit(UserControl ucPara)
        {
            InitializeComponent();
            IToolEditParas toolEditParas = ucPara as IToolEditParas;
            toolEditParas.UIImage = img;
            this.ucPara = ucPara;
            tbSetting.Content = ucPara;

            lv_inputs.ItemsSource = toolEditParas.Inputs.Keys;
        }

        public object this[string key] { get => 0; }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            ((IToolEditParas)ucPara).actionProcess();
        }

        private void lv_inputs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string paraName = lv_inputs.SelectedItem as string;
            IToolEditParas toolEditParas = (IToolEditParas)ucPara;
            ucT_ToolName.PValue = toolEditParas.Inputs[paraName].ToolName;
            ucT_ParaName.PValue = toolEditParas.Inputs[paraName].ParaName;
            ucT_value.PValue = toolEditParas.Inputs[paraName].value;
        }
    }

    
}
