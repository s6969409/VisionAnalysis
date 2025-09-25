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

        public static VisualHost drawRect(Point2f[] pfs) => draw(dc =>
        {
            UI.Point[] pts = pfs.Select(pf => pf.toUIPoint()).ToArray();
            dc.DrawLine(new Pen(Brushes.Red, 1), pts[0], pts[1]);
            dc.DrawLine(new Pen(Brushes.Red, 1), pts[1], pts[2]);
            dc.DrawLine(new Pen(Brushes.Red, 1), pts[2], pts[3]);
            dc.DrawLine(new Pen(Brushes.Red, 1), pts[3], pts[0]);
        });
        private static int ptTolerance = 5;
        public static VisualHost drawCross(Point2f pf)
        {
            UI.Point ptx1 = new UI.Point(pf.X - ptTolerance, pf.Y);
            UI.Point ptx2 = new UI.Point(pf.X + ptTolerance, pf.Y);
            UI.Point pty1 = new UI.Point(pf.X, pf.Y - ptTolerance);
            UI.Point pty2 = new UI.Point(pf.X, pf.Y + ptTolerance);

            return draw(dc =>
            {
                dc.DrawLine(new Pen(Brushes.Red, 1), ptx1, ptx2);
                dc.DrawLine(new Pen(Brushes.Red, 1), pty1, pty2);
            });
        }

        public static VisualHost drawText(string text, UI.Point p = default) => draw(dc =>
        {
            dc.DrawText(new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, UI.FlowDirection.LeftToRight, new Typeface(""), 30, Brushes.Blue), p);
        });
    }

    #endregion
}
