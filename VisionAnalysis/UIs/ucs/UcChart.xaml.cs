using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisionAnalysis
{
    /// <summary>
    /// UcChart.xaml 的互動邏輯
    /// </summary>
    public partial class UcChart : UserControl
    {
        public UcChart()
        {
            InitializeComponent();
        }

        public void update(System.Collections.IEnumerable data)
        {
            IQueryable queryable = data.AsQueryable();
            IEnumerable<string> columnNames = queryable.ElementType.GetFields().Select(r => r.Name);

            chart1.Series.Clear();
            chart1.Series.Add(sCrt(columnNames.ElementAt(0)));


            foreach (var item in queryable)
            {
                //var args = item.GetType().GetFields().Select(r => r.GetValue(item)).ToArray();
                var y = item.GetType().GetFields()[0].GetValue(item);
                chart1.Series[0].Points.AddY(y);
            }
            //chart1.Series["Series1"].Points.AddY(y);
            //chart1.ChartAreas[0].AxisX.ScaleView.Position = chart1.Series["Series1"].Points.Count - chart1.ChartAreas[0].AxisX.ScaleView.Size;
        }

        private Series sCrt(string name) => new Series(name)
        {
            BorderWidth = 2,
            ChartArea = "ChartArea1",
            ChartType = SeriesChartType.Line,
            Color = System.Drawing.Color.Black
        };
    }
}
