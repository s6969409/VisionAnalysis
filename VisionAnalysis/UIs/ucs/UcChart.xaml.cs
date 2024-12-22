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

        private void addArrElementByBase(IQueryable queryable)
        {
            chart1.Series.Add(sCrt("Value"));
            chart1.Series[0].ToolTip = "X = #VALX, Y = #VALY";
            int x = 0;
            foreach (var item in queryable)
            {
                chart1.Series[0].Points.AddXY(x++, item);
            }

        }
        private void addArrElementByStruct(IQueryable queryable)
        {
            FieldInfo[] colsF = queryable.ElementType.GetFields();
            var filterIndexes = colsF.Where(cf =>
            {
                Type elementType = cf.FieldType;
                return cf.FieldType.IsPrimitive;
            }).Select(cf => Array.IndexOf(colsF, cf));

            foreach (int i in filterIndexes)
            {
                chart1.Series.Add(sCrt(colsF[i].Name));
                chart1.Series[chart1.Series.Count - 1].ToolTip = "X = #VALX, Y = #VALY";
            }
            object[] objs = queryable.Cast<object>().ToArray();//data
            for (int x = 0; x < objs.Length; x++)
            {
                FieldInfo[] fields = objs[x].GetType().GetFields();
                for (int i = 0; i < filterIndexes.Count(); i++)
                {
                    var y = objs[x].GetType().GetFields()[filterIndexes.ElementAt(i)].GetValue(objs[x]);

                    try
                    {
                        chart1.Series[i].Points.AddXY(x, y);
                    }
                    catch (Exception e)
                    {
                        chart1.Series[i].Points.AddY(y.ToString());
                    }
                }
            }
        }
        public void update(System.Collections.IEnumerable data)
        {
            IQueryable queryable = data.AsQueryable();
            Type elementType = queryable.ElementType;

            chart1.Series.Clear();
            if (elementType.IsPrimitive || elementType == typeof(string))
            {
                addArrElementByBase(queryable);
            }
            else
            {
                addArrElementByStruct(queryable);
            }

            chart1.ChartAreas[0].AxisX.Minimum = 0;
        }
        public void update(Mat mat)
        {
            if (!mat.Type().IsInteger)
            {
                chart1.Series.Clear();
                return;
            }

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

            byte[][] bytes = new byte[][] { data0, data1, data2 };
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < chart1.Series.Count; j++)
                {
                    chart1.Series[j].Points.AddY(bytes[j][i]);
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

        private void chart1_Click(object sender, EventArgs e)
        {
            foreach (Series s in chart1.Series) 
                s.ChartType = SeriesChartType.Stock;
        }
    }
}
