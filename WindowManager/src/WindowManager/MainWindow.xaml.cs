using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using Hardcodet.Wpf.TaskbarNotification;
using System.Linq;
using WindowManager.Core;
using WindowManager.Core.Settings;
using MessageBox = System.Windows.MessageBox;

namespace WindowManager
{
    public sealed partial class MainWindow
    {
        private HotKeyManager _hotKeyManager;
        private bool _exit;
        private WindowManagerSettings _settings;

        public MainWindow()
        {
            InitializeComponent();
            ResetSettings();

            Loaded += new RoutedEventHandler(MainWindowLoaded);
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (!_settings.FirstStart)
            {
                Visibility = Visibility.Collapsed;
            }
        }

        private void ResetSettings()
        {
            try
            {
                _settings = WindowManagerSettings.Load();
            }
            catch (Exception)
            {
                _settings = WindowManagerSettings.GetDefault();
                _settings.Save();
            }

            _settingsGrid.DataContext = _settings;
            if (_hotKeyManager != null)
            {
                _hotKeyManager.Dispose();
                _hotKeyManager = null;
            }

            WindowManagerSettings s = _settings.Clone();
            _hotKeyManager = new HotKeyManager(s);
            if (_hotKeyManager.Errors.Any())
            {
                MessageBox.Show(_hotKeyManager.Errors.Aggregate((buff, a) => buff + "\n" + a), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _hotKeyManager.Dispose();
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_exit)
            {
                e.Cancel = true;
                HideWindow();
            }

            base.OnClosing(e);
        }

        private void HideWindow()
        {
            Hide();
            if (_settings.FirstStart)
            {
                _settings.FirstStart = false;
                _settings.Save();
                _taskBarIcon.ShowBalloonTip("Window Manager",
                                            "Window Manager is still opened.\nDouble click on the icon to open it.",
                                            BalloonIcon.Info);
            }
        }

        private void ShowWindowMenuItemClick(object sender, RoutedEventArgs e)
        {
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void CloseWindowMenuItemClick(object sender, RoutedEventArgs e)
        {
            _exit = true;
            Close();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            _settings.Save();
            ResetSettings();
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }

        private void DefaultSettingsClick(object sender, RoutedEventArgs e)
        {
            var firstStart = _settings.FirstStart;
            _settings = WindowManagerSettings.GetDefault();
            _settings.FirstStart = firstStart;
            _settings.Save();
            ResetSettings();
        }

        private void RibbonWindowStateChanged(object sender, EventArgs e)
        {
            if (_settings.MinimizeToTrayBar && WindowState == WindowState.Minimized)
            {
                HideWindow();
            }
        }

        private void HandleRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var navigateUri = ((Hyperlink)sender).NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }
    }
}