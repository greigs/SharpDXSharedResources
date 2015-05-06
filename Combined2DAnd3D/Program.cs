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
    using SharpDX.DXGI;

    


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
            var device10 = new SharpDX.Direct3D10.Device1(adapter1, SharpDX.Direct3D10.DeviceCreationFlags.Debug);
            var ptrVal = ((long)1073759426); // handle of shared texture

            var textureD3D10 = device10.OpenSharedResource<SharpDX.Direct3D10.Texture2D>(new IntPtr(ptrVal));

            while (true)
            {
                try
                {

                    if (true)// if (result.Success)
                    {
                        
                        string filename = "c:\\temp\\screens\\" + DateTime.Now.Ticks + ".png";
                        var stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                        textureD3D10.Save(stream, device10);
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