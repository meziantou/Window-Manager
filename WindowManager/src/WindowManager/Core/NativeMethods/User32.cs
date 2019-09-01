using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace WindowManager.Core.NativeMethods
{
    internal static class User32
    {
        public const int WmHotKey = 786;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern WindowStyle GetWindowLong(IntPtr hWnd, WindowLongs nIndex);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out uint crKey, out byte bAlpha, out LayeredWindowAttributes dwFlags);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, LayeredWindowAttributes dwFlags);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, WindowStyle dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        public static void ShowWindow(IntPtr handle)
        {
            ShowWindow(handle, WindowShowStyle.Show);
        }

        public static Rectangle GetPlacement(IntPtr handle, out bool maximized)
        {
            var placement = new WINDOWPLACEMENT();
            placement.Length = Marshal.SizeOf(placement);
            GetWindowPlacement(handle, ref placement);

            var rectangle = new Rectangle(placement.NormalPosition.X, placement.NormalPosition.Y, placement.NormalPosition.Width - placement.NormalPosition.X, placement.NormalPosition.Height - placement.NormalPosition.Y);
            maximized = placement.ShowCmd == (int)WindowShowStyle.Maximize;
            return rectangle;
        }


        public static void ShowWindowNormal(IntPtr handle)
        {
            ShowWindow(handle, WindowShowStyle.ShowNormal);
        }

        public static void ShowWindowMinimized(IntPtr handle)
        {
            ShowWindow(handle, WindowShowStyle.ShowMinimized);
        }

        public static void ShowWindowTopMost(IntPtr handle, bool topMost)
        {
            SetWindowPos(handle, (IntPtr)(topMost ? Hwnd.HwndTopMost : Hwnd.HwndNoTopMost), 0, 0, 0, 0,
                         SetWindowPosFlags.ShowWindow | SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize);
        }

        public static void ShowWindowBottom(IntPtr handle)
        {
            SetWindowPos(handle, (IntPtr)Hwnd.HwndBottom, 0, 0, 0, 0,
                         SetWindowPosFlags.ShowWindow | SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize);
        }

        public static void ShowWindowMaximized(IntPtr handle)
        {
            ShowWindow(handle, WindowShowStyle.ShowMaximized);
        }

        public static void SetWindowPosition(IntPtr handle, int x, int y, int width, int height)
        {
            SetWindowPos(handle, (IntPtr)Hwnd.HwndTop, x, y, width, height, SetWindowPosFlags.ShowWindow | SetWindowPosFlags.ShowWindow);
        }

        public static bool IsTopMost(IntPtr handle)
        {
            return (GetWindowLong(handle, WindowLongs.GWL_EXSTYLE) & WindowStyle.WS_EX_TOPMOST) == WindowStyle.WS_EX_TOPMOST;
        }

        public static Rectangle GetPlacement(IntPtr handle)
        {
            return GetPlacement(handle, out _);
        }

        internal enum Hwnd
        {
            HwndTop = 0,
            HwndBottom = 1,
            HwndTopMost = -1,
            HwndNoTopMost = -2,
        }

        internal enum LayeredWindowAttributes
        {
            LWA_COLORKEY = 0x00000001,
            LWA_ALPHA = 0x00000002,
        }

        [Flags]
        internal enum SetWindowPosFlags
        {
            IgnoreResize = 1,
            IgnoreMove = 2,
            IgnoreZOrder = 4,
            DoNotRedraw = 8,
            DoNotActivate = 16,
            DrawFrame = 32,
            FrameChanged = 32,
            ShowWindow = 64,
            HideWindow = 128,
            DoNotCopyBits = 256,
            DoNotChangeOwnerZOrder = 512,
            DoNotReposition = 512,
            DoNotSendChangingEvent = 1024,
            DeferErase = 8192,
            SynchronousWindowPosition = 16384,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int Length;
            public readonly int Flags;
            public readonly int ShowCmd;
            public readonly Point MinPosition;
            public readonly Point MaxPosition;
            public Rectangle NormalPosition;
        }

        [Flags]
        internal enum WindowLongs
        {
            GWL_WNDPROC = -4,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_STYLE = -16,
            GWL_EXSTYLE = -20,
            GWL_USERDATA = -21,
            GWL_ID = -12,
        }

        internal enum WindowShowStyle : uint
        {
            Hide = 0U,
            ShowNormal = 1U,
            ShowMinimized = 2U,
            Maximize = 3U,
            ShowMaximized = 3U,
            ShowNormalNoActivate = 4U,
            Show = 5U,
            Minimize = 6U,
            ShowMinNoActivate = 7U,
            ShowNoActivate = 8U,
            Restore = 9U,
            ShowDefault = 10U,
            ForceMinimized = 11U,
        }

        internal enum WindowStyle : uint
        {
            // Window Styles 
            WS_OVERLAPPED = 0,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x8000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_MAXIMIZE = 0x1000000,
            WS_CAPTION = 0xC00000, // WS_BORDER or WS_DLGFRAME  
            WS_BORDER = 0x800000,
            WS_DLGFRAME = 0x400000,
            WS_VSCROLL = 0x200000,
            WS_HSCROLL = 0x100000,
            WS_SYSMENU = 0x80000,
            WS_THICKFRAME = 0x40000,
            WS_GROUP = 0x20000,
            WS_TABSTOP = 0x10000,
            WS_MINIMIZEBOX = 0x20000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,

            // Extended Window Styles 
            WS_EX_DLGMODALFRAME = 0x0001,
            WS_EX_NOPARENTNOTIFY = 0x0004,
            WS_EX_TOPMOST = 0x0008,
            WS_EX_ACCEPTFILES = 0x0010,
            WS_EX_TRANSPARENT = 0x0020,
            WS_EX_MDICHILD = 0x0040,
            WS_EX_TOOLWINDOW = 0x0080,
            WS_EX_WINDOWEDGE = 0x0100,
            WS_EX_CLIENTEDGE = 0x0200,
            WS_EX_CONTEXTHELP = 0x0400,
            WS_EX_RIGHT = 0x1000,
            WS_EX_LEFT = 0x0000,
            WS_EX_RTLREADING = 0x2000,
            WS_EX_LTRREADING = 0x0000,
            WS_EX_LEFTSCROLLBAR = 0x4000,
            WS_EX_RIGHTSCROLLBAR = 0x0000,
            WS_EX_CONTROLPARENT = 0x10000,
            WS_EX_STATICEDGE = 0x20000,
            WS_EX_APPWINDOW = 0x40000,
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
            WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring
            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_NOACTIVATE = 0x08000000,
        }
    }
}