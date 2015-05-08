using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System;
using Combined2DAnd3D.ImageCompare;
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
            var ptrVal = ((long)1073754690); // handle of shared texture

            var textureD3D10 = device10.OpenSharedResource<SharpDX.Direct3D10.Texture2D>(new IntPtr(ptrVal));

            while (true)
            {
                try
                {

                        string filename = "c:\\temp\\screens\\" + DateTime.Now.Ticks + ".png";
                        var stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                        var bitmap = textureD3D10.CopyToBitmap(stream, device10);
                        bitmap.Save(stream,ImageFormat.Bmp);                        
                        stream.Flush();
                        stream.Close();
                        stream.Dispose();

                        Thread.Sleep(5000);

                        filename = "c:\\temp\\screens\\" + DateTime.Now.Ticks + ".png";
                        stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                        var bitmap2 = textureD3D10.CopyToBitmap(stream, device10);
                        bitmap2.Save(stream, ImageFormat.Bmp);
                        stream.Flush();
                        stream.Close();
                        stream.Dispose();



                        TestComparison(bitmap, bitmap2);
                }
                catch
                {

                }

            }

        }

        private static void TestComparison(Bitmap bitmap, Bitmap bitmap2)
        {
            const float similarityThreshold = 0.50f;


            // Comparison level is initially set to 0.95
            // Increment loop in steps of .01
            for (var compareLevel = 0.95; compareLevel <= 1.00; compareLevel += 0.01)
            {
                // Run the tests
                var testOne = ImageComparer.CompareImagesSlow(bitmap, bitmap, compareLevel, similarityThreshold);
                var testTwo = ImageComparer.CompareImagesSlow(bitmap, bitmap2, compareLevel, similarityThreshold);

                // Output the results
                Console.WriteLine("Test images for similarities at compareLevel: {0}", compareLevel);
                Console.WriteLine("Image 1 compared to Image 1 - {0}", testOne);
                Console.WriteLine("Image 1 compared to Image 2 - {0}", testTwo);
            }

            Console.WriteLine("End of comparison.");
            Console.ReadLine();
        }
    }
}