using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI = System.Windows;

namespace VisionAnalysis
{
    public class TepEdgePoint : BaseToolEditParas
    {
        public TepEdgePoint(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["ROIr"] = ParaDictBuilder<RotatedRect>(100, 100, 300, 200, 0.0);
            Inputs["Threshold"] = new PInput() { value = 50 };

            Outputs["egPts"] = new POutput();
            Outputs["egPt"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            byte Threshold = Convert.ToByte(Inputs["Threshold"].value);
            RotatedRect roiR = toT<RotatedRect>((Dictionary<string, PInput>)Inputs["ROIr"].value);

            //process...

            Point2f pt1St, pt1End, pt2St, Pt2End;
            ((pt1St, pt1End), (pt2St, Pt2End)) = roiR.Vector();
            IEnumerable<Point> psSt = GetLinePoints(pt1St.ToPoint(), pt2St.ToPoint());
            IEnumerable<Point> psEnd = GetLinePoints(pt1End.ToPoint(), Pt2End.ToPoint());
            int min = Math.Min(psSt.Count(), psEnd.Count());
            List<IEnumerable<Point>> lines = new List<IEnumerable<Point>>();
            for (int i = 0; i < min; i++)
            {
                lines.Add(GetLinePoints(psSt.ElementAt(i), psEnd.ElementAt(i)));
            }

            var egPts = lines.Select(l => EdgeDetectByLine(source, l.Where(pt => pt.X >= 0 && pt.Y >= 0), Threshold)).Where(pt => pt.X != 0 && pt.Y != 0).ToList();
            Outputs["egPts"].value = egPts;
            relationsEdPt = getPtRelation(egPts);

            if (egPts.Count == 0)
            {
                return;
            }
            int mostFrequentIndex = relationsEdPt.GroupBy(n => n).OrderByDescending(g => g.Count()).First().Key;
            var mostFrequentPts = relationsEdPt.Select((r, i) => r == mostFrequentIndex ? egPts[i] : default).Where(pt => pt != default);
            Outputs["egPt"].value = new Point(mostFrequentPts.Average(pt => pt.X), mostFrequentPts.Average(pt => pt.Y));
        };
        private static double radius = 3;
        private int[] relationsEdPt;
        public override Action<IParaValue, UcAnalysis> paraSelect => (p, u) =>
        {
            base.paraSelect(p, u);

            if (p.value == null || u.ucImg.Image == null) return;
            double x = u.ucImg.cvs.ActualWidth - u.ucImg.Image.Width * u.ucImg.Scale;
            double y = u.ucImg.cvs.ActualHeight - u.ucImg.Image.Height * u.ucImg.Scale;
            Point2f ofs = new Point2f((float)x / 2, (float)y / 2);
            double radiusD = radius * u.ucImg.Scale;
            if (p == Outputs["egPts"])
            {
                IEnumerable<Point> param = p.value as IEnumerable<Point>;
                var pts = param.Select(pt => pt * u.ucImg.Scale + ofs).Select(ptf => ptf.ToPoint()).ToList();
                var Brushes = relationsEdPt.Select(r => Tools.RandomBrush).ToArray();
                Point2f egPt = (Point)Outputs["egPt"].value * u.ucImg.Scale + ofs;

                u.ucImg.cvs.Children.Add(VisualHost.draw(dc =>
                {
                    for (int i = 0; i < pts.Count(); i++)
                    {
                        Point pt = pts[i];
                        dc.DrawEllipse(null, new UI.Media.Pen(Brushes[relationsEdPt[i]], 1), new UI.Point(pt.X, pt.Y), radiusD, radiusD);
                    }
                    if(pts.Count > 0) dc.DrawEllipse(null, new UI.Media.Pen(UI.Media.Brushes.Green, 2), new UI.Point(pts.Average(pt => pt.X), pts.Average(pt => pt.Y)), radiusD, radiusD);
                    dc.DrawEllipse(null, new UI.Media.Pen(UI.Media.Brushes.Blue, 2), new UI.Point(egPt.X, egPt.Y), radiusD, radiusD);
                }));
                u.focusImg();
            }
            else if (p == Outputs["egPt"])
            {
                Point2f egPt = (Point)Outputs["egPt"].value * u.ucImg.Scale + ofs; u.ucImg.cvs.Children.Add(VisualHost.draw(dc =>
                {
                    dc.DrawEllipse(null, new UI.Media.Pen(UI.Media.Brushes.Blue, 2), new UI.Point(egPt.X, egPt.Y), radiusD, radiusD);
                }));
                u.focusImg();
            }
        };
        #endregion

        private static Point EdgeDetectByLine(Mat mat, IEnumerable<Point> line, byte threshold)
        {
            Point[] pxs = line.ToArray();
            for (int i = 0; i < pxs.Length - 1; i++)
            {
                Point px = pxs[i];
                Point pxNext = pxs[i + 1];
                var curV = Enumerable.Range(0, mat.Channels()).Select(j => mat.Type().IsInteger ? mat.At<Vec3b>(px.Y, px.X)[j] : mat.At<Vec3f>(px.Y, px.X)[j]).Average();
                var nextV = Enumerable.Range(0, mat.Channels()).Select(j => mat.Type().IsInteger ? mat.At<Vec3b>(pxNext.Y, pxNext.X)[j] : mat.At<Vec3f>(pxNext.Y, pxNext.X)[j]).Average();
                if (Math.Abs(curV - nextV) > threshold)
                {
                    return px;
                }
            }
            return default;
        }
        private static int[] getPtRelation(List<Point> pts)
        {
            int[] relations = Enumerable.Range(0, pts.Count()).ToArray();

            for (int i = 0; i < relations.Length; i++)
            {
                Point ptI = pts[i];

                for (int j = 0; j < relations.Length; j++)
                {
                    if (i == j) continue;
                    Point ptJ = pts[j];
                    if (ptI.DistanceTo(ptJ) <= radius * 2)
                    {
                        if (relations[j] == j)
                            relations[j] = relations[i];
                        else relations[i] = relations[j];
                    }
                }
            }
            return relations;
        }
        private static List<Point> GetLinePoints(Point start, Point end)
        {
            List<Point> points = new List<Point>();

            int dx = Math.Abs(end.X - start.X);
            int dy = Math.Abs(end.Y - start.Y);

            int sx = start.X < end.X ? 1 : -1;
            int sy = start.Y < end.Y ? 1 : -1;

            int err = dx - dy;

            int x = start.X, y = start.Y;

            while (true)
            {
                points.Add(new Point(x, y));
                if (x == end.X && y == end.Y) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }

            return points;
        }
    }
}
