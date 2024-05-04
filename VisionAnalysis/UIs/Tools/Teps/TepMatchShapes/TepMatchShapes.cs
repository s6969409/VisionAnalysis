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
            Point[][] filtContours = contours.Where(c => c.Length > contour.Length / 4).ToArray();

            var values = filtContours.Select((c, i) => new Contour()
            {
                Index = i,
                Similarity = Cv2.MatchShapes(contour, c, method),
                Rect = TepFindContours.contoursRange(c),
                //GravityPt = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00)),
                Area = c.Length,
                Angle = Cv2.FitEllipse(c).Angle
            });
            Outputs["values"].value = values;


            int index = 0;
            double minValue = double.MaxValue;
            for (int i = 0; i < filtContours.Count(); i++)
            {
                var vv = Cv2.MatchShapes(contour, filtContours.ElementAt(i), method);
                if (vv < minValue)
                {
                    minValue = vv;
                    index = i;
                }
            }
            Mat result = source.Clone();
            Cv2.DrawContours(result, filtContours, index, new Scalar(255, 0, 0), 1);
            Outputs["Output1"].value = result;

            RotatedRect rotatedRect1 = Cv2.FitEllipse(contour);
            RotatedRect rotatedRect2 = Cv2.FitEllipse(filtContours.ElementAt(index));

            var cs2 = filtContours.ElementAt(index);

            Cv2.Ellipse(result, rotatedRect1, new Scalar(0, 0, 255));
            Cv2.Ellipse(result, rotatedRect2, new Scalar(0, 255, 0));
            Cv2.Line(result, 
                new Point() { X= (int)rotatedRect2.Center.X-10,Y= (int)rotatedRect2.Center.Y },
                new Point() { X = (int)rotatedRect2.Center.X+10, Y = (int)rotatedRect2.Center.Y }
                , new Scalar(0, 255, 0)
            );
            Cv2.Line(result,
                new Point() { X = (int)rotatedRect2.Center.X, Y = (int)rotatedRect2.Center.Y-10 },
                new Point() { X = (int)rotatedRect2.Center.X, Y = (int)rotatedRect2.Center.Y+10 }
                , new Scalar(0, 255, 0)
            );

            Outputs["rotatedRect1"].value = rotatedRect1;
            Outputs["rotatedRect2"].value = rotatedRect2;
        };
        #endregion
        private class Contour
        {
            public int Index;
            public double Similarity;
            public Rect Rect;
            public int Area;
            public double Angle;

        }
    }
}
