namespace Combined2DAnd3D
{
    using System;
    using System.Drawing;
    using AForge.Imaging;

    namespace ImageCompare
    {
        /// <summary>
        /// Image comparison class to match and rate if bitmapped images are similar.
        /// </summary>
        public static class ImageComparer
        {
            /// <summary>
            /// Compares the images.
            /// </summary>
            /// <param name="imageOne"></param>
            /// <param name="imageTwo"></param>
            /// <param name="compareLevel">The compare level.</param>
            /// <param name="similarityThreshold">The similarity threshold.</param>
            /// <returns>Boolean result</returns>
            public static Boolean CompareImagesSlow(Bitmap imageOne, Bitmap imageTwo, double compareLevel, float similarityThreshold)
            {            
                var newBitmap1 = ChangePixelFormat(new Bitmap(imageOne), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var newBitmap2 = ChangePixelFormat(new Bitmap(imageTwo), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // Setup the AForge library
                var tm = new ExhaustiveTemplateMatching(similarityThreshold);

                // Process the images
                var results = tm.ProcessImage(newBitmap1, newBitmap2);

                // Compare the results, 0 indicates no match so return false
                if (results.Length <= 0)
                {
                    return false;
                }

                // Return true if similarity score is equal or greater than the comparison level
                var match = results[0].Similarity >= compareLevel;

                return match;
            }

            /// <summary>
            /// Change the pixel format of the bitmap image
            /// </summary>
            /// <param name="inputImage">Bitmapped image</param>
            /// <param name="newFormat">Bitmap format - 24bpp</param>
            /// <returns>Bitmap image</returns>
            private static Bitmap ChangePixelFormat(Bitmap inputImage, System.Drawing.Imaging.PixelFormat newFormat)
            {
                return (inputImage.Clone(new Rectangle(0, 0, inputImage.Width, inputImage.Height), newFormat));
            }
        }
    }
}
