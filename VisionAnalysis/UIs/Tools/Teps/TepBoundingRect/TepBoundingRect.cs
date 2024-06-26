using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepBoundingRect : BaseToolEditParas
    {
        public TepBoundingRect(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["contours"] = new PInput();

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["rects"] = new POutput();
            Outputs["rImgs"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            Point[][] contours = Inputs["contours"].value as Point[][];
            #endregion

            //process...
            Mat result = source.Clone();
            Rect[] rects = contours.Select(contour => Cv2.BoundingRect(contour)).ToArray();
            Array.ForEach(rects, rect => result.Rectangle(rect, Scalar.RandomColor()));
            Outputs["Output1"].value = result;
            Outputs["rects"].value = rects;
            Outputs["rImgs"].value = rects.Select(rect => new Mat(source, rect));
        };
        #endregion
    }
}
