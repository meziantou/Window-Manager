using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;

namespace WindowManager.Core
{
    public class KeyBoardHookHotKey : IDisposable
    {
        private class HotKey : IEquatable<HotKey>, IEquatable<ICollection<Keys>>
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

                var @equals = (_keys.Count == (other.Count - skip));
                return @equals;
            }

            public override string ToString()
            {
                return string.Format("ModifierKeys: {0}, Keys: {1}", ModifierKeys, Keys);
            }

            public ModifierKeys ModifierKeys { get; set; }
            public Keys Keys { get; set; }
            public HotKeyPressedEventHandler Action { get; set; }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(HotKey other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.ModifierKeys, ModifierKeys) && Equals(other.Keys, Keys);
            }

            /// <summary>
            /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(HotKey)) return false;
                return Equals((HotKey)obj);
            }

            /// <summary>
            /// Serves as a hash function for a particular type. 
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                unchecked
                {
                    return (ModifierKeys.GetHashCode() * 397) ^ Keys.GetHashCode();
                }
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

        readonly KeyboardHook _hook;
        private readonly IList<HotKey> _hotKeys = new List<HotKey>();

        public KeyBoardHookHotKey()
        {
            _hook = new KeyboardHook();
            _hook.KeyDown += new System.Windows.Forms.KeyEventHandler(HookKeyDown);
        }

        public void RegisterHotKey(ModifierKeys modifierKeys, Keys key, HotKeyPressedEventHandler action)
        {
            var hotKey = new HotKey(modifierKeys, key);
            hotKey.Action = action;
            if (_hotKeys.Contains(hotKey))
                return; // TODO manage multiple handler

            if (_hotKeys.Count == 0)
                _hook.Install();

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
                keys.Add(e.KeyData);

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
