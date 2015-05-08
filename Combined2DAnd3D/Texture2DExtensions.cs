using System;
using System.Drawing;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Rectangle = System.Drawing.Rectangle;

namespace Combined2DAnd3D
{
    public static class Texture2DExtensions
    {

        public static Bitmap[] SplitIntoBitmapSegments(this Texture2D texture, SharpDX.Direct3D10.Device device, System.Drawing.Rectangle[] areas)
        {
            var textureCopy = new Texture2D(device, new Texture2DDescription
            {
                Width = texture.Description.Width,
                Height = texture.Description.Height,
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

            var bitmaps = new Bitmap[areas.Length];


            for(var i = 0; i < areas.Length; i++)
            {
                var area = areas[i];
                var bitmap = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppArgb);
                var boundsRect = new Rectangle(0,0,area.Width,area.Height);


                // Copy pixels from screen capture Texture to GDI bitmap
                var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                var sourcePtr = dataBox.DataPointer;

                var destPtr = mapDest.Scan0;
                
                // set start position
                // get to the right row
                sourcePtr = IntPtr.Add(sourcePtr, dataBox.Pitch * area.Top);

                for (int y = 0; y < area.Height; y++)
                {
                    // Move to the start position within this row
                    sourcePtr = IntPtr.Add(sourcePtr, 4 * (area.Left));
                    // Copy the relevant pixels in this row
                    Utilities.CopyMemory(destPtr, sourcePtr, 4 * area.Width);

                    // Advance pointer to the end of the copied data
                    sourcePtr = IntPtr.Add(sourcePtr, area.Width * 4);

                    // Advance pointers to next row
                    sourcePtr = IntPtr.Add(sourcePtr, dataBox.Pitch - (4 * (area.Right)));
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }

                bitmap.UnlockBits(mapDest);

                bitmaps[i] = bitmap;
                
            }

            return bitmaps;

        }


        public static System.Drawing.Bitmap CopyToBitmap(this Texture2D texture, SharpDX.Direct3D10.Device device)
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