using System.Windows;
using System.Windows.Controls;
using WindowManager.Core;

namespace WindowManager.Controls
{
    public sealed class HotKeyControl : Control
    {
        static HotKeyControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HotKeyControl), new FrameworkPropertyMetadata(typeof(HotKeyControl)));
        }

        public static readonly DependencyProperty HotKeyProperty =
            DependencyProperty.Register("HotKey", typeof(HotKey), typeof(HotKeyControl), new PropertyMetadata(default(HotKey)));

        public HotKey HotKey
        {
            get { return (HotKey)GetValue(HotKeyProperty); }
            set { SetValue(HotKeyProperty, value); }
        }
    }
}
