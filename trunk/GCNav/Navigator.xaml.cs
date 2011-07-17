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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.ComponentModel;
using Microsoft.Surface.Presentation.Controls;
using LADSArtworkMode;
using System.Net;
using System.IO;
using System.Timers;

namespace GCNav
{
    /// <summary>
    /// Interaction logic for Navigator.xaml
    /// </summary>
    public partial class Navigator : UserControl
    {
        private String dataDir = "Data/";
        private Size _windowSize;

        //time range of the collection
        private int _starty;
        private int _endy;

        private double _curZoomFactor;
        private Point _initScatterPos, _initScatterWH;

        private Event _eventDisplayed;
        public LADSArtworkMode.ArtworkModeWindow artmode;

        List<ImageData> _imageCollection;
        List<ImageData> _displayedCollection;

        /*List<String> _artists;
        List<String> _mediums;
        List<String> _years;*/

        private ImageData currentImage;
        private System.Windows.Forms.Timer _timer;
        private bool _artOpen, _collectionEmpty;
        private Double mapWidth;
        public FilterTimelineBox filter;

        public List<DockedItemInfo> SavedDockedItems;

        public Navigator()
        {
            InitializeComponent();

            curImageContainer.Visibility = Visibility.Hidden;
            curInfoContainer.Visibility = Visibility.Hidden;
            mainScatterViewItem.Width = 1920;
            MainCanvas.Width = 0;
            mainScatterViewItem.Background = Brushes.Transparent;
            //mainScatterViewItem.BorderThickness = new Thickness(0);
            _curZoomFactor = 1.0;
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(mainScatterViewItem, mainScatterViewItem_CenterChanged);

            _imageCollection = new List<ImageData>();
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 5000;
            _timer.Start();
            _artOpen = false;
            _collectionEmpty = true;
            timeline.nav = this;
            /*_artists = new List<String>();
            _mediums = new List<String>();
            _years = new List<String>();*/
        }
        public void setMapWidth(Double width)
        {
            mapWidth = width;
        }
        public void TimerTick_Handler(object sender, EventArgs e)
        {
            if (artmode != null)
            {
                artmode.updateWebImages();
            }
        }

        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void setTimelineMouseUpFalse()
        {
            timeline.setMouseOnAndDown(false);
        }

        public List<ImageData> getImageCollection()
        {
            return _imageCollection;
        }

        /// <summary>
        /// parse the xml and create all the imageData objects
        /// </summary>
        public void loadCollection()
        {
            InstructionBox.Width = System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            XmlDocument doc = new XmlDocument();
            doc.Load(dataDir + "NewCollection.xml");
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {

                        int startY = 1;
                        int endY = 0;
                        foreach (XmlNode node in docNode.ChildNodes)
                        {
                            if (node.Name == "Image")
                            {
                                _collectionEmpty = false;
                                String path = node.Attributes.GetNamedItem("path").InnerText;
                                String artist = node.Attributes.GetNamedItem("artist").InnerText;
                                String medium = node.Attributes.GetNamedItem("medium").InnerText;
                                String title = node.Attributes.GetNamedItem("title").InnerText;
                                int year = Convert.ToInt32(node.Attributes.GetNamedItem("year").InnerText);

                                /*if (!_artists.Contains(artist))
                                    _artists.Add(artist);
                                if (!_mediums.Contains(medium))
                                    _mediums.Add(medium);
                                if (!_years.Contains(node.Attributes.GetNamedItem("year").InnerText))
                                    _years.Add(node.Attributes.GetNamedItem("year").InnerText);*/

                                String fullPath = dataDir + "Images/" + "Thumbnail/" + path;
                                String xmlPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + dataDir + "Images/" + "DeepZoom/" + path + "/dz.xml";

                                ImageData currentImage = new ImageData(fullPath);
                                currentImage.xmlpath = xmlPath;
                                currentImage.thumbpath = fullPath;
                                currentImage.year = year;
                                currentImage.artist = artist;
                                currentImage.medium = medium;
                                currentImage.title = title;
                                currentImage.filename = path;
                                currentImage.setParent(this);

                                //populate Keywords
                                List<String> keywds = new List<String>();
                                foreach (XmlNode imgnode in node.ChildNodes)
                                {
                                    if (imgnode.Name == "Keywords")
                                    {
                                        foreach (XmlNode keywd in imgnode.ChildNodes)
                                        {
                                            if (keywd.Name == "Keyword")
                                            {
                                                keywds.Add(keywd.Attributes.GetNamedItem("Value").InnerText);
                                            }
                                        }
                                    }
                                    else if (imgnode.Name == "Locations")
                                    {
                                        foreach (XmlNode locInfo in imgnode.ChildNodes)
                                        {

                                            if (locInfo.Name == "Purchase")
                                            {
                                                Point p = this.parceLongLat(locInfo);
                                                currentImage.addButton(new MapControl.MapButton(p.X, p.Y, 2, currentImage));
                                                // currentImage.addButton(new newMap.newMapButton(p.X, p.Y, 2, currentImage));
                                                if (locInfo.Attributes.GetNamedItem("longitude") != null)
                                                {
                                                    String lon = locInfo.Attributes.GetNamedItem("longitude").InnerText;

                                                    String lat = locInfo.Attributes.GetNamedItem("latitude").InnerText;
                                                    String date = "";
                                                    String city = "";
                                                    if (locInfo.Attributes.GetNamedItem("date") != null)
                                                    {
                                                        date = locInfo.Attributes.GetNamedItem("date").InnerText;
                                                    }
                                                    if (locInfo.Attributes.GetNamedItem("city") != null)
                                                    {
                                                        city = locInfo.Attributes.GetNamedItem("city").InnerText;
                                                    }
                                                    currentImage.setLocButtonInfo("red" + "/" + lon + "/" + lat + "/" + date + "/" + city);
                                                }

                                            }
                                            else if (locInfo.Name == "Work")
                                            {
                                                Point p = this.parceLongLat(locInfo);
                                                currentImage.addButton(new MapControl.MapButton(p.X, p.Y, 0, currentImage));
                                                //  currentImage.addButton(new newMap.newMapButton(p.X, p.Y, 0, currentImage));
                                                if (locInfo.Attributes.GetNamedItem("longitude") != null)
                                                {
                                                    String lon = locInfo.Attributes.GetNamedItem("longitude").InnerText;
                                                    String lat = locInfo.Attributes.GetNamedItem("latitude").InnerText;

                                                    String date = "";
                                                    String city = "";
                                                    if (locInfo.Attributes.GetNamedItem("date") != null)
                                                    {
                                                        date = locInfo.Attributes.GetNamedItem("date").InnerText;
                                                    }
                                                    if (locInfo.Attributes.GetNamedItem("city") != null)
                                                    {
                                                        city = locInfo.Attributes.GetNamedItem("city").InnerText;
                                                    }
                                                    currentImage.setLocButtonInfo("yellow" + "/" + lon + "/" + lat + "/" + date + "/" + city);

                                                }

                                            }
                                            else if (locInfo.Name == "Display")
                                            {
                                                foreach (XmlNode displayLoc in locInfo.ChildNodes)
                                                {
                                                    if (displayLoc.Name == "Location")
                                                    {
                                                        Point p = this.parceLongLat(displayLoc);
                                                        //currentImage.addButton(new MapControl.MapButton(p.X, p.Y, 1, currentImage));

                                                        if (displayLoc.Attributes.GetNamedItem("longitude") != null)
                                                        {
                                                            String lon = displayLoc.Attributes.GetNamedItem("longitude").InnerText;

                                                            String lat = displayLoc.Attributes.GetNamedItem("latitude").InnerText;
                                                            String date = "";
                                                            String city = "";
                                                            if (displayLoc.Attributes.GetNamedItem("date") != null)
                                                            {
                                                                date = displayLoc.Attributes.GetNamedItem("date").InnerText;
                                                            }
                                                            if (displayLoc.Attributes.GetNamedItem("city") != null)
                                                            {
                                                                city = displayLoc.Attributes.GetNamedItem("city").InnerText;
                                                            }
                                                            currentImage.setLocButtonInfo("blue" + "/" + lon + "/" + lat + "/" + date + "/" + city);
                                                        }
                                                        // currentImage.addButton(new newMap.newMapButton(p.X, p.Y, 1, currentImage));

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                currentImage.keywords = keywds;
                                _imageCollection.Add(currentImage);

                                //figure out the time range of the collection
                                if (startY > endY)
                                {
                                    startY = year;
                                    endY = year;
                                }
                                else
                                {
                                    if (year < startY)
                                        startY = year;
                                    if (year > endY)
                                        endY = year;
                                }
                                if (currentImage != null)
                                    OnImageLoaded(new Helpers.ImageLoadedEventArgs(currentImage));
                            }
                        }
                        _starty = startY;
                        _endy = endY;
                    }
                }
            }
            //if the collection is empty
            if (_starty > +_endy)
            {
                _starty = 1250;
                _endy = 1950;
            }
        }

        public bool collectionEmpty()
        {
            return _collectionEmpty;
        }

        /// <summary>
        /// helper method that parses a location node with longitude and latitude
        /// </summary>
        /// <param name="locNode"></param>
        /// <returns></returns>
        private Point parceLongLat(XmlNode locNode)
        {
            double[] longLat = new double[2];
            XmlNode xs = locNode.Attributes.GetNamedItem("longitude");
            longLat[0] = Convert.ToDouble(locNode.Attributes.GetNamedItem("longitude").InnerText);
            longLat[1] = Convert.ToDouble(locNode.Attributes.GetNamedItem("latitude").InnerText);
            return new Point(longLat[0], longLat[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        public void startAll()
        {
            _displayedCollection = _imageCollection;
            this.loadCollection();
            this.arrangeImages(_starty, _endy, _windowSize.Height / 2);
            mainScatterViewItem.Center = new Point(MainCanvas.Width / 2, mainScatterViewItem.Center.Y);
            _initScatterPos = mainScatterViewItem.Center;
            _initScatterWH = new Point();
            _initScatterWH.X = mainScatterViewItem.Width;
            _initScatterWH.Y = mainScatterViewItem.Height;
            timeline.update(_starty, _endy, MainCanvas.Width);
            filterBoxContainer.Height = 450.0 / 1080.0 * _windowSize.Height;
            eventInfoContainer.Height = 500.0 / 1080.0 * _windowSize.Height;
            eventInfoContainer.Width = System.Windows.SystemParameters.PrimaryScreenWidth; //?
            //timelineFilt.init(this);
            filter.init(this);
            timeline.setRef(mainScatterViewItem);
            this.loadEvents();
            eventInfo.TextWrapping = TextWrapping.NoWrap;
            eventInfo.TextTrimming = TextTrimming.WordEllipsis;
        }

        public event Helpers.ImageLoadedHandler ImageLoaded;

        protected virtual void OnImageLoaded(Helpers.ImageLoadedEventArgs e)
        {
            if (this.ImageLoaded != null)
                this.ImageLoaded(this, e);
        }

        public event Helpers.ImageSelectedHandler HandleImageSelected;

        protected virtual void OnImageSelected(Helpers.ImageSelectedEventArgs e)
        {
            if (this.HandleImageSelected != null)
                this.HandleImageSelected(this, e);
        }

        public void HandleMapSelectedEvent(Object sender, Helpers.MapEventArgs e)
        {
            ImagesSelected(e.getImages());
        }

        public void HandleMapDeselectedEvent(Object sender, Helpers.MapEventArgs e)
        {
            ImagesSelected(_imageCollection);
        }

        public void ImagesSelected(List<ImageData> images)
        {
            int startY = 1;
            int endY = 0;
            foreach (ImageData i in images)
            {
                int year = i.year;
                if (startY > endY)
                {
                    startY = year;
                    endY = year;
                }
                else
                {
                    if (year < startY)
                        startY = year;
                    if (year > endY)
                        endY = year;
                }
            }
            _starty = startY;
            _endy = endY;
            if (images.Count == 0)
            {
                _starty = 0;
                _endy = 0;
                Message.Visibility = Visibility.Visible;
            }
            else
            {
                Message.Visibility = Visibility.Hidden;
            }

            foreach (ImageData i in _displayedCollection)
            {
                Canvas canv = i.Parent as Canvas;
                if (canv != null)
                {
                    Border bord = canv.Parent as Border;
                    if (bord != null)
                    {
                        (bord.Parent as StackPanel).Children.Remove(bord);
                        bord.Child = null;
                        canv.Children.Clear();
                    }
                }
            }
            _displayedCollection = images;

            arrangeImages(_starty, _endy, MainCanvas.Height);
            mainScatterViewItem.Center = new Point(MainCanvas.Width / 2, mainScatterViewItem.Center.Y);
            timeline.update(_starty, _endy, MainCanvas.Width);
            filterBoxContainer.Height = 450.0 / 1080.0 * _windowSize.Height;
            eventInfoContainer.Height = 500.0 / 1080.0 * _windowSize.Height;
            this.loadEvents();

            //fix highlight border of the selected image (if exists) 
            if (currentImage != null && currentImage.Parent != null)
            {
                ((Border)((Canvas)currentImage.Parent).Parent).BorderBrush = new SolidColorBrush(Color.FromRgb(0xff, 0xf6, 0x8b));
                ((Border)((Canvas)currentImage.Parent).Parent).Background = new SolidColorBrush(Color.FromRgb(0xff, 0xf6, 0x8b));
            }
        }

        private int ROWS = 3;
        /// <summary>
        /// Layout the images in the catalog. Not optimal, but works, 
        /// if there is a better way to do it, by all means change this.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="containerHeight"></param>
        private void arrangeImages(int start, int end, double containerHeight)
        {
            MainCanvas.Children.RemoveRange(0, MainCanvas.Children.Count);

            int rowHeight = (int)containerHeight / ROWS;
            int pad = 25;
            int imgHeight = (int)(containerHeight - (pad * (ROWS + 1))) / ROWS;

            //sort by year
            _displayedCollection.Sort((i1, i2) => i1.year.CompareTo(i2.year));

            if (_displayedCollection.Count >= 1)
            {
                foreach (ImageData img in _displayedCollection)
                    img.setSize(imgHeight);

                double aveWidth = _displayedCollection.Average(x => x.Width);//average width of the images
                //pixels we should assign to one year tick mark
                double unitLength = (double)_displayedCollection.Count / (double)ROWS * (double)aveWidth / (double)(end - start);
                if (end - start == 0)
                {
                    //treat the year interval as 1 instead of 0
                    unitLength = _displayedCollection.Count / (double)ROWS * aveWidth / 1;
                }
                //the space one image should take in terms of year count
                double interval = Math.Ceiling(aveWidth / unitLength);

                //divide the overall space into small parts based on the year and keep track of the availability of the spaces
                Dictionary<int, ImageCluster> space = new Dictionary<int, ImageCluster>();
                ImageCluster prevCluster = null;
                double rightPos = 1;
                foreach (ImageData img in _displayedCollection)
                {
                    int index = (int)((img.year - start) / interval);
                    ImageCluster cluster = null;
                    if (!space.ContainsKey(index))
                    {
                        Boolean collide = false;
                        //check if new cluster would collide with prevcluster
                        if (prevCluster != null)
                        {
                            if (index * aveWidth * 2 <= Canvas.GetLeft(prevCluster) + prevCluster.topRowWidth())
                            {
                                collide = true;
                                cluster = prevCluster;
                            }
                        }

                        //add new cluster
                        if (!collide)
                        {
                            cluster = new ImageCluster(ROWS);
                            space.Add(index, cluster);
                            MainCanvas.Children.Add(cluster);
                            Canvas.SetLeft(cluster, index * aveWidth * 2);//evil "*2"...
                        }
                    }
                    else
                    {
                        cluster = space[index];
                    }
                    cluster.addImage(img);
                    prevCluster = cluster;
                    //keep track of rightmost point to appropriately scale maincanvas
                    rightPos = Canvas.GetLeft(cluster) + cluster.topRowWidth();
                }

                //foreach (ImageCluster c in 

                MainCanvas.Width = rightPos;
                mainScatterViewItem.Width = MainCanvas.Width + _windowSize.Width;
            }
            //empty collection
            else
            {
                MainCanvas.Width = _windowSize.Width;
            }
            exitButton.Visibility = Visibility.Visible;
            InstructionBox.Visibility = Visibility.Visible;
            InstructionBox.Width = (_windowSize.Width / 4);
            InstructionBorder.Width = (_windowSize.Width / 4) -5;
            infoBox.Width = (_windowSize.Width / 4) -5;
            //InstructionBox.Width = System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            //MessageBox.Show("Screenwidth is: " + System.Windows.SystemParameters.PrimaryScreenWidth / 2);

        }

        /// <summary>
        /// no longer in use
        /// </summary>
        /// <param name="number"></param>
        /// <param name="spacing"></param>
        /// <param name="width"></param>
        private void drawLines(int number, int spacing, int width)
        {
            Brush br = new SolidColorBrush(Color.FromRgb(0x8b, 0x68, 0x4e));
            for (int i = 0; i < number; i++)
            {
                Line l = new Line();
                l.X1 = -1500;
                l.X2 = width + 1500;
                l.Y1 = spacing + (2 * i * spacing);
                l.Y2 = l.Y1;
                l.Stroke = br;
                l.StrokeThickness = 8;

                MainCanvas.Children.Add(l);
                Canvas.SetZIndex(l, -1);
            }
        }

        public void changeSize(Size PreviousSize, Size NewSize)
        {
            _background.Height = NewSize.Height;
            _background.Width = NewSize.Width;
            if (PreviousSize.Height != 0)
            {
                double zoomPercent = NewSize.Height / (PreviousSize.Height);
                double panPercent = (mainScatterViewItem.Center.X - _windowSize.Width / 2) / MainCanvas.ActualWidth;

                MainCanvas.Width = MainCanvas.Width * zoomPercent;
                MainCanvas.Height = MainCanvas.Height * zoomPercent;

                double marginX = (mainScatterViewItem.Width - MainCanvas.Width) / 2;
                double marginY = (mainScatterViewItem.Height - MainCanvas.Height) / 2;
                MainCanvas.Margin = new Thickness(marginX, marginY, marginX, marginY);

                timeline.zoom(zoomPercent);
                zoomImages(zoomPercent);
            }
        }

        bool zoomStopped = false;
        /// <summary>
        /// handler for mainScatterViewItem_SizeChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainScatterViewItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            //Console.WriteLine("New Width = " + e.NewSize.Width);
            if (e.NewSize.Width < 13000)
            {
                if (!zoomStopped)
                {
                    _background.Height = e.NewSize.Height;
                    _background.Width = e.NewSize.Width;
                    if (e.PreviousSize.Height != 0)
                    {
                        double zoomPercent = e.NewSize.Height / (e.PreviousSize.Height);
                        double panPercent = (mainScatterViewItem.Center.X - _windowSize.Width / 2) / MainCanvas.ActualWidth;

                        MainCanvas.Width = MainCanvas.Width * zoomPercent;
                        MainCanvas.Height = MainCanvas.Height * zoomPercent;

                        double marginX = (mainScatterViewItem.Width - MainCanvas.Width) / 2;
                        double marginY = (mainScatterViewItem.Height - MainCanvas.Height) / 2;
                        MainCanvas.Margin = new Thickness(marginX, marginY, marginX, marginY);

                        timeline.zoom(zoomPercent);
                        zoomImages(zoomPercent);
                    }
                }
                zoomStopped = false;
            }
            else
            {
                zoomStopped = true;
                mainScatterViewItem.SizeChanged -= mainScatterViewItem_SizeChanged;
                mainScatterViewItem.Height = e.PreviousSize.Height;
                mainScatterViewItem.Width = e.PreviousSize.Width;
                mainScatterViewItem.SizeChanged += mainScatterViewItem_SizeChanged;
            }
        }

        /// <summary>
        /// Zoom the images when the catalog as a whole is zoomed
        /// </summary>
        /// <param name="zoomPercent"></param>
        private void zoomImages(double zoomPercent)
        {
            _curZoomFactor *= zoomPercent;
            foreach (UIElement IC in MainCanvas.Children)
            {
                if (IC.ToString().Equals("GCNav.ImageCluster"))
                {
                    Canvas.SetLeft(IC, Canvas.GetLeft(IC) * zoomPercent);
                    ImageCluster _IC = (ImageCluster)IC;
                    foreach (ImageData i in _IC.getImages())
                    {
                        //new zoom factor that takes the border of the image into consideration
                        double izoom = ((i.Width + _IC.size_padding_constant) * zoomPercent - _IC.size_padding_constant) / i.Width;
                        i.Width *= izoom;
                        i.Height *= izoom;
                        ((Canvas)i.Parent).Height *= izoom;
                        ((Canvas)i.Parent).Width *= izoom;
                        ((TextBlock)((Canvas)i.Parent).Children[1]).Width *= izoom;
                    }
                }
                else if (IC.ToString().Equals("System.Windows.Shapes.Line"))//doesn't exist anymore
                {
                    ((Line)IC).Y1 *= zoomPercent;
                    ((Line)IC).Y2 = ((Line)IC).Y1;
                }
            }
        }

        public void resetZoom()
        {
            mainScatterViewItem.Width = _initScatterWH.X;
            mainScatterViewItem.Height = _initScatterWH.Y;
            mainScatterViewItem.Center = new Point(_initScatterPos.X, _initScatterPos.Y);
            timeline.updateTickMarks();
        }

        private void mainScatterViewItem_CenterChanged(Object sender, EventArgs e)
        {
            //left
            if (mainScatterViewItem.Center.X + MainCanvas.Width / 2 < _windowSize.Width * 1 / 2)
                mainScatterViewItem.Center = new Point(_windowSize.Width * 1 / 2 - MainCanvas.Width / 2, mainScatterViewItem.Center.Y);
            //right
            if (mainScatterViewItem.Center.X - MainCanvas.Width / 2 > _windowSize.Width * 1 / 2)
                mainScatterViewItem.Center = new Point(_windowSize.Width * 1 / 2 + MainCanvas.Width / 2, mainScatterViewItem.Center.Y);
            //up
            if (mainScatterViewItem.Center.Y + MainCanvas.Height / 2 < (_windowSize.Height / 2) * 1 / 2)
                mainScatterViewItem.Center = new Point(mainScatterViewItem.Center.X, (_windowSize.Height / 2) * 1 / 2 - MainCanvas.Height / 2);
            //down
            if (mainScatterViewItem.Center.Y - MainCanvas.Height / 2 > (_windowSize.Height / 2) * 1 / 2)
                mainScatterViewItem.Center = new Point(mainScatterViewItem.Center.X, (_windowSize.Height / 2) * 1 / 2 + MainCanvas.Height / 2);

            double panPercent = (MainCanvas.ActualWidth / 2 - mainScatterViewItem.Center.X) / MainCanvas.ActualWidth;
            if (MainCanvas.ActualWidth != Double.NaN && MainCanvas.ActualWidth != 0)
                timeline.pan(panPercent);
        }

        /// <summary>
        /// layout everything based on the window size
        /// </summary>
        /// <param name="previous"></param>
        private void setDimension(Size previous)
        {

            mainScatterView.Width = _windowSize.Width * 1000;
            mainScatterView.Height = _windowSize.Height * 1000;
            mainScatterView.Margin = new Thickness(0, _windowSize.Height / 2, 0, 0);

            double zoomPercent = _windowSize.Height / previous.Height;

            if (previous.Height == 0)
            {
                zoomPercent = 1;
            }

            //ScatterViewItem is always bigger than the actual canvas that's holding the 
            //collection, so that there can be more touchable space
            mainScatterViewItem.Height = _windowSize.Height / 2 + _windowSize.Height / 2;
            mainScatterViewItem.Width = mainScatterViewItem.ActualWidth * zoomPercent;
            MainCanvas.Height = _windowSize.Height / 2;
            MainCanvas.Width = MainCanvas.ActualWidth * zoomPercent;

            double marginX = (mainScatterViewItem.Width - MainCanvas.Width) / 2;
            double marginY = (mainScatterViewItem.Height - MainCanvas.Height) / 2;
            MainCanvas.Margin = new Thickness(marginX, marginY, marginX, marginY);

            mainScatterViewItem.MaxHeight = 11000;
            mainScatterViewItem.MaxWidth = 16000;
            mainScatterViewItem.MinWidth = _windowSize.Width;
            mainScatterViewItem.MinHeight = _windowSize.Height / 2;
            mainScatterViewItem.Center = new Point(_windowSize.Width / 2, _windowSize.Height / 4);

            curImageContainer.Height = _windowSize.Height / 3;
            curImageContainer.Width = _windowSize.Width / 4;

            curInfoContainer.Height = _windowSize.Height / 3;
            curInfoContainer.Width = _windowSize.Width / 4;
            curInfoCol.Width = _windowSize.Width / 4;
            curInfoCol.Height = _windowSize.Height / 3;
           // infoScroll.Width = curInfoCol.Width + 20;
           // infoScroll.Height = curInfoCol.Height + 20;
            
            //Console.Out.WriteLine("curinfo" + curInfoContainer.Width);
           // Console.Out.WriteLine("curImage" + curImageContainer.Width);

            //filterBoxContainer.Width =576;

            timeline.setSize(_windowSize.Width, _windowSize.Height / 12);
            Message.Margin = new Thickness(0, _windowSize.Height / 3, 0, 0);


        }

        /// <summary>
        /// get called when the window size changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowSize = e.NewSize;
            setDimension(e.PreviousSize);

        }

        public delegate void MainImageSelectedHandler(object sender, EventArgs e);
        public event MainImageSelectedHandler mainImageSelected;

        private void HandleImageTouched(object sender, EventArgs e)
        {
            /*if (artmode != null)
            {
                mainImageSelected(currentImage.filename, e);
                mainImageSelected += artmode.NewImageSelected_Handler;
            }
            else
            {*/
            if (!_artOpen)
            {
                /*artmode.MultiImage.SetImageSource(@"C:\garibaldi\garibaldi\Panels DeepZoom\Panel 10\Exported Data\panel 10\dzc_output.xml");
                artmode.MultiImageThumb.SetImageSource(@"C:\garibaldi\garibaldi\Panels DeepZoom\Panel 10\Exported Data\panel 10\dzc_output.xml");*/
                artmode = new LADSArtworkMode.ArtworkModeWindow(currentImage.filename);
                artmode.Closed += new EventHandler(onArtmodeClose);
                _timer.Tick += new EventHandler(TimerTick_Handler);
                artmode.MultiImage.SetImageSource(@currentImage.xmlpath);
                artmode.MultiImageThumb.SetImageSource(@currentImage.xmlpath);
                artmode.Show();

               
                
                _artOpen = true;
                artmode.LayoutArtworkMode(currentImage.filename);

                if (SavedDockedItems != null)
                {
                    Console.WriteLine("Saved Docked Items not null!");
                    artmode.LoadDockedItems(SavedDockedItems);
                }
                else
                {
                    Console.WriteLine("Saved Docked Items is null!");
                }
                //artmode.currentArtworkFileName = currentImage.filename;
                artmode.currentArtworkTitle = currentImage.title;
                
                //artmode.addDockedItems(dockedItems);
                
                Console.WriteLine(currentImage.filename);// add an input param in order to handle different artworks hotspots.
            }
            else
            {
                if (currentImage.filename != artmode.currentArtworkFileName)
                {
                    if (MessageBox.Show("Are you sure you want to switch artworks? You will lose what you have been working on.", "Switch", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ArtworkModeWindow newWindow = new ArtworkModeWindow(currentImage.filename);
                        artmode.Close();
                        artmode = newWindow;
                        artmode.Closed += new EventHandler(onArtmodeClose);
                        
                        
                        _timer.Tick += new EventHandler(TimerTick_Handler);
                        artmode.MultiImage.SetImageSource(@currentImage.xmlpath);
                        artmode.MultiImageThumb.SetImageSource(@currentImage.xmlpath);
                        newWindow.Show();
                        _artOpen = true;
                        artmode.LayoutArtworkMode(currentImage.filename);
                        artmode.currentArtworkTitle = currentImage.title;

                        if (SavedDockedItems != null)
                        {
                            Console.WriteLine("Saved Docked Items not null!");
                            artmode.LoadDockedItems(SavedDockedItems);
                        }
                        else
                        {
                            Console.WriteLine("Saved Docked Items is null!");
                        }
                    }
                    else
                    {
                        artmode.Show();
                        return;
                    }
                }
                else
                {
                    artmode.Hide();
                    artmode.Show();
                    artmode.ShowActivated = true;
                }
            }
            //}
        }
        List<DockableItem> dockedItems;
        public void onArtmodeClose(object sender, EventArgs e)
        {
            _timer.Tick -= TimerTick_Handler;
            //dockedItems = artmode.DockedDockableItems;
            SavedDockedItems = artmode.SavedDockedItems;
            _artOpen = false;
            artmode.Close();
        }

        public void onArtSwitched(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// loads the events and sets each one's parent to this navigator
        /// </summary>
        public void loadEvents()
        {
            List<Event> events = timeline.getEvents();
            foreach (Event e in events)
            {
                e.setParent(this);
                //e.PreviewTouchDown+=new EventHandler<TouchEventArgs>(EventTouchedHandler);
                e.PreviewMouseDown += EventTouchedHandler;
            }
        }

        public void EventTouchedHandler(object sender, EventArgs e)
        {
            eventSelected((Event)sender);
        }


        /// <summary>
        /// when an event has been selected, display its information if it is currently unselected and hide its information if its currently selected
        /// </summary>
        /// <param name="ev"></param>
        public void eventSelected(Event ev)
        {

            if (ev.infoIsDisplayed())
            {
                eventInfo.Text = "";
                ev.setInfoIsDisplayed(false);
                eventInfoContainer.Visibility = Visibility.Hidden;
                _eventDisplayed = null;
            }
            else
            {
                eventInfoContainer.Height = 500.0 / 1080.0 * _windowSize.Height;
                String newText = "";
                newText += ev.Event_Name + " (" + ev.Start + " - " + ev.End + ")";
                if (ev.Location != "")
                {
                    newText += ", " + ev.Location;
                }
                if (ev.Description != "")
                {
                    newText += ": " + ev.Description;
                }
                eventInfo.Text = newText;
                ev.setInfoIsDisplayed(true);
                eventInfoContainer.Visibility = Visibility.Visible;
                if (_eventDisplayed != null)
                {
                    _eventDisplayed.setInfoIsDisplayed(false);
                }
                _eventDisplayed = ev;
            }
            //KeywordsTitle.Visibility = Visibility.Hidden;
            //curKeywords.Visibility = Visibility.Hidden;


            /*if (((((curInfoContainer.Children[0]) as Border).Child as Grid).Children[0] as TextBlock) == curInfo && curInfo!=null)
                Console.WriteLine("YES");
            else
                Console.WriteLine("NO");*/
        }

        /*
        * Draw image scaled appropriately in the 'current image' canvas 
        * and display text in the information canvas.
        * (called from ImageData when clicked)
        */
        public void imageSelected(ImageData img)
        {
            curImageContainer.Visibility = Visibility.Visible;
            curInfoContainer.Visibility = Visibility.Visible;

            //Console.WriteLine(curInfo.Text);

            if (currentImage != null && currentImage.Parent != null)
            {
                ((Border)((Canvas)currentImage.Parent).Parent).BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x2d, 0x0c));
                ((Border)((Canvas)currentImage.Parent).Parent).Background = new SolidColorBrush(Color.FromRgb(0x00, 0x2d, 0x0c));
            }
            ((Border)((Canvas)img.Parent).Parent).BorderBrush = new SolidColorBrush(Color.FromRgb(0xff, 0xf6, 0x8b));
            ((Border)((Canvas)img.Parent).Parent).Background = new SolidColorBrush(Color.FromRgb(0xff, 0xf6, 0x8b));

            OnImageSelected(new Helpers.ImageSelectedEventArgs(img));
            curImageCanvas.Children.Clear();
            Image _curImage = new Image();
            _curImage.Source = img.Source;
            _curImage.Height = this.ActualHeight / 4;
           // curImageCanvas.Height = _curImage.Height + 50;
           // curImageCanvas.Width = _curImage.Width + 50;
            curImageCanvas.Width = _windowSize.Width / 4;
            curImageCanvas.Children.Add(_curImage);
            currentImage = img;
            _curImage.TouchDown += HandleImageTouched;
            _curImage.MouseDown += HandleImageTouched;
            Canvas.SetTop(_curImage, 25);
            Canvas.SetLeft(_curImage, 25);

           
            curInfoCol.Width = _windowSize.Width / 4;
            curInfoCol.Height = this.ActualHeight / 4;
            infoScroll.Height = curInfoCol.Height;
            ColumnDefinition width = new ColumnDefinition();
            GridLength length = new GridLength(_windowSize.Width / 4-25);
            width.Width = length;
            curInfoCol.ColumnDefinitions.Add(width);

            title.Text = "";
            artist.Text = "";
            medium.Text = "";
            date.Text = "";
            //title.UpdateLayout();
            //artist.UpdateLayout();
            //curInfo.Text = "";
           // title.MaxWidth = _windowSize.Width / 4 - 10;
           // curKeywords.MaxWidth = _windowSize.Width / 4 - 55;
            title.Text += "Title: " + img.title ;
            curInfoCol.UpdateLayout();
            titleBack.Width = _windowSize.Width / 4 -20;
            titleBack.Height = title.ActualHeight+5;
         //   Double artistTop = title.ActualHeight +10;
            Console.Out.WriteLine("title height" + title.ActualHeight);
          //  Canvas.SetTop(artist, artistTop);
          //  Canvas.SetTop(medium, artistTop + 25);
          //  Canvas.SetTop(date, artistTop + 50);
            artist.Text += "Artist: " + img.artist;
            medium.Text += "Medium: " + img.medium;
            date.Text += "Year: " + img.year;
            
            title.FontSize = 25 * _windowSize.Height / 1080.0;
            artist.FontSize = 20 * _windowSize.Height / 1080.0;
            medium.FontSize = artist.FontSize;
            date.FontSize = artist.FontSize;
            KeywordsTitle.FontSize = 18 * _windowSize.Height / 1080.0;
            curKeywords.FontSize = 18 *_windowSize.Height / 1080.0;
            curInfoCol.UpdateLayout();
            titleBack.Height = titleBack.ActualHeight + 10;
            
            //Canvas.SetTop(KeywordsTitle, artistTop + 75);
            //Canvas.SetTop(curKeywords, artistTop + 105);
           
            //infoCanvas.Height = this.ActualHeight / 4;
           // Canvas.SetTop(curInfo, 35);
            //infoCanvas.Width = _windowSize.Width / 4;
            
            // newColWidth.Width = _windowSize.Width / 4;
            //column.Width = _windowSize.Width / 4;
            if (currentImage.keywords.Count() > 0)
            {
                KeywordsTitle.Visibility = Visibility.Visible;
                curKeywords.Visibility = Visibility.Visible;
                //Canvas.SetTop(KeywordBack, artistTop + 75);
                KeywordBack.Visibility = Visibility.Visible;
                
            }
            else
            {
                KeywordsTitle.Visibility = Visibility.Hidden;
                curKeywords.Visibility = Visibility.Hidden;
                KeywordBack.Visibility = Visibility.Hidden;
            }

            KeywordBack.Width = _windowSize.Width / 4 -20;
          
            curKeywords.Text = "";

            bool b = false;
            foreach (String s in currentImage.keywords)
            {
                if (b)
                    curKeywords.Text += ",";
                else
                    b = true;
                curKeywords.Text += s;
            }
            curInfoCol.UpdateLayout();
           // curInfoCol.Height = artistTop + 105 + curKeywords.Height;
            //curKeywordsBack.Width = _windowSize.Width / 4 - 46;
            
           // curKeywordsBack.Height = curKeywords.ActualHeight + 5;
            //Canvas.SetTop(curKeywordsBack, artistTop + 105);
            //if (artistTop + 105 + curKeywords.ActualHeight > curInfoCol.Height)
            //{
            //    curInfoCol.Height = artistTop + 105 + curKeywords.ActualHeight;
            //    //if (infoScroll.Visibility == Visibility.Visible)
            //    //{
            //    //    Console.Out.WriteLine("max width" + title.MaxWidth);
            //    //    title.MaxWidth -= 60;
            //    //    title.FontSize -= 2;
            //    //    Console.Out.WriteLine("max width" + title.MaxWidth);
            //    //    curKeywords.MaxWidth -= 25;
            //    //    curKeywords.FontSize -= 2;
            //    //    curInfoCol.UpdateLayout();
            //    //}
              
            //}
          //  KeywordBack.Height = KeywordsTitle.ActualHeight + curKeywords.ActualHeight + 15;
            RowDefinition height = new RowDefinition();
            GridLength height1 = new GridLength(curKeywords.ActualHeight);
            height.Height = height1;
            curInfoCol.RowDefinitions.Add(height);

            curKeywords.Height = curKeywords.ActualHeight;
            Console.Out.WriteLine("height" + curKeywords.ActualHeight);
            infoScroll.UpdateLayout();
            Console.Out.WriteLine("height" + curKeywords.ActualHeight);
            //curInfoCol.Height = title.ActualHeight + artist.ActualHeight + date.ActualHeight + medium.ActualHeight + curKeywords.ActualHeight +100;
        }

        /// <summary>
        /// adapter for mouse control, just for testing purposes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainScatterViewItem_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                mainScatterViewItem.Width *= 1.1;
                mainScatterViewItem.Height *= 1.1;
            }
            else
            {
                mainScatterViewItem.Width *= 0.9;
                mainScatterViewItem.Height *= 0.9;
            }
        }

    }
}
