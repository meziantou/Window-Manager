using System.Windows.Input;

namespace WindowManager
{
    public static class CustomCommands
    {
        public static RoutedUICommand Exit { get; } = new RoutedUICommand("Exit", "Exit", typeof(CustomCommands));
        public static RoutedUICommand ShowWindow { get; } = new RoutedUICommand("ShowWindow", "ShowWindow", typeof(CustomCommands));
    }
}