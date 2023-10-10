using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public event Action<object> PValueChanged;

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
            set => SetValue(PValueProperty, value);
        }

        private static void onPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj != null)
            {
                UcText uc = obj as UcText;
                if (args.Property.Name.Equals("PName"))
                {
                    uc.label.Content = args.NewValue;
                }
                else if (args.Property.Name.Equals("PValue"))
                {
                    uc.updateUI();
                }
            }
        }

        private void updateUI()
        {
            comboBox.Visibility = PValue is Enum ? Visibility.Visible : Visibility.Hidden;
            textBox.Visibility = PValue is Enum ? Visibility.Hidden : Visibility.Visible;

            if (PValue is Enum)
            {
                comboBox.ItemsSource = Enum.GetValues(PValue.GetType());
                comboBox.SelectedItem = PValue;
            }
            else
            {
                textBox.Text = PValue == null ? "" : PValue.ToString();
            }
        }

        private void textChange(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            PValue = tb.Text;
        }
        private void selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            PValue = cb.SelectedItem;
        }
    }
}
