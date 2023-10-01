using System;
using System.Collections.Generic;
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
    /// WindowToolBox.xaml 的互動邏輯
    /// </summary>
    public partial class WindowToolBox : Window
    {
        private Action<Type> addTool;
        ObservableRangeCollection<ToolBoxShow> tools = new ObservableRangeCollection<ToolBoxShow>();

        public WindowToolBox(Action<Type> addTool)
        {
            InitializeComponent();
            this.addTool = addTool;
        }

        private void tv_Loaded(object sender, RoutedEventArgs e)
        {
            #region ToolThresHold 重想一下結構
            ToolBoxShow ToolThresHold = new ToolBoxShow(typeof(UcParaThresHold));
            tools.Add(ToolThresHold);
            #endregion

            tv.ItemsSource = tools;
        }

        private void tv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ToolBoxShow selectTool = tv.SelectedItem as ToolBoxShow;
            Type type = Type.GetType(selectTool.name);
            addTool?.Invoke(type);
        }
        //用itemView的onMouseDown與Up事件做拖拉

        private class ToolBoxShow : IUITreeViewItem
        {
            public ObservableRangeCollection<IUITreeViewItem> childNodes { get; } = new ObservableRangeCollection<IUITreeViewItem>();
            public string name { get; set; }
            public bool isExpanded { get; set; } = true;
            public bool CanExpand => childNodes.Count != 0;

            public ToolBoxShow(Type type)
            {
                name = type.FullName;
            }
        }
    }
}
