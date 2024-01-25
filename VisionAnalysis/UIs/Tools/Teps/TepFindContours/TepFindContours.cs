using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            Inputs["mode"] = new PInput() { value = RetrType.Tree };
            Inputs["method"] = new PInput() { value = ChainApproxMethod.ChainApproxSimple };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Contours"] = new POutput() { value = new List<object>() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                source, 
                contours, 
                null,
                TepHelper.getEnum<RetrType>(Inputs["mode"].value),
                TepHelper.getEnum<ChainApproxMethod>(Inputs["method"].value));

            Mat drawMat = new Mat(source.Rows, source.Cols, DepthType.Cv8U, 3);
            List<VectorOfPoint> cs = new List<VectorOfPoint>();
            for (int i = 0; i < contours.Size; i++) cs.Add(contours[i]);

            (Outputs["Contours"].value as List<object>).Clear();
            (Outputs["Contours"].value as List<object>).AddRange(cs.Select((c, Index) => new {
                Index,
                Size = contoursRange(c),
                GravityPt = new Point((int)CvInvoke.Moments(c).GravityCenter.X, (int)CvInvoke.Moments(c).GravityCenter.Y),
                Area = c.Size
            }));

            CvInvoke.DrawContours(drawMat, contours, -1, new Emgu.CV.Structure.MCvScalar(255, 255, 0), 1, LineType.FourConnected);
            updateUIImage((Mat)Outputs["Output1"].value);
            Outputs["Output1"].value = drawMat;
        };
        #endregion

        private Rectangle contoursRange(VectorOfPoint pts)
        {
            int minX = pts[0].X, minY = pts[0].Y, maxX = pts[0].X, maxY = pts[0].Y;

            for (int i = 1; i < pts.Size; i++)
            {
                if (pts[i].X < minX) minX = pts[i].X;
                if (pts[i].Y < minY) minY = pts[i].Y;
                if (pts[i].X > maxX) maxX = pts[i].X;
                if (pts[i].Y > maxY) maxY = pts[i].Y;
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
