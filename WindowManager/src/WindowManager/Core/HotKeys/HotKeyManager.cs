using System;
using System.Collections.Generic;
using System.Diagnostics;
using WindowManager.Core.Settings;

namespace WindowManager.Core
{
    public sealed class HotKeyManager : IDisposable
    {
        private readonly List<string> _errors;
        private readonly KeyBoardHookHotKey _hookHotKey;
        private readonly WindowManagerSettings _settings;
        private readonly SizeSelection _sizeSelection;

        public HotKeyManager(WindowManagerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _errors = new List<string>();

            _sizeSelection = new SizeSelection(_settings);
            _hookHotKey = new KeyBoardHookHotKey();
            RegisterHotKeys();
        }

        public IEnumerable<string> Errors => _errors;

        public void Dispose()
        {
            foreach (var hotkey in _settings.HotKeys.All())
            {
                if (hotkey != null)
                {
                    hotkey.UnregisterHotKey();
                }
            }

            _hookHotKey.Dispose();
        }

        private void RegisterHotKeys()
        {
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
            RegisterHotKey(_settings.HotKeys.IncreaseTransparencyHotKey, IncreaseTransparency, _errors);
            RegisterHotKey(_settings.HotKeys.DecreaseTransparencyHotKey, DecreaseTransparency, _errors);
        }

        private static void IncreaseTransparency(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.IncreaseTransparency();
        private static void DecreaseTransparency(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.DecreaseTransparency();
        private static void ReduceBottom(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.ReduceBottom();
        private static void ReduceTop(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.ReduceTop();
        private static void ReduceRight(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.ReduceRight();
        private static void ReduceLeft(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.ReduceLeft();
        private static void ExtendBottom(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.ExtendBottom();
        private static void ExtendTop(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.ExtendTop();
        private static void ExtendLeft(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.ExtendLeft();
        private static void ExtendRight(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.ExtendRight();
        private static void MinimizeWindow(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.MinimizeWindow();
        private static void SendToCenterHotKeyPressed(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.SendToCenterHotKeyPressed();
        private static void SendToHorizontalCenterHotKeyPressed(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.SendToHorizontalCenterHotKeyPressed();
        private static void SendToVerticalCenterHotKeyPressed(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.SendToVerticalCenterHotKeyPressed();
        private static void SwitchScreen(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.SwitchScreen();
        private static void BottomMost(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.BottomMost();
        private static void TopMost(object sender, EventArgs e) => Win32Window.GetForegroundWindow()?.SwitchIsTopMost();
        private static void SendToBottomLeft(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToBottomLeft();
        private void SendToFull(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToFull(_settings.FullScreenMaximized);
        private static void SendToTopLeft(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToTopLeft();
        private static void SendToBottomRight(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToBottomRight();
        private static void SendToTopRight(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToTopRight();
        private static void SendToTop(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToTop();
        private static void SendToBottom(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToBottom();
        private static void SendToLeft(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToLeft();
        private static void SendToRight(object sender, EventArgs eventArgs) => Win32Window.GetForegroundWindow()?.SendToRight();

        private void RegisterHotKey(HotKey hotkey, HotKeyPressedEventHandler handler, ICollection<string> errors)
        {
            try
            {
                if (hotkey != null)
                {
                    hotkey.HotKeyPressed += handler;
                    hotkey.RegisterHotKey();
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (HotKeyAleadyInUseException e)
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

        private void ShowWindowsHotKeyPressed(object sender, EventArgs eventArgs)
        {
            _sizeSelection.Show();
        }
    }
}