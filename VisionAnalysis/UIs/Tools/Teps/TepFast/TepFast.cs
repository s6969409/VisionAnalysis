using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepFast : BaseToolEditParas
    {
        public TepFast(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["threshold"] = new PInput() { value = 10 };
            Inputs["nonmaxSuppression"] = new PInput() { value = true };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["keyPoints"] = new POutput();
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int threshold = (int)Inputs["threshold"].value;
            bool nonmaxSuppression = (bool)Inputs["nonmaxSuppression"].value;

            //process...
            FastFeatureDetector fastFeatureDetector = FastFeatureDetector.Create(threshold, nonmaxSuppression);
            KeyPoint[] keyPoints = fastFeatureDetector.Detect(source);
            Outputs["keyPoints"].value = keyPoints;
            Mat result = new Mat();
            Cv2.DrawKeypoints(source, keyPoints, result, flags: DrawMatchesFlags.NotDrawSinglePoints);

            Outputs["Output1"].value = result;
        };
        #endregion
    }
}
