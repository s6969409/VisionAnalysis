using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
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

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Output2"] = new POutput() { value = null };
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

            // 找出結果的最小值、最大值、最小位置和最大位置
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            // 如果使用 Ccoeff 或 CcoeffNormed 方法，最大值位置即為匹配位置
            Point matchLocation = maxLocations[0];

            // 輸出匹配結果位置
            Console.WriteLine("匹配位置：X = " + matchLocation.X + ", Y = " + matchLocation.Y);
            CvInvoke.Rectangle(targetImage, new Rectangle(matchLocation, templateImage.Size), new MCvScalar(0, 0, 255));

            Outputs["Output1"].value = targetImage;

            var a = result.ToImage<Gray, double>();
            var b = a[0, 0];
            Image<Bgr, byte> img = new Image<Bgr, byte>(result.Size);
            // 遍歷圖像的每个像素，並重新調整亮度
            for (int y = 0; y < a.Height; y++)
            {
                for (int x = 0; x < a.Width; x++)
                {
                    // 獲取當前的 Bgr 值
                    var colors = colorBuilder(a[y, x].Intensity, minValues[0], maxValues[0], 5);
                    Bgr bgr = new Bgr(colors[2], colors[1], colors[0]);

                    // 將修改後的像素值重新調整
                    img[y, x] = bgr;
                }
            }

            Outputs["Output2"].value = img;
        };
        #endregion

        /*private IEnumerable<Color> turnFormat24bppRgb(int[] data, double limitDn = 0, double limitUp = 1)
        {
            int colorRanges = 5;
            int minSecond = data.Distinct().OrderBy(val => val).Skip(1).FirstOrDefault();
            int max = data.Max();
            int step = (max - minSecond) / colorRanges;

            minSecond += (int)((max - minSecond) * limitDn);
            max -= (int)((max - minSecond) * (1 - limitUp));

            IEnumerable<Color> ans = data.Select((val, i) => colorBuilder(val, minSecond, max, step, colorRanges));

            return ans;
        }*/
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

    }
}
