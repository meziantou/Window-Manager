using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowManager.Core
{
    internal sealed class KeyboardHook : IDisposable
    {
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
            }
        }

        public void Install()
        {
            _proc = HookCallback;
            using var curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var handled = false;
            if ((nCode >= 0) && (KeyDown != null || KeyUp != null || KeyPress != null))
            {
                var myKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                if (KeyDown != null && (wParam.ToInt32() == WM_KEYDOWN || wParam.ToInt32() == WM_SYSKEYDOWN))
                {
                    var keyData = (Keys)myKeyboardHookStruct.vkCode;
                    var e = new KeyEventArgs(keyData);
                    KeyDown(this, e);
                    handled = handled || e.Handled;
                }

                if (KeyPress != null && wParam.ToInt32() == WM_KEYDOWN)
                {
                    var isDownShift = (GetKeyState(VK_SHIFT) & 0x80) == 0x80;
                    var isDownCapslock = GetKeyState(VK_CAPITAL) != 0;

                    var keyState = new byte[256];
                    GetKeyboardState(keyState);
                    var inBuffer = new byte[2];
                    if (ToAscii(myKeyboardHookStruct.vkCode, myKeyboardHookStruct.scanCode, keyState, inBuffer, myKeyboardHookStruct.flags) == 1)
                    {
                        var key = (char)inBuffer[0];
                        if ((isDownCapslock ^ isDownShift) && char.IsLetter(key)) key = char.ToUpperInvariant(key);
                        var e = new KeyPressEventArgs(key);
                        KeyPress(this, e);
                        handled = handled || e.Handled;
                    }
                }

                if (KeyUp != null && (wParam.ToInt32() == WM_KEYUP || wParam.ToInt32() == WM_SYSKEYUP))
                {
                    var keyData = (Keys)myKeyboardHookStruct.vkCode;
                    var e = new KeyEventArgs(keyData);
                    KeyUp(this, e);
                    handled = handled || e.Handled;
                }
            }

            // if event handled in application do not handoff to other listeners
            if (handled)
                return new IntPtr(1);

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event KeyEventHandler KeyDown;
        public event KeyPressEventHandler KeyPress;
        public event KeyEventHandler KeyUp;

        [StructLayout(LayoutKind.Sequential)]
        private sealed class KeyboardHookStruct
        {
            /// <summary>
            /// Specifies a virtual-key code. The code must be a value in the range 1 to 254. 
            /// </summary>
            public int vkCode;
            /// <summary>
            /// Specifies a hardware scan code for the key. 
            /// </summary>
            public int scanCode;
            /// <summary>
            /// Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
            /// </summary>
            public int flags;
            /// <summary>
            /// Specifies the time stamp for this message.
            /// </summary>
            public int time;
            /// <summary>
            /// Specifies extra information associated with the message. 
            /// </summary>
            public int dwExtraInfo;
        }

        [DllImport("user32")]
        private static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        [DllImport("user32")]
        internal static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetKeyState(int vKey);

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        private const byte VK_SHIFT = 0x10;
        private const byte VK_CAPITAL = 0x14;
    }
}