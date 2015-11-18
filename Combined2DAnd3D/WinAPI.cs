using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Combined2DAnd3D
{
    class WinAPI
    {

        // Methods
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern int BitBlt(IntPtr destDc, int xDest, int yDest, int width, int height, IntPtr sourceDc, int xSource, int ySource, uint rasterOperation);
        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr dc, int width, int height);
        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll", EntryPoint = "CreateDCW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr CreateDC(string driver, IntPtr device, IntPtr output, IntPtr devMode);
        [DllImport("gdi32.dll", ExactSpelling = true)]
        internal static extern int DeleteDC(IntPtr dc);
        [DllImport("gdi32.dll", ExactSpelling = true)]
        internal static extern int DeleteObject(IntPtr gdiObject);
        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr SelectObject(IntPtr dc, IntPtr gdi);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
