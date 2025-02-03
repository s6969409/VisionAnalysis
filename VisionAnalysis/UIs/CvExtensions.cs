using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI = System.Windows;

namespace VisionAnalysis
{
    public static class CvExtensions
    {
        #region Cv object
        public static Point2f[] Point2fs(this Rect rect) => new Point2f[] {
            rect.TopLeft,
            rect.TopLeft + new Point(rect.Width, 0),
            rect.BottomRight,
            rect.TopLeft + new Point(0, rect.Height)
        };
        public static ((Point2f, Point2f), (Point2f, Point2f)) Vector(this RotatedRect rotatedRect)
        {
            var pts = rotatedRect.Points();

            for (int i = 0; i < pts.Count(); i++)
            {
                int index = i + 1 < pts.Length ? i + 1 : 0;
                var pp = pts[i] - pts[index];
                var factor = pp.X * pp.Y < 0 ? -1 : 1;
                double angle = Math.Acos(pp.X / pp.DistanceTo(default) * factor) / Math.PI * 180;
                angle = Math.Round(angle, 3);
                var diff = angle - rotatedRect.Angle;
                if(diff<0.001)
                {
                    int n2st = i - 1 < 0 ? pts.Length - 1 : i - 1;
                    int n2end = index + 1 >= pts.Length ? 0 : index + 1;

                    return ((pts[i], pts[index]), (pts[n2st], pts[n2end]));
                }
            }

            return default;
        }
        public static UI.Point toUIPoint(this Point2f pf)
        {
            return new UI.Point(pf.X, pf.Y);
        }
        #endregion

        #region UI object
        public static Point2f Point2f(this UI.Point uiP) => new Point2f { X = (float)uiP.X, Y = (float)uiP.Y };
        #endregion
    }
}
