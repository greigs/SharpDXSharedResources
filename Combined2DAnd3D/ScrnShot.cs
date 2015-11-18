using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/// <summary>
/// The ScreenShot.
/// </summary>
public static class ScreenShot
{
    /// <summary>
    /// Capturing the current screen then save image to the indicated file.
    /// </summary>
    /// <param name="file">
    /// The file.
    /// </param>
    public static void CaptureScreen(string file = null)
    {
        IntPtr screenHandle = WindowsAPI.GetDesktopWindow();
        Capture(screenHandle, file);
    }

    /// <summary>
    /// Capturing the current screen working area which does not include the task bar.
    /// </summary>
    /// <param name="file">
    /// The file.
    /// </param>
    public static void CaptureScreenWorkingArea(string file)
    {
        Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
        Capture(new Point(workingArea.X, workingArea.Y), new Point(0, 0), new Size(workingArea.Width, workingArea.Height), file);
    }

    /// <summary>
    /// Capture full screen.
    /// </summary>
    /// <param name="file">
    /// The file.
    /// </param>
    public static void CaptureFullScreen(string file)
    {
        Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
        bitmap.Save(file, ImageFormat.Jpeg);
    }

    /// <summary>
    /// Capturing the indicated application window screen and then save image to the indicated file. If the file parameter not set, function will create a random file name by GUID under the runtime directory.
    /// </summary>
    /// <param name="handle"> Window's handle. </param>
    /// <param name="file"> Target image file. </param>
    public static void CaptureApplicationScreen(IntPtr handle, string file)
    {
        if (WindowsAPI.SetForegroundWindow(handle))
        {
            Capture(handle, file);
        }
    }

    #region Helper

    /// <summary>
    /// Capture the indicated region screenshot by the indicated window.
    /// </summary>
    /// <param name="handle"> Indicated window handle. </param>
    /// <param name="file"> Save image file name. </param>
    private static void Capture(IntPtr handle, string file)
    {
        // Testing source handle.
        if (handle != IntPtr.Zero)
        {
            WindowsAPI.Rect srcRect;

            // Get the source window's information.
            if (WindowsAPI.GetWindowRect(handle, out srcRect))
            {
                int width = srcRect.Right - srcRect.Left;
                int height = srcRect.Bottom - srcRect.Top;
                Capture(srcRect.Left, srcRect.Top, width, height, file);
            }
        }
    }

    /// <summary>
    /// Capture the screenshot by coordinate from display memory.
    /// </summary>
    /// <param name="x">
    /// The x.
    /// </param>
    /// <param name="y">
    /// The y.
    /// </param>
    /// <param name="width">
    /// The width.
    /// </param>
    /// <param name="height">
    /// The height.
    /// </param>
    /// <param name="file">
    /// The file.
    /// </param>
    private static void Capture(int x, int y, int width, int height, string file)
    {
        IntPtr displayDC = IntPtr.Zero;
        IntPtr destinationDC = IntPtr.Zero;
        IntPtr bmp = IntPtr.Zero;

        try
        {
            displayDC = WindowsAPI.CreateDC("DISPLAY", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (displayDC == IntPtr.Zero)
            {
                throw new Exception("Create display DC failed!");
            }

            destinationDC = WindowsAPI.CreateCompatibleDC(displayDC);
            if (destinationDC == IntPtr.Zero)
            {
                throw new Exception("Create destination DC failed!");
            }

            bmp = WindowsAPI.CreateCompatibleBitmap(displayDC, width, height);

            // Select bmp into destination DC.
            if (WindowsAPI.SelectObject(destinationDC, bmp) == IntPtr.Zero)
            {
                throw new Exception("Select bmp into destination DC failed!");
            }

            if (0 == WindowsAPI.BitBlt(destinationDC, 0, 0, width, height, displayDC, x, y, (uint)WindowsAPI.TernaryRasterOperations.Srccopy))
            {
                throw new Exception("BitBlt failed!");
            }

            Image image = Image.FromHbitmap(bmp);

            // Save image.
            if (string.IsNullOrEmpty(file))
            {
                // Create a random file name by GUID.
                file = Guid.NewGuid().ToString();
            }

    
        }
        finally
        {
            WindowsAPI.DeleteDC(displayDC);
            WindowsAPI.DeleteDC(destinationDC);
            WindowsAPI.DeleteObject(bmp);
        }
    }

    /// <summary>
    /// Capture the screenshot by point using Graphics.CopyFromScreen.
    /// </summary>
    /// <param name="source">
    /// The source.
    /// </param>
    /// <param name="destination">
    /// The destination.
    /// </param>
    /// <param name="size">
    /// The size.
    /// </param>
    /// <param name="file">
    /// The file.
    /// </param>
    private static void Capture(Point source, Point destination, Size size, string file)
    {
        Image image = new Bitmap(size.Width, size.Height);
        Graphics g = Graphics.FromImage(image);
        g.CopyFromScreen(source, destination, size);
       
    }

    #endregion

    /// <summary>
    /// This class includes win32 api for other class calling.
    /// </summary>
    internal static class WindowsAPI
    {
        /// <summary>
        /// The ternary raster operations.
        /// </summary>
        public enum TernaryRasterOperations : uint
        {
            /// <summary>
            /// The srccopy.
            /// </summary>
            Srccopy = 0x00CC0020,

            /// <summary>
            /// The srcpaint.
            /// </summary>
            Srcpaint = 0x00EE0086,

            /// <summary>
            /// The srcand.
            /// </summary>
            Srcand = 0x008800C6,

            /// <summary>
            /// The srcinvert.
            /// </summary>
            Srcinvert = 0x00660046,

            /// <summary>
            /// The srcerase.
            /// </summary>
            Srcerase = 0x00440328,

            /// <summary>
            /// The notsrccopy.
            /// </summary>
            Notsrccopy = 0x00330008,

            /// <summary>
            /// The notsrcerase.
            /// </summary>
            Notsrcerase = 0x001100A6,

            /// <summary>
            /// The mergecopy.
            /// </summary>
            Mergecopy = 0x00C000CA,

            /// <summary>
            /// The mergepaint.
            /// </summary>
            Mergepaint = 0x00BB0226,

            /// <summary>
            /// The patcopy.
            /// </summary>
            Patcopy = 0x00F00021,

            /// <summary>
            /// The patpaint.
            /// </summary>
            Patpaint = 0x00FB0A09,

            /// <summary>
            /// The patinvert.
            /// </summary>
            Patinvert = 0x005A0049,

            /// <summary>
            /// The dstinvert.
            /// </summary>
            Dstinvert = 0x00550009,

            /// <summary>
            /// The blackness.
            /// </summary>
            Blackness = 0x00000042,

            /// <summary>
            /// The whiteness.
            /// </summary>
            Whiteness = 0x00FF0062
        }

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

        /// <summary>
        /// The rect.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            /// <summary>
            /// The left.
            /// </summary>
            public readonly int Left;

            /// <summary>
            /// The top.
            /// </summary>
            public readonly int Top;

            /// <summary>
            /// The right.
            /// </summary>
            public readonly int Right;

            /// <summary>
            /// The bottom.
            /// </summary>
            public readonly int Bottom;
        }
    }
}