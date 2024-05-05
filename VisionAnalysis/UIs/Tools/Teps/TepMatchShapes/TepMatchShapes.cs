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

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["values"] = new POutput();
            Outputs["rotatedRect1"] = new POutput();
            Outputs["rotatedRect2"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            Point[] contour = Inputs["contour"].value as Point[];
            Point[][] contours = Inputs["contours"].value as Point[][];
            ShapeMatchModes method = TepHelper.getEnum<ShapeMatchModes>(Inputs["method"].value);
            //process...
            Point[][] filtContours = contours.Where(c => Cv2.ContourArea(c) > Cv2.ContourArea(contour) / 4).ToArray();

            RotatedRect rotatedRect1 = Cv2.FitEllipse(contour);

            var values = filtContours.Select((c, i) =>
            {
                RotatedRect rRect = Cv2.FitEllipse(c);
                Point pt = (rRect.Center - rotatedRect1.Center).ToPoint();
                double templateLongAxis = Math.Max(rotatedRect1.Size.Width, rotatedRect1.Size.Height);
                double templateShortAxis = Math.Min(rotatedRect1.Size.Width, rotatedRect1.Size.Height);
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

            RotatedRect rotatedRect2 = Cv2.FitEllipse(filtContours.ElementAt(index));

            Outputs["rotatedRect1"].value = rotatedRect1;
            Outputs["rotatedRect2"].value = rotatedRect2;
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
