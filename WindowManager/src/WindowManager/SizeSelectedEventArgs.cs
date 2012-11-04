using System;
using System.Drawing;

namespace WindowManager
{
    public class SizeSelectedEventArgs : EventArgs
    {
        public SizeSelectedEventArgs(Rectangle size, bool maximized)
        {
            Rectangle = size;
            Maximized = maximized;
        }

        public Rectangle Rectangle { get; private set; }

        public bool Maximized { get; private set; }
    }
}