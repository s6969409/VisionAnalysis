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
                changeImgZoomType(true);

                mat = value; 
                img.Source = value == null || value.Total() == 0 ? null : value.ToBitmapSource();
                if (value == null) return;
                lb_size.Content = $"{value.Width}*{value.Height}*{value.Channels()}";
                lb_format.Content = $"{value.Depth()},{value.Type()}";

                lb_position.Content = $"pos:-";
                lb_value.Content = $"value:-";

                btn_save.IsEnabled = value != null && value.Width != 0 && value.Height != 0;
            }
        }

        public double Scale
        {
            get
            {
                if (Image != null)
                {
                    double heightStd = (double)img.ActualWidth * Image.Height / Image.Width;//Y/X
                    if (img.ActualHeight > heightStd) return (double)img.ActualWidth / Image.Width;
                    else return (double)img.ActualHeight / Image.Height;
                }
                else
                {
                    return 1;
                }
            }
        }

        public Action actionScaleChanged;

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            var pt = e.GetPosition((System.Windows.Controls.Image)sender);

            if (Image == null) return;
            int x = (int)(pt.X / Scale);
            int y = (int)(pt.Y / Scale);

            lb_position.Content = $"pos:{x},{y}";
            
            int channels = Image.Channels();

            var vs = Enumerable.Range(0, channels)
                .Select(i => Image.Type().IsInteger ? Image.At<Vec3b>(y, x)[i].ToString() : Image.At<Vec3f>(y, x)[i].ToString());
            lb_value.Content = $"value:{string.Join(",", vs)}";
        }


        private void img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            btn_scale.Content = $"Scale:{Scale.ToString("0.00")}";
            actionScaleChanged?.Invoke();
        }

        private void btn_scale_Click(object sender, RoutedEventArgs e)
        {
            bool isRealSize = sv_img.VerticalScrollBarVisibility == ScrollBarVisibility.Disabled;
            changeImgZoomType(!isRealSize);
            actionScaleChanged?.Invoke();
        }

        private void changeImgZoomType(bool useRealSize)
        {
            sv_img.VerticalScrollBarVisibility = useRealSize ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            sv_img.HorizontalScrollBarVisibility = useRealSize ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            sv_img.UpdateLayout();

            img.Height = double.NaN;
            img.Width = double.NaN;
        }

        private void img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sv_img.VerticalScrollBarVisibility == ScrollBarVisibility.Disabled) return;
            double zoomScale = e.Delta > 0 ? 1.5 : 0.5;
            img.Height = img.ActualHeight * zoomScale;
            img.Width = img.ActualWidth * zoomScale;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string savePath = PathSelector.getUserSelectPath(PathSelector.PathRequest.SaveFile);
            if (savePath == null) return;
            Image.ImWrite(savePath);
        }
    }
    public interface IMatProperty
    {
        Mat Image { set; get; }
    }
}
