using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;


namespace SurfaceApplication3
{
    public class Helpers
    {

        public Helpers()
        {
        }

        public bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
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

        static string[] imageExtensions = {
            ".BMP", ".JPG", ".GIF", ".JPEG", ".TIFF", ".PNG", ".TIF"
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
