using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Outputs["ContoursDetail"] = new POutput() { value = new List<object>() };
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
                    Index = Index,
                    Rect = contoursRange(c),
                    GravityPt = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00)),
                    Area = c.Length,
                    HierarchyNext = hierarchies[Index].Next,
                    HierarchyPrevious = hierarchies[Index].Previous,
                    HierarchyChild = hierarchies[Index].Child,
                    HierarchyParent = hierarchies[Index].Parent
                };
            });

            Mat drawMat = new Mat(source.Rows, source.Cols, MatType.CV_8UC3);
            Cv2.DrawContours(drawMat, contours, -1, new Scalar(255, 255, 0), 1, LineTypes.Link4);
            updateUIImage((Mat)Outputs["Output1"].value);
            Outputs["Output1"].value = drawMat;
        };
        #endregion

        private Rect contoursRange(Point[] pts)
        {
            int minX = pts[0].X, minY = pts[0].Y, maxX = pts[0].X, maxY = pts[0].Y;

            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].X < minX) minX = pts[i].X;
                if (pts[i].Y < minY) minY = pts[i].Y;
                if (pts[i].X > maxX) maxX = pts[i].X;
                if (pts[i].Y > maxY) maxY = pts[i].Y;
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        private struct Contour
        {
            public int Index;
            public Rect Rect;
            public Point GravityPt;
            public int Area;

            public int HierarchyNext;
            public int HierarchyPrevious;
            public int HierarchyChild;
            public int HierarchyParent;
        }
    }
}
