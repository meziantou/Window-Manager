using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Xml.Serialization;
using WindowManager.Core.NativeMethods;

namespace WindowManager.Core
{
    public sealed class HotKey : IDisposable, ICloneable, INotifyPropertyChanged
    {
        private readonly int _id;
        public event HotKeyPressedEventHandler HotKeyPressed;
        private IntPtr _handle;
        private bool _isKeyRegistered;
        private Keys _key;
        private ModifierKeys _modifierKeys;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if ((ModifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
                sb.Append("Ctrl+");
            if ((ModifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
                sb.Append("Alt+");
            if ((ModifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
                sb.Append("Shift+");
            if ((ModifierKeys & ModifierKeys.Windows) == ModifierKeys.Windows)
                sb.Append("Windows+");

            sb.Append((string) Key.ToString());
            return sb.ToString();
        }

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
            _id = GetHashCode();
            _handle = windowHandle;
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        public Keys Key
        {
            get { return _key; }
            set
            {
                if (_key == value)
                    return;

                _key = value;
                OnPropertyChanged("Key");
                if (!_isKeyRegistered)
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
                OnPropertyChanged("ModifierKeys");

                if (!_isKeyRegistered)
                    return;
                RegisterHotKey();
            }
        }

        public bool IsRegistered
        {
            get { return _isKeyRegistered; }
        }

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
                    _handle = value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
            UnregisterHotKey();
        }

        #endregion

        ~HotKey()
        {
            Dispose();
        }

        public object Clone()
        {
            HotKey hotKey = new HotKey();
            this.CopyTo(hotKey);
            return hotKey;
        }

        public void RegisterHotKey()
        {
            if (Key == Keys.None)
                throw new InvalidOperationException("Key must be set.");
            if (_isKeyRegistered)
                UnregisterHotKey();
            _isKeyRegistered = User32.RegisterHotKey(Handle, _id, ModifierKeys, Key);
            if (!_isKeyRegistered)
                throw new HotKeyAleadyInUse("Hotkey already in use.");
        }

        public void ReassignHotKey(ModifierKeys modifierKeys, Keys key)
        {
            if (key == Keys.None)
                throw new InvalidOperationException("Key must be set.");
            if (_isKeyRegistered)
                UnregisterHotKey();
            ModifierKeys = modifierKeys;
            Key = key;
            RegisterHotKey();
        }

        public void UnregisterHotKey()
        {
            _isKeyRegistered = !User32.UnregisterHotKey(Handle, _id);
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (handled || (msg.message != User32.WmHotKey || (int)msg.wParam != _id))
                return;
            OnHotKeyPressed();
            handled = true;
        }

        private void OnHotKeyPressed()
        {
            HotKeyPressedEventHandler pressedEventHandler = HotKeyPressed;
            if (pressedEventHandler == null)
                return;
            pressedEventHandler(this, new EventArgs());
        }

        public void CopyTo(HotKey hotKey)
        {
            hotKey.Key = Key;
            hotKey.ModifierKeys = ModifierKeys;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HotKeyAleadyInUse : Exception
    {
        public HotKeyAleadyInUse(string message) : base(message)
        {
        }

        public HotKeyAleadyInUse(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HotKeyAleadyInUse(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HotKeyAleadyInUse()
        {
        }
    }
}