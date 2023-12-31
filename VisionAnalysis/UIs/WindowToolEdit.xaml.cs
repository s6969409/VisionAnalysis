﻿using System;
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

            #region initial fronSize
            int fontSize = WindowPreference.getCfgValue<int>(
                WindowPreference.fontSize);
            FontSize = fontSize;
            #endregion

            IToolEditParas toolEditParas = ucPara as IToolEditParas;
            toolEditParas.UIImage = img;
            this.ucPara = ucPara;

            lv_inputs.ItemsSource = toolEditParas.Inputs.Keys;
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            ((IToolEditParas)ucPara).actionProcess();
        }

        private void lv_inputs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string paraName = lv_inputs.SelectedItem as string;
            IToolEditParas toolEditParas = (IToolEditParas)ucPara;
            DataContext = toolEditParas.Inputs[paraName];
            
            //var s = Enum.Parse(typeof(ThresholdType), (string)inputs["thresholdType"])
            if(toolEditParas.Inputs[paraName].value is Enum)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.ItemsSource = toolEditParas.Inputs[paraName].valueSource;
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
