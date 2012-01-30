using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LADSArtworkMode
{
    class Helpers
    {

        public Helpers()
        {
        }

        public System.Windows.Controls.Image ConvertDrawingImageToWPFImage(System.Drawing.Image gdiImg)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(gdiImg);
            IntPtr hBitmap = bmp.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            img.Source = WpfBitmap;
            return img;
        }

        //The next 2 blocks of code are for finding whether files are images or not
        static string[] imageExtensions = {
            ".BMP", ".JPG", ".GIF"
        };

        public bool IsImageFile(string filename)
        {
            return -1 != Array.IndexOf(imageExtensions, System.IO.Path.GetExtension(filename).ToUpperInvariant());
        }

        //The next 2 blocks of code are for finding whether files are videos or not
        static string[] videoExtensions = {
                                              //".MOV", ".AVI"
            ".WMV", ".ASF", ".ASX", ".AVI", ".FLV",
            ".MOV", ".MP4", ".MPG", ".RM", ".SWF", ".VOB"
        };

        public bool IsVideoFile(string filename)
        {
            return -1 != Array.IndexOf(videoExtensions, System.IO.Path.GetExtension(filename).ToUpperInvariant());
        }
        //The next 2 blocks of code are for finding whether files are DirectShow videos or not
        static string[] DirShowExtensions = {
                                              ".MOV", ".AVI"
            //".WMV", ".ASF", ".ASX", ".AVI", ".FLV",
            //".MOV", ".MP4", ".MPG", ".RM", ".SWF", ".VOB"
        };

        public bool IsDirShowFile(string filename)
        {
            return -1 != Array.IndexOf(DirShowExtensions, System.IO.Path.GetExtension(filename).ToUpperInvariant());
        }

    }
}
