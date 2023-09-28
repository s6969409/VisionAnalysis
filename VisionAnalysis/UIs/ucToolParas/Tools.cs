using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace VisionAnalysis
{
    public class Tools
    {
        // 將 Emgu.CV 的 Mat 轉換為 WPF 的 BitmapSource
        public static BitmapSource ToBitmapSource(Mat mat)
        {
            if (mat != null)
            {
                using (System.Drawing.Bitmap bitmap = mat.ToBitmap())
                {
                    IntPtr hBitmap = bitmap.GetHbitmap();
                    try
                    {
                        return Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                    finally
                    {
                        DeleteObject(hBitmap); // 釋放 GDI 資源
                    }
                }
            }
            return null;
        }

        // P/Invoke 函數，用於釋放 GDI 資源
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
