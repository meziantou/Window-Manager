using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace WindowManager.Core
{
    public static class Win32WindowPositionner
    {
        private static Rectangle GetScreen(Win32Window window)
        {
            return Screen.FromHandle(window.Handle).WorkingArea;
        }

        private static int[] ComputeArray(int origin, int size, int position)
        {
            int[] sizes = new int[Splits.Length];
            for (int i = 0; i < Splits.Length; i++)
            {
                sizes[i] = (int)(origin + (size) * Splits[i] - position);
            }

            return sizes;
        }

        public static void ReduceBottom(this Win32Window window)
        {
            Trace.WriteLine("Reduce Bottom HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int minimumSize = screen.Height / 4;

            if (placement.Height <= minimumSize)
                return;

            int[] sizes = ComputeArray(screen.Y, screen.Height, placement.Y);
            int size = MaximumValueLessThan(sizes, placement.Height);

            int x = placement.X;
            int y = placement.Y;
            int width = placement.Width;
            int height = size;

            if (height < minimumSize)
                height = minimumSize;

            SetWindowPosition(window, x, y, width, height);
        }

        public static void ReduceTop(this Win32Window window)
        {
            Trace.WriteLine("Reduce Top HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int minimumSize = screen.Height / 4;


            if (placement.Height <= minimumSize)
                return;

            int[] sizes = ComputeArray(screen.Y, screen.Height, placement.Y);
            int size = MaximumValueLessThan(sizes, 0);

            int x = placement.X;
            int y = placement.Y + size;
            int width = placement.Width;
            int height = placement.Height - size;

            if (height < minimumSize)
                height = minimumSize;

            SetWindowPosition(window, x, y, width, height);
        }

        public static void ReduceRight(this Win32Window window)
        {
            Trace.WriteLine("Reduce Right HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int minimumSize = screen.Width / 4;

            if (placement.Width <= minimumSize)
                return;

            int[] sizes = ComputeArray(screen.X, screen.Width, placement.X);
            int size = MaximumValueLessThan(sizes, placement.Width);

            int x = placement.X;
            int y = placement.Y;
            int width = size;
            int height = placement.Height;

            if (width < minimumSize)
                width = minimumSize;

            SetWindowPosition(window, x, y, width, height);
        }

        public static void ReduceLeft(this Win32Window window)
        {
            Trace.WriteLine("Reduce Left HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int minimumSize = screen.Width / 4;

            if (placement.Width <= minimumSize)
                return;

            int[] sizes = ComputeArray(screen.X, screen.Width, placement.X);
            int size = MinimumValueGreaterThan(sizes, 0);


            int x = placement.X + size;
            int y = placement.Y;
            int width = placement.Width - size;
            int height = placement.Height;

            if (width < minimumSize)
                width = minimumSize;

            SetWindowPosition(window, x, y, width, height);
        }

        public static readonly double[] Splits = new[] { 0.0, 1.0 / 4.0, 1.0 / 3.0, 2.0 / 4.0, 2.0 / 3.0, 3.0 / 4.0, 1.0 };

        public static int MinimumValueGreaterThan(IList<int> size, int height)
        {
            for (int i = 0; i < size.Count; i++)
            {
                if (size[i] > height)
                    return size[i];
            }

            return size[size.Count - 1];
        }

        public static int MaximumValueLessThan(IList<int> size, int height)
        {
            for (int i = size.Count - 1; i >= 0; i--)
            {
                if (size[i] < height)
                    return size[i];
            }

            return size[0];
        }

        public static void ExtendBottom(this Win32Window window)
        {
            Trace.WriteLine("Extend Bottom HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int[] sizes = ComputeArray(screen.Y, screen.Height, placement.Y);
            int size = MinimumValueGreaterThan(sizes, placement.Height);

            int x = placement.X;
            int y = placement.Y;
            int width = placement.Width;
            int height = size;

            SetWindowPosition(window, x, y, width, height);
        }

        public static void ExtendTop(this Win32Window window)
        {
            Trace.WriteLine("Extend Top HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int[] sizes = ComputeArray(screen.Y, screen.Height, placement.Y);
            int size = MaximumValueLessThan(sizes, 0);

            int x = placement.X;
            int y = placement.Y + size;
            int width = placement.Width;
            int height = placement.Height - size;

            SetWindowPosition(window, x, y, width, height);
        }

        public static void ExtendLeft(this Win32Window window)
        {
            Trace.WriteLine("Extend Left HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int[] sizes = ComputeArray(screen.X, screen.Width, placement.X);
            int size = MaximumValueLessThan(sizes, 0);

            int x = placement.X + size;
            int y = placement.Y;
            int width = placement.Width - size;
            int height = placement.Height;

            SetWindowPosition(window, x, y, width, height);
        }

        public static void ExtendRight(this Win32Window window)
        {
            Trace.WriteLine("Extend Right HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int[] sizes = ComputeArray(screen.X, screen.Width, placement.X);
            int size = MinimumValueGreaterThan(sizes, placement.Width);

            int x = placement.X;
            int y = placement.Y;
            int width = size;
            int height = placement.Height;

            SetWindowPosition(window, x, y, width, height);
        }

        public static void MinimizeWindow(this Win32Window window)
        {
            Trace.WriteLine("Minimize HotKey");
            if (window.WindowState == WindowState.Maximized)
            {
                Trace.WriteLine("FullScreen => Restore");
                window.WindowState = WindowState.Normal;
            }
            else
            {
                Trace.WriteLine("Normal => Minimize");
                window.WindowState = WindowState.Minimized;
            }
        }

        public static void SendToCenterHotKeyPressed(this Win32Window window)
        {
            Trace.WriteLine("Center HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X + screen.Width / 4;
            int y = screen.Y + screen.Height / 4;
            int width = screen.Width / 2;
            int height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("Center HotKey*2");
                x = screen.X + screen.Width / 3;
                y = screen.Y + screen.Height / 3;
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToHorizontalCenterHotKeyPressed(this Win32Window window)
        {
            Trace.WriteLine("Horizontal Center HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X;
            int y = screen.Y + screen.Height / 4;
            int width = screen.Width;
            int height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("Horizontal Center HotKey*2");
                y = screen.Y + screen.Height / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToVerticalCenterHotKeyPressed(this Win32Window window)
        {
            Trace.WriteLine("Vertical Center HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X + screen.Width / 4;
            int y = screen.Y;
            int width = screen.Width / 2;
            int height = screen.Height;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("Vertical Center HotKey*2");
                x = screen.X + screen.Width / 3;
                width = screen.Width / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SwitchScreen(this Win32Window window)
        {
            Screen screen = Screen.FromHandle(window.Handle);
            List<Screen> screens = Screen.AllScreens.ToList();
            int index = screens.IndexOf(screen) + 1;
            Screen newScreen = screens.Count == index ? screens[0] : screens[index];

            double ratioX = (double)newScreen.WorkingArea.Width / screen.WorkingArea.Width;
            double ratioY = (double)newScreen.WorkingArea.Height / screen.WorkingArea.Height;

            bool maximized = window.WindowState == WindowState.Maximized;
            Rectangle placement = window.Rectangle;
            int x = (int)((placement.X - screen.WorkingArea.X) * ratioX + newScreen.WorkingArea.X);
            int y = (int)((placement.Y - screen.WorkingArea.Y) * ratioY + newScreen.WorkingArea.Y);
            int width = (int)(placement.Width * ratioX);
            int height = (int)(placement.Height * ratioY);

            SetWindowPosition(window, x, y, width, height);

            if (maximized)
            {
                window.WindowState = WindowState.Maximized;
            }
        }

        public static void BottomMost(this Win32Window window)
        {
            Trace.WriteLine("BottomMost HotKey");
            window.SendToBack();
        }

        public static void TopMost(this Win32Window window)
        {
            Trace.WriteLine("TopMost HotKey");
            if (window.IsTopMost)
            {
                Trace.WriteLine("Window already top most => Remove top most");
                window.IsTopMost = false;
            }
            else
            {
                window.IsTopMost = true;
            }
        }

        public static void SendToBottomLeft(this Win32Window window)
        {
            Trace.WriteLine("BottomLeft HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X;
            int y = screen.Y + screen.Height / 2;
            int width = screen.Width / 2;
            int height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("BottomLeft HotKey*2");
                y = screen.Y + 2 * screen.Height / 3;
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToFull(this Win32Window window, bool maximize)
        {
            Trace.WriteLine("FullScreen HotKey");

            Rectangle screen = GetScreen(window);

            bool maximized = window.WindowState == WindowState.Maximized;
            if (maximized)
            {
                Trace.WriteLine("FullScreen => Restore");
                window.WindowState = WindowState.Normal;
            }
            else
            {
                if (maximize)
                {
                    Trace.WriteLine("Restore => Maximized");
                    window.WindowState = WindowState.Maximized;
                }
                else
                {
                    Trace.WriteLine("Restore => FullScreen");
                    int x = screen.X;
                    int y = screen.Y;
                    int width = screen.Width;
                    int height = screen.Height;
                    SetWindowPosition(window, x, y, width, height);
                }
            }
        }

        public static void SendToTopLeft(this Win32Window window)
        {
            Trace.WriteLine("TopLeft HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X;
            int y = screen.Y;
            int width = screen.Width / 2;
            int height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("TopLeft HotKey*2");
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToBottomRight(this Win32Window window)
        {
            Trace.WriteLine("BottomRight HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X + screen.Width / 2;
            int y = screen.Y + screen.Height / 2;
            int width = screen.Width / 2;
            int height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("BottomRight HotKey*2");
                x = screen.X + 2 * screen.Width / 3;
                y = screen.Y + 2 * screen.Height / 3;
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToTopRight(this Win32Window window)
        {
            Trace.WriteLine("TopRight HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X + screen.Width / 2;
            int y = screen.Y;
            int width = screen.Width / 2;
            int height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("TopRight HotKey*2");
                x = screen.X + 2 * screen.Width / 3;
                width = screen.Width / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToTop(this Win32Window window)
        {
            Trace.WriteLine("Top HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X;
            int y = screen.Y;
            int width = screen.Width;
            int height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("Top HotKey*2");
                height = screen.Height / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToBottom(this Win32Window window)
        {
            Trace.WriteLine("Bottom HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X;
            int y = screen.Y + screen.Height / 2;
            int width = screen.Width;
            int height = screen.Height / 2;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("Bottom HotKey*2");
                y = screen.Y + 2 * screen.Height / 3;
                height = screen.Height / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToLeft(this Win32Window window)
        {
            Trace.WriteLine("Left HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X;
            int y = screen.Y;
            int width = screen.Width / 2;
            int height = screen.Height;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("Left HotKey*2");
                width = screen.Width / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        public static void SendToRight(this Win32Window window)
        {
            Trace.WriteLine("Right HotKey");

            Rectangle screen = GetScreen(window);
            Rectangle placement = window.Rectangle;

            int x = screen.X + screen.Width / 2;
            int y = screen.Y;
            int width = screen.Width / 2;
            int height = screen.Height;

            if (placement.X == x && placement.Y == y && placement.Width == width && placement.Height == height)
            {
                Trace.WriteLine("Right HotKey*2");
                x = screen.X + 2 * screen.Width / 3;
                width = screen.Width / 3;
            }

            SetWindowPosition(window, x, y, width, height);
        }

        private static void SetWindowPosition(Win32Window window, int x, int y, int width, int height)
        {
            window.WindowState = WindowState.Normal;
            window.Rectangle = new Rectangle(x, y, width, height);
        }
    }
}