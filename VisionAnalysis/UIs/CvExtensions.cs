using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public static class CvExtensions
    {
        public static Point2f[] Point2fs(this Rect rect) => new Point2f[] {
            rect.TopLeft,
            rect.TopLeft + new Point(rect.Width, 0),
            rect.BottomRight,
            rect.TopLeft + new Point(0, rect.Height)
        };
    }
}
