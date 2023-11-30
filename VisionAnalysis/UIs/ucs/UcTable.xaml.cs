using System;
using System.Collections.Generic;
using System.Dynamic;
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
    /// UcTable.xaml 的互動邏輯
    /// </summary>
    public partial class UcTable : UserControl
    {
        public UcTable()
        {
            InitializeComponent();
        }

        public void update(IEnumerable<object> data)
        {
            dg.ItemsSource = data;
        }
    }
}
