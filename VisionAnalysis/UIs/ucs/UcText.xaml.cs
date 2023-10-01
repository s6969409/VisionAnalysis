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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisionAnalysis
{
    /// <summary>
    /// UcText.xaml 的互動邏輯
    /// </summary>
    public partial class UcText : UserControl
    {
        public UcText()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PNameProperty =
        DependencyProperty.Register("PName", typeof(string), typeof(UcText), new PropertyMetadata("PName", onPropertyChanged));
        public static readonly DependencyProperty PValueProperty =
        DependencyProperty.Register("PValue", typeof(object), typeof(UcText), new PropertyMetadata("PValue", onPropertyChanged));

        public string PName
        {
            get => (string)GetValue(PNameProperty);
            set => SetValue(PNameProperty, value);
        }
        public object PValue
        {
            get => GetValue(PValueProperty);
            set{
                SetValue(PValueProperty, value);
                tb.Text = value == null ? "" : value.ToString();
            }
        }
        public event TextChangedEventHandler PValueChanged;
        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            PValue = tb.Text;
            PValueChanged?.Invoke(sender, e);
        }

        private static void onPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj != null)
            {
                UcText uc = obj as UcText;
                if (args.Property.Name.Equals("PName"))
                {
                    uc.lb.Content = args.NewValue;
                }
                else if (args.Property.Name.Equals("PValue"))
                {
                    uc.tb.Text = args.NewValue == null ? "" : args.NewValue.ToString();
                }
            }
        }
    }
}
