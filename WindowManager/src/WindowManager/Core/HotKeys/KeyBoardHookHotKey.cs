using Meziantou.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;

namespace WindowManager.Core
{
    internal sealed class KeyBoardHookHotKey : IDisposable
    {
        private sealed class HotKey : IEquatable<HotKey>, IEquatable<ICollection<Keys>>
        {
            private readonly IList<Keys> _keys = new List<Keys>();

            public HotKey(ModifierKeys modifierKeys, Keys keys)
            {
                ModifierKeys = modifierKeys;
                Keys = keys;

                if ((modifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
                    _keys.Add(Keys.Menu);
                if ((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
                    _keys.Add(Keys.Control);
                if ((modifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
                    _keys.Add(Keys.Shift);
                if ((modifierKeys & ModifierKeys.Windows) == ModifierKeys.Windows)
                    _keys.Add(Keys.LWin);

                _keys.Add(keys);
            }

            private static void PrintCollection<T>(IEnumerable<T> collection)
            {
                foreach (T element in collection)
                {
                    Trace.Write(element.ToString() + "+");
                }

                Trace.WriteLine("");
            }

            public bool Equals(ICollection<Keys> other)
            {
                Trace.Write("HotKey: "); PrintCollection(_keys);
                Trace.Write("Pressed keys: "); PrintCollection(other);


                var skip = 0;
                foreach (Keys keys in other)
                {
                    if (keys == Keys.LMenu || keys == Keys.RMenu)
                    {
                        skip++;
                        continue;
                    }

                    if (!_keys.Contains(keys))
                        return false;
                }

                var @equals = _keys.Count == (other.Count - skip);
                return @equals;
            }

            public override string ToString()
            {
                return string.Format("ModifierKeys: {0}, Keys: {1}", ModifierKeys, Keys);
            }

            public ModifierKeys ModifierKeys { get; set; }
            public Keys Keys { get; set; }
            public HotKeyPressedEventHandler Action { get; set; }

            public bool Equals(HotKey other)
            {
                if (other is null)
                    return false;

                return Equals(other.ModifierKeys, ModifierKeys) && Equals(other.Keys, Keys);
            }

            public override bool Equals(object obj)
            {
                return obj is HotKey hotKey ? Equals(hotKey) : false;
            }

            public override int GetHashCode()
            {
                var hashCode = new HashCodeCombiner();
                hashCode.Add(ModifierKeys);
                hashCode.Add(Keys);
                return hashCode;
            }

            public static bool operator ==(HotKey left, HotKey right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(HotKey left, HotKey right)
            {
                return !Equals(left, right);
            }
        }

        private readonly KeyboardHook _hook;
        private readonly IList<HotKey> _hotKeys = new List<HotKey>();

        public KeyBoardHookHotKey()
        {
            _hook = new KeyboardHook();
            _hook.KeyDown += new System.Windows.Forms.KeyEventHandler(HookKeyDown);
        }

        public void RegisterHotKey(ModifierKeys modifierKeys, Keys key, HotKeyPressedEventHandler action)
        {
            var hotKey = new HotKey(modifierKeys, key)
            {
                Action = action,
            };

            if (_hotKeys.Contains(hotKey))
                return;

            if (_hotKeys.Count == 0)
            {
                _hook.Install();
            }

            _hotKeys.Add(hotKey);
        }

        void HookKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // List pressed keys
            var keys = new List<Keys>();
            var state = KeyboardState.GetCurrent();
            for (var i = 0; i < 256; i++)
            {
                if (state.IsDown((Keys)i))
                {
                    keys.Add((Keys)i);
                }
            }

            if (!keys.Contains(e.KeyData))
            {
                keys.Add(e.KeyData);
            }

            // Find hotkey
            foreach (HotKey hotKey in _hotKeys)
            {
                if (hotKey.Equals(keys))
                {
                    if (hotKey.Action != null)
                    {
                        hotKey.Action(sender, EventArgs.Empty);
                        e.Handled = true;
                    }
                }
            }
        }

        public void Dispose()
        {
            _hook.Dispose();
        }
    }
}
