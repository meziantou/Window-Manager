using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WindowManager.Core
{
    public class HotKeyManager : IDisposable
    {
        private readonly List<string> _errors;
        private readonly KeyBoardHookHotKey _hookHotKey;
        private readonly Settings.Settings _settings;
        private readonly ISizeSelection _sizeSelection;

        public HotKeyManager(Settings.Settings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            _errors = new List<string>();
            _settings = settings;

            ISizeSelection exportedValue = GetSizeSelectionInstance();

            _sizeSelection = exportedValue;
            _hookHotKey = new KeyBoardHookHotKey();
            RegisterHotKeys();
        }

        public IEnumerable<string> Errors
        {
            get { return _errors; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Trace.WriteLine("Unregister HotKeys");
            HotKeyFluent.Unregister(_settings.HotKeys.CenterHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.HorizontalCenterHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.VerticalCenterHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.ShowSizeSelectionWindowHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.BottomLeftHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.BottomHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.BottomRightHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.LeftHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.FullScreenHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.RightHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.TopLeftHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.TopHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.TopRightHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.TopMostHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.BottomMostHotKey);
            HotKeyFluent.Unregister(_settings.HotKeys.SwitchScreenHotKey);
            _hookHotKey.Dispose();
        }

        #endregion

        private ISizeSelection GetSizeSelectionInstance()
        {
            string location = Assembly.GetEntryAssembly().Location;
            string path = Path.GetDirectoryName(location);
            Debug.Assert(path != null, "path != null");
            var exeCatalog = new DirectoryCatalog(path, "*.exe");
            var dllCatalog = new DirectoryCatalog(path, "*.dll");
            var catalog = new AggregateCatalog(exeCatalog, dllCatalog);
            var container = new CompositionContainer(catalog);
            container.ComposeExportedValue("Settings", _settings);
            var exportedValue = container.GetExportedValue<ISizeSelection>();
            return exportedValue;
        }

        private void RegisterHotKeys()
        {
            Trace.WriteLine("Register HotKeys");
            RegisterHotKey(_settings.HotKeys.CenterHotKey, SendToCenterHotKeyPressed, _errors);
            RegisterHotKey(_settings.HotKeys.HorizontalCenterHotKey, SendToHorizontalCenterHotKeyPressed, _errors);
            RegisterHotKey(_settings.HotKeys.VerticalCenterHotKey, SendToVerticalCenterHotKeyPressed, _errors);
            RegisterHotKey(_settings.HotKeys.ShowSizeSelectionWindowHotKey, ShowWindowsHotKeyPressed, _errors);
            RegisterHotKey(_settings.HotKeys.BottomLeftHotKey, SendToBottomLeft, _errors);
            RegisterHotKey(_settings.HotKeys.BottomHotKey, SendToBottom, _errors);
            RegisterHotKey(_settings.HotKeys.BottomRightHotKey, SendToBottomRight, _errors);
            RegisterHotKey(_settings.HotKeys.LeftHotKey, SendToLeft, _errors);
            RegisterHotKey(_settings.HotKeys.FullScreenHotKey, SendToFull, _errors);
            RegisterHotKey(_settings.HotKeys.RightHotKey, SendToRight, _errors);
            RegisterHotKey(_settings.HotKeys.TopLeftHotKey, SendToTopLeft, _errors);
            RegisterHotKey(_settings.HotKeys.TopHotKey, SendToTop, _errors);
            RegisterHotKey(_settings.HotKeys.TopRightHotKey, SendToTopRight, _errors);
            RegisterHotKey(_settings.HotKeys.TopMostHotKey, TopMost, _errors);
            RegisterHotKey(_settings.HotKeys.BottomMostHotKey, BottomMost, _errors);
            RegisterHotKey(_settings.HotKeys.SwitchScreenHotKey, SwitchScreen, _errors);
            RegisterHotKey(_settings.HotKeys.MinimizeWindowHotKey, MinimizeWindow, _errors);
            RegisterHotKey(_settings.HotKeys.ExtendBottomHotKey, ExtendBottom, _errors);
            RegisterHotKey(_settings.HotKeys.ExtendTopHotKey, ExtendTop, _errors);
            RegisterHotKey(_settings.HotKeys.ExtendLeftHotKey, ExtendLeft, _errors);
            RegisterHotKey(_settings.HotKeys.ExtendRightHotKey, ExtendRight, _errors);

            RegisterHotKey(_settings.HotKeys.ReduceBottomHotKey, ReduceBottom, _errors);
            RegisterHotKey(_settings.HotKeys.ReduceTopHotKey, ReduceTop, _errors);
            RegisterHotKey(_settings.HotKeys.ReduceLeftHotKey, ReduceLeft, _errors);
            RegisterHotKey(_settings.HotKeys.ReduceRightHotKey, ReduceRight, _errors);
        }

        private void ReduceBottom(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().ReduceBottom();
        }

        private void ReduceTop(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().ReduceTop();
        }

        private void ReduceRight(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().ReduceRight();
        }

        private void ReduceLeft(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().ReduceLeft();
        }

        private void ExtendBottom(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().ExtendBottom();
        }

        private void ExtendTop(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().ExtendTop();
        }

        private void ExtendLeft(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().ExtendLeft();
        }

        private void ExtendRight(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().ExtendRight();
        }

        private void MinimizeWindow(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().MinimizeWindow();
        }

        private void SendToCenterHotKeyPressed(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().SendToCenterHotKeyPressed();
        }

        private void SendToHorizontalCenterHotKeyPressed(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().SendToHorizontalCenterHotKeyPressed();
        }

        private void SendToVerticalCenterHotKeyPressed(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().SendToVerticalCenterHotKeyPressed();
        }

        private void RegisterHotKey(HotKey hotkey, HotKeyPressedEventHandler handler, ICollection<string> errors)
        {
            try
            {
                hotkey.AddHandler(handler);
                hotkey.Register();
            }
            catch (InvalidOperationException)
            {
            }
            catch (HotKeyAleadyInUse e)
            {
                if (_settings.OverrideExistingHotKeys)
                {
                    _hookHotKey.RegisterHotKey(hotkey.ModifierKeys, hotkey.Key, handler);
                }
                else
                {
                    Trace.WriteLine("Error while registering hot key " + hotkey + "\n" + e);
                    errors.Add("Error while registering hot key " + hotkey);
                }
            }
        }

        private void SwitchScreen(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().SwitchScreen();
        }

        private void BottomMost(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().BottomMost();
        }

        private void TopMost(object sender, EventArgs e)
        {
            Win32Window.GetForegroundWindow().TopMost();
        }

        private void SendToBottomLeft(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToBottomLeft();
        }

        private void SendToFull(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToFull(_settings.FullScreenMaximized);
        }

        private void SendToTopLeft(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToTopLeft();
        }

        private void SendToBottomRight(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToBottomRight();
        }

        private void SendToTopRight(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToTopRight();
        }

        private void SendToTop(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToTop();
        }

        private void SendToBottom(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToBottom();
        }

        private void SendToLeft(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToLeft();
        }

        private void SendToRight(object sender, EventArgs eventArgs)
        {
            Win32Window.GetForegroundWindow().SendToRight();
        }

        private void ShowWindowsHotKeyPressed(object sender, EventArgs eventArgs)
        {
            _sizeSelection.Show();
        }
    }
}