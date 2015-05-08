using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System;
using SharpDX.DXGI;

namespace Combined2DAnd3D
{

    internal static class Program
    {

        public static void Main()
        {
            Run();
        }

        public static void Run()
        {
            var factory1 = new Factory1();
            var adapter1 = factory1.GetAdapter1(0);
            var device10 = new SharpDX.Direct3D10.Device1(adapter1);
            var ptrVal = ((long)1073752770); // handle of shared texture

            var textureD3D10 = device10.OpenSharedResource<SharpDX.Direct3D10.Texture2D>(new IntPtr(ptrVal));

            while (true)
            {
                try
                {

                    if (true)// if (result.Success)
                    {
                        
                        string filename = "c:\\temp\\screens\\" + DateTime.Now.Ticks + ".png";
                        var stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                        var bitmap = textureD3D10.CopyToBitmap(stream, device10);
                        bitmap.Save(stream,ImageFormat.Bmp);                        
                        stream.Flush();
                        stream.Close();
                        stream.Dispose();
                    }
                }
                catch
                {

                }


                Thread.Sleep(1000);

            }

        }
    }
}