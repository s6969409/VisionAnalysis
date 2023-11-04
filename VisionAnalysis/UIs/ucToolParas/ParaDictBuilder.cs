using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class ParaDictBuilder
    {
        #region Rectangle
        public static Dictionary<string, PInput> Rectangle()
        {
            Dictionary<string, PInput> pDict = new Dictionary<string, PInput>();
            pDict["p1"] = new PInput() { value = Point() };
            pDict["p2"] = new PInput() { value = Point() };

            return pDict;
        }
        public static Dictionary<string, PInput> Rectangle(JToken jToken)
        {
            Dictionary<string, PInput> pDict = new Dictionary<string, PInput>();
            var temp = jToken["value"]["p1"];

            pDict["p1"] = JObjectToPInput(jToken["value"]["p1"]);
            pDict["p2"] = JObjectToPInput(jToken["value"]["p2"]);

            return pDict;
        }
        #endregion
        #region Point
        public static Dictionary<string, PInput> Point()
        {
            Dictionary<string, PInput> pDict = new Dictionary<string, PInput>();
            pDict["x"] = new PInput() { value = 200 };
            pDict["y"] = new PInput() { value = 200 };

            return pDict;
        }
        public static Dictionary<string, PInput> Point(JToken jToken)
        {
            Dictionary<string, PInput> pDict = new Dictionary<string, PInput>();
            pDict["x"] = JObjectToPInput(jToken["value"]["x"]);
            pDict["y"] = JObjectToPInput(jToken["value"]["y"]);

            return pDict;
        }
        #endregion
        #region BaseTypeTranfer
        public static PInput JObjectToPInput(JToken jToken)
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
            else value = JTokenDataCast(jToken["value"]);

            return buildCrossReference(jToken, value);
        }
        public static PInput JObjectToPInput<T>(JToken jToken)
        {
            object value;

            if (typeof(T).IsEnum)
            {
                value = Enum.Parse(typeof(T), (string)jToken["value"]);
            }
            else throw new InvalidCastException($"{typeof(T)} 目前尚未實做!!");

            return buildCrossReference(jToken, value);
        }
        private static PInput buildCrossReference(JToken jToken,object value) => new PInput()
        {
            ToolName = jToken["ToolName"] == null ? null : jToken["ToolName"].ToString(),
            ParaName = jToken["ParaName"] == null ? null : jToken["ParaName"].ToString(),
            value = value
        };
        private static object JTokenDataCast(JToken jToken)
        {
            if (jToken.Type == JTokenType.Boolean) return (bool)jToken;
            else if (jToken.Type == JTokenType.Float) return (float)jToken;
            else if (jToken.Type == JTokenType.Integer) return (int)jToken;
            else if (jToken.Type == JTokenType.String) return (string)jToken;
            else throw new InvalidCastException($"{jToken.Type} 目前尚未實做!!");
        }
        #endregion
    }
    public class ParaDictRead
    {
        public static Rectangle Rectangle(Dictionary<string, PInput> dict)
        {
            Point p1 = Point((Dictionary<string, PInput>)dict["p1"].value);
            Point p2 = Point((Dictionary<string, PInput>)dict["p2"].value);
            Size size = new Size(p2 - new Size(p1));

            return new Rectangle(p1, size);
        }

        public static Point Point(Dictionary<string, PInput> dict)
        {
            return new Point((int)dict["x"].value, (int)dict["y"].value);
        }
    }
    public class UcPHelper
    {
        public static void readInputs(IToolEditParas tool, ObservableRangeCollection<Nd> nodes)
        {
            foreach (string inputKey in tool.Inputs.Keys)
            {
                foreach (Nd nd in nodes)
                {
                    if (nd.name == tool.Inputs[inputKey].ToolName)
                    {
                        IToolEditParas toolEditParas = nd.value as IToolEditParas;
                        foreach (string pNameKey in toolEditParas.Outputs.Keys)
                        {
                            if (pNameKey == tool.Inputs[inputKey].ParaName && toolEditParas.Outputs[pNameKey].value != null)
                            {
                                tool.Inputs[inputKey].value = toolEditParas.Outputs[pNameKey].value;
                                break;
                            }
                        }
                    }
                }
            }
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
}
