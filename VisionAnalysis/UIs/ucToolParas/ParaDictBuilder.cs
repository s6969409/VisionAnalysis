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
        public static Dictionary<string, PInput> Rectangle()
        {
            Dictionary<string, PInput> rect = new Dictionary<string, PInput>();
            rect["p1"] = new PInput() { value = Point() };
            rect["p2"] = new PInput() { value = Point() };

            return rect;
        }

        public static Dictionary<string, PInput> Point()
        {
            Dictionary<string, PInput> p = new Dictionary<string, PInput>();
            p["x"] = new PInput() { value = 200 };
            p["y"] = new PInput() { value = 200 };

            return p;
        }
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
