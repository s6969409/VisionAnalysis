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
        }

        public void update(object obj)
        {
            node.value = obj;
            node.childNodes.Clear();
            buildObjStruct(node, obj);
            if (tv.ItemsSource == null) tv.ItemsSource = node.childNodes;
        }
        private void buildObjStruct(Node node, object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            if (!(obj is string) && properties.Length > 0)
            {
                foreach (var p in properties)
                {
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            update(new { a = 10, bb="AA",m = new { NN=100} });
            tv.ItemsSource = node.childNodes;
        }
    }
}
