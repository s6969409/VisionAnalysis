using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UI = System.Windows;

namespace VisionAnalysis
{
    public class TepHelper
    {
        public static void readInputs(IToolEditParas tool, ObservableRangeCollection<Nd> nodes)
        {
            //iterate through all input para of tool
            foreach (string inputKey in tool.Inputs.Keys)
            {
                //iterate through all ref tool of node
                foreach (Nd nd in nodes)
                {
                    if (nd.name == tool.Inputs[inputKey].ToolName)
                    {
                        bool isfound = false;
                        IToolEditParas toolEditParas = nd.value as IToolEditParas;
                        //iterate through all output para of ref node(nd)
                        foreach (string pNameKey in toolEditParas.Outputs.Keys)
                        {
                            if (pNameKey == tool.Inputs[inputKey].ParaName && toolEditParas.Outputs[pNameKey].value != null)
                            {
                                tool.Inputs[inputKey].value = toolEditParas.Outputs[pNameKey].value;
                                isfound = true;
                                break;
                            }
                        }
                        if (isfound) break;
                    }
                }
            }
        }
        public static T getEnum<T>(object arg)
        {
            string code = arg.ToString();
            return (T)Enum.Parse(typeof(T), code);
        }
        public static Nd NdBuild<T>(IToolEditParas tool, KeyValuePair<string, T> kvInput) where T : IParaValue
        {
            Nd nd = new Nd(tool, kvInput.Key, kvInput.Value);
            if (kvInput.Value.value is Dictionary<string, T>)
            {
                Dictionary<string, T> childs = kvInput.Value.value as Dictionary<string, T>;
                nd.childNodes.AddRange(childs.Select(child => NdBuild(tool, child)));
            }
            return nd;
        }

        public static byte colorBuilder(double val, double min, double max) => (byte)((val - min) / (max - min) * byte.MaxValue);
        public static Mat ConvertGrayImg<T>(Mat mat, double min, double max) where T : struct
        {
            Mat newMat = new Mat(mat.Rows, mat.Cols, MatType.CV_8UC1, Scalar.Black);
            Parallel.For(0, mat.Height, y =>
            {
                for (int x = 0; x < mat.Width; x++)
                {
                    var a = mat.Get<T>(y, x);
                    byte intensity = colorBuilder(Convert.ToDouble(mat.Get<T>(y, x)), min, max);
                    newMat.Set(y, x, intensity);
                }
            });
            return newMat;
        }
    }

    #region IToolEditParas interface & base sample implement
    public interface IToolEditParas
    {
        string ToolName { get; set; }
        UcImage UIImage { get; set; }
        Dictionary<string, PInput> Inputs { get; }
        Dictionary<string, POutput> Outputs { get; }
        Action actionProcess { get; }
        Func<string, JObject> getJObjectAndSaveImg { get; }
        Action<JObject, string> loadParas { get; }
        #region for UI
        Action<IParaValue, UcAnalysis> paraSelect { get; }
        #endregion
    }
    public class BaseToolEditParas : IToolEditParas, IToolTip
    {
        protected ObservableRangeCollection<Nd> nodes;

        public BaseToolEditParas(ObservableRangeCollection<Nd> nodes) { this.nodes = nodes; }

        public virtual string ToolName { get; set; }
        public virtual UcImage UIImage { get; set; }
        public virtual Dictionary<string, PInput> Inputs { get; } = new Dictionary<string, PInput>();
        public virtual Dictionary<string, POutput> Outputs { get; } = new Dictionary<string, POutput>();
        public virtual Action actionProcess => () => TepHelper.readInputs(this, nodes);
        public virtual Func<string, JObject> getJObjectAndSaveImg => (imgDirPath) =>
        {
            JObject jobject = new JObject();
            jobject["ToolType"] = GetType().FullName;
            jobject["ToolName"] = ToolName;
            jobject["Inputs"] = PInput.getJObjectAndSaveImg(Inputs, imgDirPath);

            return jobject;
        };
        public virtual Action<JObject, string> loadParas => (jobject, imgDirPath) =>
        {
            ToolName = (string)jobject["ToolName"];
            JObject inputs = (JObject)jobject["Inputs"];

            string[] keys = Inputs.Keys.Select(k => k.ToString()).ToArray();
            foreach (var key in keys)
            {
                if (Inputs[key].value is Mat)
                {
                    Inputs[key] = BuildMatByCrossReference(inputs[key], imgDirPath);
                }
                else if (Inputs[key].value is Enum)
                {
                    Inputs[key] = JObjectToPInput(inputs[key], Inputs[key]);
                }
                else if (inputs[key] != null)
                {
                    Inputs[key] = JObjectToPInput(inputs[key], Inputs[key]);
                }
                else
                {
                    System.Windows.MessageBox.Show($"ToolName:{ToolName} in saveFile not contain parameter:[{key}], system will use default.");
                }
            }
        };
        protected virtual Action<Mat> updateUIImage => mat =>
        {
            if (UIImage != null) UIImage.Image = mat;
        };

        public virtual Action<IParaValue, UcAnalysis> paraSelect => (p, u) =>
        {
            u.ucImg.cvs.Children.Clear();
            PInput pInput = p as PInput;
            if (pInput == null || u.ucImg.Image == null) return;
            double x = u.ucImg.cvs.ActualWidth - u.ucImg.Image.Width * u.ucImg.Scale;
            double y = u.ucImg.cvs.ActualHeight - u.ucImg.Image.Height * u.ucImg.Scale;
            if (pInput.Type == typeof(Rect))
            {
                Rect roi = toT<Rect>((Dictionary<string, PInput>)p.value);

                u.ucImg.cvs.Children.Add(VisualHost.draw(dc =>
                {
                    dc.DrawRectangle(null, new UI.Media.Pen(UI.Media.Brushes.Red, 1), new UI.Rect(roi.X * u.ucImg.Scale + x / 2, roi.Y * u.ucImg.Scale + y / 2, roi.Width * u.ucImg.Scale, roi.Height * u.ucImg.Scale));
                }));
            }
            else if (pInput.Type == typeof(RotatedRect))
            {
                RotatedRect rotatedRect = toT<RotatedRect>((Dictionary<string, PInput>)p.value);
                Point2f ofs = new Point2f((float)x / 2, (float)y / 2);

                u.ucImg.cvs.Children.Add(VisualHost.draw(dc =>
                {
                    Point2f[] pfs = rotatedRect.Points().Select(pf => pf * u.ucImg.Scale + ofs).ToArray();
                    dc.DrawLine(new UI.Media.Pen(UI.Media.Brushes.Red, 1), new UI.Point((int)pfs[0].X, (int)pfs[0].Y), new UI.Point((int)pfs[1].X, (int)pfs[1].Y));
                    dc.DrawLine(new UI.Media.Pen(UI.Media.Brushes.Red, 1), new UI.Point((int)pfs[1].X, (int)pfs[1].Y), new UI.Point((int)pfs[2].X, (int)pfs[2].Y));
                    dc.DrawLine(new UI.Media.Pen(UI.Media.Brushes.Red, 1), new UI.Point((int)pfs[2].X, (int)pfs[2].Y), new UI.Point((int)pfs[3].X, (int)pfs[3].Y));
                    dc.DrawLine(new UI.Media.Pen(UI.Media.Brushes.Red, 1), new UI.Point((int)pfs[3].X, (int)pfs[3].Y), new UI.Point((int)pfs[0].X, (int)pfs[0].Y));
                }));
            }
        };

        public static string PathImgDir(string pathJson)
        {
            return $@"{Path.GetDirectoryName(pathJson)}\Images";
        }

        #region BaseTypeTranfer
        public static PInput JObjectToPInput(JToken jToken, PInput pInput)
        {
            Type type = pInput.Type;

            if (jToken["value"] == null || jToken["value"].Type == JTokenType.Null) pInput.value = null;
            else if (jToken["value"].Type == JTokenType.Object && pInput.value is Dictionary<string, PInput> dict)
            {
                foreach (JProperty jProperty in jToken["value"].ToObject<JObject>().Properties())
                {
                    dict[jProperty.Name] = JObjectToPInput(jProperty.Value, dict[jProperty.Name]);
                }
                pInput.value = dict;
            }
            else if (type != null && type.IsEnum && (jToken["value"].Type == JTokenType.String || jToken["value"].Type == JTokenType.Integer))
            {
                pInput.value = Enum.Parse(type, (string)jToken["value"]);
            }
            else if (jToken["value"].Type == JTokenType.Boolean) pInput.value = (bool)jToken["value"];
            else if (jToken["value"].Type == JTokenType.Float) pInput.value = (double)jToken["value"];
            else if (jToken["value"].Type == JTokenType.Integer) pInput.value = (int)jToken["value"];
            else if (jToken["value"].Type == JTokenType.String) pInput.value = (string)jToken["value"];
            else throw new InvalidCastException($"jToken[value].Type = {jToken["value"].Type} + type = {type} 目前尚未實做!!");

            pInput.ToolName = jToken["ToolName"] == null ? null : jToken["ToolName"].ToString();
            pInput.ParaName = jToken["ParaName"] == null ? null : jToken["ParaName"].ToString();
            return pInput;
        }
        public static PInput BuildMatByCrossReference(JToken jToken, string imgDirPath)
        {
            string imgPath = Path.IsPathRooted((string)jToken["value"]) ? (string)jToken["value"] : $@"{imgDirPath}\{(string)jToken["value"]}";
            return new PInput()
            {
                ToolName = jToken["ToolName"] == null ? null : jToken["ToolName"].ToString(),
                ParaName = jToken["ParaName"] == null ? null : jToken["ParaName"].ToString(),
                value = File.Exists(imgPath) ? new Mat(imgPath) : null
            };
        }
        #endregion
        #region paraType Tranfer
        protected static PInput ParaDictBuilder<T>(params object[] ps)
        {
            Dictionary<string, PInput> dictPara = new Dictionary<string, PInput>();

            if (typeof(T) == typeof(Rect))
            {
                string[] keyNames = { "p1", "p2" };
                dictPara = keyNames.Select((keyName, index) => new { Key = keyName, Value = ParaDictBuilder<Point>(ps[index * 2], ps[index * 2 + 1]) }).ToDictionary(item => item.Key, item => item.Value);
            }
            else if (typeof(T) == typeof(RotatedRect))
            {
                string[] keyNames = { "rect", "angle" };
                dictPara["rect"] = ParaDictBuilder<Rect>(ps[0], ps[1], ps[2], ps[3]);
                dictPara["angle"] = new PInput() { value = ps[4] };
            }
            else if (typeof(T) == typeof(Point))
            {
                string[] keyNames = { "x", "y" };
                dictPara = keyNames.Select((keyName, index) => new { Key = keyName, Value = new PInput() { value = ps[index] } }).ToDictionary(item => item.Key, item => item.Value);
            }
            else if (typeof(T) == typeof(Size))
            {
                string[] keyNames = { "Width", "Height" };
                dictPara = keyNames.Select((keyName, index) => new { Key = keyName, Value = new PInput() { value = ps[index] } }).ToDictionary(item => item.Key, item => item.Value);
            }
            else if (typeof(T) == typeof(Scalar))
            {
                string[] keyNames = { "v0", "v1", "v2", "v3" };
                dictPara = keyNames.Select((keyName, index) => new { Key = keyName, Value = new PInput() { value = ps[index] } }).ToDictionary(item => item.Key, item => item.Value);
            }
            else throw new Exception($"{typeof(T).Name} not define method in ParaDictBuilder!");

            return new PInput() { value = dictPara, Type = typeof(T) };
        }
        protected static T toT<T>(Dictionary<string, PInput> dict)
        {
            if (typeof(T) == typeof(Rect))
            {
                Point p1 = Base<Point>((Dictionary<string, PInput>)dict["p1"].value);
                Point p2 = Base<Point>((Dictionary<string, PInput>)dict["p2"].value);
                Point diff = p2 - p1;
                Size size = new Size(diff.X, diff.Y);

                return (T)(object)new Rect(p1, size);
            }
            else if (typeof(T) == typeof(RotatedRect))
            {
                Rect rect = toT<Rect>((Dictionary<string, PInput>)dict["rect"].value);
                double angle = (double)dict["angle"].value;
                Point2f ct = new Point2f(rect.Location.Y + rect.Size.Width / 2, rect.Location.X + rect.Size.Height / 2);
                return (T)(object)new RotatedRect(ct, rect.Size, (float)angle);
            }
            else
            {
                return Base<T>(dict);
            }
        }
        private static T Base<T>(Dictionary<string, PInput> dict)
        {
            return (T)Activator.CreateInstance(typeof(T), dict.Values.Select(kvPair => kvPair.value).ToArray());
        }
        #endregion
        public string ToolTip => $"type: {GetType()}\nname: {ToolName}";
    }
    public interface IParaValue
    {
        object value { get; }
        Type Type { get; }
    }
    public class PInput : POutput
    {
        protected string _ToolName;
        public string ToolName
        {
            get => _ToolName;
            set
            {
                _ToolName = value;
                onPropertyChanged(nameof(ToolName));
            }
        }
        protected string _ParaName;
        public string ParaName
        {
            get => _ParaName;
            set
            {
                _ParaName = value;
                onPropertyChanged(nameof(ParaName));
            }
        }

        public static JObject getJObjectAndSaveImg(Dictionary<string, PInput> inputs, string imgDirPath)
        {
            JObject jobject = new JObject();
            foreach (string key in inputs.Keys)
            {
                jobject[key] = getJObjectAndSaveImg(inputs[key], imgDirPath);
            }
            return jobject;
        }
        public static JObject getJObjectAndSaveImg(PInput pi, string imgDirPath)
        {
            JObject jobject = new JObject();
            jobject["ToolName"] = pi.ToolName;
            jobject["ParaName"] = pi.ParaName;
            if (pi.value is PInput)
            {
                PInput input = pi.value as PInput;
                jobject["value"] = getJObjectAndSaveImg(input, imgDirPath);
            }
            else if (pi.value is Dictionary<string, PInput>)
            {
                jobject["value"] = new JObject();

                Dictionary<string, PInput> paras = pi.value as Dictionary<string, PInput>;
                foreach (var p in paras)
                {
                    jobject["value"][p.Key] = getJObjectAndSaveImg(p.Value, imgDirPath);
                }
            }
            else if (pi.value is Mat matVal && matVal.Data != IntPtr.Zero)
            {
                string imgPath = $@"{imgDirPath}\{Directory.GetFiles(imgDirPath).Length}.bmp";
                matVal.SaveImage(imgPath);
                jobject["value"] = Path.GetFileName(imgPath);
            }
            else if (pi.value is int intVal) jobject["value"] = intVal;
            else if (pi.value is double doubleVal) jobject["value"] = doubleVal;
            else if (pi.value is bool boolVal) jobject["value"] = boolVal;
            else if (pi.value is string strVal) jobject["value"] = strVal;
            else if (pi.value == null) jobject["value"] = null;
            else if (pi.value.GetType().IsEnum) jobject["value"] = pi.value.ToString();
            //else throw new Exception($"{pi.value.GetType().FullName} not define saveMethod!");

            return jobject;
        }
        public Array valueSource => _value == null ? null : Enum.GetValues(_value.GetType());
    }
    public class POutput : IParaValue, IToolTip, INotifyPropertyChanged
    {
        protected object _value;
        public virtual object value
        {
            get => _value;
            set
            {
                _value = value;
                if (Type == null) Type = _value?.GetType();
                onPropertyChanged(nameof(this.value));
                onPropertyChanged(nameof(ToolTip));
            }
        }
        public virtual string ToolTip
        {
            get
            {
                Type type = Type == null ? value?.GetType() : Type;
                string tip = $"type: {type}";
                if (value?.GetType() == type) tip += $"\nvalue: {value}";

                return tip;
            }
        }

        public Type Type { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
