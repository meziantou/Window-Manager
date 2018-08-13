using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using WindowManager.Core.NativeMethods;

namespace WindowManager.Core
{
    public delegate void WindowCreatedEventHandler(object sender, Win32Window window);

    public delegate void WindowDestroyedEventHandler(object sender, Win32Window window);

    public sealed class WindowCollection : IDisposable, IEnumerable<Win32Window>
    {
        private static WindowCollection _instance;
        private readonly Dictionary<IntPtr, Win32Window> _windows = new Dictionary<IntPtr, Win32Window>();
        private ShellHook _shellHook;

        private WindowCollection(IntPtr handle)
        {
            _shellHook = new ShellHook(handle);
            _shellHook.WindowCreated += ShellHookWindowCreated;
            _shellHook.WindowDestroyed += ShellHookWindowDestroyed;
            _shellHook.EnumWindows();
        }

        public static WindowCollection Instance => _instance;

        public void Dispose()
        {
            if (_shellHook == null)
                return;

            Trace.WriteLine("Dispose Shell Hook");
            _shellHook.Dispose();
            _shellHook = null;
        }

        public IEnumerator<Win32Window> GetEnumerator()
        {
            return _windows.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event WindowCreatedEventHandler WindowCreated;
        public event WindowDestroyedEventHandler WindowDestroyed;

        public static void Initialize(IntPtr handle)
        {
            if (Instance != null)
                throw new InvalidOperationException("WindowCollection already initialized.");

            var windowCollection = new WindowCollection(handle);
            _instance = windowCollection;
        }

        private void ShellHookWindowCreated(ShellHook sender, IntPtr hWnd)
        {
            if (_windows.TryGetValue(hWnd, out Win32Window window))
            {
                window = new Win32Window(hWnd);
                _windows.Add(hWnd, window);
            }

            OnWindowCreated(window);
        }

        private void ShellHookWindowDestroyed(ShellHook sender, IntPtr hwnd)
        {
            if (_windows.TryGetValue(hwnd, out Win32Window window))
            {
                OnWindowDestroyed(window);
                _windows.Remove(hwnd);
            }
        }

        private void OnWindowDestroyed(Win32Window window)
        {
            WindowDestroyed?.Invoke(this, window);
        }

        private void OnWindowCreated(Win32Window window)
        {
            WindowCreated?.Invoke(this, window);
        }
    }

    public unsafe delegate void GetMinRectShellHookEventHandler(ShellHook sender, SHELLHOOKINFO* rect);

    public delegate void GeneralShellHookEventHandler(ShellHook sender, IntPtr hWnd);

    public delegate void AppcommandShellHookEventHandler(ShellHook sender, APPCOMMAND command);

    public struct APPCOMMAND
    {
    }

    public sealed class ShellHook : NativeWindow, IDisposable
    {
        private readonly uint _wmShellHook;

        public ShellHook(IntPtr hWnd)
        {
            var cp = new CreateParams();

            /*// Fill in the CreateParams details.
            cp.Caption = "Click here";
            cp.ClassName = "Button";

            // Set the position on the form
            cp.X = 100;
            cp.Y = 100;
            cp.Height = 100;
            cp.Width = 100;

            // Specify the form as the parent.
            cp.Parent = User32.GetDesktopWindow();

            // Create as a child of the specified parent
            cp.Style = (int) WindowStyle.WS_POPUP;// WS_CHILD | WS_VISIBLE;
            cp.ExStyle = (int)WindowStyleEx.WS_EX_TOOLWINDOW;*/

            // Create the actual window
            CreateHandle(cp);


            // User32.SetShellWindow(hWnd);
            User32.SetTaskmanWindow(hWnd);
            if (User32.RegisterShellHookWindow(Handle))
            {
                _wmShellHook = User32.RegisterWindowMessage("SHELLHOOK");
                Trace.WriteLine("WM_ShellHook: " + _wmShellHook);
            }
        }

        /// <summary>
        ///   A top-level, unowned window has been created. The window exists when the system calls this hook.
        /// </summary>
        public event GeneralShellHookEventHandler WindowCreated;

        /// <summary>
        ///   A top-level, unowned window is about to be destroyed. The window still exists when the system calls this hook.
        /// </summary>
        public event GeneralShellHookEventHandler WindowDestroyed;

        /// <summary>
        ///   The shell should activate its main window.
        /// </summary>
        public event GeneralShellHookEventHandler ActivateShellWindow;

        /// <summary>
        ///   The activation has changed to a different top-level, unowned window.
        /// </summary>
        public event GeneralShellHookEventHandler WindowActivated;

        /// <summary>
        ///   A window is being minimized or maximized. The system needs the coordinates of the minimized rectangle for the window.
        /// </summary>
        public event GetMinRectShellHookEventHandler GetMinRect;

        /// <summary>
        ///   The title of a window in the task bar has been redrawn.
        /// </summary>
        public event GeneralShellHookEventHandler Redraw;

        /// <summary>
        ///   The user has selected the task list. A shell application that provides a task list should return TRUE to prevent Microsoft Windows from starting its task list.
        /// </summary>
        public event GeneralShellHookEventHandler Taskman;

        /// <summary>
        ///   Keyboard language was changed or a new keyboard layout was loaded.
        /// </summary>
        public event GeneralShellHookEventHandler Language;

        public event GeneralShellHookEventHandler Sysmenu;
        public event GeneralShellHookEventHandler EndTask;

        /// <summary>
        ///   Windows 2000/XP: The accessibility state has changed.
        /// </summary>
        public event GeneralShellHookEventHandler Accessibilitystate;

        /// <summary>
        ///   Windows 2000/XP: The user completed an input event (for example, pressed an application command button on the mouse or an application command key on the keyboard), and the application did not handle the WM_APPCOMMAND message generated by that input.
        /// </summary>
        public event AppcommandShellHookEventHandler Appcommand;

        /// <summary>
        ///   Windows XP: A top-level window is being replaced. The window exists when the system calls this hook.
        /// </summary>
        public event GeneralShellHookEventHandler WindowReplaced;

        public event GeneralShellHookEventHandler WindowReplacing;
        public event GeneralShellHookEventHandler Flash;
        public event GeneralShellHookEventHandler RudeAppActivated;

        protected override unsafe void WndProc(ref Message m)
        {
            if (m.Msg == _wmShellHook)
            {
                switch ((User32.ShellEvents)m.WParam)
                {
                    case User32.ShellEvents.HSHELL_WINDOWCREATED:
                        if (IsAppWindow(m.LParam))
                        {
                            OnWindowCreated(m.LParam);
                        }
                        break;
                    case User32.ShellEvents.HSHELL_WINDOWDESTROYED:
                        WindowDestroyed?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_ACTIVATESHELLWINDOW:
                        ActivateShellWindow?.Invoke(this, IntPtr.Zero);
                        break;
                    case User32.ShellEvents.HSHELL_WINDOWACTIVATED:
                        WindowActivated?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_GETMINRECT:
                        if (GetMinRect != null)
                        {
                            var ptr = (SHELLHOOKINFO*)m.LParam.ToPointer();
                            GetMinRect(this, ptr);
                        }
                        break;
                    case User32.ShellEvents.HSHELL_REDRAW:
                        Redraw?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_TASKMAN:
                        Taskman?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_LANGUAGE:
                        Language?.Invoke(this, IntPtr.Zero);
                        break;
                    case User32.ShellEvents.HSHELL_SYSMENU:
                        Sysmenu?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_ENDTASK:
                        EndTask?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_ACCESSIBILITYSTATE:
                        Accessibilitystate?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_APPCOMMAND:
                        if (Appcommand != null)
                        {
                            throw new NotSupportedException("APPCOMMAND event currently not supported by shellhook");
                        }
                        break;
                    case User32.ShellEvents.HSHELL_WINDOWREPLACED:
                        WindowReplaced?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_WINDOWREPLACING:
                        WindowReplacing?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_FLASH:
                        Flash?.Invoke(this, m.LParam);
                        break;
                    case User32.ShellEvents.HSHELL_RUDEAPPACTIVATED:
                        RudeAppActivated?.Invoke(this, m.LParam);
                        break;
                    default:
                        break;
                }
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            User32.RegisterShellHook(Handle, 0);
        }

        public void EnumWindows()
        {
            User32.EnumWindows(EnumWindowsProc, IntPtr.Zero);
        }

        private bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            if (IsAppWindow(hWnd))
            {
                OnWindowCreated(hWnd);
            }
            return true;
        }

        private bool OnWindowCreated(IntPtr hWnd)
        {
            WindowCreated?.Invoke(this, hWnd);

            return true;
        }

        private bool IsAppWindow(IntPtr hWnd)
        {
            if ((User32.GetWindowLong(hWnd, User32.WindowLongs.GWL_STYLE) & (int)User32.WindowStyle.WS_SYSMENU) == 0)
                return false;

            if (User32.IsWindowVisible(hWnd))
            {
                if ((User32.GetWindowLong(hWnd, User32.WindowLongs.GWL_EXSTYLE) &
                     (int)User32.WindowStyle.WS_EX_TOOLWINDOW) == 0)
                {
                    if (User32.GetParent(hWnd) != IntPtr.Zero)
                    {
                        IntPtr hwndOwner = User32.GetWindow(hWnd, User32.GetWindowEnum.GW_OWNER);
                        return hwndOwner != IntPtr.Zero &&
                               ((User32.GetWindowLong(hwndOwner, User32.WindowLongs.GWL_STYLE) &
                                 ((int)User32.WindowStyle.WS_VISIBLE | (int)User32.WindowStyle.WS_CLIPCHILDREN)) !=
                                ((int)User32.WindowStyle.WS_VISIBLE | (int)User32.WindowStyle.WS_CLIPCHILDREN)) ||
                               (User32.GetWindowLong(hwndOwner, User32.WindowLongs.GWL_EXSTYLE) &
                                (int)User32.WindowStyle.WS_EX_TOOLWINDOW) != 0;
                    }
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}