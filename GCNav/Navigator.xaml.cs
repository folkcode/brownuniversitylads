﻿using System;
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

        private ImageData currentImage;
        private System.Windows.Forms.Timer _timer;
        private bool _artOpen, _collectionEmpty;
        public FilterTimelineBox filter;

        public List<DockedItemInfo> SavedDockedItems;

        // The arrows that indicate that there is more timeline content offscreen.
        Polygon _left_arrow, _right_arrow;

        public Navigator()
        {
            InitializeComponent();

            curImageContainer.Visibility = Visibility.Hidden;
            curInfoContainer.Visibility = Visibility.Hidden;
            mainScatterViewItem.Width = 1920;
            MainCanvas.Width = 0;
            mainScatterViewItem.Background = Brushes.Transparent;
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
        }

        public void TimerTick_Handler(object sender, EventArgs e)
        {
            if (artmode != null)
            {
                artmode.updateWebImages();
            }
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

                                String fullPath = dataDir + "Images/" + path;
                                String thumbPath = dataDir + "Images/" + "Thumbnail/" + path;
                                String xmlPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + dataDir + "Images/" + "DeepZoom/" + path + "/dz.xml";

                                ImageData currentImage = new ImageData(thumbPath);
                                currentImage.xmlpath = xmlPath;
                                currentImage.fullpath = fullPath;
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
                                                    currentImage.setLocButtonInfo("red" + "///" + lon + "///" + lat + "///" + date + "///" + city);
                                                }

                                            }
                                            else if (locInfo.Name == "Work")
                                            {
                                                Point p = this.parceLongLat(locInfo);
                                                currentImage.addButton(new MapControl.MapButton(p.X, p.Y, 0, currentImage));
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
                                                    currentImage.setLocButtonInfo("yellow" + "///" + lon + "///" + lat + "///" + date + "///" + city);

                                                }

                                            }
                                            else if (locInfo.Name == "Display")
                                            {
                                                foreach (XmlNode displayLoc in locInfo.ChildNodes)
                                                {
                                                    if (displayLoc.Name == "Location")
                                                    {
                                                        Point p = this.parceLongLat(displayLoc);
                                                        
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
                                                            currentImage.setLocButtonInfo("blue" + "///" + lon + "///" + lat + "///" + date + "///" + city);
                                                        }
                                                       
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
            //if the collection is empty, arbitrarily set the start and end values
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
        /// Loads collection, timeline, etc.
        /// </summary>
        public void startAll()
        {
            _displayedCollection = _imageCollection;
            this.loadCollection();
            double timeline_length = this.arrangeImages(_starty, _endy, _windowSize.Height / 2);
            mainScatterViewItem.Center = new Point(MainCanvas.Width / 2 + _windowSize.Width * 999 / 2, mainScatterViewItem.Center.Y);
            _initScatterPos = mainScatterViewItem.Center;
            _initScatterWH = new Point();
            _initScatterWH.X = mainScatterViewItem.Width;
            _initScatterWH.Y = mainScatterViewItem.Height;
            timeline.update(_starty, _endy, timeline_length/*MainCanvas.Width*/);
            filterBoxContainer.Height = 450.0 / 1080.0 * _windowSize.Height;
            eventInfoContainer.Height = 500.0 / 1080.0 * _windowSize.Height;
            eventInfoContainer.Width = System.Windows.SystemParameters.PrimaryScreenWidth; //?
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

            double timeline_width = arrangeImages(_starty, _endy, MainCanvas.Height);
            mainScatterViewItem.Center = new Point(MainCanvas.Width / 2 + _windowSize.Width * 999 / 2, mainScatterViewItem.Center.Y);
            timeline.update(_starty, _endy, timeline_width); // MICHAEL PRICE!!! WE NEED TO TALK! -- yudi
            //timeline.update(_starty, _endy, MainCanvas.Width); 
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
        private double arrangeImages(int start, int end, double containerHeight)
        {
            double timeline_length = 0;

            MainCanvas.Children.RemoveRange(0, MainCanvas.Children.Count);

            int rowHeight = (int)containerHeight / ROWS;
            int pad = 25;  // The padding between rows.
            int imgHeight = (int)(containerHeight - (pad * (ROWS + 1))) / ROWS;

            // Sort all the images by year.
            _displayedCollection.Sort((i1, i2) => i1.year.CompareTo(i2.year));

            if (_displayedCollection.Count >= 1)
            {
                // Give each image the same height to fit in a row.
                foreach (ImageData img in _displayedCollection)
                    img.setSize(imgHeight);

                double aveWidth = _displayedCollection.Average(x => x.Width);  // The average width of the images.

                // The number of pixels taken up by one year in the timeline:  Images per row * average width / time interval.
                double unitLength = (double)_displayedCollection.Count / (double)ROWS * aveWidth / (double)(end - start);
                if (end - start == 0)
                {
                    //  If time range is 0,  treat it as 1 instead.
                    unitLength = _displayedCollection.Count / (double)ROWS * aveWidth / 1;
                }
                Console.WriteLine("unit_length: " + unitLength);

                double new_unit_length = this.arrangeHelper(start, end, aveWidth, unitLength);
                Console.WriteLine("new_unit_length: " + new_unit_length);

                MainCanvas.Children.Clear();
                foreach (ImageData img in _displayedCollection)
                {
                    (img.Parent as Canvas).Children.Clear();
                }

                this.arrangeHelper(start, end, aveWidth, new_unit_length);

                ImageCluster lastCluster = null;
                foreach (ImageCluster ic in MainCanvas.Children)
                {
                    if (lastCluster == null ||
                        lastCluster.maxYear < ic.maxYear)
                    {
                        lastCluster = ic;
                    }
                }

                timeline_length = MainCanvas.Width;// -lastCluster.longestRowWidth();
            }
            else
            {
                MainCanvas.Width = _windowSize.Width;
                timeline_length = MainCanvas.Width;
            }
            InstructionBox.Visibility = Visibility.Visible;
            InstructionBox.Width = (_windowSize.Width / 4);
            InstructionBorder.Width = (_windowSize.Width / 4) - 5;
            infoBox.Width = (_windowSize.Width / 4) - 5;

            return (timeline_length == 0) ? 1 : timeline_length ;
        }

        // Does one pass of arranging the images into clusters.  Returns the longest cluster.
        private double arrangeHelper(int start, int end, double aveWidth, double unitLength)
        {
            // The space one image should take in terms of year count.
            double interval_width = Math.Ceiling(aveWidth / unitLength);

            // Dividee the overall space into small parts based on the year and keep track of the availability of the spaces.
            Dictionary<int, ImageCluster> space = new Dictionary<int, ImageCluster>();
            ImageCluster prevCluster = null;
            double rightPos = 1;
            foreach (ImageData img in _displayedCollection)
            {
                // The index of the chunk of timeline space that contains the year of this image.
                int index = (int)((img.year - start) / interval_width);
                ImageCluster cluster = null;
                if (!space.ContainsKey(index))
                {
                    Boolean collide = false;
                    // Check if the new cluster would collide with the previous cluster. If so, add to the previous cluster instead.
                    if (prevCluster != null)
                    {
                        if (index * aveWidth <= Canvas.GetLeft(prevCluster) + prevCluster.longestRowWidth())
                        {
                            collide = true;
                            cluster = prevCluster;
                        }
                    }

                    // Add a new cluster.
                    if (!collide)
                    {
                        cluster = new ImageCluster(ROWS);
                        space.Add(index, cluster);
                        MainCanvas.Children.Add(cluster);
                        Canvas.SetLeft(cluster, index * aveWidth);
                    }
                }
                else
                {
                    cluster = space[index];
                }
                cluster.addImage(img);
                prevCluster = cluster;

                // Center the cluster by shifting it left by half the width.
                //Canvas.SetLeft(cluster, Canvas.GetLeft(cluster) - cluster.longestRowWidth() / 2.0);

                //keep track of rightmost point to appropriately scale maincanvas
                rightPos = Canvas.GetLeft(cluster) + cluster.longestRowWidth();
            }

            MainCanvas.Width = rightPos;
            mainScatterViewItem.Width = MainCanvas.Width + _windowSize.Width;

            double marginX = (mainScatterViewItem.Width - MainCanvas.Width) / 2;
            double marginY = (mainScatterViewItem.Height - MainCanvas.Height) / 2;
            MainCanvas.Margin = new Thickness(marginX, marginY, marginX, marginY);

            ImageCluster longest_cluster = null;
            // Find the longest cluster.
            foreach (ImageCluster ic in space.Values)
            {
                if (longest_cluster == null ||
                    longest_cluster.longestRowWidth() < ic.longestRowWidth())
                {
                    longest_cluster = ic;
                }
            }

            int diff = longest_cluster.maxYear - longest_cluster.minYear;
            diff = (diff == 0) ? 1 : diff;
            return (double)longest_cluster.getSize() / ((double)ROWS) * aveWidth / ((double) diff);
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
            if (e.NewSize.Height < 11000 && e.NewSize.Height > _windowSize.Height / 2)
            {
                if (!zoomStopped)
                {
                    if (e.PreviousSize.Height != 0)
                    {
                        double zoomPercent = e.NewSize.Height / (e.PreviousSize.Height);
                        //double panPercent = (mainScatterViewItem.Center.X - _windowSize.Width / 2) / MainCanvas.ActualWidth;

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
            if (!this.collectionEmpty() && _left_arrow != null)
            {
                _left_arrow.Visibility = Visibility.Visible;
                _right_arrow.Visibility = Visibility.Visible;
            }

            //left
            if (mainScatterViewItem.Center.X - _windowSize.Width * 999 / 2 + MainCanvas.Width / 2 < _windowSize.Width * 1 / 2)
            {
                mainScatterViewItem.Center = new Point(_windowSize.Width * 1 / 2 - MainCanvas.Width / 2 + _windowSize.Width * 999 / 2, mainScatterViewItem.Center.Y);
                if (_right_arrow != null)
                    _right_arrow.Visibility = Visibility.Collapsed;
            }
            //right
            if (mainScatterViewItem.Center.X - _windowSize.Width * 999 / 2 - MainCanvas.Width / 2 > _windowSize.Width * 1 / 2)
            {
                mainScatterViewItem.Center = new Point(_windowSize.Width * 1 / 2 + MainCanvas.Width / 2 + _windowSize.Width * 999 / 2, mainScatterViewItem.Center.Y);
                if (_left_arrow != null)
                    _left_arrow.Visibility = Visibility.Collapsed;
            }
            //up
            if (mainScatterViewItem.Center.Y + MainCanvas.Height / 2 < (_windowSize.Height / 2) * 1 / 2)
                mainScatterViewItem.Center = new Point(mainScatterViewItem.Center.X, (_windowSize.Height / 2) * 1 / 2 - MainCanvas.Height / 2);
            //down
            if (mainScatterViewItem.Center.Y - MainCanvas.Height / 2 > (_windowSize.Height / 2) * 1 / 2)
                mainScatterViewItem.Center = new Point(mainScatterViewItem.Center.X, (_windowSize.Height / 2) * 1 / 2 + MainCanvas.Height / 2);

            double panPercent = (MainCanvas.ActualWidth / 2 - (mainScatterViewItem.Center.X - _windowSize.Width * 999 / 2)) / MainCanvas.ActualWidth;
            if (MainCanvas.ActualWidth != Double.NaN && MainCanvas.ActualWidth != 0)
                timeline.pan(panPercent);
        }

        /// <summary>
        /// layout everything based on the window size
        /// </summary>
        /// <param name="previous"></param>
        private void setDimension(Size previous)
        {
            //MainGrid.Height = _windowSize.Height;
            //MainGrid.Width = _windowSize.Width;

            mainScatterView.Width = _windowSize.Width * 1000;
            mainScatterView.Height = _windowSize.Height * 1000;
            //shift the main scatterview to the left by _windowSize.Width * 999 / 2
            mainScatterView.Margin = new Thickness(-_windowSize.Width * 999 / 2, _windowSize.Height / 2, 0, 0);

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
            //mainScatterViewItem.MinWidth = _windowSize.Width;
            mainScatterViewItem.MinHeight = _windowSize.Height / 2;
            mainScatterViewItem.Center = new Point(_windowSize.Width / 2 + _windowSize.Width * 999 / 2, _windowSize.Height / 4);
            //the scatterview item needs to be shifted to the right by _windowSize.Width * 999 / 2, so that we can see it on screen

            MainCanvas.MaxHeight = mainScatterViewItem.MaxHeight - _windowSize.Height / 2;

            curImageContainer.Height = _windowSize.Height / 3;
            curImageContainer.Width = _windowSize.Width / 4;
            curImageCanvas.Width = _windowSize.Width / 4 - 10;
            curImageCanvas.Height = _windowSize.Height / 3 - 10;
            curImageCanvas1.Width = _windowSize.Width / 4 - 10;
            curImageCanvas1.Height = _windowSize.Height / 3 - 10;
            curInfoContainer.Height = _windowSize.Height / 3;
            curInfoContainer.Width = _windowSize.Width / 4;
            curInfoCol.Width = _windowSize.Width / 4;
            infoScroll.MaxHeight = _windowSize.Height / 3;

            timeline.setSize(_windowSize.Width, _windowSize.Height / 12);
            Message.Margin = new Thickness(0, _windowSize.Height / 3, 0, 0);

            // Make the triangles to indicate that there is more content on the timeline offscreen.
            if (_left_arrow == null)
            {
                _left_arrow = new Polygon();
                PointCollection l_arrow_points = new PointCollection();
                l_arrow_points.Add(new Point(0.5, 0));
                l_arrow_points.Add(new Point(0.5, 1));
                l_arrow_points.Add(new Point(0.0, 0.5));
                _left_arrow.Points = l_arrow_points;
                _left_arrow.Fill = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)); ;
                _left_arrow.Stretch = Stretch.Fill;
                _left_arrow.Stroke = Brushes.Transparent;
                _left_arrow.StrokeThickness = 5;
                _left_arrow.Height = _windowSize.Height / 4.0;
                _left_arrow.Width = _windowSize.Height / 16.0;

                MainGrid.Children.Add(_left_arrow);
                _left_arrow.Margin = new Thickness(0, _windowSize.Height * 5 / 8, 0, _windowSize.Height / 8);
                _left_arrow.HorizontalAlignment = HorizontalAlignment.Left;

                _right_arrow = new Polygon();
                PointCollection r_arrow_points = new PointCollection();
                r_arrow_points.Add(new Point(0, 0));
                r_arrow_points.Add(new Point(0, 1));
                r_arrow_points.Add(new Point(0.5, 0.5));
                _right_arrow.Points = r_arrow_points;
                _right_arrow.Fill = new SolidColorBrush(Color.FromArgb(200,0,0,0));
                _right_arrow.Stretch = Stretch.Fill;
                _right_arrow.Stroke = Brushes.Transparent;
                _right_arrow.StrokeThickness = 5;
                _right_arrow.Height = _windowSize.Height / 4.0;
                _right_arrow.Width = _windowSize.Height / 16.0;

                MainGrid.Children.Add(_right_arrow);
                _right_arrow.Margin = new Thickness(0, _windowSize.Height * 5 / 8, 0, _windowSize.Height / 8);
                _right_arrow.HorizontalAlignment = HorizontalAlignment.Right;

                _left_arrow.Visibility = Visibility.Collapsed;
                _right_arrow.Visibility = Visibility.Collapsed;
            }

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

        private void HandleImageTouched(object sender, EventArgs e)
        {
            if (!_artOpen)
            {
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
                    artmode.LoadDockedItems(SavedDockedItems);
                }
                else
                {
                }
                artmode.currentArtworkTitle = currentImage.title;
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
                            artmode.LoadDockedItems(SavedDockedItems);
                        }
                        else
                        {
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
        }
        public void onArtmodeClose(object sender, EventArgs e)
        {
            _timer.Tick -= TimerTick_Handler;
            SavedDockedItems = artmode.SavedDockedItems;
            _artOpen = false;
            artmode.Close();
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

            if (currentImage != null && currentImage.Parent != null)
            {
                ((Border)((Canvas)currentImage.Parent).Parent).BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x2d, 0x0c));
                ((Border)((Canvas)currentImage.Parent).Parent).Background = new SolidColorBrush(Color.FromRgb(0x00, 0x2d, 0x0c));
            }
            ((Border)((Canvas)img.Parent).Parent).BorderBrush = new SolidColorBrush(Color.FromRgb(0xff, 0xf6, 0x8b));
            ((Border)((Canvas)img.Parent).Parent).Background = new SolidColorBrush(Color.FromRgb(0xff, 0xf6, 0x8b));

            OnImageSelected(new Helpers.ImageSelectedEventArgs(img));
            curImageCanvas.Children.Clear();

            Image curImage = new Image();
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(img.fullpath, UriKind.Relative);
            bitmap.EndInit();
            curImage.Source = bitmap;

            curImageCanvas.Children.Add(curImage);
            curImageContainer.Height = _windowSize.Height / 3;
            curImageContainer.Width = _windowSize.Width / 4;
            curImageCanvas.Width = _windowSize.Width / 4 - 10;
            curImageCanvas.Height = _windowSize.Height / 3 - 10;
            curImageCanvas1.Width = _windowSize.Width / 4 - 10;
            curImageCanvas1.Height = _windowSize.Height / 3 - 10;
            Double actualWidth = curImage.Source.Width;
            Double actualHeight = curImage.Source.Height;
            Double ratio = actualWidth / actualHeight;
            Double canvasRatio = curImageCanvas.Width / curImageCanvas.Height;
            ScaleTransform tran = new ScaleTransform();
            if (ratio > canvasRatio)
            {
                Double scale = (curImageCanvas.ActualWidth - 30) / actualWidth;
                tran.ScaleX = scale;
                tran.ScaleY = scale;
            }
            else
            {
                Double scale = (curImageCanvas.ActualHeight - 30) / actualHeight;
                tran.ScaleX = scale;
                tran.ScaleY = scale;
            }
            curImage.RenderTransform = tran;
            curImageCanvas.UpdateLayout();
            Canvas.SetTop(curImage, (curImageCanvas.Height - curImage.ActualHeight * tran.ScaleY) / 2);
            Canvas.SetLeft(curImage, (curImageCanvas.Width - curImage.ActualWidth * tran.ScaleX) / 2);
            currentImage = img;
            curImage.TouchDown += HandleImageTouched;
            curImage.MouseDown += HandleImageTouched;

            curInfoCol.Width = _windowSize.Width / 4;
            curInfoCol.Height = _windowSize.Height / 3;

            infoScroll.Height = curInfoCol.Height;
            ColumnDefinition width = new ColumnDefinition();
            GridLength length = new GridLength(_windowSize.Width / 4 - 40);
            width.Width = length;
            curInfoCol.ColumnDefinitions.Add(width);

            title.Text = "";
            artist.Text = "";
            medium.Text = "";
            date.Text = "";
            title.Text += "Title: " + img.title;
            curInfoCol.UpdateLayout();
            titleBack.Width = _windowSize.Width / 4 - 20;
            titleBack.Height = title.ActualHeight + 5;
            artist.Text += "Artist: " + img.artist;
            medium.Text += "Medium: " + img.medium;
            date.Text += "Year: " + img.year;

            title.FontSize = 25 * _windowSize.Height / 1080.0;
            artist.FontSize = 20 * _windowSize.Height / 1080.0;
            medium.FontSize = artist.FontSize;
            date.FontSize = artist.FontSize;
            KeywordsTitle.FontSize = 18 * _windowSize.Height / 1080.0;
            curKeywords.FontSize = 18 * _windowSize.Height / 1080.0;
            curInfoCol.UpdateLayout();
            titleBack.Height = titleBack.ActualHeight + 10;

            if (currentImage.keywords.Count() > 0)
            {
                KeywordsTitle.Visibility = Visibility.Visible;
                curKeywords.Visibility = Visibility.Visible;
                KeywordBack.Visibility = Visibility.Visible;
            }
            else
            {
                KeywordsTitle.Visibility = Visibility.Hidden;
                curKeywords.Visibility = Visibility.Hidden;
                KeywordBack.Visibility = Visibility.Hidden;
            }

            KeywordBack.Width = _windowSize.Width / 4 - 20;
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
            curKeywords.UpdateLayout();
            curInfoCol.UpdateLayout();
            RowDefinition height = new RowDefinition();
            GridLength height1 = new GridLength(curKeywords.ActualHeight);
            height.Height = height1;
            curInfoCol.RowDefinitions.Add(height);
            KeywordBack.Height = KeywordsTitle.ActualHeight * 3 + curKeywords.ActualHeight;
            curInfoCol.UpdateLayout();
            infoScroll.UpdateLayout();

            curInfoCol.Height = titleBack.ActualHeight + artist.ActualHeight + date.ActualHeight + medium.ActualHeight + KeywordBack.ActualHeight;

            if (curInfoCol.Height > _windowSize.Height / 3 - 50)
            {
                curInfoContainer.Height = _windowSize.Height / 3;
                infoScroll.Height = _windowSize.Height / 3 - 50;
                title.MaxWidth = _windowSize.Width / 4 - 80;
                title.UpdateLayout();
                titleBack.Height = title.ActualHeight + 5;
                curKeywords.MaxWidth = _windowSize.Width / 4 - 80;
                curKeywords.UpdateLayout();
                KeywordBack.Height = KeywordsTitle.ActualHeight * 3 + curKeywords.ActualHeight;
                curInfoCol.Height = title.ActualHeight + 5 + artist.ActualHeight +
                    date.ActualHeight + medium.ActualHeight + KeywordsTitle.ActualHeight * 3 + curKeywords.ActualHeight;
            }
            else
            {
                curInfoContainer.Height = curInfoCol.Height + 50;
                infoScroll.Height = curInfoCol.Height + 50;
                title.MaxWidth = _windowSize.Width / 4 - 40;
                curKeywords.MaxWidth = _windowSize.Width / 4 - 40;
            }
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
