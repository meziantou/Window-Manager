using System.Windows.Input;

namespace WindowManager
{
    public static class CustomCommands
    {
        private static readonly RoutedUICommand ExitCommand = new RoutedUICommand("Exit", "Exit",
                                                                                  typeof (CustomCommands));

        private static readonly RoutedUICommand ShowWindowCommand = new RoutedUICommand("ShowWindow", "ShowWindow",
                                                                                        typeof (CustomCommands));

        private static readonly RoutedUICommand ExportSettingsCommand = new RoutedUICommand("ExportSettings", "ExportSettings",
                                                                                       typeof(CustomCommands));

        private static readonly RoutedUICommand ImportSettingsCommand = new RoutedUICommand("ImportSettings", "ImportSettings",
                                                                                       typeof(CustomCommands));


        static CustomCommands()
        {
        }

        public static RoutedUICommand Exit => ExitCommand;

        public static RoutedUICommand ShowWindow => ShowWindowCommand;


        public static RoutedUICommand ExportSettings => ExportSettingsCommand;

        public static RoutedUICommand ImportSettings => ImportSettingsCommand;
    }
}