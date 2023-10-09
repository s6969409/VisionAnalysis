using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VisionAnalysis
{
    /// <summary>
    /// WindowPreference.xaml 的互動邏輯
    /// </summary>
    public partial class WindowPreference : Window
    {
        Node node;

        public WindowPreference()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            JObject jObject = readCfg();
            JToken jToken = jObject.GetValue(fontSize);
            var v2 = jToken.Type;//JTokenType.Integer;

            node = new Node();
            foreach (JProperty jProperty in jObject.Properties())
            {
                Node nd = new Node()
                {
                    name = jProperty.Name,
                    valueType = jProperty.Value.Type,
                    value = jProperty.Value
                };
                node.childNodes.Add(nd);
            }

            tv.ItemsSource = node.childNodes;
        }

        #region preference interfacce
        public static readonly string Preference = $@"{Directory.GetCurrentDirectory()}\preference.cfg";
        #region define propertyNames
        public static readonly string fontSize = nameof(fontSize);
        public static readonly string runException = nameof(runException);
        public static readonly string language = nameof(language);
        public enum Languages { Default, zhCn, enUs }

        #endregion

        private static void saveCfg(JObject jObject)
        {
            File.Delete(Preference);
            File.WriteAllText(Preference, jObject.ToString());
        }
        private static JObject readCfg()
        {
            try
            {
                return JObject.Parse(File.ReadAllText(Preference));
            }
            catch
            {
                return new JObject();
            }
        }
        public static void saveCfgValue(string propertyName, object o)
        {
            JObject jObject = readCfg();
            JToken value = JToken.FromObject(o);
            if (jObject.ContainsKey(propertyName))
            {
                jObject[propertyName] = value;
            }
            else
            {
                jObject.Add(propertyName, value);
            }
            saveCfg(jObject);
        }
        public static T getCfgValue<T>(string propertyName)
        {
            JObject jObject = readCfg();
            if (jObject.ContainsKey(propertyName))
            {
                return jObject[propertyName].ToObject<T>();
            }
            return default(T);
        }

        public static bool isUserDuke
        {
            get
            {
                return Environment.MachineName.Equals("YJECNB-HSIEN");
            }
        }

        #endregion

        private class Node
        {
            public string name { get; set; }
            private object _value;
            public object value
            {
                get { return _value; }
                set
                {
                    if (valueType == JTokenType.Integer)
                    {
                        try
                        {
                            _value = int.Parse(value.ToString());
                            saveCfgValue(name, _value);
                        }
                        catch (FormatException) { }
                    }
                    else if (valueType == JTokenType.Boolean)
                    {
                        _value = bool.Parse(value.ToString());
                        saveCfgValue(name, _value);
                    }
                    else //if (valueType == JTokenType.String)
                    {
                        _value = value;
                        saveCfgValue(name, value);
                    }

                }
            }
            public JTokenType valueType { get; set; }
            public List<Node> childNodes { get; set; } = new List<Node>();
        }

        private void ContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            ContentControl contentControl = sender as ContentControl;
            Node nd = contentControl.DataContext as Node;
            Binding binding = new Binding("value");
            if (nd.valueType == JTokenType.Boolean)
            {
                CheckBox checkBox = new CheckBox() { DataContext = nd };
                checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);
                contentControl.Content = checkBox;
            }
            else
            {
                TextBox textBox = new TextBox() { DataContext = nd, MaxWidth = 400 };
                textBox.SetBinding(TextBox.TextProperty, binding);
                contentControl.Content = textBox;
            }
        }
    }
}
