using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepMatchTemplate : BaseToolEditParas
    {
        public TepMatchTemplate(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["TargetImage"] = new PInput() { value = new Mat() };
            Inputs["TemplateImage"] = new PInput() { value = new Mat() };
            Inputs["method"] = new PInput() { value = TemplateMatchingType.CcoeffNormed };
            Inputs["FindVal"] = new PInput() { value = 0 };

            Outputs["OutputResult"] = new POutput() { value = null };
            Outputs["OutputArr"] = new POutput() { value = null };
            Outputs["OutputMatch"] = new POutput() { value = null };
            Outputs["OutputReScale"] = new POutput() { value = null };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            #region process... & output
            Mat targetImage = Inputs["TargetImage"].value as Mat;
            Mat templateImage = Inputs["TemplateImage"].value as Mat;
            TemplateMatchingType method = TepHelper.getEnum<TemplateMatchingType>(Inputs["method"].value);

            Mat result = new Mat(targetImage.Size, DepthType.Cv32F, 1);

            CvInvoke.MatchTemplate(targetImage, templateImage, result, method);
            Outputs["OutputResult"].value = result;
            Outputs["OutputArr"].value = toDataSets(result);
            #endregion

            #region find max & min value
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
            #endregion

            #region output match locatiom information
            float findVal = (float)Inputs["FindVal"].value;
            var resultDatas = result.GetData().Cast<float>().Select((val, index) => new { val, x = index % result.Cols, y = index / result.Cols });
            IEnumerable<Point> findPts = resultDatas.Where(v => v.val == findVal).Select(v => new Point(v.x, v.y));

            Mat OutputMatch = targetImage.Clone();
            CvInvoke.Rectangle(OutputMatch, new Rectangle(maxLocations[0], templateImage.Size), new MCvScalar(0, 0, 255));
            CvInvoke.PutText(OutputMatch, $"Max:{maxValues[0]}", maxLocations[0] + new Size(0, 30), FontFace.HersheySimplex, 1, new MCvScalar(0, 0, 255), 2);
            foreach (Point pt in findPts)
            {
                CvInvoke.Rectangle(OutputMatch, new Rectangle(pt, templateImage.Size), new MCvScalar(0, 255, 0));
                CvInvoke.PutText(OutputMatch, $"FindVal:{findVal}", pt + new Size(0, 30), FontFace.HersheySimplex, 1, new MCvScalar(0, 255, 0), 2);
            }
            CvInvoke.Rectangle(OutputMatch, new Rectangle(minLocations[0], templateImage.Size), new MCvScalar(255, 0, 0));
            CvInvoke.PutText(OutputMatch, $"Min:{minValues[0]}", minLocations[0] + new Size(0, 30), FontFace.HersheySimplex, 1, new MCvScalar(255, 0, 0), 2);
            Outputs["OutputMatch"].value = OutputMatch;
            UIImage.Image = OutputMatch;
            #endregion

            #region output calculate value and turn to gray image by max & min scale value
            Image<Gray, double> img = result.ToImage<Gray, double>();
            Parallel.For(0, img.Height, y =>
            {
                for (int x = 0; x < img.Width; x++)
                {
                    var intensity = colorBuilder(img[y, x].Intensity, minValues[0], maxValues[0], 5);
                    img.Data[y, x, 0] = intensity;
                }
            });

            Outputs["OutputReScale"].value = img.Convert<Gray, byte>();
            #endregion
        };
        #endregion

        private byte colorBuilder(double val, double min, double max, int colorRanges) => (byte)((val - min) / (max - min) * byte.MaxValue);
        private IEnumerable<object> toDataSets(IInputArray array)
        {
            Image<Gray, double> img = array.GetInputArray().GetMat().ToImage<Gray, double>();
            ConcurrentBag<object> list = new ConcurrentBag<object>();
            Parallel.For(0, img.Height, y =>
            {
                for (int x = 0; x < img.Width; x++)
                {
                    list.Add(new { x, y, value = img.Data[y, x, 0] });
                }
            });
            return list;
        }
    }
}
