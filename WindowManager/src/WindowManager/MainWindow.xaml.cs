using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;
using Hardcodet.Wpf.TaskbarNotification;
using System.Linq;
using WindowManager.Core;
using WindowManager.Core.Settings;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using WindowCollection = WindowManager.Core.WindowCollection;

namespace WindowManager
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private HotKeyManager _hotKeyManager;
        private bool _exit;
        private Settings _settings;


        public MainWindow()
        {
            InitializeComponent();
            ResetSettings();

            var commandLine = new CommandLine();
            if (!commandLine.ShowMainWindow && !_settings.FirstStart)
            {
                Hide();
            }

            this.Loaded += new RoutedEventHandler(MainWindowLoaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            WindowCollection.Instance.Dispose();
        }


        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr intPtr = new WindowInteropHelper(this).Handle;
            WindowCollection.Initialize(intPtr);
        }




        //if (msg == (int)User32.WindowMessage.WM_SYSCOMMAND)
        //{
        //    // Execute the appropriate code for the System Menu item that was clicked
        //    int int32 = wParam.ToInt32();
        //    if (int32 == DefaultTransparencyMenuId)
        //    {
        //        ClearTransparency();
        //    }
        //    else if (int32 > TransparencyPercentId && int32 <= TransparencyPercentId + 100)
        //    {
        //        double percent = (int32 - TransparencyPercentId) / 100d;
        //        byte transparency = (byte)(byte.MaxValue * percent);
        //        SetTransparency(transparency);
        //    }
        //}

        //return IntPtr.Zero;


        private void SettingsChanged()
        {
            if (_hotKeyManager != null)
            {
                _hotKeyManager.Dispose();
                _hotKeyManager = null;
            }

            Settings s = _settings.Clone();
            _hotKeyManager = new HotKeyManager(s);
            if (_hotKeyManager.Errors.Any())
            {
                MessageBox.Show(_hotKeyManager.Errors.Aggregate((buff, a) => buff + "\n" + a), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ResetSettings()
        {
            try
            {
                _settings = Settings.Load();
            }
            catch (Exception)
            {
                _settings = Settings.GetDefault();
                _settings.Save();
            }

            _settingsGrid.DataContext = _settings;
            if (_hotKeyManager != null)
            {
                _hotKeyManager.Dispose();
                _hotKeyManager = null;
            }

            Settings s = _settings.Clone();
            _hotKeyManager = new HotKeyManager(s);
            if (_hotKeyManager.Errors.Any())
            {
                MessageBox.Show(_hotKeyManager.Errors.Aggregate((buff, a) => buff + "\n" + a), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }



            _hotKeysComboBox.ItemsSource = Settings.GetHotKeysNames();
            _hotKeysComboBox.SelectedItem = _settings.CurrentHotKeys;


            _themeComboBox.ItemsSource = Settings.GetThemeNames();
            _themeComboBox.SelectedItem = _settings.CurrentTheme;


        }

        private static string GetVersion()
        {
            return ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : "Dev";
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

        private void CheckForUpdate(object sender, RoutedEventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                UpdateCheckInfo detailedUpdate = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate();
                if (detailedUpdate.UpdateAvailable)
                {
                    MessageBoxResult result = MessageBox.Show(
                        this,
                        string.Format("Version {0} is available.\nWould you like to install it ?", detailedUpdate.AvailableVersion),
                        "Check For Update",
                        MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK)
                    {
                        if (ApplicationDeployment.CurrentDeployment.Update())
                        {
                            App.Restart();
                        }
                    }
                }
                else
                {
                    MessageBox.Show(this, "Your version is up-to-date", "Check For Update");
                }
            }
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
            _settings = Settings.GetDefault();
            _settings.FirstStart = firstStart;
            _settings.Save();
            ResetSettings();
        }

        private void RibbonWindowStateChanged(object sender, EventArgs e)
        {
            if (_settings.MinimizeToTrayBar && this.WindowState == WindowState.Minimized)
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

        private void ExportSettingsMenuItemClick(object sender, ExecutedRoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog();
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = ".xml";
            fileDialog.OverwritePrompt = true;
            fileDialog.Filter = "WindowManager Profile (.xml)|*.xml";
            var showDialog = fileDialog.ShowDialog();
            if (showDialog == true)
            {
                try
                {
                    _settings.Export(fileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while exporting Profile.\n" + ex.Message, "Error");
                }
            }
        }

        private void ImportSettingsMenuItemClick(object sender, ExecutedRoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = ".xml";
            fileDialog.Filter = "WindowManager Profile (.xml)|*.xml";
            var showDialog = fileDialog.ShowDialog();
            if (showDialog == true)
            {
                try
                {
                    Settings.Import(fileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while importing Profile.\n" + ex.Message, "Error");
                }
            }
        }

        private void HotKeysChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var selectedItem = _hotKeysComboBox.SelectedItem;
            if (selectedItem == null)
                return;
            var name = selectedItem.ToString();
            if (_settings.CurrentHotKeys == name)
                return;

            _settings.SetHotKeys(name);
            SettingsChanged();
        }

        private void ThemeChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var selectedItem = _themeComboBox.SelectedItem;
            if (selectedItem == null)
                return;
            var name = selectedItem.ToString();
            if (_settings.CurrentTheme == name)
                return;

            _settings.SetTheme(name);
            SettingsChanged();
        }

        private void CreateHotkeys(object sender, RoutedEventArgs e)
        {
            var input = new NameInputWindow();
            var showDialog = input.ShowDialog();
            if (showDialog == true)
            {
                _settings.CreateHotKeys(input.Value);
                _settings.Save();
                ResetSettings();
            }

        }

        private void DeleteHotKeys(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(string.Format("Do you want to delete {0} hot keys ?", _settings.CurrentHotKeys), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;

            _settings.DeleteHotKeys(_settings.CurrentHotKeys);
            _settings.Save();
            ResetSettings();
        }

        private void DeleteTheme(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(string.Format("Do you want to delete {0} theme ?", _settings.CurrentTheme), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;

            _settings.DeleteTheme(_settings.CurrentTheme);
            _settings.Save();
            ResetSettings();
        }

        private void CreateTheme(object sender, RoutedEventArgs e)
        {
            var input = new NameInputWindow();
            var showDialog = input.ShowDialog();
            if (showDialog == true)
            {
                _settings.CreateTheme(input.Value);
                _settings.Save();
                ResetSettings();
            }
        }
    }
}