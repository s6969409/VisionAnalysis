﻿using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace VisionAnalysis
{
    /// <summary>
    /// UcImage.xaml 的互動邏輯
    /// </summary>
    public partial class UcImage : UserControl, IMatProperty
    {
        public UcImage()
        {
            InitializeComponent();
        }

        public Mat Image 
        { 
            get => (Mat)cvIb.Image;
            set
            {
                cvIb.Image = value;
                cvIb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
                if (value == null) return;
                lb_size.Content = $"{value.Width}*{value.Height}*{value.NumberOfChannels}";
                lb_format.Content = $"{value.Depth},Dims={value.Dims}";
            }
        }

        private double scale
        {
            get
            {
                if (Image != null)
                {
                    double heightStd = (double)cvIb.ClientSize.Width * Image.Height / Image.Width;//Y/X
                    if (cvIb.ClientSize.Height > heightStd) return (double)cvIb.ClientSize.Width / Image.Width;
                    else return (double)cvIb.ClientSize.Height / Image.Height;
                }
                else
                {
                    return 1;
                }
            }
        }

        private void cvIb_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ImageBox ib = sender as ImageBox;
            Mat mat = ib.Image as Mat;
            if (mat == null) return;
            int offsetX = (int)(ib.Width / scale - mat.Width)/2;
            int x = (int)(e.X / scale - offsetX);
            int y = (int)(e.Y / scale);

            if(x < 0 || x >= mat.Width || y < 0 || y >= mat.Height)
            {
                lb_position.Content = $"滑鼠座標:-";
                lb_value.Content = $"value:-";
                return;
            }
            lb_position.Content = $"滑鼠座標:{x},{y}";

            IEnumerable<object> vs = new object[mat.NumberOfChannels];
            vs = vs.Select((a, i) => mat.GetData().GetValue(y, x, i));
            lb_value.Content = $"value:{string.Join(",", vs)}";
        }
        private void cvIb_SizeChanged(object sender, EventArgs e)
        {
            lb_scale.Content = $"放大倍率:{scale.ToString("0.00")}";
        }
    }
    public interface IMatProperty
    {
        Mat Image { set; get; }
    }
}
