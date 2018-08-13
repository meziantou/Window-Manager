using System.Drawing;
using System.Runtime.InteropServices;

namespace WindowManager.Core.NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
        public Rectangle Rect => new Rectangle(this.left, this.top, this.right - this.left, this.bottom - this.top);
        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x,
                            y,
                            x + width,
                            y + height);
        }
        public static RECT FromRectangle(Rectangle rect)
        {
            return new RECT(rect.Left,
                            rect.Top,
                            rect.Right,
                            rect.Bottom);
        }

        public override string ToString()
        {
            return "{left=" + left + ", " + "top=" + top + ", " +
                   "right=" + right + ", " + "bottom=" + bottom + "}";
        }
    }
}