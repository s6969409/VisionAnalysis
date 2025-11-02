using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using UI = System.Windows;

namespace VisionAnalysis
{
    public class TepFindContours : BaseToolEditParas
    {
        public TepFindContours(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["mode"] = new PInput() { value = RetrievalModes.Tree };
            Inputs["method"] = new PInput() { value = ContourApproximationModes.ApproxSimple };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["ContoursDetail"] = new POutput();
            Outputs["Contours"] = new POutput();
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;

            Cv2.FindContours(
                source, 
                out Point[][] contours, 
                out HierarchyIndex[] hierarchies,
                TepHelper.getEnum<RetrievalModes>(Inputs["mode"].value),
                TepHelper.getEnum<ContourApproximationModes>(Inputs["method"].value));
            Outputs["Contours"].value = contours;
            Outputs["ContoursDetail"].value = contours.Select((c, Index) =>
            {
                Moments moments = Cv2.Moments(c);
                return new Contour()
                {
                    Pts = c,
                    Rect = contoursRange(c),
                    GravityPt = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00)),
                    Area = Cv2.ContourArea(c),
                    HierarchyNext = hierarchies[Index].Next,
                    HierarchyPrevious = hierarchies[Index].Previous,
                    HierarchyChild = hierarchies[Index].Child,
                    HierarchyParent = hierarchies[Index].Parent
                };
            });

            Mat drawMat = new Mat(source.Rows, source.Cols, MatType.CV_8UC3, Scalar.Black);
            Cv2.BitwiseAnd(drawMat, Scalar.All(0), drawMat);
            Cv2.DrawContours(drawMat, contours, -1, new Scalar(255, 255, 0), 1, LineTypes.Link4);
            updateUIImage((Mat)Outputs["Output1"].value);
            Outputs["Output1"].value = drawMat;
        };

        public override Action<IParaValue, UcAnalysis> paraSelect => (p, u) =>
        {
            if (p == Outputs["ContoursDetail"] && p.value != null && u.ucImg.Image != null)
            {
                IEnumerable<Contour> cts = Outputs["ContoursDetail"].value as IEnumerable<Contour>;

                Brush[] bs = cts.Select(c => Tools.RandomBrush).ToArray();

                u.ucImg.MouseDown = e =>
                {

                };
                u.ucImg.MouseMove = (e, pt) =>
                {
                    //pt: Fov TopLeft position
                    u.ucImg.cvs.Children.Clear();
                    var ptM = e.GetPosition(u.ucImg.img).Point2f() * (1 / u.ucImg.Scale);
                    IEnumerable<Contour> cIn = cts.Where(ct => ((Point)ptM).IsInContour(ct.Pts));

                    for (int i = 0; i < cIn.Count(); i++)
                    {
                        Brush brush = bs[i % bs.Length];
                        Contour ctr = cIn.ElementAt(i);
                        Point2f ofs = GetOfs(u);
                        
                        double tx = (ctr.Rect.Right > u.ucImg.Image.Width - 500 ? ctr.Rect.Left : ctr.Rect.Right) * u.ucImg.Scale + ofs.X;
                        double ty = (ctr.Rect.Bottom > u.ucImg.Image.Height - 50 ? ctr.Rect.Top : ctr.Rect.Bottom) * u.ucImg.Scale + ofs.Y;
                        u.ucImg.cvs.Children.Add(VisualHost.drawText($"{ctr.Area}", new UI.Point(tx, ty), brush));
                        u.ucImg.cvs.Children.Add(VisualHost.drawGeometry(ctr.Pts.Select(pf => pf * u.ucImg.Scale + ofs).ToArray(), u.ucImg.Scale, true, brush));
                    }
                };

            }
            else base.paraSelect(p, u);
        };
        #endregion

        public static Rect contoursRange(Point[] pts)
        {
            int minX = pts[0].X, minY = pts[0].Y, maxX = pts[0].X, maxY = pts[0].Y;

            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].X < minX) minX = pts[i].X;
                if (pts[i].Y < minY) minY = pts[i].Y;
                if (pts[i].X > maxX) maxX = pts[i].X;
                if (pts[i].Y > maxY) maxY = pts[i].Y;
            }

            return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
        private struct Contour
        {
            public Point[] Pts;

            public Rect Rect;
            public Point GravityPt;
            public double Area;

            public int HierarchyNext;
            public int HierarchyPrevious;
            public int HierarchyChild;
            public int HierarchyParent;
        }
    }
}
