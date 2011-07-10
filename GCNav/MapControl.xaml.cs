using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.IO;


namespace GCNav
{
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {

        // Private properties
        private string _mapImageLocations = "Data/Map/Images/Large";
        private string _mapSubDivImageLocations = "Data/Map/Images/Subdivided";
        private static double _mapAspectRatio = 0.0;
        private static double _mapLoadWidth = 1080;
        //random shit
        //private Random r =  new Random();
        private MapRegion[] regions;
        private Point lastTouchDownPoint;

        private bool isZoomed = false;

        private int CanvasLeft, CanvasRight, CanvasTop, CanvasBottom;

        public MapControl()
        {
            InitializeComponent();
            LoadMap();
            this.TouchDown += new EventHandler<TouchEventArgs>(MapTouchHandler);
        }

        private void MapTouchHandler(object sender, TouchEventArgs e)
        {

            if (isZoomed)
            {
                bool isMainHit = false;
                foreach (MapRegion mr in regions)
                {
                    if (mr._zoomed)
                    {
                        Point currentPoint = e.GetTouchPoint(mr.RegionImage).Position;
                        if (mr.TryZoomHit((int)currentPoint.X, (int)currentPoint.Y))
                        {
                            isMainHit = true;
                        }
                    }
                }

                if (!isMainHit)
                {
                    foreach (MapRegion mr in regions)
                    {
                        mr.Unzoom(CanvasLeft, CanvasRight, CanvasTop, CanvasBottom);
                        isZoomed = false;
                        OnRegionDeselected(new Helpers.MapEventArgs(new List<ImageData>()));
                    }
                }
                
            }
            else
            {
                foreach (MapRegion mr in regions)
                {
                    Point currentPoint = e.GetTouchPoint(mr.RegionImage).Position;
                    Image img = mr.RegionImage;
                    if (mr.TryHit((int)currentPoint.X, (int)currentPoint.Y))
                    {
                        isZoomed = true;
                        FadeAll(mr);
                        mr.Zoom(CanvasLeft, CanvasRight, CanvasTop, CanvasBottom);
                        OnRegionSelected(new Helpers.MapEventArgs(mr.getArtworks()));
                    }
                }
            }
        }

        public event Helpers.MapEventHandler RegionSelected;
        public event Helpers.MapEventHandler RegionDeselected;

        public virtual void OnRegionSelected(Helpers.MapEventArgs e)
        {
            if (RegionSelected!=null)
            RegionSelected(this, e);
        }

        public virtual void OnRegionDeselected(Helpers.MapEventArgs e)
        {
            if (RegionDeselected!=null)
            RegionDeselected(this, e);
        }


        public void HandleImageLoadedEvent(Object sender, Helpers.ImageLoadedEventArgs e)
        {
            //random shit
            /*foreach (MapRegion mr in regions)
            {
                if (r.Next(0, 3) == 1)
                {
                    mr.AddArtwork(e.getImage());
                }
            }*/
            //add image to region -- TryTexHit(x,y (0,1)) in mapregion
            foreach (MapButton button in e.getImage().getLocButtons()) 
            {
                foreach (MapRegion mr in regions)
                {
                    if (mr.TryTextureHit(button.X, button.Y)) 
                    {
                        mr.AddArtwork(button);
                    }
                }
            }
        }

        public void HandleImageSelectedEvent(Object sender, Helpers.ImageSelectedEventArgs e)
        {
            foreach (MapRegion mr in regions)
            {
                mr.HideButtons();
                foreach (ImageData data in mr.getArtworks())
                {
                    if (data.Equals(e.getImage()))
                    {
                        mr.flash();
                        mr.ShowButton(e.getImage());
                    }
                }
            }
        }

        private void FadeAll(MapRegion exclude)
        {
            foreach (MapRegion mr in regions)
            {
                if (!mr.Equals(exclude))
                {
                    mr.Fade(0.3);
                }
            }
        }

        private void LoadMap()
        {
            DirectoryInfo di = new DirectoryInfo(_mapImageLocations);
            FileInfo[] files = di.GetFiles();
            regions = new MapRegion[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                regions[i] = new MapRegion(files[i].FullName);
                String subDiv = _mapSubDivImageLocations + "/" + files[i].Name;
                regions[i].LoadSubDivImage(subDiv);
                MapImages.Children.Add(regions[i].RegionImage);
                MapImages.Children.Add(regions[i].RegionSubdiv);
            }
            double minHeight = double.MaxValue;
            foreach (MapRegion r in regions)
            {
                if (r.Top < minHeight)
                    minHeight = r.Top;
            }
            Canvas.SetTop(MapImages, 0);
            //Canvas.SetTop(MapImages, -minHeight);
            BitmapImage bi = regions[0].RegionImage.Source as BitmapImage;
            _mapAspectRatio = bi.Width / bi.Height;
        }

        private void AddButton(double lon, double lat)
        {
            double x = ((1 / 360.0) * (180 + lon));
            double y = ((1 / 180.0) * (90 - lat));

        }


        private void ResizeImagesOnSizeChange(Size s)
        {
            double width = s.Width;
            width = width / 2;
            double height = width / _mapAspectRatio;
            double minHeight = double.MaxValue;
            foreach (MapRegion mr in regions)
            {
                mr.resize(width, height);
            }
            Canvas.SetTop(MapImages, 0);
            //Canvas.SetTop(MapImages, -minHeight);
        }

        private void MoveControlOnSizeChange(Size s)
        {
            double minHeight = double.MaxValue;
            double w, hRatio = 0;
            //Currently the following isn't used... it *would* move the control well, if the map were poorly formed
            foreach (MapRegion mr in regions)
            {
                //mr.RegionImage.Width = width;
                //mr.RegionImage.Height = height;
                w = mr.RegionImage.Width;
                
                hRatio = mr.RegionImage.Height / ((BitmapImage)mr.RegionImage.Source).PixelHeight;
                if (mr.Top * hRatio < minHeight)
                {
                    minHeight = mr.Top * hRatio;
                    //Console.WriteLine("MinHeight: " + minHeight);
                }
            }
            //onsole.WriteLine("MH:" + minHeight);
            CanvasTop = (int)(s.Height / 30);
            CanvasLeft = (int)((s.Width / 2 - (s.Width / 2) / 2));
            
            Canvas.SetTop(this, CanvasTop);
            Canvas.SetLeft(this, CanvasLeft - s.Width/10);
            
            CanvasRight = (int)(s.Width - CanvasLeft);
            CanvasBottom = CanvasTop + (int)(s.Width / 2 / _mapAspectRatio);
        }

        public void WindowSizeChanged(object sender, System.EventArgs e)
        {
            foreach (MapRegion mr in regions)
            {
                Image i = mr.RegionImage;
                SizeChangedEventArgs args = e as SizeChangedEventArgs;
                ResizeImagesOnSizeChange(args.NewSize);
                MoveControlOnSizeChange(args.NewSize);
            }
        }


        private class MapRegion
        {

            public Image RegionImage { get; set; }
            public Image RegionSubdiv { get; set; }
            private List<ImageData> _containedArtworks;
            public double Width, Height, Left, Right, Top, Bottom, CenterX, CenterY;
            public bool _zoomed = false;
            private byte[] _MainPixels;
            private byte[] _SubDivPixels;
            private Transform _originalTransform;
            private List<InternalRegion> _regions;
            private Dictionary<ImageData, Canvas> _buttons;
            private List<UIElement> _addedViaButton;
            private List<MapButton> _mapbuttons;
            private ImageData currentImage = null;

            public MapRegion(string imageLocation)
            {
                Width = MapControl._mapLoadWidth;
                _containedArtworks = new List<ImageData>();
                _buttons = new Dictionary<ImageData, Canvas>();
                LoadMainImage(imageLocation);
                _regions = new List<InternalRegion>();
                _addedViaButton = new List<UIElement>();
                _mapbuttons = new List<MapButton>();
            }

            public void removeButtonAdditions()
            {
                foreach (UIElement el in _addedViaButton)
                {
                    el.Visibility = Visibility.Collapsed;
                }
                _addedViaButton.Clear();
            }

            public void resize(double width, double height)
            {
                stopAnimations();
                RegionImage.Width = width;
                RegionImage.Height = height;
                RegionSubdiv.Width = width;
                RegionSubdiv.Height = height;
                foreach (Canvas c in _buttons.Values)
                {
                    c.Width = width;
                    c.Height = height;
                }

                foreach (MapButton b in _mapbuttons)
                {
                    b.canvas.Width = width;
                    b.canvas.Height = height;
                }
                //if (Top * RegionImage.Height / ((BitmapImage)RegionImage.Source).PixelHeight < minHeight)
                  //  minHeight = Top * RegionImage.Height / ((BitmapImage)RegionImage.Source).PixelHeight;
            }

            public void HideButtons()
            {
                //Console.WriteLine("hiding");
                foreach (MapButton b in _mapbuttons)
                {
                    b.canvas.Visibility = Visibility.Collapsed;
                }
            }

            public void ShowButton(ImageData img)
            {

                currentImage = img;
                foreach (MapButton b in _mapbuttons)
                {
                    if (b.ImageData.Equals(img)) b.canvas.Visibility=Visibility.Visible;
                }
            }

            public void LoadMainImage(string loc)
            {
                RegionImage = new Image();
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(loc);
                src.DecodePixelWidth = (int)Width;
                src.EndInit();
                Height = src.Height;
                GetRegionInfo(src,true);
                RegionImage.Source = src;
                RegionImage.Stretch = Stretch.Uniform;
                RegionImage.IsManipulationEnabled = true;
                RegionImage.Opacity = 0.6;
                Canvas.SetZIndex(RegionImage, -1);
            }

            public void LoadSubDivImage(string loc)
            {
                RegionSubdiv = new Image();
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(loc,UriKind.Relative);
                src.DecodePixelWidth = (int)Width;
                src.EndInit();
                Height = src.Height;
                GetRegionInfo(src,false);
                RegionSubdiv.Source = src;
                RegionSubdiv.Stretch = Stretch.Uniform;
                RegionSubdiv.IsManipulationEnabled = true;
                RegionSubdiv.Opacity = 0;
                Canvas.SetZIndex(RegionSubdiv, -1);
            }

            public void AddArtwork(MapButton button)
            {
                //maybe a boolean flag or hashmap-ish thing instead?
                if (!_containedArtworks.Contains(button.ImageData))
                    _containedArtworks.Add(button.ImageData);
                this.AddButton(button);
            }

            public List<ImageData> getArtworks()
            {
                return _containedArtworks;
            }

            void ButtonSelected(object send, RoutedEventArgs e)
            {
                e.Handled = true;
                ((Ellipse)send).CaptureTouch(((TouchEventArgs)e).TouchDevice);
                removeButtonAdditions();
                foreach (MapButton b in _mapbuttons)
                {
                    if (b.Ellipse.Equals(send))
                    {
                        TextBox text = new TextBox();
                        text.Background = new SolidColorBrush(Color.FromRgb(167, 201, 159));
                        text.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                        text.Text = currentImage.title + b.Type;
                        _addedViaButton.Add(text);
                        Canvas.SetTop(text, 30);
                        Canvas.SetLeft(text, 30);
                        ((Canvas)b.Ellipse.Parent).Children.Add(text);
                    }
                }
            }
            //random shit
            //Random r = new Random();
            public bool AddButton(MapButton button)
            {
                //never used
                /*double longitude = art.getLongLat()[0];
                double latitude = art.getLongLat()[1];
                longitude = 1.0 - longitude / (2 * Math.PI);
                latitude = Math.Abs(0.5 - latitude / Math.PI);

                double u = longitude - Math.Floor(longitude);
                double v = latitude;*/

                //old stuff
                /*MapButton button = new MapButton();
                button.x = 0;
                button.y = 0;
                switch(r.Next(3))
                {
                    case 0:
                        button.type = " was worked on here";
                        break;
                    case 1:
                        button.type = " was displayed here";
                        break;
                    case 2:
                        button.type = " was purchased here";
                        break;
                }
                Canvas c = new Canvas();
                button.canvas = c;
                Canvas.SetZIndex(c, -1);*/
                //c.Width = RegionImage.Width;
                //c.Height = RegionImage.Height;
                //c.RenderTransform = RegionImage.RenderTransform;

                //random shit!
                /*bool found = false;
                while (!found)
                {
                    int testX = r.Next((int)Left, (int)Right);
                    int testY = r.Next((int)Top, (int)Bottom);
                    found = TryHit(testX, testY);
                    button.x = testX;
                    button.y = testY;
                }
                addToSubdiv((int)button.x, (int)button.y, art);
                c.RenderTransform = new TranslateTransform(button.x, button.y);
                */
                this.addTexToSubdiv(button.X, button.Y, button.ImageData);
                button.canvas.RenderTransform = new TranslateTransform(button.X * RegionImage.Width, button.Y * RegionImage.Height);

                //old stuff
                /*Ellipse buttonellipse = new Ellipse();
                button.ellipse = buttonellipse;
                buttonellipse.Fill = Brushes.Blue;
                buttonellipse.PreviewTouchDown += ButtonSelected;
                buttonellipse.StrokeThickness = 0;
                buttonellipse.Width = 20;
                buttonellipse.Height = 20;
                c.Children.Add(buttonellipse);
                ((Canvas)RegionImage.Parent).Children.Add(c);
                c.Visibility = Visibility.Collapsed;
                button.imageData = art;*/
                button.Ellipse.PreviewTouchDown += ButtonSelected;
                ((Canvas)RegionImage.Parent).Children.Add(button.canvas);
                _mapbuttons.Add(button);
                /*
                int[] colorHit = getColorHit((int)(u * Width), (int)(v * Height));

                if (colorHit == null)
                {
                    return false;
                }

                foreach (InternalRegion region in _regions)
                {
                    if (!region.TestColor(colorHit))
                        break;
                    region.AddArt(art);
                }
                */
                return true;
            }

            private int[] getColorHit(int x, int y)
            {
                BitmapImage bitmapImage = (BitmapImage)RegionImage.Source;
                try
                {
                    int[] color = new int[3];
                    color[0] = _MainPixels[x * 4 + (y * bitmapImage.PixelWidth * 4)];
                    color[1] = _MainPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 1];
                    color[2] = _MainPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 2];
                    return color;
                    //return (pixelByteArray[x * 4 + (y  * bitmapImage.PixelWidth * 4)] != 0);
                    //return (pixelByteArray[x * (int)(RegionImage.Source.Width / Width) * 4 + (y *(int)(RegionImage.Source.Height / Height) * bitmapImage.PixelWidth * 4)] != 0);
                }
                catch (Exception e)
                {
                    return null;
                }
            }


            /*
             * Takes in two coordinates from 0-1, returns true if the input coordinate is part of the region
             */
            public bool TryTextureHit(double x, double y)
            {
                int xPrime = (int)(x * ((BitmapImage)RegionImage.Source).Width);
                int yPrime = (int)(y * ((BitmapImage)RegionImage.Source).Height);
                return TryHitHelper(xPrime, yPrime);
            }

            public bool TryHit(int x, int y)
            {
                //int xPrime = (int)(x * RegionImage.Width / Width);
                //int yPrime = (int)(y * RegionImage.Height /Height);

                int xPrime = (int)(x * ((BitmapImage)RegionImage.Source).Width / RegionImage.Width);
                int yPrime = (int)(y * ((BitmapImage)RegionImage.Source).Height / RegionImage.Height);

                return TryHitHelper(xPrime, yPrime);
            }

            private bool TryHitHelper(int x, int y)
            {
                BitmapImage bitmapImage = (BitmapImage)RegionImage.Source;
                if ((x * 4 + (y * bitmapImage.PixelWidth * 4) + 3) >= _SubDivPixels.GetLength(0)) return false;
                try
                {
                    return (_MainPixels[x * 4 + (y * bitmapImage.PixelWidth * 4)] != 0);
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            /*
             * Takes in two values, 0-1, and adds a button to the correct subdivision
             */
            public bool addTexToSubdiv(double x, double y, ImageData img)
            {
                int xPrime = (int)(x * ((BitmapImage)RegionImage.Source).Width);
                int yPrime = (int)(y * ((BitmapImage)RegionImage.Source).Height);
                mapColor c = TryZoomHitHelper(xPrime, yPrime);
                if (c.a < 0) return false;
                else
                {
                    Console.Out.WriteLine("internal region count: " + _regions.Count);
                    foreach (InternalRegion r in _regions)
                    {
                        if (r.TestColor(c))
                        {
                            Console.Out.WriteLine("added to subregion");
                            r.AddArt(img);
                            break;
                        }
                    }
                    return true;
                }
            }

            public bool addToSubdiv(int x, int y, ImageData img)
            {

                //Console.WriteLine("Orig: " + ((BitmapImage)RegionImage.Source).Width + " next: " + RegionImage.Width);
                int xPrime = (int)(x / ((BitmapImage)RegionImage.Source).Width / RegionImage.Width);
                int yPrime = (int)(y / ((BitmapImage)RegionImage.Source).Height / RegionImage.Height);
                mapColor c = TryZoomHitHelper(xPrime, yPrime);
                if (c.a < 0) return false;
                else
                {
                    foreach (InternalRegion r in _regions)
                    {
                        if (r.TestColor(c))
                        {
                            r.AddArt(img);
                            break;
                        }
                    }
                    return true;
                }
            }

            private List<UIElement> subdivadded = new List<UIElement>();

            public bool TryZoomHit(int x, int y)
            {
                foreach (UIElement e in subdivadded)
                {
                    e.Visibility = Visibility.Collapsed;
                }
                subdivadded.Clear();

                int xPrime = (int)(x * ((BitmapImage)RegionImage.Source).Width / RegionImage.Width);
                int yPrime = (int)(y * ((BitmapImage)RegionImage.Source).Height / RegionImage.Height);
                mapColor c = TryZoomHitHelper(xPrime, yPrime);
                if (c.a < 0) return false;
                else
                {
                    foreach (InternalRegion r in _regions)
                    {
                        if (r.TestColor(c))
                        {
                            TextBox text = new TextBox();
                            text.Background = new SolidColorBrush(Color.FromRgb(167, 201, 159));
                            text.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                            if (r.GetArtworks().Count == 1) text.Text = "1 artwork was purchased, displayed, or worked on in this region";
                            else text.Text = r.GetArtworks().Count + " artworks were purchased, displayed, or worked on in this region";
                            subdivadded.Add(text);
                            Canvas can = (Canvas)((Canvas)RegionImage.Parent).Parent;
                            can.Children.Add(text);
                            Console.WriteLine(can.ActualWidth + " " + text.ActualWidth);
                            Canvas.SetLeft(text, (can.ActualWidth - text.ActualWidth)/2+200);
                            Canvas.SetTop(text, -15);
                            break;
                        }
                    }
                    return true;
                }
                /*
                 * mapColor c = new mapColor(_SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4)],
                        _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 1],
                            _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 2],
                                _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 3]);
                                bool found = false;
                                foreach (mapColor col in colorOuts.Keys)
                                {
                                    if (c.a == col.a && c.b == col.b && c.g == col.g && c.r == col.r)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                 */
            }

            public struct mapColor
            {
                public int r, g, b, a;
                public mapColor(int ir, int ig, int ib, int ia)
                {
                    r = ir;
                    g = ig;
                    b = ib;
                    a = ia;
                }
            }

            private mapColor TryZoomHitHelper(int x, int y)
            {
                BitmapImage bitmapImage = (BitmapImage)RegionSubdiv.Source;
                if ((x * 4 + (y * bitmapImage.PixelWidth * 4) + 3) >= _SubDivPixels.GetLength(0)) return new mapColor(-1, -1, -1, -1);
                try
                {
                    if (_SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 3] != 0)
                    {
                        return new mapColor(_SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4)],
                        _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 1],
                            _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 2],
                                _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 3]);
                    }
                    else return new mapColor(-1, -1, -1, -1);
                }
                catch (Exception e)
                {
                    return new mapColor(-1, -1, -1, -1);
                }
            }

            private Dictionary<mapColor, string> colorOuts = new Dictionary<mapColor, string>(); 
            private void GetRegionInfo(BitmapSource src,bool isMain)
            {
                if (isMain)
                {
                    BitmapImage bitmapImage = (BitmapImage)src;
                    int height = bitmapImage.PixelHeight;
                    int width = bitmapImage.PixelWidth;
                    _mapAspectRatio = width / height;
                    int nStride = (bitmapImage.PixelWidth * bitmapImage.Format.BitsPerPixel + 7) / 8;
                    _MainPixels = new byte[bitmapImage.PixelHeight * nStride];
                    bitmapImage.CopyPixels(_MainPixels, nStride, 0);

                    int maxX = -1, maxY = -1, minX = int.MaxValue, minY = int.MaxValue, sumX = 0, sumY = 0, numColoredPixels = 0;
                    for (int x = 0; x < src.PixelWidth; x++)
                    {
                        for (int y = 0; y < src.PixelHeight; y++)
                        {
                            if (_MainPixels[x * 4 + (y * src.PixelWidth * 4)] != 0)
                            {
                                numColoredPixels += 1;
                                sumX = sumX + x;
                                sumY = sumY + y;
                                if (minY > y) minY = y;
                                if (minX > x) minX = x;
                                if (y > maxY) maxY = y;
                                if (x > maxX) maxX = x;
                            }
                        }
                    }
                    Left = minX;
                    Right = maxX;
                    Top = minY;
                    Bottom = maxY;
                    CenterX = sumX / numColoredPixels;
                    CenterY = sumY / numColoredPixels;
                }
                else
                {
                    BitmapImage bitmapImage = (BitmapImage)src;
                    int height = bitmapImage.PixelHeight;
                    int width = bitmapImage.PixelWidth;
                    _mapAspectRatio = width / height;
                    int nStride = (bitmapImage.PixelWidth * bitmapImage.Format.BitsPerPixel + 7) / 8;
                    _SubDivPixels = new byte[bitmapImage.PixelHeight * nStride];
                    bitmapImage.CopyPixels(_SubDivPixels, nStride, 0);
                    for (int x = 0; x < src.PixelWidth; x++)
                    {
                        for (int y = 0; y < src.PixelHeight; y++)
                        {
                            if (_SubDivPixels[x * 4 + (y * src.PixelWidth * 4)] != 0)
                            {
                                mapColor c = new mapColor(_SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4)],
                        _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 1],
                            _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 2],
                                _SubDivPixels[x * 4 + (y * bitmapImage.PixelWidth * 4) + 3]);
                                bool found = false;
                                foreach (mapColor col in colorOuts.Keys)
                                {
                                    if (c.a == col.a && c.b == col.b && c.g == col.g && c.r == col.r)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    InternalRegion internalRegion = new InternalRegion(c);
                                    _regions.Add(internalRegion);
                                    //int numArtworks = r.Next(5) + 2;//this is also random shit?
                                    int numArtworks = -1;
                                    colorOuts[c] = "There have been " + numArtworks + " pieces created here";
                                }
                            }
                        }
                    }
                }
            }

            public void Zoom(int zoomleft, int zoomright, int zoomtop, int zoombottom)
            {
                removeButtonAdditions();
                Canvas.SetZIndex(RegionImage, 0);
                Canvas.SetZIndex(RegionSubdiv, 0);
                
                double w = zoomright - zoomleft;
                double h = zoombottom - zoomtop;
                double aspectRatio = w / h;
                double wDelta = w / (Right - Left);
                double hDelta = h / (Bottom - Top);
                if (wDelta > hDelta) wDelta = hDelta;
                else hDelta = wDelta;

                double cx = (Left + (Right - Left) / 2) / (Width / (RegionImage.Width * wDelta));
                double cy = (Top + (Bottom - Top) / 2) / (Height / (RegionImage.Height * hDelta));
                Point p1 = new Point(cx, cy);
                Point p2 = new Point((zoomright - zoomleft) / 2, (zoombottom - zoomtop) / 2);

                List<MapButton> visiblebuttons = new List<MapButton>();
                foreach (MapButton b in _mapbuttons)
                {
                    if (b.canvas.IsVisible)
                    {
                        visiblebuttons.Add(b);
                    }
                }

                TranslateTransform trans = new TranslateTransform();
                TranslateTransform sdivTrans = new TranslateTransform();
                RegionImage.RenderTransform = trans;
                RegionSubdiv.RenderTransform = sdivTrans;

                DoubleAnimation widthAnimation = Helpers.makeDoubleAnimation(RegionImage.Width, RegionImage.Width * wDelta, 1);
                DoubleAnimation heightAnimation = Helpers.makeDoubleAnimation(RegionImage.Height, RegionImage.Height * hDelta, 1);
                DoubleAnimation transXAnimation = Helpers.makeDoubleAnimation(trans.X, p2.X - p1.X, 1);
                DoubleAnimation transYAnimation = Helpers.makeDoubleAnimation(trans.Y, p2.Y - p1.Y, 1);

                foreach (MapButton b in visiblebuttons)
                {
                    Canvas button = b.canvas;
                    if (button != null)
                    {
                        Canvas.SetZIndex(button, 0);
                        TransformGroup group = new TransformGroup();
                        group.Children.Add(RegionImage.RenderTransform);
                        TranslateTransform t = new TranslateTransform();
                        group.Children.Add(t);
                        button.RenderTransform = group;
                        //DoubleAnimation buttonXAnimation = Helpers.makeDoubleAnimation((CenterX * 0.9 + b.x), (CenterX * 0.9 + b.x) * wDelta, 1);
                        //DoubleAnimation buttonYAnimation = Helpers.makeDoubleAnimation((CenterY * 0.9 + b.y), (CenterY * 0.9 + b.y) * hDelta, 1);
                        DoubleAnimation buttonXAnimation = Helpers.makeDoubleAnimation((b.X * RegionImage.Width), (b.X * RegionImage.Width) * wDelta, 1);
                        DoubleAnimation buttonYAnimation = Helpers.makeDoubleAnimation((b.Y * RegionImage.Height), (b.Y * RegionImage.Height) * hDelta, 1);
                        t.BeginAnimation(TranslateTransform.XProperty, buttonXAnimation);
                        t.BeginAnimation(TranslateTransform.YProperty, buttonYAnimation);
                    }
                }

                DoubleAnimation sdivwidthAnimation = Helpers.makeDoubleAnimation(RegionImage.Width, RegionImage.Width * wDelta, 1);
                DoubleAnimation sdivheightAnimation = Helpers.makeDoubleAnimation(RegionImage.Height, RegionImage.Height * hDelta, 1);
                DoubleAnimation sdivtransXAnimation = Helpers.makeDoubleAnimation(sdivTrans.X, p2.X - p1.X, 1);
                DoubleAnimation sdivtransYAnimation = Helpers.makeDoubleAnimation(sdivTrans.Y, p2.Y - p1.Y, 1);

                DoubleAnimation mainOpacityAnimation = Helpers.makeDoubleAnimation(RegionImage.Opacity, .85, 1);
                DoubleAnimation subdivOpacityAnimation = Helpers.makeDoubleAnimation(0, 1, 1);


                RegionImage.BeginAnimation(Image.WidthProperty, widthAnimation);
                RegionImage.BeginAnimation(Image.HeightProperty, heightAnimation);
                RegionImage.BeginAnimation(Image.OpacityProperty, mainOpacityAnimation);

                RegionSubdiv.BeginAnimation(Image.OpacityProperty, subdivOpacityAnimation);
                RegionSubdiv.BeginAnimation(Image.WidthProperty, sdivwidthAnimation);
                RegionSubdiv.BeginAnimation(Image.HeightProperty, sdivheightAnimation);

                sdivTrans.BeginAnimation(TranslateTransform.XProperty, sdivtransXAnimation);
                sdivTrans.BeginAnimation(TranslateTransform.YProperty, sdivtransYAnimation);

                trans.BeginAnimation(TranslateTransform.XProperty, transXAnimation);
                trans.BeginAnimation(TranslateTransform.YProperty, transYAnimation);

                RegionImage.Opacity = 0.85;
                _zoomed = true;
            }
            public void Unzoom(int zoomleft, int zoomright, int zoomtop, int zoombottom)
            {
                removeButtonAdditions();
                RegionImage.Opacity = .95;
                Canvas.SetZIndex(RegionImage, -1);
                Canvas.SetZIndex(RegionSubdiv, -1);
                if (!_zoomed)
                {
                    Fade(0.6);
                    return;
                } 

                double w = zoomright - zoomleft;
                double h = zoombottom - zoomtop;
                double aspectRatio = w / h;
                double wDelta = w / (RegionImage.Width);
                double hDelta = h / (RegionImage.Height);
                if (wDelta > hDelta) wDelta = hDelta;
                else hDelta = wDelta;

                double cx = (Left + (Right - Left) / 2) / (Width / (RegionImage.Width * wDelta));
                double cy = (Top + (Bottom - Top) / 2) / (Height / (RegionImage.Height * hDelta));
                Point p1 = new Point(cx, cy);
                Point p2 = new Point((zoomright - zoomleft) / 2, (zoombottom - zoomtop) / 2);

                TranslateTransform trans = (TranslateTransform)RegionImage.RenderTransform;

                DoubleAnimation widthAnimation = Helpers.makeDoubleAnimation(RegionImage.Width, RegionImage.Width * wDelta, .4);
                DoubleAnimation heightAnimation = Helpers.makeDoubleAnimation(RegionImage.Height, RegionImage.Height * hDelta, .4);
                DoubleAnimation transXAnimation = Helpers.makeDoubleAnimation(trans.X, 0, .4);
                DoubleAnimation transYAnimation = Helpers.makeDoubleAnimation(trans.Y, 0, .4);

                List<MapButton> visiblebuttons = new List<MapButton>();
                foreach (MapButton b in _mapbuttons)
                {
                    if (b.canvas.IsVisible)
                    {
                        visiblebuttons.Add(b);
                    }
                }

                foreach (MapButton b in visiblebuttons)
                {
                    Canvas canvas = b.canvas;
                    if (canvas != null)
                    {
                        Canvas.SetZIndex(canvas, -1);
                        TransformGroup group = new TransformGroup();
                        group.Children.Add(RegionImage.RenderTransform);
                        TranslateTransform t = new TranslateTransform();
                        group.Children.Add(t);
                        canvas.RenderTransform = group;
                        DoubleAnimation buttonXAnimation = Helpers.makeDoubleAnimation(b.X * RegionImage.Width, (b.X * RegionImage.Width * wDelta), .4);
                        DoubleAnimation buttonYAnimation = Helpers.makeDoubleAnimation(b.Y * RegionImage.Height, (b.Y * RegionImage.Height * hDelta), .4);
                        t.BeginAnimation(TranslateTransform.XProperty, buttonXAnimation);
                        t.BeginAnimation(TranslateTransform.YProperty, buttonYAnimation);

                        //Console.Out.WriteLine("unzoom button loc from: " + ((b.X * RegionImage.Width / wDelta) / wDelta) + "," + ((b.Y * RegionImage.Height / hDelta) / hDelta));
                    }
                }

                RegionImage.BeginAnimation(Image.WidthProperty, widthAnimation);
                RegionImage.BeginAnimation(Image.HeightProperty, heightAnimation);
                trans.BeginAnimation(TranslateTransform.XProperty, transXAnimation);
                trans.BeginAnimation(TranslateTransform.YProperty, transYAnimation);

                

                TranslateTransform sdivtrans = (TranslateTransform)RegionSubdiv.RenderTransform;

                DoubleAnimation sdivwidthAnimation = Helpers.makeDoubleAnimation(RegionSubdiv.Width, RegionSubdiv.Width * wDelta, .4);
                DoubleAnimation sdivheightAnimation = Helpers.makeDoubleAnimation(RegionSubdiv.Height, RegionSubdiv.Height * hDelta, .4);
                DoubleAnimation sdivtransXAnimation = Helpers.makeDoubleAnimation(trans.X, 0, .4);
                DoubleAnimation sdivtransYAnimation = Helpers.makeDoubleAnimation(trans.Y, 0, .4);

                RegionSubdiv.BeginAnimation(Image.WidthProperty, sdivwidthAnimation);
                RegionSubdiv.BeginAnimation(Image.HeightProperty, sdivheightAnimation);
                sdivtrans.BeginAnimation(TranslateTransform.XProperty, sdivtransXAnimation);
                sdivtrans.BeginAnimation(TranslateTransform.YProperty, sdivtransYAnimation);


                DoubleAnimation mainOpacityAnimation = Helpers.makeDoubleAnimation(RegionImage.Opacity, 0.6, .4);
                DoubleAnimation subdivOpacityAnimation = Helpers.makeDoubleAnimation(1, 0, .4);
                RegionImage.BeginAnimation(Image.OpacityProperty, mainOpacityAnimation);
                RegionSubdiv.BeginAnimation(Image.OpacityProperty, subdivOpacityAnimation);
                
                
                _zoomed = false;
            }

            public void stopAnimations()
            {
                RegionSubdiv.BeginAnimation(Image.WidthProperty, null);
                RegionSubdiv.BeginAnimation(Image.HeightProperty, null);
                RegionImage.BeginAnimation(Image.WidthProperty, null);
                RegionImage.BeginAnimation(Image.HeightProperty, null);
                RegionImage.BeginAnimation(Image.OpacityProperty, null);
                RegionSubdiv.BeginAnimation(Image.OpacityProperty, null);

                List<MapButton> visiblebuttons = new List<MapButton>();
                foreach (MapButton b in _mapbuttons)
                {
                    if (b.canvas.IsVisible)
                    {
                        visiblebuttons.Add(b);
                    }
                }

                foreach (MapButton b in visiblebuttons)
                {
                    Canvas button = b.canvas;

                    if (button != null)
                    {
                        button.BeginAnimation(Canvas.HeightProperty, null);
                        button.BeginAnimation(Canvas.WidthProperty, null);
                    }
                }
            }

            public void Fade(double f)
            {
                RegionImage.BeginAnimation(Image.OpacityProperty, null);
                RegionSubdiv.BeginAnimation(Image.OpacityProperty, null);
                RegionImage.Opacity = f;
            }

            public void flash()
            {
                DoubleAnimation mainOpacity = Helpers.makeDoubleAnimation(RegionImage.Opacity, 1, .15);
                mainOpacity.AutoReverse = true;
                DoubleAnimation subdivOpacity = Helpers.makeDoubleAnimation(RegionSubdiv.Opacity, 1, .1);
                subdivOpacity.AutoReverse = true;
                RegionImage.BeginAnimation(Image.OpacityProperty, mainOpacity);
                RegionSubdiv.BeginAnimation(Image.OpacityProperty, subdivOpacity);
            }

            private class InternalRegion
            {
                private mapColor _rgb;
                private List<ImageData> artworks;

                public InternalRegion(mapColor rgb)
                {
                    artworks = new List<ImageData>();
                    _rgb = rgb;
                }

                public void AddArt(ImageData art)
                {
                    artworks.Add(art);
                }

                public List<ImageData> GetArtworks()
                {
                    return artworks;
                }

                public bool TestColor(mapColor rgb)
                {
                    return ((_rgb.r == rgb.r) && (_rgb.g == rgb.g) && (_rgb.b == rgb.b));
                }
            }
        }

        public class MapButton
        {
            /*public double x { get; set; }
            public double y { get; set; }
            public string type { get; set; }
            public ImageData imageData { get; set; }
            public Canvas canvas { get; set; }
            public Ellipse ellipse { get; set; }*/
            //old stuff

            private double _x;
            public double X { get { return _x; } }
            private double _y;
            public double Y { get { return _y; } }
            private int _type;
            public string Type
            {
                get
                {
                    switch (_type)
                    {
                        case 0:
                            return " was worked on here";
                        case 1:
                            return " was displayed here";
                        case 2:
                            return " was purchased here";
                        default:
                            return "";
                    }
                }
            }

            private ImageData _imageData;
            public ImageData ImageData { get { return _imageData; } }

            private Canvas _canvas;
            public Canvas canvas { get { return _canvas; } }

            private Ellipse _ellipse;
            public Ellipse Ellipse { get { return _ellipse; } }

            //x, y are percentage (your u, v)
            public MapButton(double x, double y, int type, ImageData img)
            {
                _x = x;
                _y = y;
                _type = type;
                _canvas = new Canvas();
                Canvas.SetZIndex(_canvas, -1);
                _ellipse = new Ellipse();
                switch (type) {
                    case 0:
                        _ellipse.Fill = Brushes.Red;
                        break;
                    case 1:
                        _ellipse.Fill = Brushes.Blue;
                        break;
                    case 2:
                        _ellipse.Fill = Brushes.Yellow;
                        break;
                    default:
                        _ellipse.Fill = Brushes.White;
                        break;
                }
                //_ellipse.PreviewTouchDown += ButtonSelected;
                _ellipse.StrokeThickness = 0;
                _ellipse.Width = 20;
                _ellipse.Height = 20;
                _canvas.Children.Add(_ellipse);
                //((Canvas)RegionImage.Parent).Children.Add(_canvas);
                _canvas.Visibility = Visibility.Collapsed;
                _imageData = img;
            }
        }
    }
}
