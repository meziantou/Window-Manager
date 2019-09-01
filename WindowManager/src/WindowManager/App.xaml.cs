using Meziantou.Framework;
using System;
using System.Windows;
using System.Windows.Threading;

namespace WindowManager
{
    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        public const string Name = "WindowManager";
        private SingleInstance _singleInstance;

        protected override void OnStartup(StartupEventArgs e)
        {
            _singleInstance = new SingleInstance(Guid.Parse("{ff1b2b33-9a71-4e8d-9ace-dec72894c4d7}"));
            if (!_singleInstance.StartApplication())
            {
                _singleInstance.NotifyFirstInstance(e.Args);
                Environment.Exit(0);
                return;
            }

            _singleInstance.NewInstance += SingleInstance_NewInstance;
            base.OnStartup(e);
        }

        private static void SingleInstance_NewInstance(object sender, SingleInstanceEventArgs e)
        {
            Current.Dispatcher.BeginInvoke((Action)SetVisibility, DispatcherPriority.Render);

            static void SetVisibility()
            {
                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Visibility = Visibility.Visible;
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _singleInstance?.Dispose();
            base.OnExit(e);
        }
    }
}