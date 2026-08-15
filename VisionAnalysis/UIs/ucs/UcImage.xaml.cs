using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI = System.Windows;

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
                try { img.Source = value.ToBitmapSource(); }
                catch { img.Source = null; }
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

        private void img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            btn_scale.Content = $"Scale:{Scale:F2}";
            actionScaleChanged?.Invoke();
        }

        private void btn_scale_Click(object sender, RoutedEventArgs e)
        {
            bool isRealSize = sv_img.VerticalScrollBarVisibility == ScrollBarVisibility.Disabled;
            changeImgZoomType(!isRealSize);
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

            var pt = e.GetPosition((Image)sender);
            double mx = pt.X - sv_img.HorizontalOffset;
            double my = pt.Y - sv_img.VerticalOffset;
            double sbx = pt.X * zoomScale - mx;
            double sby = pt.Y * zoomScale - my;
            if (sbx > 0 && sbx < sv_img.ScrollableWidth * zoomScale)
            {
                sv_img.ScrollToHorizontalOffset(sbx);
            }
            if (sby > 0 && sby < sv_img.ScrollableHeight * zoomScale)
            {
                sv_img.ScrollToVerticalOffset(sby);
            }
            e.Handled = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string savePath = PathSelector.getUserSelectPath(PathSelector.PathRequest.SaveFile);
            if (savePath == null) return;
            Image.ImWrite(savePath);
        }

        public Action<MouseButtonEventArgs> MouseDown;
        public Action<MouseEventArgs, UI.Point> MouseMove;
        public Action<MouseButtonEventArgs> MouseUp;
        public Action<MouseEventArgs> MouseLeave;

        private UI.Point clickPt;
        private void cvs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickPt = e.GetPosition(img);
            MouseDown?.Invoke(e);
        }
        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            var pt = e.GetPosition((Image)sender);
            MouseMove?.Invoke(e, new UI.Point(sv_img.HorizontalOffset, sv_img.VerticalOffset));

            if (Image == null) return;
            int x = (int)(pt.X / Scale);
            int y = (int)(pt.Y / Scale);

            lb_position.Content = $"pos:{x},{y}";

            int channels = Image.Channels();

            var vs = Enumerable.Range(0, channels)
                .Select(i => Image.Type().IsInteger ? Image.At<Vec3b>(y, x)[i].ToString() : Image.At<Vec3f>(y, x)[i].ToString());
            lb_value.Content = $"value:{string.Join(",", vs)}";
            
            #region mouse move Image
            if (e.RightButton == MouseButtonState.Pressed && pt.X >= 0 && pt.X < img.ActualWidth && pt.Y >= 0 && pt.Y < img.ActualHeight)
            {
                var ofs = pt - clickPt;
                double sbx = sv_img.HorizontalOffset - ofs.X;
                double sby = sv_img.VerticalOffset - ofs.Y;
                if (sbx > 0 && sbx < sv_img.ScrollableWidth)
                {
                    sv_img.ScrollToHorizontalOffset(sbx);
                }
                if (sby > 0 && sby < sv_img.ScrollableHeight)
                {
                    sv_img.ScrollToVerticalOffset(sby);
                }
            }
            #endregion
        }
        private void cvs_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(e);
        }
        private void cvs_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(e);
        }
        public void MouseEventClear()
        {
            MouseDown = null;
            MouseMove = null;
            MouseUp = null;
            MouseLeave = null;
        }
    }
    public interface IMatProperty
    {
        Mat Image { set; get; }
    }
}
