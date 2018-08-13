using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows;
using WindowManager.Core.NativeMethods;

namespace WindowManager.Core
{
    public class Win32Window : IEquatable<Win32Window>
    {
        private const int TransparencyPercentId = 1000;
        private readonly IntPtr _handle;
        private int? _originalWindowLongExStyle;
        private IntPtr _transparencyMenuHandle;
        private const int DefaultTransparencyMenuId = 1000;

        public Win32Window(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("handle must valid", "handle");
            _handle = handle;
        }

        public static Win32Window GetForegroundWindow()
        {
            IntPtr foregroundWindow = User32.GetForegroundWindow();
            return new Win32Window(foregroundWindow);
        }

        public IntPtr Handle => _handle;

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
                        throw new ArgumentOutOfRangeException("value");
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

        public void BringToFront()
        {
            User32.ShowWindow(Handle);
        }

        public void SetTransparency(byte transparency)
        {
            if (_originalWindowLongExStyle == null)
                _originalWindowLongExStyle = User32.GetWindowLong(Handle, User32.WindowLongs.GWL_EXSTYLE);

            User32.SetWindowLong(Handle, (int)User32.WindowLongs.GWL_EXSTYLE, User32.GetWindowLong(Handle, User32.WindowLongs.GWL_EXSTYLE) ^ (int)User32.WindowStyle.WS_EX_LAYERED);
            User32.SetLayeredWindowAttributes(Handle, 0, transparency, (int)User32.LayeredWindowAttributes.LWA_ALPHA);
        }

        public void ClearTransparency()
        {
            if (_originalWindowLongExStyle != null)
                User32.SetWindowLong(Handle, (int)User32.WindowLongs.GWL_EXSTYLE, _originalWindowLongExStyle.Value);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Win32Window other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._handle.Equals(_handle);
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
            if (obj.GetType() != typeof(Win32Window)) return false;
            return Equals((Win32Window)obj);
        }

        public static bool operator ==(Win32Window left, Win32Window right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Win32Window left, Win32Window right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return _handle.GetHashCode();
        }

        public void RemoveTransparencyMenu()
        {
            // This function removes the Transparency menu we added to the system menu.
            IntPtr windowMenuHandle = User32.GetSystemMenu(Handle, false);
            var index = User32.GetMenuItemCount(windowMenuHandle);
            User32.RemoveMenu(windowMenuHandle, index - 3, (int)User32.Menu.MF_BYPOSITION);
            User32.RemoveMenu(windowMenuHandle, index - 3, (int)User32.Menu.MF_BYPOSITION);

            User32.DestroyMenu(_transparencyMenuHandle);

        }

        public void AddTransparencyMenu()
        {
            Trace.WriteLine("Add Transparency on Window: " + this.ToString());
            //HwndSource source = HwndSource.FromHwnd(Handle);
            //if (source != null)
            //{
            //    Trace.WriteLine("Add HwndSource Hook on Window: " + this.ToString());
            //    source.AddHook(WndProc);
            //}



            // This gets a handle to the system menu for a window. Once we have that handle, we can add our
            // own menu items.
            IntPtr windowMenuHandle = User32.GetSystemMenu(Handle, false);
            var index = User32.GetMenuItemCount(windowMenuHandle);

            _transparencyMenuHandle = User32.CreateMenu();
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)User32.Menu.MF_BYPOSITION, TransparencyPercentId + 100, "100%");
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)User32.Menu.MF_BYPOSITION, TransparencyPercentId + 95, "95%");
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)User32.Menu.MF_BYPOSITION, TransparencyPercentId + 90, "90%");
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)User32.Menu.MF_BYPOSITION, TransparencyPercentId + 85, "85%");
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)User32.Menu.MF_BYPOSITION, TransparencyPercentId + 80, "80%");
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)User32.Menu.MF_BYPOSITION, TransparencyPercentId + 75, "75%");
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)User32.Menu.MF_BYPOSITION, TransparencyPercentId + 70, "70%");
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)(User32.Menu.MF_BYPOSITION | User32.Menu.MF_SEPARATOR), 0, "");
            User32.InsertMenu(_transparencyMenuHandle, -1, (int)User32.Menu.MF_BYPOSITION, DefaultTransparencyMenuId, "Default Transparency");

            User32.InsertMenu(windowMenuHandle, index - 2, (int)(User32.Menu.MF_BYPOSITION | User32.Menu.MF_POPUP), _transparencyMenuHandle, "Transparency");
            User32.InsertMenu(_transparencyMenuHandle, -2, (int)(User32.Menu.MF_BYPOSITION | User32.Menu.MF_SEPARATOR), 0, "");
        }

        public override string ToString()
        {
            return WindowText;
        }
    }
}
