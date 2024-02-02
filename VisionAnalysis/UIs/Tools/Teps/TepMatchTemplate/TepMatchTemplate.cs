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

            //process...
            Mat targetImage = Inputs["TargetImage"].value as Mat;
            Mat templateImage = Inputs["TemplateImage"].value as Mat;
            TemplateMatchingType method = TepHelper.getEnum<TemplateMatchingType>(Inputs["method"].value);

            Mat result = new Mat(targetImage.Size, DepthType.Cv32F, 1);

            CvInvoke.MatchTemplate(targetImage, templateImage, result, method);
            Outputs["OutputResult"].value = result;
            Outputs["OutputArr"].value = toDataSets(result);

            // 找出結果的最小值、最大值、最小位置和最大位置
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            // 如果使用 Ccoeff 或 CcoeffNormed 方法，最大值位置即為匹配位置
            Point matchLocation = maxLocations[0];

            // 輸出匹配結果位置
            Console.WriteLine("匹配位置：X = " + matchLocation.X + ", Y = " + matchLocation.Y);
            CvInvoke.Rectangle(targetImage, new Rectangle(matchLocation, templateImage.Size), new MCvScalar(0, 0, 255));

            Outputs["OutputMatch"].value = targetImage;
            
            Image<Gray, double> img = result.ToImage<Gray, double>();
            Parallel.For(0, img.Height, y =>
            {
                for (int x = 0; x < img.Width; x++)
                {
                    var intensity = colorBuilderX(img[y, x].Intensity, minValues[0], maxValues[0], 5);
                    img.Data[y, x, 0] = intensity;
                }
            });

            Outputs["OutputReScale"].value = img.Convert<Gray, byte>();
        };
        #endregion

        private byte colorBuilderX(double val, double min, double max, int colorRanges)
        {
            var newVal = (byte)((val - min) / (max - min) * byte.MaxValue);

            return newVal;
        }
        private byte[] colorBuilder(double val, double min, double max, int colorRanges)
        {
            if (val == 0) return new byte[]{ 0,0,0,};

            var step = (max - min) / colorRanges;
            var index = (int)((val - min) * colorRanges / (max - min));
            var h = (index + 1) * step + min;
            var m = index * step + min;
            var localR = ((double)val - m) / (h - m);

            byte[] color = colorBdr(index, localR);

            return color;
        }
        private byte[] colorBdr(int index, double range)
        {
            byte red = 0, green = 0, blue = 0, maxR = 200, maxG = 200, maxB = 200;
            if (index == 0)//藍=>水藍
            {
                red = 0;
                green = (byte)(range * maxG);
                blue = (byte)maxB;
            }
            else if (index == 1)//水藍=>綠
            {
                red = 0;
                green = (byte)maxG;
                blue = (byte)((1 - range) * maxB);
            }
            else if (index == 2)//綠=>黃
            {
                red = (byte)(range * maxR);
                green = (byte)maxG;
                blue = 0;
            }
            else if (index == 3)//黃=>紅
            {
                red = (byte)(maxR);
                green = (byte)((1 - range) * maxG);
                blue = 0;
            }
            else if (index == 4)//紅=>紫
            {
                red = (byte)maxR;
                green = 0;
                blue = (byte)(range * maxB);
            }
            Color color = Color.FromArgb(red, green, blue);
            return new[] { red, green, blue };
        }
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
