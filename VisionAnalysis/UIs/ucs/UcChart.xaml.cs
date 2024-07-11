using OpenCvSharp;
using System;
using System.Collections.Concurrent;
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
        public void update(Mat mat)
        {
            if (!mat.Type().IsInteger) return;
            byte[] data0 = new byte[256];
            byte[] data1 = new byte[256];
            byte[] data2 = new byte[256];
            Parallel.For(0, mat.Rows, y =>
            {
                for (int x = 0; x < mat.Cols; x++)
                {
                    Vec3b val = mat.At<Vec3b>(y, x);
                    data0[val.Item0]++;
                    data1[val.Item1]++;
                    data2[val.Item2]++;
                }
            });

            chart1.Series.Clear();
            for (int i = 0; i < mat.Channels(); i++)
            {
                chart1.Series.Add(sCrt($"Channel {i}"));
            }

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < chart1.Series.Count; j++)
                {
                    chart1.Series[j].Points.AddY(data0[i]);
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
