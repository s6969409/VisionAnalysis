using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            Inputs["method"] = new PInput() { value = TemplateMatchModes.CCoeffNormed };
            Inputs["FindVal"] = new PInput() { value = 0 };

            Outputs["OutputResult"] = new POutput() { value = null };
            Outputs["OutputArr"] = new POutput() { value = null };
            Outputs["OutputMatch"] = new POutput() { value = null };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        { 
            base.actionProcess();//read paras

            #region process... & output
            Mat targetImage = Inputs["TargetImage"].value as Mat;
            Mat templateImage = Inputs["TemplateImage"].value as Mat;
            TemplateMatchModes method = TepHelper.getEnum<TemplateMatchModes>(Inputs["method"].value);

            Mat result = new Mat(targetImage.Size(), MatType.CV_32F);

            Cv2.MatchTemplate(targetImage, templateImage, result, method);
            Outputs["OutputResult"].value = result;
            Outputs["OutputArr"].value = ImageProcess.ConvertDataSets(result);
            #endregion

            #region find max & min value
            result.MinMaxIdx(out double minValue, out double maxValue);
            result.MinMaxLoc(out Point minLocation, out Point maxLocation);
            #endregion

            #region output match locatiom information
            double findVal = Convert.ToDouble(Inputs["FindVal"].value);
            result.GetArray(out float[] resultDatas);
            IEnumerable<Point> findPts = resultDatas.Select((val, index) => new { val, x = index % result.Cols, y = index / result.Cols }).Where(v => v.val == findVal).Select(v => new Point(v.x, v.y));

            Mat OutputMatch = targetImage.Clone();
            Cv2.Rectangle(OutputMatch, new Rect(maxLocation, templateImage.Size()), new Scalar(0, 0, 255));
            Cv2.PutText(OutputMatch, $"Max:{maxValue}", maxLocation + new Point(0, 30), HersheyFonts.HersheySimplex, 1, new Scalar(0, 0, 255), 2);
            foreach (Point pt in findPts)
            {
                Cv2.Rectangle(OutputMatch, new Rect(pt, templateImage.Size()), new Scalar(0, 255, 0));
                Cv2.PutText(OutputMatch, $"FindVal:{findVal}", pt + new Point(0, 30), HersheyFonts.HersheySimplex, 1, new Scalar(0, 255, 0), 2);
            }
            Cv2.Rectangle(OutputMatch, new Rect(minLocation, templateImage.Size()), new Scalar(255, 0, 0));
            Cv2.PutText(OutputMatch, $"Min:{minValue}", minLocation + new Point(0, 30), HersheyFonts.HersheySimplex, 1, new Scalar(255, 0, 0), 2);
            Outputs["OutputMatch"].value = OutputMatch;
            #endregion

        };
        #endregion

        
    }
}
