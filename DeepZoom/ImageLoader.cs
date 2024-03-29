﻿using System;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

namespace DeepZoom
{
    public static class ImageLoader
    {
        /// <summary>
        /// Loads an image from a given Uri, synchronously.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static BitmapSource LoadImage(Uri uri)
        {
            try
            {
                var bi = new BitmapImage();
                MemoryStream mem;
                using (var client = new WebClient())
                {
                    var buffer = client.DownloadData(uri);
                    mem = new MemoryStream(buffer);
                }
                bi.BeginInit();
                bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bi.CacheOption = BitmapCacheOption.None;
                bi.StreamSource = mem;
                bi.EndInit();

                //bi.Freeze();

                return (bi);
            }
            catch (WebException)
            {
                // Server error or image not found, do nothing
            }
            catch (FileNotFoundException)
            {
                // Local file not found, do nothing
            }
            catch (FileFormatException)
            {
                // Corrupted image, do nothing
            }
            return null;
        }
    }
}
