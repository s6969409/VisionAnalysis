using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
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
    /// UcTable.xaml 的互動邏輯
    /// </summary>
    public partial class UcTable : UserControl
    {
        public UcTable()
        {
            InitializeComponent();
        }

        public void update(System.Collections.IEnumerable data)
        {
            IQueryable queryable = data.AsQueryable();
            FieldInfo[] colsF = queryable.ElementType.GetFields();
            DataTable dataTable = new DataTable();
            foreach (FieldInfo fInfo in colsF)
            {
                dataTable.Columns.Add(fInfo.Name, fInfo.FieldType);
            }
            foreach (var item in queryable)
            {
                var args = item.GetType().GetFields().Select(r=>r.GetValue(item)).ToArray();
                dataTable.Rows.Add(args);
            }
            dg.IsReadOnly = true;
            dg.ItemsSource = dataTable.DefaultView;
        }
    }
}
