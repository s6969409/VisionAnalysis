using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepHoughLinesP : BaseToolEditParas
    {
        public TepHoughLinesP(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["rho"] = new PInput() { value = 1.0 };
            Inputs["theta"] = new PInput() { value = 0.0175 };
            Inputs["threshold"] = new PInput() { value = 100 };
            Inputs["minLineLength"] = new PInput() { value = 0.0 };
            Inputs["maxLineGap"] = new PInput() { value = 0.0 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Lines"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            double rho = Convert.ToDouble(Inputs["rho"].value);
            double theta = Convert.ToDouble(Inputs["theta"].value);
            int threshold = Convert.ToInt32(Inputs["threshold"].value);
            double minLineLength = Convert.ToDouble(Inputs["minLineLength"].value);
            double maxLineGap = Convert.ToDouble(Inputs["maxLineGap"].value);
            #endregion

            //process...
            LineSegmentPoint[] lines = Cv2.HoughLinesP(source, rho, theta, threshold, minLineLength, maxLineGap);
            Mat result = new Mat(source.Size(), MatType.CV_8UC3, Scalar.Black);
            foreach (LineSegmentPoint line in lines)
            {
                result.Line(line.P1, line.P2, Scalar.RandomColor());
            }
            Outputs["Output1"].value = result;
            updateUIImage(result);
            Outputs["Lines"].value = lines;
        };
        #endregion
    }

    public class TepHoughCircles : BaseToolEditParas
    {
        public TepHoughCircles(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["method"] = new PInput() { value = HoughModes.Gradient };
            Inputs["dp"] = new PInput() { value = 1.0 };
            Inputs["minDist"] = new PInput() { value = 1.0 };
            Inputs["param1"] = new PInput() { value = 100.0 };
            Inputs["param2"] = new PInput() { value = 100.0 };
            Inputs["minRadius"] = new PInput() { value = 0 };
            Inputs["maxRadius"] = new PInput() { value = 0 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Lines"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            HoughModes method = TepHelper.getEnum<HoughModes>(Inputs["method"].value);
            double dp = Convert.ToDouble(Inputs["dp"].value);
            double minDist = Convert.ToDouble(Inputs["minDist"].value);
            double param1 = Convert.ToDouble(Inputs["param1"].value);
            double param2 = Convert.ToDouble(Inputs["param2"].value);
            int minRadius = Convert.ToInt32(Inputs["minRadius"].value);
            int maxRadius = Convert.ToInt32(Inputs["maxRadius"].value);
            #endregion

            //process...
            CircleSegment[] circles = Cv2.HoughCircles(source, method, dp, minDist, param1, param2, minRadius, maxRadius);
            Mat result = new Mat(source.Size(), MatType.CV_8UC3, Scalar.Black);
            foreach (CircleSegment circle in circles)
            {
                result.Circle(circle.Center.ToPoint(), (int)circle.Radius, Scalar.RandomColor());
            }
            Outputs["Output1"].value = result;
            updateUIImage(result);
            Outputs["Lines"].value = circles;
        };
        #endregion
    }
}
