using System;

namespace WindowManager.Core.NativeMethods
{
    public struct SHELLHOOKINFO
    {
        public IntPtr hwnd;
        public RECT rc;
    }
}