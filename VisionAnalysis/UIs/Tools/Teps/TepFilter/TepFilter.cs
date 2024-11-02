using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepGaussianBlur : BaseToolEditParas
    {
        public TepGaussianBlur(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["ksize"] = new PInput() { value = ParaDictBuilder<Size>(5, 5) };
            Inputs["sigmaX"] = new PInput() { value = 0 };
            Inputs["sigmaY"] = new PInput() { value = 0 };
            Inputs["borderType"] = new PInput() { value = BorderTypes.Default };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            Size ksize = toT<Size>((Dictionary<string, PInput>)Inputs["ksize"].value);
            double sigmaX = (double)Inputs["sigmaX"].value;
            double sigmaY = (double)Inputs["sigmaY"].value;
            BorderTypes borderType = TepHelper.getEnum<BorderTypes>(Inputs["borderType"].value);

            //process...
            Mat mat = new Mat();
            Cv2.GaussianBlur(source, mat, ksize, sigmaX, sigmaY, borderType);
            Outputs["Output1"].value = mat;
            updateUIImage(mat);
        };
        #endregion
    }

    public class TepBilateralFilter : BaseToolEditParas
    {
        public TepBilateralFilter(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["d"] = new PInput() { value = 0 };
            Inputs["sigmaColor"] = new PInput() { value = 0 };
            Inputs["sigmaSpace"] = new PInput() { value = 0 };
            Inputs["borderType"] = new PInput() { value = BorderTypes.Default };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int d = (int)Inputs["d"].value;
            double sigmaColor = (double)Inputs["sigmaColor"].value;
            double sigmaSpace = (double)Inputs["sigmaSpace"].value;
            BorderTypes borderType = TepHelper.getEnum<BorderTypes>(Inputs["borderType"].value);

            //process...
            Mat mat = new Mat();
            Cv2.BilateralFilter(source, mat, d, sigmaColor, sigmaSpace, borderType);
            Outputs["Output1"].value = mat;
            updateUIImage(mat);
        };
        #endregion
    }

    public class TepBlur : BaseToolEditParas
    {
        public TepBlur(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["ksize"] = new PInput() { value = ParaDictBuilder<Size>(5, 5) };
            Inputs["anchor"] = new PInput() { value = ParaDictBuilder<Point>(-1, -1) };
            Inputs["borderType"] = new PInput() { value = BorderTypes.Default };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            Size ksize = toT<Size>((Dictionary<string, PInput>)Inputs["ksize"].value);
            Point anchor = toT<Point>((Dictionary<string, PInput>)Inputs["anchor"].value);
            BorderTypes borderType = TepHelper.getEnum<BorderTypes>(Inputs["borderType"].value);

            //process...
            Mat mat = new Mat();
            Cv2.Blur(source, mat, ksize, anchor, borderType);
            Outputs["Output1"].value = mat;
            updateUIImage(mat);
        };
        #endregion
    }

    public class TepMedianBlur : BaseToolEditParas
    {
        public TepMedianBlur(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["InputImage"] = new PInput() { value = new Mat() };
            Inputs["ksize"] = new PInput() { value = 3 };

            Outputs["Output1"] = new POutput() { value = new Mat() };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            Mat source = Inputs["InputImage"].value as Mat;
            int ksize = (int)Inputs["ksize"].value;

            //process...
            Mat mat = new Mat();
            Cv2.MedianBlur(source, mat, ksize);
            Outputs["Output1"].value = mat;
            updateUIImage(mat);
        };
        #endregion
    }

}
