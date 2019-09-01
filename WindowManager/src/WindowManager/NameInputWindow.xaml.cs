using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WindowManager
{
    /// <summary>
    /// Interaction logic for NameInputWindow.xaml
    /// </summary>
    public sealed partial class NameInputWindow : Window
    {
        public string Value { get; set; }

        public NameInputWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var value = textBox.Text.Trim();
            if (string.IsNullOrEmpty(value))
            {
                textBox.BorderBrush = Brushes.Red;
                return;
            }

            Value = value;
            DialogResult = true;
            Close();
        }

        private void NameInputWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            textBox.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Escape)
                Close();
        }
    }
}
