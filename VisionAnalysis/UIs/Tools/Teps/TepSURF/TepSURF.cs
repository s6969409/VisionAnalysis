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
            Inputs["hessianThreshold"] = new PInput() { value = 400 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["keyPoints"] = new POutput() { value = new List<object>() };

            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int hessianThreshold = Convert.ToInt32(Inputs["hessianThreshold"].value);

            //process...
            SURF surf = SURF.Create(hessianThreshold);
            KeyPoint[] keyPoints;
            Mat descriptors = new Mat();
            surf.DetectAndCompute(source, null, out keyPoints, descriptors);
            Mat result = new Mat();
            Cv2.DrawKeypoints(source, keyPoints, result);

            Outputs["Output1"].value = result;
            updateUIImage(result);

            (Outputs["keyPoints"].value as List<object>).Clear();
            (Outputs["keyPoints"].value as List<object>).AddRange(keyPoints.Select(kpt=>new { 
                kpt.Octave,
                kpt.Response,
                kpt.ClassId,
                kpt.Angle,
                kpt.Pt,
                kpt.Size
            }));
        };
        #endregion
    }
}
