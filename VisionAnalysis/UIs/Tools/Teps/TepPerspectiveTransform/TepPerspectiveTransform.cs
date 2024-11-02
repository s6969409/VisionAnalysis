using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepPerspectiveTransform : BaseToolEditParas
    {
        public TepPerspectiveTransform(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["img1"] = new PInput() { value = new Mat() };
            Inputs["img2"] = new PInput() { value = new Mat() };
            Inputs["queryDescriptors"] = new PInput() { value = new Mat() };
            Inputs["trainDescriptors"] = new PInput() { value = new Mat() };
            Inputs["keypoints1"] = new PInput() { value = { } };
            Inputs["keypoints2"] = new PInput() { value = { } };

            Outputs["ProjectLink"] = new POutput() { value = new Mat() };
            Outputs["ProjectLink2"] = new POutput() { value = new Mat() };
            Outputs["Match"] = new POutput() { value = new Mat() };
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

            #region FLANN KnnMatch process
            const int FLANN_INDEX_KDTREE = 1;
            OpenCvSharp.Flann.KDTreeIndexParams indexParams = new OpenCvSharp.Flann.KDTreeIndexParams();
            indexParams.SetInt("Algorithm", FLANN_INDEX_KDTREE);
            indexParams.SetInt("Trees", 5);

            OpenCvSharp.Flann.SearchParams searchParams = new OpenCvSharp.Flann.SearchParams();
            searchParams.SetInt("Checks", 50);
            FlannBasedMatcher flann = new FlannBasedMatcher(indexParams, searchParams);

            DMatch[][] matches = flann.KnnMatch(queryDescriptors, trainDescriptors, k: 3);
            #endregion

            #region feature filt
            List<DMatch> good = new List<DMatch>();
            foreach (DMatch[] match in matches)
            {
                if (match[0].Distance < 0.7 * match[1].Distance)
                {
                    good.Add(match[0]);
                }
            }
            #endregion

            #region match process
            int MIN_MATCH_COUNT = 4;
            if (good.Count >= MIN_MATCH_COUNT)
            {
                Point2d[] src_pts = good.Select(m => Point2d.FromPoint(keypoints1.ElementAt(m.QueryIdx).Pt.ToPoint())).ToArray();
                Point2d[] dst_pts = good.Select(m => Point2d.FromPoint(keypoints2.ElementAt(m.TrainIdx).Pt.ToPoint())).ToArray();
                Mat homographyMask = Cv2.FindHomography(src_pts, dst_pts, HomographyMethods.Ransac, 5.0);
                int h = img1.Height;
                int w = img1.Width;
                Point2d[] pts = new Point2d[] { new Point(0, 0), new Point(0, h - 1), new Point(w - 1, h - 1), new Point(w - 1, 0) };
                Point2d[] ptsPT = Cv2.PerspectiveTransform(pts, homographyMask);
                Mat matMatch = img2.Clone();
                Cv2.Polylines(matMatch, new List<Point[]> { ptsPT.Select(p => p.ToPoint()).ToArray() }, true, new Scalar(0, 0, 255), 1, LineTypes.AntiAlias);
                Outputs["Match"].value = matMatch;
            }
            else
            {
                Outputs["Match"].value = img2;
            }
            #endregion

            #region Project
            Mat result = new Mat();
            Cv2.DrawMatches(img1, keypoints1, img2, keypoints2, good, result);
            Outputs["ProjectLink"].value = result;
            #endregion

            #region ProjectLink2 test
            List<DMatch> good2 = new List<DMatch>();
            foreach (DMatch[] match in matches)
            {
                if (match[1].Distance < 0.7 * match[2].Distance)
                {
                    good2.Add(match[1]);
                }
            }
            Mat result2 = new Mat();
            Cv2.DrawMatches(img1, keypoints1, img2, keypoints2, good2, result2);
            Outputs["ProjectLink2"].value = result2;
            #endregion
        };
        #endregion
    }
}
