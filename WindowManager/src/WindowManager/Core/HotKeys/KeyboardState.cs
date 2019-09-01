using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WindowManager.Core
{
    internal sealed class KeyboardState
    {
        private readonly byte[] _keyboardStateNative;

        /// <summary>
        /// Makes a snapshot of a keyboard state to the moment of call and returns an 
        /// instance of <see cref="KeyboardState"/> class.
        /// </summary>
        /// <returns>An instance of <see cref="KeyboardState"/> class representing a snapshot of keyboard state at certain moment.</returns>
        public static KeyboardState GetCurrent()
        {
            var keyboardStateNative = new byte[256];
            KeyboardHook.GetKeyboardState(keyboardStateNative);
            return new KeyboardState(keyboardStateNative);
        }

        internal byte[] GetNativeState()
        {
            return _keyboardStateNative;
        }

        private KeyboardState(byte[] keyboardStateNative)
        {
            _keyboardStateNative = keyboardStateNative;
        }

        /// <summary>
        /// Indicates wether specified key was down at the moment when snapshot was created or not.
        /// </summary>
        /// <param name="key">Key (corresponds to the virtual code of the key)</param>
        /// <returns><b>true</b> if key was down, <b>false</b> - if key was up.</returns>
        public bool IsDown(Keys key)
        {
            var keyState = GetKeyState(key);
            var isDown = GetHighBit(keyState);
            return isDown;
        }

        /// <summary>
        /// Indiceate weather specified key was toggled at the moment when snapshot was created or not.
        /// </summary>
        /// <param name="key">Key (corresponds to the virtual code of the key)</param>
        /// <returns>
        /// <b>true</b> if toggle key like (CapsLock, NumLocke, etc.) was on. <b>false</b> if it was off.
        /// Ordinal (non toggle) keys return always false.
        /// </returns>
        public bool IsToggled(Keys key)
        {
            var keyState = GetKeyState(key);
            var isToggled = GetLowBit(keyState);
            return isToggled;
        }

        /// <summary>
        /// Idicates weather every of specified keys were down at the moment when snapshot was created.
        /// The method returns flase if even one of them was up.  
        /// </summary>
        /// <param name="keys">Keys to verify wether they were down or not.</param>
        /// <returns><b>true</b> - all were down. <b>false</b> - at least one was up.</returns>
        public bool AreAllDown(IEnumerable<Keys> keys)
        {
            return keys.All(IsDown);
        }

        private byte GetKeyState(Keys key)
        {
            var virtualKeyCode = (int)key;
            if (virtualKeyCode < 0 || virtualKeyCode > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(key), key, "The value must be between 0 and 255.");
            }
            return _keyboardStateNative[virtualKeyCode];
        }

        private static bool GetHighBit(byte value)
        {
            return (value >> 7) != 0;
        }

        private static bool GetLowBit(byte value)
        {
            return (value & 1) != 0;
        }
    }
}