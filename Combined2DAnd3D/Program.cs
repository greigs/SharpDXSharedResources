﻿using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Combined2DAnd3D.ImageCompare;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Device1 = SharpDX.Direct3D10.Device1;

namespace Combined2DAnd3D
{

    public class Program
    {
        [DllImport("ConsoleApplication1.dll")]
        private static extern IStream StartOBS();
      

        public static void Main()
        {


            var stream = StartOBS();

            const int stringlen = 40;
            byte[] bytes = new byte[stringlen];
            int offset = 0;
  
            var str = string.Empty;


            while (string.IsNullOrWhiteSpace(str))
            {
                //Console.WriteLine(BitConverter.ToInt32(bytes, 0));
                //for (var i = 0; i < 10; i++)
                //{
                    
                    Thread.Sleep(100);
                    //stream.Seek(4, 0, NewPosition);
                    //Console.WriteLine(Marshal.ReadInt64(NewPosition));
                    //stream.Seek(0, 0, IntPtr.Zero);
                    bytes = new byte[stringlen];
                    stream.Read(bytes, stringlen, IntPtr.Zero);

                    stream.Seek(offset, 0, IntPtr.Zero);

                    //stream.Seek(4, 0, IntPtr.Zero);
                    str = string.Empty;
                    foreach (var b in bytes)
                    {
                        str += Convert.ToChar(b);
                    }

                    var trimEndIndex = str.LastIndexOf("\0\0\0\0", StringComparison.Ordinal);
                    str = str.Substring(0, trimEndIndex - 1);

                    str = str.Replace("\0", string.Empty);
                    //str = str.TrimEnd();
                    Console.WriteLine(str);
                    //Console.WriteLine(BitConverter.ToInt32(bytes, 0));
                    offset += stringlen;
                //}
                offset = 0;
            }

            var program = new Program();
                
            program.Run(long.Parse(str));
            program.ComputeSingleIconSet();
        }


        private Texture2D textureD3D10;
        private IEnumerable<Bitmap> icons;
        private Device1 device10;
        private string[] filenames = new string[]
        {
                        "sword",
                        "dirt",
                        "pickaxe",
                        "sand",
                        "steak",
                        "sword",
                        "torch",
                        "wood",
                        "iron",
                        "stone",
                        "sword_dark",
                        "dirt_dark",
                        "pickaxe_dark",
                        "sand_dark",
                        "steak_dark",
                        "sword_dark",
                        "torch_dark",
                        "wood_dark",
                        "iron_dark",
                        "stone_dark",
                        "empty"

        };

        public void Start()
        {
            Run(0);
            //new SynchronousSocketListener(this).Start();
            
        }

        public void Run(long ptr)
        {
            var factory1 = new Factory1();
            var adapter1 = factory1.GetAdapter1(0);
            
            device10 = new SharpDX.Direct3D10.Device1(adapter1);
            var ptrVal = ((long)ptr); // handle of shared texture


  

            
            icons = filenames.Select(LoadBmp);


            textureD3D10 = device10.OpenSharedResource<SharpDX.Direct3D10.Texture2D>(new IntPtr(ptrVal));
        }

        private string ComputeSingleIconSet()
        {
            string result = string.Empty;
            try
            {

                const int distancebetween = 80;
                const int start = 1920 - 1308;
                var areas = new[]
                {
                        new Rectangle(start + distancebetween * 0, 13,56,56),
                        new Rectangle(start + distancebetween * 1 ,13,56,56),
                        new Rectangle(start + distancebetween * 2 ,13,56,56),
                        new Rectangle(start + distancebetween * 3 ,13,56,56),
                        new Rectangle(start + distancebetween * 4 ,13,56,56),
                        new Rectangle(start + distancebetween * 5 ,13,56,56),
                        new Rectangle(start + distancebetween * 6 ,13,56,56),
                        new Rectangle(start + distancebetween * 7 ,13,56,56),
                        new Rectangle(start + distancebetween * 8 ,13,56,56),
                    };


                var bitmaps = textureD3D10.SplitIntoBitmapSegments(device10, areas);

                //Console.ReadLine();

                //var bitmap2 = textureD3D10.CopyToBitmap(device10);
                
                var position = 0;

                foreach (var bitmap in bitmaps)
                {
                    position++;
                    var changed = ChangePixelFormat(bitmap, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    SaveToDisk(changed);

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

                    string message;
                   

                    if (highest > 0.78f)
                    {
                        message = position + ":" + highesticonname;
                    }
                    else
                    {
                        message = position + ":empty";
                    }

                    Console.WriteLine(message);
                    if (result == string.Empty)
                    {
                        result += message;
                    }
                    else
                    {
                        result += "," + message;
                    }

                }



            }
            catch (Exception ex)
            {
            }

            return result;

        } 

        private static Bitmap LoadBmp(string filename)
        {
            var img = ChangePixelFormat(new Bitmap((Bitmap)System.Drawing.Bitmap.FromFile(filename + ".bmp")), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            img.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return img;
        }

        private static Bitmap ChangePixelFormat(Bitmap inputImage, System.Drawing.Imaging.PixelFormat newFormat)
        {
            return (inputImage.Clone(new Rectangle(0, 0, inputImage.Width, inputImage.Height), newFormat));
        }


        private static void SaveToDisk(Bitmap bitmap)
        {
            string filename = "c:\\temp\\screens\\" + DateTime.Now.Ticks + ".bmp";
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

        public string GetMessage()
        {
            return ComputeSingleIconSet();
        }
    }
}