using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Capture the screen region coresponding to the artwork
        /// </summary>
        public void captureScreen(UIElement source, double scale)
        {
            {
                double actualHeight = source.RenderSize.Height;
                double actualWidth = source.RenderSize.Width;

                double renderHeight = actualHeight * scale;
                double renderWidth = actualWidth * scale;

                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
                VisualBrush sourceBrush = new VisualBrush(source);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                using (drawingContext)
                {
                    drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
                }
                renderTarget.Render(drawingVisual);

                BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
                bmpEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

                byte[] _imageArray = new byte[(int)renderWidth * (int)renderHeight * 4];

                using (MemoryStream outputStream = new MemoryStream())
                {
                    bmpEncoder.Save(outputStream);
                    outputStream.Read(_imageArray, 0, _imageArray.Length);
                }
                m_tools.ImageWidth = (int)renderWidth;
                m_tools.ImageHeight = (int)renderHeight;
                m_tools.SourceBytes = _imageArray;
                m_tools.ModifiedBytes = _imageArray;
                m_tools.History = new List<String>();
            }
        }
    }
}
