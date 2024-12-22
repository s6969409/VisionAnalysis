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

        private DataView dvCraByBase(IQueryable queryable)
        {
            Type elementType = queryable.ElementType;
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Index", typeof(int));
            dataTable.Columns.Add("Value", elementType);
            int index = 0;
            foreach (var item in queryable)
            {
                dataTable.Rows.Add(index++, item);
            }
            return dataTable.DefaultView;
        }
        private DataView dvCraByStruct(IQueryable queryable)
        {
            Type elementType = queryable.ElementType;
            DataTable dataTable = new DataTable();

            FieldInfo[] colsF = queryable.ElementType.GetFields();
            bool containIndex = colsF.Any(fInfo => fInfo.Name == "Index");
            if (!containIndex) dataTable.Columns.Add("Index", typeof(int));
            foreach (var fInfo in colsF)
            {
                dataTable.Columns.Add(fInfo.Name, fInfo.FieldType);
            }
            int index = 0;
            foreach (var item in queryable)
            {
                var values = item.GetType().GetFields().Select(r => r.GetValue(item)).ToArray();
                if (!containIndex) values = new object[] { index++ }.Concat(values).ToArray();
                dataTable.Rows.Add(values);
            }
            return dataTable.DefaultView;
        }
        public void update(System.Collections.IEnumerable data)
        {
            IQueryable queryable = data.AsQueryable();
            Type elementType = queryable.ElementType;

            if (elementType.IsPrimitive || elementType == typeof(string))
            {
                dg.ItemsSource = dvCraByBase(queryable);
            }
            else
            {
                dg.ItemsSource = dvCraByStruct(queryable);
            }

            dg.IsReadOnly = true;
        }
    }
}
