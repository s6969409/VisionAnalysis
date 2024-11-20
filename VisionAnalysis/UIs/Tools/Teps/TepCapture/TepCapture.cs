using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI = System.Windows;

namespace VisionAnalysis
{
    public class TepCapture : BaseToolEditParas
    {
        public TepCapture(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["ROI"] = new PInput() { value = ParaDictBuilder<Rect>(100, 100, 200, 200) };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }

        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat inputImage = Inputs["InputImage"].value as Mat;
            Dictionary<string, PInput> dictROI = (Dictionary<string, PInput>)Inputs["ROI"].value;
            Rect roi = toT<Rect>(dictROI);

            Outputs["Output1"].value = new Mat(inputImage, roi);
            updateUIImage((Mat)Outputs["Output1"].value);
        }; 
        public override Action<IParaValue, UcAnalysis> paraSelect => (p, u) =>
        {
            u.ucImg.cvs.Children.Clear();
            if (p.value != Inputs["ROI"].value) return;
            Dictionary<string, PInput> dictROI = (Dictionary<string, PInput>)Inputs["ROI"].value;
            Rect roi = toT<Rect>(dictROI);

            if (u.ucImg.Image == null) return;
            double x = u.ucImg.cvs.ActualWidth - u.ucImg.Image.Width * u.ucImg.Scale;
            double y = u.ucImg.cvs.ActualHeight - u.ucImg.Image.Height * u.ucImg.Scale;

            u.ucImg.cvs.Children.Add(VisualHost.draw(dc =>
            {
                dc.DrawRectangle(null, new UI.Media.Pen(UI.Media.Brushes.Red, 1), new UI.Rect(roi.X * u.ucImg.Scale + x / 2, roi.Y * u.ucImg.Scale + y / 2, roi.Width * u.ucImg.Scale, roi.Height * u.ucImg.Scale));
            }));
        };
        #endregion
    }
}
