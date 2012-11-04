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

        public static RoutedUICommand Exit
        {
            get { return ExitCommand; }
        }

        public static RoutedUICommand ShowWindow
        {
            get { return ShowWindowCommand; }
        }


        public static RoutedUICommand ExportSettings
        {
            get { return ExportSettingsCommand; }
        }

        public static RoutedUICommand ImportSettings
        {
            get { return ImportSettingsCommand; }
        }
    }
}