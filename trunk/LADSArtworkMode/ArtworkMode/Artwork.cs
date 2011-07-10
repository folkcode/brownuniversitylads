using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace LADSArtworkMode
{
    public class Artwork
    {
        /// <summary>
        /// Hold the bitmap decoder to convert from images to byte array
        /// </summary>
        BitmapDecoder m_image;
        public BitmapDecoder Image
        {
            get { return m_image; }
            set { m_image = value; }
        }

        /// <summary>
        /// Used to apply image manipulation tools 
        /// </summary>
        ImageProcessingTools m_tools;
        internal ImageProcessingTools Tools
        {
            get { return m_tools; }
            set { m_tools = value; }
        }

        /// <summary>
        /// Hold hotspot icons.
        /// </summary>
        Hotspot[] m_hotspots;
        internal Hotspot[] Hotspots
        {
            get { return m_hotspots; }
            set { m_hotspots = value; }
        }

        String m_filename;

        public String Filename
        {
            get { return m_filename; }
            set { m_filename = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Artwork()
        {
            m_tools = new ImageProcessingTools();
        }



        public void setFileName(String name)
        {
            m_filename = name;
        }


        /*public void addImage(String path)
        {
            try
            {
                m_image = new JpegBitmapDecoder(new Uri(path), BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
                // TODO: fill in RGB and HSV values for m_tools.
                byte[] ImageBytes = new byte[m_image.Frames[0].PixelWidth * 4 * m_image.Frames[0].PixelHeight];
                m_image.Frames[0].CopyPixels(ImageBytes, m_image.Frames[0].PixelWidth * 4, 0);
                m_tools.RGB = m_tools.byteToRGBA(ImageBytes, Image.Frames[0].PixelWidth, Image.Frames[0].PixelHeight);
                m_tools.ImageWidth = Image.Frames[0].PixelWidth;
                m_tools.ImageHeight = Image.Frames[0].PixelHeight;
                //MessageBox.Show("Image added");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }*/

        /*
        public BitmapSource returnImage()
        {
            byte[] ImageBytes = m_tools.RGBAToByte(m_tools.RGB);

            //ImageBytes = ImageBytes.Reverse(); // ToArray(); // jcchin // diem: don't need this function, it screws up the R-G-B-A components order.

            BitmapSource myNewImage = BitmapSource.Create(m_tools.ImageWidth, m_tools.ImageHeight, 96, 96, PixelFormats.Bgra32, null, ImageBytes, m_tools.ImageWidth*4);

            return myNewImage;


        }*/


        /*public void captureScreen(int x, int y, int width, int height)
        {
            System.Drawing.Bitmap screen = new System.Drawing.Bitmap(width, height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(screen);
            g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
            g.Dispose();


            MemoryStream ms = new MemoryStream();
            screen.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] data = ms.ToArray();

            m_tools.RGB = m_tools.byteToRGBA(data, width,height);
            m_tools.ImageHeight = height;
            m_tools.ImageWidth = width;

        }*/

        /// <summary>
        /// Capture the screen region coresponding to the artwork
        /// </summary>
        public void captureScreen(UIElement source, double scale)
        {
           // if (m_tools.Modified == false)
            {
                double actualHeight = source.RenderSize.Height;
                double actualWidth = source.RenderSize.Width;

                //MessageBox.Show("h =" + actualHeight + " w =" + actualWidth);

                double renderHeight = actualHeight * scale;
                double renderWidth = actualWidth * scale;

                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
                VisualBrush sourceBrush = new VisualBrush(source);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                using (drawingContext)
                {
                    //drawingContext.PushTransform(new ScaleTransform(scale, scale));
                    drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
                }
                renderTarget.Render(drawingVisual);

                BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
                bmpEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

                //int[] a = int[2];
                byte[] _imageArray = new byte[(int)renderWidth * (int)renderHeight * 4];

                using (MemoryStream outputStream = new MemoryStream())
                {
                    bmpEncoder.Save(outputStream);
                    outputStream.Read(_imageArray, 0, _imageArray.Length);
                }

                //m_tools.RGB = m_tools.byteToRGBA(_imageArray, (int)renderWidth, (int)renderHeight);
                m_tools.ImageWidth = (int)renderWidth;
                m_tools.ImageHeight = (int)renderHeight;
                m_tools.SourceBytes = _imageArray;
                //m_tools.ModifiedBytes = new Byte[_imageArray.Length];
                m_tools.ModifiedBytes = _imageArray;
                m_tools.History = new List<String>();
            }
        }

        /*public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }*/

        
    }
}
