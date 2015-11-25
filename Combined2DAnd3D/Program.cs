using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
            var ptrVal = ((long)-1073728894); // handle of shared texture

            var filenames = new string[]
            {
                "sword",
                "dirt",
                "pickaxe",
                "sand",
                "steak",
                "sword",
                "torch",
                "wood"
            };


            var icons = filenames.Select(LoadBmp);

            //var swordBitmap2 = (Bitmap)System.Drawing.Bitmap.FromFile("635834899909840625.png");
            //TestComparison(swordBitmap2, swordBitmap);

            var textureD3D10 = device10.OpenSharedResource<SharpDX.Direct3D10.Texture2D>(new IntPtr(ptrVal));



             while (true)
             {
                 try
                 {
                     //Console.ReadLine();

                     var areas = new[]
                     {
                         new Rectangle(1920 - 1148 ,13,56,56),
                         //new Rectangle(100,100,56,56)
                     };


                     var bitmaps = textureD3D10.SplitIntoBitmapSegments(device10, areas);

                     for (int i = 0; i < areas.Length; i++)
                     {
                         //SaveToDisk(bitmaps[i]);
                     }


                    //Console.ReadLine();

                    //var bitmap2 = textureD3D10.CopyToBitmap(device10);
                    //SaveToDisk(bitmap2);
                    var changed = ChangePixelFormat(bitmaps[0], System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    //SaveToDisk(changed);

                     var highest = 0f;
                     string highesticonname = null;

                    var count = 0;
                    foreach (var icon in icons)
                    {
                        var tmp = TestComparison(changed, icon);
                         if (tmp > highest)
                         {
                             highest = tmp;
                             highesticonname = filenames[count];
                         }
                        count++;
                    }
                    
                     
                     //SaveToDisk(swordBitmap);
                     Console.WriteLine(highest + " " + highesticonname);

                     Thread.Sleep(3000);



                 }
                 catch (Exception ex)
                 {
                 }
             }
        }

        private static Bitmap LoadBmp(string filename)
        {
            return ChangePixelFormat(new Bitmap((Bitmap)System.Drawing.Bitmap.FromFile(filename + ".bmp")), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }

        private static Bitmap ChangePixelFormat(Bitmap inputImage, System.Drawing.Imaging.PixelFormat newFormat)
        {
            return (inputImage.Clone(new Rectangle(0, 0, inputImage.Width, inputImage.Height), newFormat));
        }


        private static void SaveToDisk(Bitmap bitmap)
        {
            string filename = "c:\\temp\\screens\\" + DateTime.Now.Ticks + ".png";
            var stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
            bitmap.Save(stream, ImageFormat.Bmp);
            stream.Flush();
            stream.Close();
            stream.Dispose();
        }

        private static float TestComparison(Bitmap bitmap, Bitmap bitmap2)
        {
            // The threshold is the minimal acceptable similarity between template candidate. 
            // Min (loose) is 0.0 Max (strict) is 1.0
             float similarityThreshold = 0.5f;


                // Run the tests
                //var testOne = ImageComparer.CompareImagesSlow(bitmap, bitmap, compareLevel, similarityThreshold);
                var test = ImageComparer.CompareImagesSlow(bitmap, bitmap2, similarityThreshold);


            return test;

        }
    }
}