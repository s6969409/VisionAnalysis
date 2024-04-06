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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisionAnalysis
{
    /// <summary>
    /// UcAnalysis.xaml 的互動邏輯
    /// </summary>
    public partial class UcAnalysis : UserControl
    {
        public UcAnalysis()
        {
            InitializeComponent();
        }

        private void focus(DependencyObject dependencyObject)
        {
            TabItem tabItem = LogicalTreeHelper.GetParent(dependencyObject) as TabItem;
            TabControl tabControl = LogicalTreeHelper.GetParent(tabItem) as TabControl;
            tabControl.SelectedItem = tabItem;
        }
        private void loadImg(Mat mat)
        {
            img.Image = mat == null ? null : mat;
            focus(img);
        }
        public void update(object value)
        {
            if (value is PInput pInput)
            {
                if(pInput.value is Mat mat) loadImg(mat);
            }
            else if (value is POutput pOutput)
            {
                if (pOutput.value == null) return;
                if (pOutput.value is Mat mat) loadImg(mat);
                else if (pOutput.value is System.Collections.IEnumerable items)
                {
                    table.update(items);
                    ti_table.Header = $"table({items.Cast<object>().Count()})";
                    ti_table.Focus();
                }
                else
                {
                    tv_obj.update(pOutput.value);
                    focus(tv_obj);
                }
            }
            else if (value is IToolEditParas)
            {

            }
            else
            {
                throw new ArgumentException($"{value} 沒有定義處理函數");
            }
        }
    }
}
