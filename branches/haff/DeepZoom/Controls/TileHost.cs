using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;

namespace DeepZoom.Controls
{
    /// <summary>
    /// Simple FrameworkElement that draws and animates an image in the screen with the lowest possible overhead.
    /// </summary>
    public class TileHost : FrameworkElement
    {
        // Create a collection of child visual objects.
        private DrawingVisual _visual;
        private static readonly AnimationTimeline _opacityAnimation = 
            new DoubleAnimation(1, TimeSpan.FromMilliseconds(500)) { EasingFunction = new ExponentialEase() };

        public TileHost()
        {
            IsHitTestVisible = false;
        }

        public TileHost(ImageSource source, double scale)
            : this()
        {
            Source = source;
            Scale = scale;
        }

        #region Dependency Properties

        #region Source

        /// <summary>
        /// Source Dependency Property
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(TileHost),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(RefreshTile)));

        /// <summary>
        /// Gets or sets the Source property. This dependency property 
        /// indicates the source of the image to be displayed.
        /// </summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion

        #region Scale

        /// <summary>
        /// Scale Dependency Property
        /// </summary>
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(TileHost),
                new FrameworkPropertyMetadata(1.0,
                    new PropertyChangedCallback(RefreshTile)));

        /// <summary>
        /// Gets or sets the Scale property. This dependency property 
        /// indicates the scaling to be applied to this tile.
        /// </summary>
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        #endregion

        #endregion

        #region Private methods

        /// <summary>
        /// Called when the tile should be refreshed (Scale or Source changed)
        /// </summary>
        private static void RefreshTile(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tileHost = d as TileHost;
            if (tileHost != null && tileHost.Source != null && tileHost.Scale > 0)
                tileHost.RenderTile();
        }

        private static void RefreshTileScale(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tileHost = d as TileHost;
            //if (tileHost != null && tileHost.Source != null && tileHost.Scale > 0)
               // tileHost.RenderTile();
        }

        private void RenderTile()
        {

                if (_visual != null)
                {
                    _visual = null;
                    this.RemoveVisualChild(_visual);
                    this.RemoveLogicalChild(_visual);

                }
          
                _visual = new DrawingVisual();
                Width = Source.Width * Scale;
                Height = Source.Height * Scale;
                var dc = _visual.RenderOpen();
                //BitmapImage test = (BitmapImage)Source;

                Width = Source.Width * Scale;
                Height = Source.Height * Scale;
                WriteableBitmap test = new WriteableBitmap((BitmapSource)Source);
                //BitmapImage test = (BitmapImage)Source;

                int w = (int)test.PixelWidth;
                int h = (int)test.PixelHeight;
                Int32Rect rect = new Int32Rect(0, 0, w, h);
                int b = test.Format.BitsPerPixel;
                
                Byte[] pixels = new Byte[w * h * b / 8];
                int stride = w * b / 8;
                test.CopyPixels(pixels, stride, 0);
                double sc = Scale;

                /*//if (Modified == true)
                //{
                    //Drawing DrawObj = new Drawing();
                    Byte[] output;// = pixels ;
                    //Thread MulThread = new Thread(delegate()
                    //{
                    output = Drawing.DrawImage(pixels, w, h, b, value);
                    //});
                    // MulThread.Start();

                    BitmapSource image = BitmapSource.Create(w, h, 96, 96, test.Format, null, output, stride);

                    /*MemoryStream mStream = new MemoryStream(output);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage _bitmap = new BitmapImage();
                    _bitmap.BeginInit();
                    _bitmap.StreamSource = mStream;
                    _bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    _bitmap.EndInit();

                    Source = _bitmap;
                    test.WritePixels(rect, output, stride, 0);
                    //Source = test;
                    //Scale = sc;

                }*/

                dc.DrawImage(Source, new Rect(0, 0, Width, Height));

                dc.Close();
                
               CacheMode = new BitmapCache(1 / Scale);
           // catch (Exception e)
            // Animate opacity
            Opacity = 0;
            BeginAnimation(OpacityProperty, _opacityAnimation);
        }

        #endregion

        #region FrameworkElement overrides

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _visual == null ? 0 : 1; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            return _visual;
        }

        #endregion
    }
}


