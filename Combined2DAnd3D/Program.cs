/*
 * Shared resource between Direct 2D and Direct 3D 11 using Sharp DX
 *  
 * This is a 'one file' example of a shared surfce between two direct X devices written in C# / SharpDX. 
 * Windows 8 Direct2D.1 has native support for this but as long as you are on Windows7 you need some trickery.
 *
 * Direct2D only works when you create a Direct3D 10.1 device, but it can share surfaces with Direct3D 11. 
 * All you need to do is create both devices and render all of your Direct2D content to a texture that you share between them. 
 * 
 * A basic outline of the process you will need to use is:
 * 
 * - Create your Direct3D 11 device like you do normally.
 * - Create a texture with the D3D10_RESOURCE_MISC_SHARED_KEYEDMUTEX option in order to allow access to the ID3D11KeyedMutex interface.
 * - Use the GetSharedHandle to get a handle to the texture that can be shared among devices.
 * - Create a Direct3D 10.1 device, ensuring that it is created on the same adapter.
 * - Use OpenSharedResource function on the Direct3D 10.1 device to get a version of the texture for Direct3D 10.1.
 * - Get access to the D3D10 KeyedMutex interface for the texture.
 * - Use the Direct3D 10.1 version of the texture to create the RenderTarget using Direct2D.
 * - When you want to render with D2D, use the keyed mutex to lock the texture for the D3D10 device. Then, acquire it in D3D11 and render the texture like you were probably already trying to do.
 *  	
 * It's not trivial, but it works well, and it is the way that they intended you to interoperate between them.
 *
 * http://stackoverflow.com/a/9071915* 
 * https://github.com/enix/SharpDXSharedResources
 *
 * Coded by Aaron Auseth and Ernst Naezer
 * 
 * Freeware: The author, of this software accepts no responsibility for damages resulting from the use of this product and makes no warranty or representation, 
 * either express or implied, including but not limited to, any implied warranty of merchantability or fitness for a particular purpose. 
 * This software is provided "AS IS", and you, its user, assume all risks when using it.
 * 
 * All I ask is that I be given credit if you use as a tutorial or for educational purposes.
 */

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using SharpDX.WIC;

namespace Combined2DAnd3D
{
    using System;
    using System.Windows.Forms;
    using SharpDX;
    using SharpDX.D3DCompiler;
    using SharpDX.DXGI;
    using SharpDX.Direct2D1;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.Windows;
    using Buffer = SharpDX.Direct3D11.Buffer;
    using System.Windows.Media.Imaging;
    using SharpDX.Toolkit.Graphics;
    
    using Device11 = SharpDX.Direct3D11.Device;
    using Device10 = SharpDX.Direct3D10.Device1;

    using FeatureLevel = SharpDX.Direct3D10.FeatureLevel;
    using Resource = SharpDX.Direct3D11.Resource;

    internal static class Program
    {


        public static void Main()
        {
            Run();
        }

        private static SharpDX.Direct3D10.Texture2D GetCopy(this SharpDX.Direct3D10.Texture2D tex)
        {
            var teximg = new SharpDX.Direct3D10.Texture2D(tex.Device, new SharpDX.Direct3D10.Texture2DDescription
            {
                Usage = SharpDX.Direct3D10.ResourceUsage.Staging,
                BindFlags = SharpDX.Direct3D10.BindFlags.None,
                CpuAccessFlags = SharpDX.Direct3D10.CpuAccessFlags.Read,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                OptionFlags = SharpDX.Direct3D10.ResourceOptionFlags.None,
                ArraySize = tex.Description.ArraySize,
                Height = tex.Description.Height,
                Width = tex.Description.Width,
                MipLevels = tex.Description.MipLevels,
                SampleDescription = tex.Description.SampleDescription,
            });
            tex.Device.CopyResource(tex, teximg);
            return teximg;
        }

        /*
        public static unsafe WriteableBitmap GetBitmap(this SharpDX.Direct3D10.Texture2D tex)
        {
            DataRectangle db;
            using (var copy = tex.GetCopy())
            using (var surface = copy.QueryInterface<SharpDX.DXGI.Surface>())
            {
                // can't destroy the surface now with WARP driver
                DataStream ds;
                db = surface.Map(SharpDX.DXGI.MapFlags.Read, out ds);

                int w = tex.Description.Width;
                int h = tex.Description.Height;
                var wb = new WriteableBitmap(w, h, 96.0, 96.0, PixelFormats.Bgra32, null);
                wb.Lock();
                try
                {
                    uint* wbb = (uint*) wb.BackBuffer;

                    ds.Position = 0;
                    for (int y = 0; y < h; y++)
                    {
                        ds.Position = y*db.Pitch;
                        for (int x = 0; x < w; x++)
                        {
                            var c = ds.Read<uint>();
                            wbb[y*w + x] = c;
                        }
                    }
                    ds.Dispose();
                }
                finally
                {
                    wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
                    wb.Unlock();
                }
                return wb;
            }
        }*/



        public static void Run()
        {
            var form = new RenderForm("2d and 3d combined...it's like magic");
            form.KeyDown += (sender, args) => { if (args.KeyCode == Keys.Escape) form.Close(); };

            // DirectX DXGI 1.1 factory
            var factory1 = new Factory1();

            // The 1st graphics adapter
            var adapter1 = factory1.GetAdapter1(0);

            // ---------------------------------------------------------------------------------------------
            // Setup direct 3d version 11. It's context will be used to combine the two elements
            // ---------------------------------------------------------------------------------------------

            var description = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Sequential,
                Usage = Usage.Shared,
                Flags = SwapChainFlags.AllowModeSwitch
            };

            Device11 device11;
            SwapChain swapChain;

            Device11.CreateWithSwapChain(adapter1, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug, description, out device11, out swapChain);

            // create a view of our render target, which is the backbuffer of the swap chain we just created
            //RenderTargetView renderTargetView;
            //using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
                //renderTargetView = new RenderTargetView(device11, resource);

            // setting a viewport is required if you want to actually see anything
            //var context = device11.ImmediateContext;


            //
            // Create the DirectX11 texture2D. This texture will be shared with the DirectX10 device. 
            //
            // The DirectX10 device will be used to render text onto this texture.  
            // DirectX11 will then draw this texture (blended) onto the screen.
            // The KeyedMutex flag is required in order to share this resource between the two devices.
            /*var textureD3D11 = new Texture2D(device11, new Texture2DDescription
            {
                Width = form.ClientSize.Width,
                Height = form.ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.SharedKeyedmutex
            });
            */



            // ---------------------------------------------------------------------------------------------
            // Setup a direct 3d version 10.1 adapter
            // ---------------------------------------------------------------------------------------------
            var device10 = new Device10(adapter1,SharpDX.Direct3D10.DeviceCreationFlags.Debug);


            // # of graphics card adapter
            const int numAdapter = 0;

            // # of output device (i.e. monitor)
            const int numOutput = 0;

            // Create DXGI Factory1
            //var factory = new Factory1();
            //var adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            //var device = new Device11(adapter);


            var ptrVal = ((long)1073743362);

            var textureD3D10temp = device10.OpenSharedResource<SharpDX.Direct3D10.Texture2D>(new IntPtr(ptrVal));

            //var mutex = textureD3D11temp.QueryInterface<KeyedMutex>();

            while (true)
            {

                var released = false;
                //textureD3D10temp.GetBitmap();
                try
                {

                    //var result = mutex.Acquire(0, 10000);
                    if (true)// if (result.Success)
                    {
                        


                        int width = textureD3D10temp.Description.Width;
                        int height = textureD3D10temp.Description.Height;


                        /*

                        var textureCopy = new SharpDX.Direct3D11.Texture2D(device11, new SharpDX.Direct3D11.Texture2DDescription
                        {
                            Width = (int)textureD3D11temp.Description.Width,
                            Height = (int)textureD3D11temp.Description.Height,
                            MipLevels = 1,
                            ArraySize = 1,
                            Format = textureD3D11temp.Description.Format,
                            Usage = SharpDX.Direct3D11.ResourceUsage.Staging,
                            SampleDescription = new SampleDescription(1, 0),
                            BindFlags = SharpDX.Direct3D11.BindFlags.None,
                            CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read,
                            OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None
                        });

                        device11.ImmediateContext.CopyResource(textureD3D11temp, textureCopy);*/

                        string filename = "c:\\temp\\screens\\" + DateTime.Now.Ticks + ".png";
                        var stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                        textureD3D10temp.Save(stream, device10);
                        stream.Flush();
                        stream.Close();
                        stream.Dispose();

                        //DataStream stream;
                        //var mapSource = textureCopy.Map(0, SharpDX.Direct3D10.MapMode.Read,SharpDX.Direct3D10.MapFlags.None);






                        /*



                        var bitmap = new System.Drawing.Bitmap(width, height,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        var boundsRect = new System.Drawing.Rectangle(0, 0, width, height);



                        // Copy pixels from screen capture Texture to GDI bitmap
                        var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        var sourcePtr = mapSource.DataPointer;
                        
                        var destPtr = mapDest.Scan0;
                        for (int y = 0; y < height; y++)
                        {
                            // Copy a single line 
                            Utilities.CopyMemory(destPtr, sourcePtr, mapDest.Stride);

                            // Advance pointers
                            sourcePtr = IntPtr.Add(sourcePtr, mapSource.Pitch);
                            destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                        }
                        


                        // Release source and dest locks
                        bitmap.UnlockBits(mapDest);
                        //device.ImmediateContext.UnmapSubresource(textureD3D10temp, 0);

                        mutex.Release(0);
                        released = true;


                        var path = "c:\\temp\\screens\\" + DateTime.Now.Ticks + ".bmp";

                        // Save the output
                        bitmap.Save(path, ImageFormat.Jpeg);
                        bitmap.Dispose();*/
                    }
                }
                catch
                {

                }finally 
                {
                    if (!released)
                    {
                       // mutex.Release(0);
                    }
                   
                }

                Thread.Sleep(100);

            }





            //textureD3D10temp.GetBitmap().WriteTga(new FileStream("c:\\test.tga",FileMode.OpenOrCreate));
            

            //int stride = (int)bitmapSource.PixelWidth * (bitmapSource.Format.BitsPerPixel / 8);
            //byte[] pixels = new byte[(int)bitmapSource.PixelHeight * stride];

            //bitmapSource.CopyPixels(pixels, stride, 0);




            //File.WriteAllBytes(path, pixels);

        }
    }
}