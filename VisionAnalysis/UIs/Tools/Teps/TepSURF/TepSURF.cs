﻿using OpenCvSharp;
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
            Inputs["nOctaves"] = new PInput() { value = 4 };
            Inputs["nOctaveLayers"] = new PInput() { value = 2 };
            Inputs["extended"] = new PInput() { value = true };
            Inputs["upright"] = new PInput() { value = false };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["keyPoints"] = new POutput() { value = new List<object>() };
            Outputs["descriptors"] = new POutput() { value = new Mat() };

            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int hessianThreshold = Convert.ToInt32(Inputs["hessianThreshold"].value);
            int nOctaves = Convert.ToInt32(Inputs["nOctaves"].value);
            int nOctaveLayers = Convert.ToInt32(Inputs["nOctaveLayers"].value);
            bool extended = Convert.ToBoolean(Inputs["extended"].value);
            bool upright = Convert.ToBoolean(Inputs["upright"].value);
            #endregion

            #region process...
            SURF surf = SURF.Create(hessianThreshold, nOctaves, nOctaveLayers, extended, upright);
            KeyPoint[] keyPoints;
            Mat descriptors = new Mat();
            surf.DetectAndCompute(source, null, out keyPoints, descriptors);
            #endregion

            #region output
            Outputs["descriptors"].value = descriptors;
            Mat result = new Mat();
            Cv2.DrawKeypoints(source, keyPoints, result, flags: DrawMatchesFlags.DrawRichKeypoints);

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
            #endregion
        };
        #endregion
    }
}
