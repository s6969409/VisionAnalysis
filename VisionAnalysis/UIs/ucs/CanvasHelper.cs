using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI = System.Windows;
using System.Windows.Media;
using OpenCvSharp;

namespace VisionAnalysis
{
    internal class CanvasHelper
    {
    }
    #region Image canvas support draw
    public partial class VisualHost : UI.UIElement
    {
        private Visual _visual;

        public VisualHost(Visual visual)
        {
            _visual = visual;
            AddVisualChild(_visual);
        }

        protected override Visual GetVisualChild(int index) => _visual;
        protected override int VisualChildrenCount => 1;

        public static VisualHost draw(Action<DrawingContext> actionDraw)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                actionDraw(dc);
            }

            return new VisualHost(drawingVisual);
        }

        public static VisualHost drawRect(Point2f[] pfs, Brush brush = null) => draw(dc =>
        {
            UI.Point[] pts = pfs.Select(pf => pf.toUIPoint()).ToArray();
            dc.DrawLine(new Pen(brush ?? Brushes.Red, 1), pts[0], pts[1]);
            dc.DrawLine(new Pen(brush ?? Brushes.Red, 1), pts[1], pts[2]);
            dc.DrawLine(new Pen(brush ?? Brushes.Red, 1), pts[2], pts[3]);
            dc.DrawLine(new Pen(brush ?? Brushes.Red, 1), pts[3], pts[0]);
        });
        private static int ptTolerance = 5;
        public static VisualHost drawCross(Point2f pf, Brush brush = null)
        {
            UI.Point ptx1 = new UI.Point(pf.X - ptTolerance, pf.Y);
            UI.Point ptx2 = new UI.Point(pf.X + ptTolerance, pf.Y);
            UI.Point pty1 = new UI.Point(pf.X, pf.Y - ptTolerance);
            UI.Point pty2 = new UI.Point(pf.X, pf.Y + ptTolerance);

            return draw(dc =>
            {
                dc.DrawLine(new Pen(brush ?? Brushes.Red, 1), ptx1, ptx2);
                dc.DrawLine(new Pen(brush ?? Brushes.Red, 1), pty1, pty2);
            });
        }
        public static VisualHost drawPts(Point2f[] pfs, double scale, Brush brush = null) => draw(dc =>
        {
            UI.Point[] pts = pfs.Select(pf => pf.toUIPoint()).ToArray();
            foreach (var pt in pts)
            {
                dc.DrawEllipse(brush ?? Brushes.Red, new Pen(brush ?? Brushes.Red, 1), pt, scale, scale);
            }
        });
        public static VisualHost drawGeometry(Point2f[] pfs, double scale, bool closed = false, Brush brush = null) => draw(dc =>
        {
            UI.Point[] pts = pfs.Select(pf => pf.toUIPoint()).ToArray();
            double sc = scale < 1 ? 1 : scale / 2;
            UI.Vector ofs = new UI.Vector(sc, sc);
            for (int i = 0; i < pts.Length - 1; i++)
            {
                dc.DrawLine(new Pen(brush ?? Brushes.Red, 1), pts[i] + ofs, pts[i + 1] + ofs);
            }
            if (closed) dc.DrawLine(new Pen(brush ?? Brushes.Red, 1), pts[pts.Length - 1] + ofs, pts[0] + ofs);
        });
        public static VisualHost drawText(string text, UI.Point p = default, Brush brush = null)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                var dpiScale = VisualTreeHelper.GetDpi(drawingVisual);
                double pixelsPerDip = dpiScale.PixelsPerDip;
                dc.DrawText(new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, UI.FlowDirection.LeftToRight, new Typeface(""), 30, brush ?? Brushes.Red, 10), p);
            }

            return new VisualHost(drawingVisual);
        }
    }

    #endregion
}
