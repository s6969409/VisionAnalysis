using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VisionAnalysis
{
    internal class CanvasHelper
    {
    }
    #region Image canvas support draw
    public class VisualHost : UIElement
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
    }

    #endregion
}
