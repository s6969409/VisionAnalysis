using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisionAnalysis
{
    /// <summary>
    /// UcTree.xaml 的互動邏輯
    /// </summary>
    public partial class UcTree : UserControl
    {
        private Node node = new Node() { name = "root" };
        public UcTree()
        {
            InitializeComponent();
            tv.ItemsSource = node.childNodes;
        }

        public void update(object obj)
        {
            node.value = obj;
            node.childNodes.Clear();
            if (obj.GetType() == typeof(string) || obj.GetType() == typeof(int) || obj.GetType() == typeof(double) || obj.GetType().IsArray)
            {
                Node newNd = new Node() { name = "", value = obj };
                node.childNodes.Add(newNd);
            }
            buildObjStruct(node, obj);
        }
        private void buildObjStruct(Node node, object obj)
        {
            FieldInfo[] fields = obj.GetType().GetFields();
            if (!(obj is string) && fields.Length > 0)
            {
                foreach (var p in fields)
                {
                    if (!p.IsPublic || p.IsStatic) continue;
                    object val = p.GetValue(obj);
                    Node newNd = new Node() { name = p.Name, value = val };
                    node.childNodes.Add(newNd);
                    buildObjStruct(newNd, val);
                }
            }
        }

        private class Node
        {
            public string name { get; set; }
            public object value { get; set; }
            public ObservableRangeCollection<Node> childNodes { get; } = new ObservableRangeCollection<Node>();
        }
    }
}
