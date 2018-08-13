using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WindowManager
{
    /// <summary>
    /// Interaction logic for NameInputWindow.xaml
    /// </summary>
    public partial class NameInputWindow : Window
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

            this.Value = value;
            this.DialogResult = true;
            this.Close();
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
