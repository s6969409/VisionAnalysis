using OpenCvSharp;
using OpenCvSharp.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    internal class TepSURF : BaseToolEditParas
    {
        public TepSURF(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["hessianThreshold"] = new PInput() { value = 0.1 };
            Inputs["nOctaves"] = new PInput() { value = 4 };
            Inputs["nOctaveLayers"] = new PInput() { value = 2 };
            Inputs["extended"] = new PInput() { value = true };
            Inputs["upright"] = new PInput() { value = false };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["descriptors"] = new POutput() { value = new Mat() };
            Outputs["keyPoints"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            double hessianThreshold = Convert.ToDouble(Inputs["hessianThreshold"].value);
            int nOctaves = Convert.ToInt32(Inputs["nOctaves"].value);
            int nOctaveLayers = Convert.ToInt32(Inputs["nOctaveLayers"].value);
            bool extended = Convert.ToBoolean(Inputs["extended"].value);
            bool upright = Convert.ToBoolean(Inputs["upright"].value);
            #endregion

            #region process...
            SURF surf = SURF.Create(hessianThreshold, nOctaves, nOctaveLayers, extended, upright);
            Mat descriptors = new Mat();
            surf.DetectAndCompute(source, null, out KeyPoint[] keyPoints, descriptors);
            #endregion

            #region output
            Outputs["descriptors"].value = descriptors;
            Mat result = new Mat();
            Cv2.DrawKeypoints(source, keyPoints, result, flags: DrawMatchesFlags.DrawRichKeypoints);

            Outputs["Output1"].value = result;
            updateUIImage(result);
            Outputs["keyPoints"].value = keyPoints;
            #endregion
        };
        #endregion
    }
}
