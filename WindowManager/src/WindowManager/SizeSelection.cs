using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using WindowManager.Core;
using WindowManager.Core.Settings;

namespace WindowManager
{
    [Export(typeof(ISizeSelection))]
    public class SizeSelection : ISizeSelection
    {
        private readonly IList<SizeSelectionWindow> _screens = new List<SizeSelectionWindow>();
        private readonly Settings _settings;

        [ImportingConstructor]
        public SizeSelection([Import("Settings")]Settings settings)
        {
            _settings = settings;
        }

        public void Show()
        {
            Trace.WriteLine("Show SizeSelectionWindow HotKey");
            if (_screens.Count != 0)
            {
                Trace.WriteLine("SizeSelectionWindows already opened");
                return;
            }
            foreach (Screen screen in Screen.AllScreens)
            {
                SizeSelectionWindow window = new SizeSelectionWindow(screen, _settings.Theme.NbItemsPerRow,
                                                                     _settings.Theme.NbItemsPerColumn, _settings);
                window.Show();
                _screens.Add(window);
            }

            foreach (SizeSelectionWindow sizeSelectionWindow in _screens)
            {
                sizeSelectionWindow.Deactivated += WindowDeactivated;
                sizeSelectionWindow.SizeSelected += WindowSizeSelected;
                sizeSelectionWindow.Closed += WindowSizeClosed;
            }
        }

        private void WindowSizeClosed(object sender, EventArgs e)
        {
            Trace.WriteLine("SizeSelectionWindow Closed");
            SizeSelectionWindow window = (SizeSelectionWindow)sender;
            UnregisterEvent(window);
            _screens.Remove(window);
            CloseAllWindow();
        }

        private void WindowSizeSelected(object sender, SizeSelectedEventArgs e)
        {
            Trace.WriteLine(string.Format("Size Selected: x={0}, y={1}, width={2}, height={3}", e.Rectangle.X, e.Rectangle.Y, e.Rectangle.Width, e.Rectangle.Height));
            CloseAllWindow();
            Win32Window window = Win32Window.GetForegroundWindow();
            window.Rectangle = e.Rectangle;

            if (!e.Maximized || !_settings.FullScreenMaximized)
                window.WindowState = WindowState.Normal;
            else
                window.WindowState = WindowState.Maximized;
        }

        private void WindowDeactivated(object sender, EventArgs e)
        {
            WpfHelpers.DoEvents();
            if (_screens.Any(v => v.IsActive))
                return;
            CloseAllWindow();
        }


        private void CloseAllWindow()
        {
            foreach (SizeSelectionWindow sizeSelectionWindow in _screens)
            {
                UnregisterEvent(sizeSelectionWindow);
                try
                {
                    sizeSelectionWindow.Close();
                }
                catch (InvalidOperationException)
                {
                }
            }
            _screens.Clear();
        }

        private void UnregisterEvent(SizeSelectionWindow sizeSelectionWindow)
        {
            sizeSelectionWindow.Deactivated -= WindowDeactivated;
            sizeSelectionWindow.SizeSelected -= WindowSizeSelected;
            sizeSelectionWindow.Closed -= WindowSizeClosed;
        }
    }
}