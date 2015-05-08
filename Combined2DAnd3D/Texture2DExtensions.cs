using System;
using System.Drawing.Imaging;
using System.IO;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;

namespace Combined2DAnd3D
{
    public static class Texture2DExtensions
    {

        public static System.Drawing.Bitmap CopyToBitmap(this Texture2D texture, Stream stream, SharpDX.Direct3D10.Device device)
        {
            var textureCopy = new Texture2D(device, new Texture2DDescription
            {
                Width = (int) texture.Description.Width,
                Height = (int) texture.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = texture.Description.Format,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            });
            device.CopyResource(texture, textureCopy);

            DataStream mipsize;
            var dataBox = textureCopy.Map(
                0,
                MapMode.Read,
                SharpDX.Direct3D10.MapFlags.None,
                out mipsize);


            var bitmap = new System.Drawing.Bitmap(textureCopy.Description.Width, textureCopy.Description.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var boundsRect = new System.Drawing.Rectangle(0, 0, textureCopy.Description.Width, textureCopy.Description.Height);



            // Copy pixels from screen capture Texture to GDI bitmap
            var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var sourcePtr = dataBox.DataPointer;

            var destPtr = mapDest.Scan0;
            for (int y = 0; y < textureCopy.Description.Height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, dataBox.Pitch);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, dataBox.Pitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            bitmap.UnlockBits(mapDest);

            return bitmap;
        }
    }
}