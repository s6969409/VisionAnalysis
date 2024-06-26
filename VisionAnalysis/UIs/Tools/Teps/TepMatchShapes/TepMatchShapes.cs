using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepMatchShapes : BaseToolEditParas
    {
        public TepMatchShapes(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["contour"] = new PInput();
            Inputs["contours"] = new PInput();
            Inputs["method"] = new PInput() { value = ShapeMatchModes.I1 };
            Inputs["range"] = new PInput() { value = 0.1 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["values"] = new POutput();
            Outputs["rotatedTemplate"] = new POutput();
            Outputs["filtContours"] = new POutput();
            Outputs["rotateds"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras
            
            Mat source = Inputs["InputImage"].value as Mat;
            Point[] contour = Inputs["contour"].value as Point[];
            Point[][] contours = Inputs["contours"].value as Point[][];
            ShapeMatchModes method = TepHelper.getEnum<ShapeMatchModes>(Inputs["method"].value);
            double range = Convert.ToDouble(Inputs["range"].value);
            #endregion

            #region process...
            Point[][] filtContours = contours.Where(c =>
            {
                double as1 = axisScale(c);
                double as2 = axisScale(contour);

                return as1 >= as2 * (1 - range) && as1 <= as2 * (1 + range);
            }).Where(c => 
            {
                RotatedRect rRect = Cv2.MinAreaRect(c);
                return rRect.Size.Width >= 1 && rRect.Size.Height >= 1;
            }).ToArray();
            RotatedRect rotatedTemplate = Cv2.MinAreaRect(contour);

            var values = filtContours.Select((c, i) =>
            {
                RotatedRect rRect = Cv2.MinAreaRect(c);
                Point pt = (rRect.Center - rotatedTemplate.Center).ToPoint();
                double templateLongAxis = Math.Max(rotatedTemplate.Size.Width, rotatedTemplate.Size.Height);
                double templateShortAxis = Math.Min(rotatedTemplate.Size.Width, rotatedTemplate.Size.Height);
                double LongAxis = Math.Max(rRect.Size.Width, rRect.Size.Height);
                double ShortAxis = Math.Min(rRect.Size.Width, rRect.Size.Height);
                double scale = (LongAxis / templateLongAxis + ShortAxis / templateShortAxis) / 2;

                return new Contour()
                {
                    Index = i,
                    Similarity = Cv2.MatchShapes(contour, c, method),
                    Rect = TepFindContours.contoursRange(c),
                    Area = Cv2.ContourArea(c),
                    Pt = rRect.Center.ToPoint(),
                    Size = rRect.Size,
                    Angle = rRect.Angle,
                    Scale = scale,
                    SizeScale = LongAxis/ShortAxis
                };
            });
            Outputs["values"].value = values;

            int index = values.OrderBy(c => c.Similarity).First().Index;

            Mat result = source.Clone();
            drawAll(result, filtContours, values);
            Cv2.DrawContours(result, contours, index, Scalar.Green, 1);
            drawCtMsg(result, values.ElementAt(index), contours[index], Scalar.Green, 2);

            Outputs["Output1"].value = result;

            Outputs["rotatedTemplate"].value = rotatedTemplate;
            Outputs["filtContours"].value = filtContours;
            Outputs["rotateds"].value = filtContours.Select(c => Cv2.MinAreaRect(c));
            #endregion
        };
        #endregion
        private class Contour
        {
            public int Index;
            public double Similarity;
            public Rect Rect;
            public double Area;
            public Point Pt;
            public Size2f Size;
            public double Angle;
            public double Scale;
            public double SizeScale;
        }
        private double axisScale(Point[] contour)
        {
            RotatedRect rRect = Cv2.MinAreaRect(contour);
            double LongAxis = Math.Max(rRect.Size.Width, rRect.Size.Height);
            double ShortAxis = Math.Min(rRect.Size.Width, rRect.Size.Height);
            return LongAxis / ShortAxis;
        }
        private void drawAll(Mat mat, Point[][] contours, IEnumerable<Contour> ctMsg)
        {
            Array.ForEach(ctMsg.ToArray(), c => drawCtMsg(mat, c, contours[c.Index], Scalar.RandomColor()));
        }
        private void drawCtMsg(Mat mat, Contour ct, Point[] contour, Scalar scalar = default, int thickness = 1)
        {
            Cv2.DrawContours(mat, new Point[][] { contour}, -1, scalar, thickness);

            Point pt1 = new Point(ct.Pt.X - 10 * (float)Math.Cos(ct.Angle * Math.PI / 180), ct.Pt.Y - 10 * (float)Math.Sin(ct.Angle * Math.PI / 180));
            Point pt2 = new Point(ct.Pt.X + 10 * (float)Math.Cos(ct.Angle * Math.PI / 180), ct.Pt.Y + 10 * (float)Math.Sin(ct.Angle * Math.PI / 180));
            Point pt3 = new Point(ct.Pt.X - 10 * (float)Math.Cos((ct.Angle + 90) * Math.PI / 180), ct.Pt.Y - 10 * (float)Math.Sin((ct.Angle + 90) * Math.PI / 180));
            Point pt4 = new Point(ct.Pt.X + 10 * (float)Math.Cos((ct.Angle + 90) * Math.PI / 180), ct.Pt.Y + 10 * (float)Math.Sin((ct.Angle + 90) * Math.PI / 180));


            Cv2.Line(mat, pt1, pt2, scalar, thickness);
            Cv2.Line(mat, pt3, pt4, scalar, thickness);
            Cv2.PutText(mat, ct.Similarity.ToString(), ct.Pt, HersheyFonts.HersheyScriptSimplex, 1, scalar, thickness);
            Point anglePt = new Point(ct.Pt.X, ct.Pt.Y+30);
            Cv2.PutText(mat, $"Angle:{ct.Angle.ToString("0.000")}", anglePt, HersheyFonts.HersheyScriptSimplex, 1, scalar, thickness);
            Point sPt = new Point(ct.Pt.X, ct.Pt.Y + 60);
            Cv2.PutText(mat, $"Scale:{ct.Scale.ToString("0.000")}", sPt, HersheyFonts.HersheyScriptSimplex, 1, scalar, thickness);
            Point ssPt = new Point(ct.Pt.X, ct.Pt.Y + 90);
            Cv2.PutText(mat, $"SizeScale:{ct.SizeScale.ToString("0.000")}", ssPt, HersheyFonts.HersheyScriptSimplex, 1, scalar, thickness);
            Cv2.Ellipse(mat, new RotatedRect(ct.Pt, ct.Size, (float)ct.Angle), scalar, thickness);
        }
    }
}
