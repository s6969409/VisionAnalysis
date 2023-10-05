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

            initUI();
        }

        private void initUI()
        {
            textBox.TextChanged += (sender, e) =>
            {
                TextBox tb = sender as TextBox;
                PValue = tb.Text;
                PValueChanged?.Invoke(PValue);
            };
            comboBox.SelectionChanged += (sender, e) =>
            {
                ComboBox cb = sender as ComboBox;
                PValue = cb.SelectedItem;
                PValueChanged?.Invoke(PValue);
            };
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
            set{
                SetValue(PValueProperty, value);
                //tb.Text = value == null ? "" : value.ToString();
                //做更換UI
                //先觀察執行順序 2
                updateUI();
            }
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
                    //uc.tb.Text = args.NewValue == null ? "" : args.NewValue.ToString();
                    //先觀察執行順序 1
                    uc.updateUI();
                    //if(uc.comboBox.Visibility == Visibility.Visible)
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
    }
}
