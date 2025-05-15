using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VisionAnalysis
{
    public class Model
    {
        protected string path;
        protected ObservableRangeCollection<Nd> nodes = new ObservableRangeCollection<Nd>();

        public Model(string path)
        {
            this.path = path;


        }

        public void Load()
        {
            if (!File.Exists(path)) return;

            string imgDirPath = BaseToolEditParas.PathImgDir(path);
            nodes.Clear();
            string str = File.ReadAllText(path);
            JArray jArray = JArray.Parse(str);

            foreach (JObject jobject in jArray)
            {
                string toolType = (string)jobject["ToolType"];
                Type type = Type.GetType(toolType);
                if (type == null) throw new Exception($"無法解析ToolType:\n{toolType}");

                IToolEditParas input = Activator.CreateInstance(type, new object[] { nodes }) as IToolEditParas;
                input.loadParas(jobject, imgDirPath);
                nodes.Add(new Nd(input));
            }
        }
        public void Run(bool runException = true)
        {
            foreach (Nd nd in nodes)
            {
                IToolEditParas tool = (IToolEditParas)nd.value;
                if (runException) tool.actionProcess();
                else
                {
                    try { tool.actionProcess(); }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), $"{tool.ToolName} Exception!");
                        break;
                    }
                }
            }


        }
        /// <summary>
        /// when call func. <see cref="Run"/>, <c>Tools</c> need to asign again.
        /// <br/>
        /// <code>
        /// Ex:
        /// var tool = model.Tools;
        /// model.Run();
        /// tool = model.Tools;
        /// </code>
        /// </summary>
        public IToolEditParas[] Tools => nodes.Select(nd => nd.value).Cast<IToolEditParas>().ToArray();

    }
}
