using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Xml.Serialization;
using WindowManager.Core.NativeMethods;

namespace WindowManager.Core
{
    public sealed class HotKey : IDisposable
    {
        private static int _lastHotKeyId = 0;

        private readonly int _id;
        private IntPtr _handle;
        private Keys _key;
        private ModifierKeys _modifierKeys;

        public event HotKeyPressedEventHandler HotKeyPressed;

        public HotKey()
            : this(ModifierKeys.None, Keys.None)
        {
        }

        public HotKey(ModifierKeys modifierKeys, Keys key)
            : this(modifierKeys, key, System.Windows.Application.Current.MainWindow)
        {
        }

        public HotKey(ModifierKeys modifierKeys, Keys key, Window window)
            : this(modifierKeys, key, new WindowInteropHelper(window))
        {
        }

        public HotKey(ModifierKeys modifierKeys, Keys key, WindowInteropHelper window)
            : this(modifierKeys, key, window.Handle)
        {
        }

        public HotKey(ModifierKeys modifierKeys, Keys key, IntPtr windowHandle)
        {
            Key = key;
            ModifierKeys = modifierKeys;
            _id = Interlocked.Increment(ref _lastHotKeyId);
            _handle = windowHandle;
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        public static HotKey None()
        {
            return new HotKey(ModifierKeys.None, Keys.None);
        }

        public Keys Key
        {
            get { return _key; }
            set
            {
                if (_key == value)
                    return;

                _key = value;
                if (!IsRegistered)
                    return;
                RegisterHotKey();
            }
        }

        public ModifierKeys ModifierKeys
        {
            get { return _modifierKeys; }
            set
            {
                if (_modifierKeys == value)
                    return;

                _modifierKeys = value;

                if (!IsRegistered)
                    return;
                RegisterHotKey();
            }
        }

        [XmlIgnore]
        public bool IsRegistered { get; private set; }

        [XmlIgnore]
        public IntPtr Handle
        {
            get { return _handle; }
            set
            {
                if (IsRegistered)
                {
                    UnregisterHotKey();
                    _handle = value;
                    RegisterHotKey();
                }
                else
                {
                    _handle = value;
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if ((ModifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
            {
                sb.Append("Ctrl+");
            }

            if ((ModifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                sb.Append("Alt+");
            }

            if ((ModifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                sb.Append("Shift+");
            }

            if ((ModifierKeys & ModifierKeys.Windows) == ModifierKeys.Windows)
            {
                sb.Append("Windows+");
            }

            sb.Append(Key);
            return sb.ToString();
        }

        public void Dispose()
        {
            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
            UnregisterHotKey();
        }

        public void RegisterHotKey()
        {
            if (Key == Keys.None)
                throw new InvalidOperationException("Key must be set.");

            if (IsRegistered)
            {
                UnregisterHotKey();
            }

            IsRegistered = User32.RegisterHotKey(Handle, _id, ModifierKeys, Key);
            if (!IsRegistered)
                throw new HotKeyAleadyInUseException("Hotkey already in use.");
        }

        public void UnregisterHotKey()
        {
            IsRegistered = !User32.UnregisterHotKey(Handle, _id);
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (handled || msg.message != User32.WmHotKey || (int)msg.wParam != _id)
                return;

            OnHotKeyPressed();
            handled = true;
        }

        private void OnHotKeyPressed()
        {
            HotKeyPressedEventHandler pressedEventHandler = HotKeyPressed;
            if (pressedEventHandler == null)
                return;

            pressedEventHandler(this, EventArgs.Empty);
        }
    }
}