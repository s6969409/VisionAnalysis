using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepBFMatcher : BaseToolEditParas
    {
        public TepBFMatcher(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["img1"] = new PInput() { value = new Mat() };
            Inputs["img2"] = new PInput() { value = new Mat() };
            Inputs["queryDescriptors"] = new PInput() { value = new Mat() };
            Inputs["trainDescriptors"] = new PInput() { value = new Mat() };
            Inputs["keypoints1"] = new PInput() { value = { } };
            Inputs["keypoints2"] = new PInput() { value = { } };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat img1 = Inputs["img1"].value as Mat;
            Mat img2 = Inputs["img2"].value as Mat;
            Mat queryDescriptors = Inputs["queryDescriptors"].value as Mat;
            Mat trainDescriptors = Inputs["trainDescriptors"].value as Mat;
            IEnumerable<KeyPoint> keypoints1 = Inputs["keypoints1"].value as IEnumerable<KeyPoint>;
            IEnumerable<KeyPoint> keypoints2 = Inputs["keypoints2"].value as IEnumerable<KeyPoint>;
            #endregion

            #region process...
            BFMatcher matcher = new BFMatcher();
            DMatch[] matches = matcher.Match(queryDescriptors, trainDescriptors);
            Mat result = new Mat();
            Cv2.DrawMatches(img1, keypoints1, img2, keypoints2, matches, result);
            #endregion

            #region output
            Outputs["Output1"].value = result;
            #endregion
        };
        #endregion
    }
}
