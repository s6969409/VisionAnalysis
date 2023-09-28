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
    /// WindowToolBox.xaml 的互動邏輯
    /// </summary>
    public partial class WindowToolBox : Window
    {
        public WindowToolBox()
        {
            InitializeComponent();
        }

        private void tv_Loaded(object sender, RoutedEventArgs e)
        {
            List<Nd> tools = new List<Nd>();
            #region ToolThresHold 重想一下結構
            //Nd paraThresHold = new Nd() { name = "ThresHold" };
            //Nd ToolThresHold = new Nd() { name = "ToolThresHold" };
            //ToolThresHold.childNodes.Add(paraThresHold);
            //tools.Add(ToolThresHold);
            #endregion

            tv.ItemsSource = tools;
        }
    }
}
