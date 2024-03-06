using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
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

        private Mat mat;
        public Mat Image 
        {
            get => mat;
            set
            {
                mat = value;
                img.Source = value == null ? null : value.ToBitmapSource();
                if (value == null) return;
                lb_size.Content = $"{value.Width}*{value.Height}*{value.Channels()}";
                lb_format.Content = $"{value.Depth()},Dims={value.Dims}";
            }
        }

        private double scale
        {
            get
            {
                if (Image != null)
                {
                    double heightStd = (double)img.Width * Image.Height / Image.Width;//Y/X
                    if (img.Height > heightStd) return (double)img.Width / Image.Width;
                    else return (double)img.Height / Image.Height;
                }
                else
                {
                    return 1;
                }
            }
        }

        private void cvIb_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Image == null) return;
            int offsetX = (int)(Image.Width / scale - mat.Width) / 2;
            int offsetY = (int)(Image.Height / scale - mat.Height) / 2;
            int x = (int)(e.X / scale - offsetX);
            int y = (int)(e.Y / scale - offsetY);

            if(x < 0 || x >= mat.Width || y < 0 || y >= mat.Height)
            {
                lb_position.Content = $"滑鼠座標:-";
                lb_value.Content = $"value:-";
                return;
            }
            lb_position.Content = $"滑鼠座標:{x},{y}";

            int channels = Image.Channels();
            IEnumerable<byte> vs = Enumerable.Range(0, channels)
                .Select(i => channels == 1 ? mat.Get<byte>(y, x) : mat.Get<byte>(y, x, i));
            lb_value.Content = $"value:{string.Join(",", vs)}";
        }
        private void cvIb_SizeChanged(object sender, EventArgs e)
        {
            lb_scale.Width = 100;//Content改變不知道為什麼不會改變外觀長度...
            lb_scale.Content = $"放大倍率:{scale.ToString("0.00")}";
        }

        private void cvIb_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //ImageViewer imageViewer = new ImageViewer(Image);
            //imageViewer.ShowDialog();
        }
    }
    public interface IMatProperty
    {
        Mat Image { set; get; }
    }
}
