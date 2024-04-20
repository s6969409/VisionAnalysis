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
            int index = 0;
            double minValue = 2;
            for (int i = 0; i < contours.Length; i++)
            {
                var vv = Cv2.MatchShapes(contour, contours[i], method);
                if (vv < minValue)
                {
                    minValue = vv;
                    index = i;
                }
            }
            Mat result = source.Clone();
            Cv2.DrawContours(result, contours, index, new Scalar(255, 0, 0), 1);
            Outputs["Output1"].value = result;

            RotatedRect rotatedRect1 = Cv2.FitEllipse(contour);
            RotatedRect rotatedRect2 = Cv2.FitEllipse(contours[index]);

            var cs2 = contours[index];

            Cv2.Ellipse(result, rotatedRect1, new Scalar(0, 0, 255));
            Cv2.Ellipse(result, rotatedRect2, new Scalar(0, 255, 0));

            Outputs["rotatedRect1"].value = rotatedRect1;
            Outputs["rotatedRect2"].value = rotatedRect2;
        };
        #endregion
    }
}
