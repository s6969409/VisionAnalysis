using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;

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
        public static Nd NdBuild<T>(KeyValuePair<string,T> kvInput)where T: IParaValue
        {
            Nd nd = new Nd(kvInput.Key, kvInput.Value);
            if (kvInput.Value.value is Dictionary<string, T>)
            {
                Dictionary<string, T> childs = kvInput.Value.value as Dictionary<string, T>;
                nd.childNodes.AddRange(childs.Select(child => NdBuild(child)));
            }
            return nd;
        }
    }

    #region IToolEditParas interface & base sample implement
    public interface IToolEditParas
    {
        string ToolName { get; set; }
        IMatProperty UIImage { get; set; }
        Dictionary<string, PInput> Inputs { get; }
        Dictionary<string, POutput> Outputs { get; }
        Action actionProcess { get; }
        Func<string, JObject> getJObjectAndSaveImg { get; }
        Action<JObject> loadParas { get; }
    }
    public class BaseToolEditParas : IToolEditParas
    {
        protected ObservableRangeCollection<Nd> nodes;

        public BaseToolEditParas(ObservableRangeCollection<Nd> nodes) { this.nodes = nodes; }

        public virtual string ToolName { get; set; }
        public virtual IMatProperty UIImage { get; set; }
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
        public virtual Action<JObject> loadParas => (jobject) => 
        {
            ToolName = (string)jobject["ToolName"];
            JObject inputs = (JObject)jobject["Inputs"];

            string[] keys = Inputs.Keys.Select(k => k.ToString()).ToArray();
            foreach (var key in keys)
            {
                if (Inputs[key].value is Mat)
                {
                    Inputs[key] = BuildMatByCrossReference(inputs[key]);
                }
                else if (Inputs[key].value is Enum)
                {
                    Inputs[key] = JObjectToPInput(inputs[key], Inputs[key].value.GetType());
                }
                else
                {
                    Inputs[key] = JObjectToPInput(inputs[key]);
                }
            }
        };
        protected virtual Action<Mat> updateUIImage => mat =>
        {
            if (UIImage != null) UIImage.Image = mat;
        };

        #region BaseTypeTranfer
        public static PInput JObjectToPInput(JToken jToken, Type type = null)
        {
            object value;

            if (jToken["value"].Type == JTokenType.Object)
            {
                Dictionary<string, PInput> pInput = new Dictionary<string, PInput>();
                foreach (JProperty jProperty in jToken["value"].ToObject<JObject>().Properties())
                {
                    pInput[jProperty.Name] = JObjectToPInput(jProperty.Value);
                }
                value = pInput;
            }
            else if (type != null && type.IsEnum && (jToken["value"].Type == JTokenType.String || jToken["value"].Type == JTokenType.Integer))
            {
                value = Enum.Parse(type, (string)jToken["value"]);
            }
            else if (jToken["value"].Type == JTokenType.Boolean) value = (bool)jToken["value"];
            else if (jToken["value"].Type == JTokenType.Float) value = (float)jToken["value"];
            else if (jToken["value"].Type == JTokenType.Integer) value = (int)jToken["value"];
            else if (jToken["value"].Type == JTokenType.String) value = (string)jToken["value"];
            else throw new InvalidCastException($"jToken[value].Type = {jToken["value"].Type} + type = {type} 目前尚未實做!!");

            return BuildCrossReference(jToken, value);
        }
        private static PInput BuildCrossReference(JToken jToken, object value) => new PInput()
        {
            ToolName = jToken["ToolName"] == null ? null : jToken["ToolName"].ToString(),
            ParaName = jToken["ParaName"] == null ? null : jToken["ParaName"].ToString(),
            value = value
        };
        public static PInput BuildMatByCrossReference(JToken jToken) => new PInput()
        {
            ToolName = jToken["ToolName"] == null ? null : jToken["ToolName"].ToString(),
            ParaName = jToken["ParaName"] == null ? null : jToken["ParaName"].ToString(),
            value = File.Exists((string)jToken["value"]) ? new Mat((string)jToken["value"]) : null
        };
        #endregion
        #region paraType Tranfer
        protected static Dictionary<string, PInput> ParaDictBuilder<T>(params object[] ps)
        {
            if (typeof(T) == typeof(Rectangle))
            {
                string[] keyNames = { "p1", "p2" };
                return keyNames.Select((keyName, index) => new { Key = keyName, Value = new PInput() { value = ParaDictBuilder<Point>(ps[index * 2], ps[index * 2 + 1]) } }).ToDictionary(item => item.Key, item => item.Value);
            }
            else if (typeof(T) == typeof(Point))
            {
                string[] keyNames = { "x", "y" };
                return keyNames.Select((keyName, index) => new { Key = keyName, Value = new PInput() { value = ps[index] } }).ToDictionary(item => item.Key, item => item.Value);
            }
            else if (typeof(T) == typeof(Size))
            {
                string[] keyNames = { "Width", "Height" };
                return keyNames.Select((keyName, index) => new { Key = keyName, Value = new PInput() { value = ps[index] } }).ToDictionary(item => item.Key, item => item.Value);
            }
            else if (typeof(T) == typeof(MCvScalar))
            {
                string[] keyNames = { "v0", "v1", "v2", "v3" };
                return keyNames.Select((keyName, index) => new { Key = keyName, Value = new PInput() { value = ps[index] } }).ToDictionary(item => item.Key, item => item.Value);
            }
            else throw new Exception();
        }
        protected static T toT<T>(Dictionary<string, PInput> dict)
        {
            if (typeof(T) == typeof(Rectangle))
            {
                Point p1 = Base<Point>((Dictionary<string, PInput>)dict["p1"].value);
                Point p2 = Base<Point>((Dictionary<string, PInput>)dict["p2"].value);
                Size size = new Size(p2 - new Size(p1));

                return (T)(object)new Rectangle(p1, size);
            }
            else
            {
                return Base<T>(dict);
            }
        }
        private static T Base<T>(Dictionary<string, PInput> dict)
        {
            return (T)Activator.CreateInstance(typeof(T), dict.Values.Select(v => v.value).ToArray());
        }
        #endregion
    }
    public interface IParaValue
    {
        object value { get; }
    }
    public class PInput : IParaValue, INotifyPropertyChanged
    {
        private string _ToolName;
        public string ToolName
        {
            get => _ToolName;
            set
            {
                _ToolName = value;
                onPropertyChanged(nameof(ToolName));
            }
        }
        private string _ParaName;
        public string ParaName
        {
            get => _ParaName;
            set
            {
                _ParaName = value;
                onPropertyChanged(nameof(ParaName));
            }
        }
        private object _value;
        public object value
        {
            get => _value;
            set
            {
                if (_value == null)
                {
                    _value = value;
                }
                else
                {
                    _value = Convert.ChangeType(value, _value.GetType());
                }

                onPropertyChanged(nameof(this.value));
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
            else if (pi.value is Mat)
            {
                string imgPath = $@"{imgDirPath}\{Directory.GetFiles(imgDirPath).Length}.bmp";
                ((Mat)pi.value).Save(imgPath);
                jobject["value"] = imgPath;
            }
            else if (pi.value is int) jobject["value"] = (int)pi.value;
            else if (pi.value is float) jobject["value"] = (float)pi.value;
            else jobject["value"] = pi.value == null ? null : pi.value.ToString();

            return jobject;
        }
        public Type Type => _value == null ? null : _value.GetType();
        public Array valueSource => _value == null ? null : Enum.GetValues(_value.GetType());

        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class POutput : IParaValue, INotifyPropertyChanged
    {
        private object _value;
        public object value
        {
            get => _value;
            set
            {
                _value = value;
                onPropertyChanged(nameof(value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
