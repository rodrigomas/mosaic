using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    class Utils
    {

        public static Image ByteToImage(byte[] imageBytes)
        {
            if (imageBytes == null) return null;
            MemoryStream ms = new MemoryStream(imageBytes);

            Image returnImage = Image.FromStream(ms);

            return returnImage;
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }


        public static float Luminance(System.Drawing.Color color)
        {
            return (float)(Math.Sqrt( 0.241 * Math.Pow(color.R / 255.0, 2)  + 0.691* Math.Pow( color.G / 255.0, 2 ) + 0.068* Math.Pow(color.B  / 255.0, 2 )));
        }

        /// <summary>
        /// Gets a hash of the file using SHA1.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetSHA1Hash(string filePath)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
                return GetHash(filePath, sha1);
        }

        /// <summary>
        /// Gets a hash of the file using SHA1.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetSHA1Hash(Stream s)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
                return GetHash(s, sha1);
        }

        /// <summary>
        /// Gets a hash of the file using MD5.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string filePath)
        {
            using (var md5 = new MD5CryptoServiceProvider())
                return GetHash(filePath, md5);
        }

        /// <summary>
        /// Gets a hash of the file using MD5.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5Hash(Stream s)
        {
            using (var md5 = new MD5CryptoServiceProvider())
                return GetHash(s, md5);
        }

        private static string GetHash(string filePath, HashAlgorithm hasher)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return GetHash(fs, hasher);
        }

        private static string GetHash(Stream s, HashAlgorithm hasher)
        {
            var hash = hasher.ComputeHash(s);
            var hashStr = Convert.ToBase64String(hash);
            return hashStr.TrimEnd('=');
        }

        public static bool ThumbnailCallback()
        {
            return false;
        }

        internal static System.Drawing.Color GetColorMap(System.Drawing.Bitmap bitmap)
        {
            //Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);

//            Color c = Color.Black;
            AForge.Imaging.ImageStatistics stat = new AForge.Imaging.ImageStatistics(bitmap);

           /* using (Bitmap img = bitmap.GetThumbnailImage(1, 1, myCallback, IntPtr.Zero) as Bitmap)
            {
                c = img.GetPixel(0, 0);
            }*/

            return Color.FromArgb((int)(stat.Red.Mean), (int)(stat.Green.Mean), (int)(stat.Blue.Mean));
        }

        internal static System.Drawing.Color [] GetColorMapArray(System.Drawing.Bitmap bitmap)
        {
            //Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);

            List<Color> c = new List<Color>();

            AForge.Imaging.ImageStatistics stat = new AForge.Imaging.ImageStatistics(bitmap);

            /*

            using (Bitmap img = bitmap.GetThumbnailImage(4, 4, myCallback, IntPtr.Zero) as Bitmap)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Color cc = img.GetPixel(i, j);

                        if (!c.Contains(cc))
                        {
                            c.Add(cc);
                        }
                    }
                }                
            }
            */

            c.Add(Color.FromArgb( (int)(stat.Red.Mean), (int)(stat.Green.Mean), (int)(stat.Blue.Mean)));

            return c.ToArray();
        }
    }
}
