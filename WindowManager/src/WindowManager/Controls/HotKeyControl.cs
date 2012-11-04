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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowManager.Core;

namespace WindowManager.Controls
{
    public class HotKeyControl : Control
    {
        static HotKeyControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HotKeyControl), new FrameworkPropertyMetadata(typeof(HotKeyControl)));
        }

        public static readonly DependencyProperty HotKeyProperty =
            DependencyProperty.Register("HotKey", typeof (HotKey), typeof (HotKeyControl), new PropertyMetadata(default(HotKey)));

        public HotKey HotKey
        {
            get { return (HotKey) GetValue(HotKeyProperty); }
            set { SetValue(HotKeyProperty, value); }
        }
    }
}
