﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepSIFT : BaseToolEditParas
    {
        public TepSIFT(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["nFeatures"] = new PInput() { value = 0 };
            Inputs["nOctaveLayers"] = new PInput() { value = 3 };
            Inputs["contrastThreshold"] = new PInput() { value = 0.04 };
            Inputs["edgeThreshold"] = new PInput() { value = 10.0 };
            Inputs["sigma"] = new PInput() { value = 1.6 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Output2"] = new POutput() { value = new Rect(100, 123, 50, 70) };
            Outputs["descriptors"] = new POutput() { value = new Mat() };
            Outputs["keyPoints"] = new POutput() { value = new List<object>() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            #region get input para
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int nFeatures = Convert.ToInt32(Inputs["nFeatures"].value);
            int nOctaveLayers = Convert.ToInt32(Inputs["nOctaveLayers"].value);
            double contrastThreshold = Convert.ToDouble(Inputs["contrastThreshold"].value);
            double edgeThreshold = Convert.ToDouble(Inputs["edgeThreshold"].value);
            double sigma = Convert.ToDouble(Inputs["sigma"].value);
            #endregion

            #region process...
            OpenCvSharp.Features2D.SIFT sIFT = OpenCvSharp.Features2D.SIFT.Create(nFeatures, nOctaveLayers, contrastThreshold, edgeThreshold, sigma);
            KeyPoint[] keyPoints;
            Mat descriptors = new Mat();
            sIFT.DetectAndCompute(source, null, out keyPoints, descriptors);
            #endregion

            #region output
            Outputs["descriptors"].value = descriptors;
            Mat result = new Mat();
            Cv2.DrawKeypoints(source, keyPoints, result, flags: DrawMatchesFlags.DrawRichKeypoints);
            Outputs["Output1"].value = result;
            updateUIImage(result);
            (Outputs["keyPoints"].value as List<object>).Clear();
            (Outputs["keyPoints"].value as List<object>).AddRange(keyPoints.Select(kpt => new {
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