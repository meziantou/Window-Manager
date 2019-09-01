using Meziantou.Framework;
using System;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using WindowManager.Core.NativeMethods;

namespace WindowManager.Core
{
    public sealed class Win32Window
    {
        private const byte TransparencyInterval = 15;
        private static readonly double[] _splits = new[] { 0.0, 1.0 / 4.0, 1.0 / 3.0, 2.0 / 4.0, 2.0 / 3.0, 3.0 / 4.0, 1.0 };

        public Win32Window(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("handle must valid", nameof(handle));
            Handle = handle;
        }

        public static Win32Window GetForegroundWindow()
        {
            IntPtr foregroundWindow = User32.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return null;

            return new Win32Window(foregroundWindow);
        }

        public IntPtr Handle { get; }

        public Rectangle Rectangle
        {
            get
            {
                Rectangle placement = User32.GetPlacement(Handle);
                return placement;
            }
            set
            {
                User32.SetWindowPosition(Handle, value.X, value.Y, value.Width, value.Height);
            }
        }

        public string WindowText
        {
            get
            {
                var title = new StringBuilder(256);
                User32.GetWindowText(Handle, title, 256);
                return title.ToString().Trim();
            }
        }

        public WindowState WindowState
        {
            get
            {
                if (User32.IsZoomed(Handle))
                    return WindowState.Maximized;

                if (User32.IsIconic(Handle))
                    return WindowState.Minimized;

                return WindowState.Normal;
            }
            set
            {
                switch (value)
                {
                    case WindowState.Normal:
                        User32.ShowWindowNormal(Handle);
                        break;
                    case WindowState.Minimized:
                        User32.ShowWindowMinimized(Handle);
                        break;
                    case WindowState.Maximized:
                        User32.ShowWindowMaximized(Handle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        public bool IsTopMost
        {
            get
            {
                var isTopMost = User32.IsTopMost(Handle);
                return isTopMost;
            }
            set
            {
                User32.ShowWindowTopMost(Handle, value);
            }
        }

        public void SendToBack()
        {
            User32.ShowWindowBottom(Handle);
        }

        public void SetTransparency(byte transparency)
        {
            var currentAttributes = User32.GetWindowLong(Handle, User32.WindowLongs.GWL_EXSTYLE);
            if (!currentAttributes.HasFlag(User32.WindowStyle.WS_EX_LAYERED))
            {
                User32.SetWindowLong(Handle, (int)User32.WindowLongs.GWL_EXSTYLE, currentAttributes | User32.WindowStyle.WS_EX_LAYERED);
            }

            User32.SetLayeredWindowAttributes(Handle, 0, transparency, User32.LayeredWindowAttributes.LWA_ALPHA);
        }

        private byte GetCurrentTransparency()
        {
            var currentAttributes = User32.GetWindowLong(Handle, User32.WindowLongs.GWL_EXSTYLE);
            if (!currentAttributes.HasFlag(User32.WindowStyle.WS_EX_LAYERED))
            {
                // The window is not transparent
                return 255;
            }

            User32.GetLayeredWindowAttributes(Handle, out _, out var alpha, out _);
            return alpha;
        }

        internal void IncreaseTransparency()
        {
            var value = GetCurrentTransparency();
            if (value > TransparencyInterval)
            {
                SetTransparency((byte)(value - TransparencyInterval));
            }
        }

        internal void DecreaseTransparency()
        {
            var value = GetCurrentTransparency();
            if (value < (255 - TransparencyInterval))
            {
                value += TransparencyInterval;
                SetTransparency(value);
            }
            else if (value != 255)
            {
                value = 255;
                SetTransparency(value);
            }
        }

        public override string ToString()
        {
            return WindowText;
        }

        public void ReduceBottom()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var minimumSize = screen.Height / 4;

            if (placement.Height <= minimumSize)
                return;

            var sizes = ComputeArray(screen.Y, screen.Height, placement.Y);
            var size = MaximumValueLessThan(sizes, placement.Height);

            var x = placement.X;
            var y = placement.Y;
            var width = placement.Width;
            var height = size;

            if (height < minimumSize)
                height = minimumSize;

            SetWindowPosition(x, y, width, height);
        }

        public void ReduceTop()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var minimumSize = screen.Height / 4;


            if (placement.Height <= minimumSize)
                return;

            var sizes = ComputeArray(screen.Y, screen.Height, placement.Y);
            var size = MaximumValueLessThan(sizes, 0);

            var x = placement.X;
            var y = placement.Y + size;
            var width = placement.Width;
            var height = placement.Height - size;

            if (height < minimumSize)
                height = minimumSize;

            SetWindowPosition(x, y, width, height);
        }

        public void ReduceRight()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var minimumSize = screen.Width / 4;

            if (placement.Width <= minimumSize)
                return;

            var sizes = ComputeArray(screen.X, screen.Width, placement.X);
            var size = MaximumValueLessThan(sizes, placement.Width);

            var x = placement.X;
            var y = placement.Y;
            var width = size;
            var height = placement.Height;

            if (width < minimumSize)
            {
                width = minimumSize;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void ReduceLeft()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var minimumSize = screen.Width / 4;

            if (placement.Width <= minimumSize)
                return;

            var sizes = ComputeArray(screen.X, screen.Width, placement.X);
            var size = MinimumValueGreaterThan(sizes, 0);


            var x = placement.X + size;
            var y = placement.Y;
            var width = placement.Width - size;
            var height = placement.Height;

            if (width < minimumSize)
            {
                width = minimumSize;
            }

            SetWindowPosition(x, y, width, height);
        }


        public void ExtendBottom()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var sizes = ComputeArray(screen.Y, screen.Height, placement.Y);
            var size = MinimumValueGreaterThan(sizes, placement.Height);

            var x = placement.X;
            var y = placement.Y;
            var width = placement.Width;
            var height = size;

            SetWindowPosition(x, y, width, height);
        }

        public void ExtendTop()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var sizes = ComputeArray(screen.Y, screen.Height, placement.Y);
            var size = MaximumValueLessThan(sizes, 0);

            var x = placement.X;
            var y = placement.Y + size;
            var width = placement.Width;
            var height = placement.Height - size;

            SetWindowPosition(x, y, width, height);
        }

        public void ExtendLeft()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var sizes = ComputeArray(screen.X, screen.Width, placement.X);
            var size = MaximumValueLessThan(sizes, 0);

            var x = placement.X + size;
            var y = placement.Y;
            var width = placement.Width - size;
            var height = placement.Height;

            SetWindowPosition(x, y, width, height);
        }

        public void ExtendRight()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var sizes = ComputeArray(screen.X, screen.Width, placement.X);
            var size = MinimumValueGreaterThan(sizes, placement.Width);

            var x = placement.X;
            var y = placement.Y;
            var width = size;
            var height = placement.Height;

            SetWindowPosition(x, y, width, height);
        }

        public void MinimizeWindow()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Minimized;
            }
        }

        public void SendToCenterHotKeyPressed()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X + screen.Width / 4;
            var y = screen.Y + screen.Height / 4;
            var width = screen.Width / 2;
            var height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                x = screen.X + screen.Width / 3;
                y = screen.Y + screen.Height / 3;
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToHorizontalCenterHotKeyPressed()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X;
            var y = screen.Y + screen.Height / 4;
            var width = screen.Width;
            var height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                y = screen.Y + screen.Height / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToVerticalCenterHotKeyPressed()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X + screen.Width / 4;
            var y = screen.Y;
            var width = screen.Width / 2;
            var height = screen.Height;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                x = screen.X + screen.Width / 3;
                width = screen.Width / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SwitchScreen()
        {
            var screen = Screen.FromHandle(Handle);
            var screens = Screen.AllScreens;
            var index = screens.IndexOf(screen) + 1;
            Screen newScreen = screens.Length == index ? screens[0] : screens[index];

            var ratioX = (double)newScreen.WorkingArea.Width / screen.WorkingArea.Width;
            var ratioY = (double)newScreen.WorkingArea.Height / screen.WorkingArea.Height;

            var maximized = WindowState == WindowState.Maximized;
            Rectangle placement = Rectangle;
            var x = (int)((placement.X - screen.WorkingArea.X) * ratioX + newScreen.WorkingArea.X);
            var y = (int)((placement.Y - screen.WorkingArea.Y) * ratioY + newScreen.WorkingArea.Y);
            var width = (int)(placement.Width * ratioX);
            var height = (int)(placement.Height * ratioY);

            SetWindowPosition(x, y, width, height);

            if (maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        public void BottomMost()
        {
            SendToBack();
        }

        public void SwitchIsTopMost()
        {
            IsTopMost = !IsTopMost;
        }

        public void SendToBottomLeft()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X;
            var y = screen.Y + screen.Height / 2;
            var width = screen.Width / 2;
            var height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                y = screen.Y + 2 * screen.Height / 3;
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToFull(bool maximize)
        {
            Rectangle screen = GetScreen();

            var maximized = WindowState == WindowState.Maximized;
            if (maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                if (maximize)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    var x = screen.X;
                    var y = screen.Y;
                    var width = screen.Width;
                    var height = screen.Height;
                    SetWindowPosition(x, y, width, height);
                }
            }
        }

        public void SendToTopLeft()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X;
            var y = screen.Y;
            var width = screen.Width / 2;
            var height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToBottomRight()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X + screen.Width / 2;
            var y = screen.Y + screen.Height / 2;
            var width = screen.Width / 2;
            var height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                x = screen.X + 2 * screen.Width / 3;
                y = screen.Y + 2 * screen.Height / 3;
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToTopRight()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X + screen.Width / 2;
            var y = screen.Y;
            var width = screen.Width / 2;
            var height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                x = screen.X + 2 * screen.Width / 3;
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToTop()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X;
            var y = screen.Y;
            var width = screen.Width;
            var height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                height = screen.Height / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToBottom()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X;
            var y = screen.Y + screen.Height / 2;
            var width = screen.Width;
            var height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                y = screen.Y + 2 * screen.Height / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToLeft()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X;
            var y = screen.Y;
            var width = screen.Width / 2;
            var height = screen.Height;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                width = screen.Width / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        public void SendToRight()
        {
            Rectangle screen = GetScreen();
            Rectangle placement = Rectangle;

            var x = screen.X + screen.Width / 2;
            var y = screen.Y;
            var width = screen.Width / 2;
            var height = screen.Height;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                x = screen.X + 2 * screen.Width / 3;
                width = screen.Width / 3;
            }

            SetWindowPosition(x, y, width, height);
        }

        private static int[] ComputeArray(int origin, int size, int position)
        {
            var sizes = new int[_splits.Length];
            for (var i = 0; i < _splits.Length; i++)
            {
                sizes[i] = (int)(origin + size * _splits[i] - position);
            }

            return sizes;
        }

        private void SetWindowPosition(int x, int y, int width, int height)
        {
            WindowState = WindowState.Normal;
            Rectangle = new Rectangle(x, y, width, height);
        }

        private Rectangle GetScreen()
        {
            return Screen.FromHandle(Handle).WorkingArea;
        }

        private static int MinimumValueGreaterThan(int[] size, int height)
        {
            for (var i = 0; i < size.Length; i++)
            {
                if (size[i] > height)
                    return size[i];
            }

            return size[size.Length - 1];
        }

        private static int MaximumValueLessThan(int[] size, int height)
        {
            for (var i = size.Length - 1; i >= 0; i--)
            {
                if (size[i] < height)
                    return size[i];
            }

            return size[0];
        }
    }
}
