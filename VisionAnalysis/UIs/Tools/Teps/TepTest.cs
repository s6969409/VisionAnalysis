﻿using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepTest : BaseToolEditParas
    {
        public TepTest(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            Outputs["Output2"] = new POutput() { value = new Rect(100,123,50,70) };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;

            //process...
            OpenCvSharp.XFeatures2D.SURF surf = OpenCvSharp.XFeatures2D.SURF.Create(400);
            KeyPoint[] keyPoints;
            Mat descriptors = new Mat();
            surf.DetectAndCompute(source, null, out keyPoints, descriptors);
            Mat result = new Mat();
            Cv2.DrawKeypoints(source, keyPoints, result);

            surf.HessianThreshold = 50000;
            surf.DetectAndCompute(source, null, out keyPoints, descriptors);
            Cv2.DrawKeypoints(source, keyPoints, result);

            Outputs["Output1"].value = result;
        };
        #endregion
    }
}
