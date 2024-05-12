using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
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
            FieldInfo[] colsF = queryable.ElementType.GetFields();

            chart1.Series.Clear();
            for (int i = 0; i < colsF.Length; i++)
            {
                chart1.Series.Add(sCrt(colsF[i].Name));
            }

            foreach (var item in queryable)
            {
                FieldInfo[] fields = item.GetType().GetFields();
                for (int i = 0; i < fields.Length; i++)
                {
                    var y = item.GetType().GetFields()[i].GetValue(item);
                    try
                    {
                        chart1.Series[i].Points.AddY(y);
                    }
                    catch(Exception e)
                    {
                        chart1.Series[i].Points.AddY(y.ToString());
                    }
                }
            }
        }

        private Random random = new Random();
        private Series sCrt(string name) => new Series(name)
        {
            BorderWidth = 2,
            ChartArea = "ChartArea1",
            ChartType = SeriesChartType.Line,
            Color = System.Drawing.Color.FromArgb(random.Next(256), random.Next(256), random.Next(256))
        };
    }
}
