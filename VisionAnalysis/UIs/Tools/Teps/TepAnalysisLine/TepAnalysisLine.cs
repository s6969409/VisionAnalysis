using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepAnalysisLine : BaseToolEditParas
    {
        public TepAnalysisLine(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["p1"] = ParaDictBuilder<Point>(0, 0);
            Inputs["p2"] = ParaDictBuilder<Point>(0, 0);
            Inputs["channel"] = new PInput() { value = 0 };

            Outputs["Output1"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            Point p1 = toT<Point>((Dictionary<string, PInput>)Inputs["p1"].value);
            Point p2 = toT<Point>((Dictionary<string, PInput>)Inputs["p2"].value);
            int channel = (int)Inputs["channel"].value;

            //process...
            var aa = EdgeDetectByLineX(source.Clone(), p1, p2);
            Outputs["Output1"].value = aa.ElementAt(channel);
        };
        #endregion

        private static IEnumerable<object> EdgeDetectByLineX(Mat mat, Point start, Point end)
        {
            LineIterator lineIterator = new LineIterator(mat, start, end);
            LineIterator.Pixel[] pxs = lineIterator.ToArray();

            return Enumerable.Range(0, mat.Channels()).Select(i =>
            {
                return pxs.Select(px =>
                {
                    return mat.Type().IsInteger ? mat.At<Vec3b>(px.Pos.Y, px.Pos.X)[i] : mat.At<Vec3f>(px.Pos.Y, px.Pos.X)[i];
                });
            });
        }
    }
}
