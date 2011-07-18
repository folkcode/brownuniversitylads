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
using System.Windows.Forms;
using System.Windows.Navigation;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Microsoft.Surface.Presentation.Controls.Primitives;
using System.Windows.Shapes;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using DeepZoom;
using DeepZoom.Controls;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for map.xaml
    /// </summary>
    public partial class map : System.Windows.Controls.UserControl
    {
        private int RadioColor; //Represents which radioButton is chosen
        private String currentMarker;
        private List<string> originX;
       // private List<string> originY;
       // private List<string> exhibitX;
       // private List<string> exhibitY;
        private List<string> currentX;
       // private List<string> currentY;
       // private List<SurfaceRadioButton> origin;
       // private List<SurfaceRadioButton> exhibit;
       // private List<SurfaceRadioButton> current;
        private List<SurfaceRadioButton> radioButtons;
        private Dictionary<SurfaceRadioButton, string> dic;
        private Dictionary<SurfaceRadioButton, Ellipse> dicRb;
        private AddNewImageControl big;
        private List<Ellipse> ellipses;
        private bool originExists, currLocationExists, canAdd;
        private Dictionary<SurfaceRadioButton, String> dateInfo;
        private Dictionary<SurfaceRadioButton, String> cityInfo;
        private Double mapWidth, mapHeight;
        private mapWindow newMapWindow;
        private SurfaceRadioButton buttonChecked;
        /// <summary>
        /// Default constructor
        /// </summary>
        public map()
        {
            InitializeComponent();
          //  map1.TouchDown += new EventHandler<TouchEventArgs>(MapTouchHandler);
            map1.MouseUp += new MouseButtonEventHandler(MapMouseHandler);
            RadioColor = 0; //Means not chosen yet
            originX = new List<string>();
           // originY = new List<string>();
           // exhibitX = new List<string>();
           // exhibitY = new List<string>();
            currentX = new List<string>();
           // currentY = new List<string>();
            radioButtons = new List<SurfaceRadioButton>();
            dic = new Dictionary<SurfaceRadioButton, string>();
            ellipses = new List<Ellipse>();
            dicRb = new Dictionary<SurfaceRadioButton, Ellipse>();
            dateInfo = new Dictionary<SurfaceRadioButton, String>() ;
            cityInfo = new Dictionary<SurfaceRadioButton, String>() ;
           // origin = new List<SurfaceRadioButton>();
          //  exhibit = new List<SurfaceRadioButton>();
           // current = new List<SurfaceRadioButton>();

            originExists = false;
            currLocationExists = false;
            canAdd = true;
            currentMarker = "";
            
            //this.showMap();
            //this.loadPositions();
           
        }
        public void findImageSize()
        {
            
            XmlDocument newDoc = new XmlDocument();
            String imageFolder = "Data/Map/newmap.jpg" + "/" + "dz.xml";
            newDoc.Load(imageFolder);
            if (newDoc.HasChildNodes)
            {
                foreach (XmlNode image in newDoc.ChildNodes)
                {
                    if (image.Name == "Image")
                    {
                        XmlNode size = image.FirstChild;
                        String width = size.Attributes.GetNamedItem("Width").InnerText;
                        String height = size.Attributes.GetNamedItem("Height").InnerText;
                        Double width1 = Convert.ToDouble(width);
                        Double height1 = Convert.ToDouble(height);
                        mapWidth = width1;
                        mapHeight = height1;

                    }
                }

            }
            
        }
        /// <summary>
        /// Load the map from file
        /// </summary>
        public void showMap()
        {
            //String mapPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\" + "fullmap.png";
            //BitmapImage myBitmapImage = new BitmapImage();
            //myBitmapImage.BeginInit();
            //myBitmapImage.UriSource = new Uri(mapPath);
            //myBitmapImage.EndInit();

            //set image source
            //map1.Source = myBitmapImage;
            
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\data\\";
            String mapUri = dataUri + "Map/newmap.jpg/dz.xml";
            map1.SetImageSource(mapUri);
            map1.UpdateLayout();
           // this.Visibility = Visibility.Visible;
            //MapCanvas.Children.Add(newImage);
            // newImage.Source = new MultiScaleTileSource();
            //mapImage.Loaded +=new RoutedEventHandler(mapImage_Loaded);

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
            //dpd.RemoveValueChanged(mapImage.GetZoomableCanvas, LocationChanged);

            ZoomableCanvas msi = map1.GetZoomableCanvas;
            dpd.AddValueChanged(msi, LocationChanged);
            //System.Windows.Forms.MessageBox.Show("scale" + map1.GetZoomableCanvas.Scale);
        }

        
         public void LocationChanged(Object sender, EventArgs e)
        {
           foreach (SurfaceRadioButton rb in radioButtons)
            {
                Ellipse newEllipse = dicRb[rb];
                rb.Visibility = Visibility.Visible;
                newEllipse.Visibility = Visibility.Visible;
                String str = dic[rb];
                String[] locInfo = Regex.Split(str, ",");
                double lon = Convert.ToDouble(locInfo[1]);
                double lat = Convert.ToDouble(locInfo[2]);

                Double mapcurWidth = mapWidth * map1.GetZoomableCanvas.Scale; //the size of the zoomed map
                Double mapcurHeight = mapHeight * map1.GetZoomableCanvas.Scale;

                //Console.Out.WriteLine("curWidth" + mapcurWidth);
                //Console.Out.WriteLine("curHeight" + mapcurHeight);
                Double screenPosX = (lon * mapcurWidth ) - map1.GetZoomableCanvas.Offset.X;
                //Console.Out.WriteLine("offsest" + map1.GetZoomableCanvas.Offset.X);
                Double screenPosY = (lat * mapcurHeight) - map1.GetZoomableCanvas.Offset.Y;


                Canvas.SetLeft(newEllipse, screenPosX - 20.8);
                Canvas.SetTop(newEllipse, screenPosY - 5);

                Canvas.SetLeft(rb, screenPosX - 22.8);
                Canvas.SetTop(rb, screenPosY - 13);

                //Console.Out.WriteLine(screenPosX);
                if (screenPosX < 0 || screenPosX > mapCover.Width)
                {
                    rb.Visibility = Visibility.Collapsed;
                    newEllipse.Visibility = Visibility.Collapsed;
                }
                if (screenPosY < 0 || screenPosY > mapCover.Height)
                {
                    rb.Visibility = Visibility.Collapsed;
                    newEllipse.Visibility = Visibility.Collapsed;
                }
            }
           
             
        }

        /**
         public void setLocationChange()
         {

             DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
             //dpd.RemoveValueChanged(mapImage.GetZoomableCanvas, LocationChanged);

             ZoomableCanvas msi = map1.GetZoomableCanvas;
             dpd.AddValueChanged(msi, LocationChanged);
         }
        */

        /// <summary>
        /// Handle a touch on the map: get the position and process it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapTouchHandler(object sender, TouchEventArgs e)
        {
            Point p = e.TouchDevice.GetCenterPosition(this);
            this.CreateNewPoints(p);

            Point newPoint = e.TouchDevice.GetCenterPosition(map1);
            foreach (UIElement ele in mapCover.Children)
            {
                SurfaceRadioButton t = ele as SurfaceRadioButton;

                if (t != null)
                {
                    Double canvasLeft = Canvas.GetLeft(t);
                    Double canvasTop = Canvas.GetTop(t);
                    map1.UpdateLayout();
                    Double width = t.ActualWidth - 20;
                    Double height = t.ActualHeight - 20;
                    // Console.Out.WriteLine("Actual width" + width);

                    if (newPoint.X > canvasLeft && newPoint.X < canvasLeft + width && newPoint.Y > canvasTop && newPoint.Y < canvasTop + height)
                    {
                        // Console.Out.WriteLine("buttonselected");
                        newMarker_Click(t);
                        // Console.Out.WriteLine("buttonSelected");
                    }

                }
            }
        }

        /// <summary>
        /// Handle a click on the map: get the position and process it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapMouseHandler(object sender, MouseButtonEventArgs e)
        {
            Point p = e.MouseDevice.GetCenterPosition(this);
            this.CreateNewPoints(p);

            Point newPoint = e.MouseDevice.GetCenterPosition(map1);
            foreach (UIElement ele in mapCover.Children)
            {
                SurfaceRadioButton t = ele as SurfaceRadioButton;

                if (t != null)
                {
                    Double canvasLeft = Canvas.GetLeft(t);
                    Double canvasTop = Canvas.GetTop(t);
                    map1.UpdateLayout();
                    Double width = t.ActualWidth - 20;
                    Double height = t.ActualHeight - 20;
                   // Console.Out.WriteLine("Actual width" + width);

                    if (newPoint.X > canvasLeft && newPoint.X < canvasLeft + width && newPoint.Y > canvasTop && newPoint.Y < canvasTop + height)
                    {
                       // Console.Out.WriteLine("buttonselected");
                        newMarker_Click(t);
                        // Console.Out.WriteLine("buttonSelected");
                    }

                }
            }
        }
       

        /// <summary>
        /// Create new hotspots on the map
        /// </summary>
        /// <param name="newPoint"></param>
        public void CreateNewPoints(Point newPoint)
        {
            if (currLocationExists && currentMarker == "current")
            {
                canAdd = false;
            }
            else if (originExists && currentMarker == "origin")
            {
                canAdd = false;
            }
            else
            {
                canAdd = true;
            }
            if (buttonChecked != null)
            {
                if (canAdd)
                {
                    if (RadioColor != 0)
                    {   //get the point clicked and calculate the longitute and latitude
                        LengthConverter myLengthConverter = new LengthConverter();
                        Double db1 = newPoint.X - 27;
                        Double db2 = newPoint.Y - 87;
                        //System.Windows.Forms.MessageBox.Show("coreate new circles");

                        //These operations are used to calculate the exact longitude and latitude before saving into the xml file.
                        this.findImageSize();
                        //Console.Out.WriteLine("mapWidht" + mapWidth);
                        Double mapcurWidth = mapWidth * map1.GetZoomableCanvas.Scale;
                        Double mapcurHeight = mapHeight * map1.GetZoomableCanvas.Scale;

                        //Console.Out.WriteLine("mapCurWidth"+mapcurWidth);

                        Point newP = map1.GetZoomableCanvas.Offset;
                        //Console.Out.WriteLine("point" + newP);
                        Double canvasLeft = (db1 + newP.X) / mapcurWidth;
                        Double canvasTop = (db2 + newP.Y) / mapcurHeight;
                        // Console.Out.WriteLine(canvasLeft);
                        //Double longitude = canvasLeft / (map1.Width * map1.GetZoomableCanvas.Scale);
                        // Double latitude = canvasTop / (map1.Height * map1.GetZoomableCanvas.Scale);
                        String lon = canvasLeft.ToString();
                        String lat = canvasTop.ToString();
                        //  Console.Out.WriteLine("longitude" + lon);
                        //  Console.Out.WriteLine("latitude" + lat);

                        Ellipse newEllipse = new Ellipse();
                        ellipses.Add(newEllipse);

                        SurfaceRadioButton newMarker = new SurfaceRadioButton();
                        //newMarker.Checked += new RoutedEventHandler(newButton_Checked);


                        SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                        if (currentMarker == "current")//yellow
                        {
                            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                            currentX.Add(lon);
                            //currentY.Add(lat);
                            dic.Add(newMarker, "yellow" + "," + lon + "," + lat);
                            currLocationExists = true;
                        }
                        else if (currentMarker == "exhibit")//blue
                        {
                            mySolidColorBrush.Color = Color.FromArgb(255, 0, 0, 255);
                            //exhibitX.Add(lon);
                            // exhibitY.Add(lat);
                            dic.Add(newMarker, "blue" + "," + lon + "," + lat);
                        }
                        else if (currentMarker == "origin")//red
                        {
                            mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
                            originX.Add(lon);
                            // originY.Add(lat);
                            dic.Add(newMarker, "red" + "," + lon + "," + lat);
                            originExists = true;
                            canAdd = true;
                            currentMarker = null;
                        }


                        mapCover.Children.Add(newEllipse);
                        mapCover.Children.Add(newMarker);

                        newEllipse.Width = 13.5;
                        newEllipse.Height = 13.5;
                        newEllipse.Fill = mySolidColorBrush;
                        foreach (SurfaceRadioButton rb in radioButtons)
                        {
                            rb.IsChecked = false;
                        }
                      //  newMarker.Checked += new RoutedEventHandler(newMarker_Click);
                        newMarker.IsChecked = true;

                        radioButtons.Add(newMarker);
                        dicRb.Add(newMarker, newEllipse);



                        //Set the location of the circle on the map 
                        //Canvas.SetLeft(newEllipse, db1 - 6);
                        // Canvas.SetTop(newEllipse, db2 - 6);

                        // Canvas.SetLeft(newMarker, db1 - 9);
                        //Canvas.SetTop(newMarker, db2 - 13.5);

                        Double screenPosX = canvasLeft - map1.GetZoomableCanvas.Offset.X; //need to reset the location thing
                        Double screenPosY = (map1.GetZoomableCanvas.Scale / (1 / 15) * db2) - map1.GetZoomableCanvas.Offset.Y;
                        //double screenPosX = canvasLeft;
                        //double screenPosY = canvasTop;
                        Canvas.SetLeft(newEllipse, db1 - 20.8);
                        Canvas.SetTop(newEllipse, db2 - 5);

                        Canvas.SetLeft(newMarker, db1 - 22.8);
                        Canvas.SetTop(newMarker, db2 - 13);
                        //System.Windows.Forms.MessageBox.Show("scale" + map1.GetZoomableCanvas.Scale);
                        buttonChecked.IsChecked = false; //only enable to create one circle at a time
                        RadioColor = 0;
                        currentMarker = null;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("There may only be one location of origin and one current location.");
                    if (buttonChecked != null)
                    {
                        buttonChecked.IsChecked = false;
                        buttonChecked = null;
                    }

                    buttonChecked = null;
                    currentMarker = null;
                    return;
                }
            }

        }
        public void newMarker_Click(SurfaceRadioButton sender)
        {
            sender.IsChecked = true;
            date.IsReadOnly = false;
            city.IsReadOnly = false;
            dateTo.IsReadOnly = false;
            //date.Text = "";
            //dateTo.Text = "";
            //Make sure that if it's the origin location, no date could be entered
            String str = dic[(SurfaceRadioButton)sender];
            String[] strs = Regex.Split(str, ",");
            //Console.Out.WriteLine(strs[0]);
            if (strs[0] == "red")
            {
                date.IsReadOnly = true;
                dateTo.IsReadOnly = true;
            }

            if (dateInfo.ContainsKey((SurfaceRadioButton)sender))
            {

                if (dateInfo[(SurfaceRadioButton)sender].Contains("/"))
                {

                    String[] strings = Regex.Split(dateInfo[(SurfaceRadioButton)sender], "/");
                    //Console.Out.WriteLine("string length"+strings.Length);
                    if (strings[0] == "null")
                    {
                        date.Text = "";
                        dateTo.Text = strings[1];
                    }
                    else if (strings[1] == "null")
                    {
                        date.Text = strings[0];
                        dateTo.Text = "";
                    }

                    else if (strings[0] == "null" && strings[1] == "null")
                    {
                        date.Text = "";
                        dateTo.Text = "";
                    }
                    else
                    {
                        date.Text = strings[0];
                        dateTo.Text = strings[1];
                    }
                }
                else
                {
                    date.Text = "";
                    dateTo.Text = "";
                }
            }
            else
            {
                date.Text = "";
                dateTo.Text = "";
            }

            if (cityInfo.ContainsKey((SurfaceRadioButton)sender))
            {
                city.Text = cityInfo[(SurfaceRadioButton)sender];
            }
            else
            {
                city.Text = "";
            }
        }

        /// <summary>
        /// Remove a single hotspot from the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void newButton_Click(object sender, RoutedEventArgs e)
        {
            mapCover.Children.Remove((SurfaceRadioButton)sender);
            mapCover.Children.Remove(dicRb[(SurfaceRadioButton)sender]);

            radioButtons.Remove((SurfaceRadioButton)sender);
            //need to find out which kind of location the spot is and remove from the list before saving
            String str = dic[(SurfaceRadioButton)sender];
            String[] strings = Regex.Split(str, ",");
            if (strings[0] == "red")
            {

                originX.Remove(strings[1]);
                originY.Remove(strings[2]);

            }
            else if (strings[0] == "blue")
            {

                exhibitX.Remove(strings[1]);
                exhibitY.Remove(strings[2]);

            }
            else if (strings[0] == "yellow")
            {
                // Console.Out.WriteLine("called yellow");
                currentX.Remove(strings[1]);
                currentY.Remove(strings[2]);
            }

        }*/

        /// <summary>
        /// Save the hostpots to XML file and close the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_close_click(object sender, RoutedEventArgs e)
        {
            this.save();
            newMapWindow.Visibility = Visibility.Collapsed;
           // newMapWindow.mapControl.Visibility = Visibility.Hidden;
        }
        public void setParentWindow(mapWindow parent)
        {
            newMapWindow = parent;
        }
       

        /// <summary>
        /// Load hotspots from XML file
        /// </summary>
        public void loadPositions()
        {
            String dataDir = "Data/";
            XmlDocument doc = new XmlDocument();
            doc.Load(dataDir + "NewCollection.xml");
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {

                        foreach (XmlNode node in docNode.ChildNodes)
                        {
                            if (node.Name == "Image")
                            {
                                String path = node.Attributes.GetNamedItem("path").InnerText;
                                if (big.getImageName() == path)
                                {

                                    foreach (XmlNode imgnode in node.ChildNodes)
                                    {
                                        if (imgnode.Name == "Locations")
                                        {
                                            foreach (XmlNode location in imgnode.ChildNodes)
                                            {
                                                
                                                if (location.Name == "Work")
                                                {
                                                    String lon = location.Attributes.GetNamedItem("longitude").InnerText;
                                                    String lat = location.Attributes.GetNamedItem("latitude").InnerText;
                                                    String dateInfo = "";
                                                    String cityInfo = "";
                                                    if (location.Attributes.GetNamedItem("date") != null)
                                                    {
                                                        dateInfo = location.Attributes.GetNamedItem("date").InnerText;
                                                    }
                                                    if (location.Attributes.GetNamedItem("city") != null)
                                                    {
                                                        cityInfo = location.Attributes.GetNamedItem("city").InnerText;
                                                    }
                                                    
                                                    this.createCircles(lon, lat, "current",dateInfo,cityInfo);
                                                    currLocationExists = true;
                                                }
                                                if (location.Name == "Display")
                                                {
                                                    foreach (XmlNode display in location.ChildNodes)
                                                    {
                                                        String lon = display.Attributes.GetNamedItem("longitude").InnerText;
                                                        String lat = display.Attributes.GetNamedItem("latitude").InnerText;
                                                        String dateInfo = "";
                                                        String cityInfo = "";
                                                        if (display.Attributes.GetNamedItem("date") != null)
                                                        {
                                                            dateInfo = display.Attributes.GetNamedItem("date").InnerText;
                                                        }
                                                        if (display.Attributes.GetNamedItem("city") != null)
                                                        {
                                                            cityInfo = display.Attributes.GetNamedItem("city").InnerText;
                                                        }
                                                        this.createCircles(lon, lat, "exhibit",dateInfo,cityInfo);
                                                    }

                                                }
                                                if (location.Name == "Purchase")
                                                {
                                                    String lon = location.Attributes.GetNamedItem("longitude").InnerText;
                                                    String lat = location.Attributes.GetNamedItem("latitude").InnerText;
                                                    String dateInfo = "";
                                                    String cityInfo = "";
                                                    if (location.Attributes.GetNamedItem("date") != null)
                                                    {
                                                        dateInfo = location.Attributes.GetNamedItem("date").InnerText;
                                                    }
                                                    if (location.Attributes.GetNamedItem("city") != null)
                                                    {
                                                        cityInfo = location.Attributes.GetNamedItem("city").InnerText;
                                                    }

                                                    this.createCircles(lon, lat, "origin",dateInfo,cityInfo);
                                                    originExists = true;
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                        }


                    }
                }
            }
        }
    

        

        /// <summary>
        /// Create hotspot icons in correct positions and colors
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="color"></param>
        private void createCircles(String lon, String lat, string marker, string date, string city)
        {
            
            double db1 = Convert.ToDouble(lon);
            double db2 = Convert.ToDouble(lat);
            //Console.Out.WriteLine(db1);

            Ellipse newEllipse = new Ellipse();
            ellipses.Add(newEllipse);

            SurfaceRadioButton newMarker = new SurfaceRadioButton();
            //newMarker.Checked += new RoutedEventHandler(newButton_Checked);
            newMarker.IsChecked = true;
            

            foreach (SurfaceRadioButton rb in radioButtons)
            {
                rb.IsChecked = false;
            }

            dicRb.Add(newMarker, newEllipse);

            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            dateInfo.Add(newMarker, date);
            cityInfo.Add(newMarker, city);
            if (marker == "current")//yellow
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                currentX.Add(lon);
             //   currentY.Add(lat);
                dic.Add(newMarker, "yellow" + "," + lon + "," + lat);

            }
            else if (marker == "exhibit")//blue
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 0, 0, 255);
            //    exhibitX.Add(lon);
           //     exhibitY.Add(lat);
                dic.Add(newMarker, "blue" + "," + lon + "," + lat);

            }
            else if (marker == "origin")//red
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
                originX.Add(lon);
             //   originY.Add(lat);
                dic.Add(newMarker, "red" + "," + lon + "," + lat);
            }
            //newMarker.Checked +=new RoutedEventHandler(newMarker_Click);
            radioButtons.Add(newMarker);
            newEllipse.Width = 13.5;
            newEllipse.Height = 13.5;
            newEllipse.Fill = mySolidColorBrush;

            newMarker.Height = 1;
            newMarker.Width = 1;

            mapCover.Children.Add(newEllipse);
            mapCover.Children.Add(newMarker);

            //System.Windows.Forms.MessageBox.Show("locations loaded!");
            //Set the location of the circle on the map    
            Point newP = map1.GetZoomableCanvas.Offset;
            this.findImageSize();
            Double mapcurWidth = mapWidth * map1.GetZoomableCanvas.Scale; //the size of the zoomed map
            Double mapcurHeight = mapHeight * map1.GetZoomableCanvas.Scale;


            Double screenPosX = (db1 * mapcurWidth) - map1.GetZoomableCanvas.Offset.X;
            //Console.Out.WriteLine("offsest" + map1.GetZoomableCanvas.Offset.X);
            Double screenPosY = (db2 * mapcurHeight) - map1.GetZoomableCanvas.Offset.Y;

            Canvas.SetLeft(newEllipse, screenPosX - 20.8);
            Canvas.SetTop(newEllipse, screenPosY - 5);

            Canvas.SetLeft(newMarker, screenPosX - 22.8);
            Canvas.SetTop(newMarker, screenPosY - 13);
            this.newMarker_Click(newMarker);
        }


        /// <summary>
        /// ` hotspots to XML file
        /// </summary>
        private void save()
        {
            //There can be only one point of origin and current location
            if (currentX.Count > 1 || originX.Count > 1)
            {
                System.Windows.Forms.MessageBox.Show("There can be only one point of origin and current location");
                
                return;
            }
            else
            {
                String dataDir = "Data/";
                XmlDocument doc = new XmlDocument();
                doc.Load(dataDir + "NewCollection.xml");
                if (doc.HasChildNodes)
                {

                    foreach (XmlNode docNode in doc.ChildNodes)
                    {
                        if (docNode.Name == "Collection")
                        {

                            foreach (XmlNode node in docNode.ChildNodes)
                            {
                                if (node.Name == "Image")
                                {
                                    String path = node.Attributes.GetNamedItem("path").InnerText;
                                    if (big.getImageName() == path)
                                    {

                                        foreach (XmlNode imgnode in node.ChildNodes)
                                        {

                                            if (imgnode.Name == "Locations")
                                            {
                                                node.RemoveChild(imgnode);
                                            }
                                            
                                        }
                                        if (radioButtons.Count != 0)
                                        {
                                            //Boolean exhibitExists = false;
                                            XmlElement newLocation = doc.CreateElement("Locations");
                                            XmlElement newDisplay = doc.CreateElement("Display");
                                            foreach (SurfaceRadioButton rb in radioButtons)
                                            {
                                                String str = dic[rb];
                                                String[] strs = Regex.Split(str, ",");
                                                String lon = strs[1];
                                                String lat = strs[2];
                                                String timeInfo = "";
                                                String placeInfo = "";

                                                if (dateInfo.ContainsKey(rb))
                                                {
                                                    timeInfo = dateInfo[rb];
                                                }
                                                if (cityInfo.ContainsKey(rb))
                                                {
                                                    placeInfo = cityInfo[rb];
                                                }

                                                if (strs[0] == "yellow")
                                                {
                                                    XmlElement newPurchase = doc.CreateElement("Work");
                                                    newPurchase.SetAttribute("longitude", lon);
                                                    newPurchase.SetAttribute("latitude", lat);
                                                    newLocation.AppendChild(newPurchase);
                                                    newPurchase.SetAttribute("date", timeInfo);
                                                    newPurchase.SetAttribute("city", placeInfo);

                                                }
                                                else if (strs[0] == "red")
                                                {
                                                    XmlElement newOrigin = doc.CreateElement("Purchase");
                                                    newOrigin.SetAttribute("longitude", lon);
                                                    newOrigin.SetAttribute("latitude", lat);
                                                    newLocation.AppendChild(newOrigin);
                                                    newOrigin.SetAttribute("date", timeInfo);
                                                    newOrigin.SetAttribute("city", placeInfo);
                                                }
                                                else
                                                {

                                                    XmlElement location = doc.CreateElement("Location");
                                                    location.SetAttribute("longitude", lon);
                                                    location.SetAttribute("latitude", lat);
                                                    location.SetAttribute("date", timeInfo);
                                                    location.SetAttribute("city", placeInfo);
                                                    newDisplay.AppendChild(location);
                                                    newLocation.AppendChild(newDisplay);

                                                }
                                            }
                                            node.AppendChild(newLocation);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                doc.Save(dataDir + "NewCollection.xml");
                this.cancel();
                this.Hide();
            }
        }
                            
             


        public void setBigWindow(AddNewImageControl window)
        {
            big = window;

        }

        public void Show()
        {
            Visibility = Visibility.Visible;
            //this.showMap();
           
        }
        public void Hide()
        {
            Visibility = Visibility.Hidden;
        }

        private void SurfaceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioColor = 1;
            currentMarker = "origin";
            buttonChecked = (SurfaceRadioButton)sender;

        }

        private void SurfaceRadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            RadioColor = 2;
            currentMarker = "exhibit";
            buttonChecked = (SurfaceRadioButton)sender;
        }

        private void SurfaceRadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            RadioColor = 3;
            currentMarker = "current";
            buttonChecked = (SurfaceRadioButton)sender;
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.cancel();
            this.loadPositions();
        }

        /// <summary>
        /// Remove all hotspots created in this session
        /// </summary>
        private void cancel()
        {

            //clear all the newly-added points
            foreach (SurfaceRadioButton toRemove in radioButtons)
            {
                mapCover.Children.Remove(toRemove);
                mapCover.Children.Remove(dicRb[toRemove]);

            }

            //Clear the stored points
            radioButtons = new List<SurfaceRadioButton>();
            ellipses = new List<Ellipse>();
            originX = new List<string>();
            //originY = new List<string>();
            //exhibitX = new List<string>();
           // exhibitY = new List<string>();
            currentX = new List<string>();
           // currentY = new List<string>();
            dic = new Dictionary<SurfaceRadioButton, String>();
            dicRb = new Dictionary<SurfaceRadioButton, Ellipse>();
            dateInfo = new Dictionary<SurfaceRadioButton, String>();
            cityInfo = new Dictionary<SurfaceRadioButton, String>();
            date.IsReadOnly = true;
            city.IsReadOnly = true;
            originExists = false;
            currLocationExists = false;
            date.Text = "";
            city.Text = "";

        }
        /// <summary>
        /// Remove a hotspot from the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveOne_Click(object sender, RoutedEventArgs e)
        {

            Boolean isChecked = false;
            SurfaceRadioButton checkedButton = new SurfaceRadioButton();
            foreach (SurfaceRadioButton rb in radioButtons)
            {
                if (rb.IsChecked == true)
                {
                    isChecked = true;
                    checkedButton = rb;
                }

            }
            if (isChecked)
            {
                mapCover.Children.Remove(checkedButton);
                mapCover.Children.Remove(dicRb[checkedButton]);
                radioButtons.Remove(checkedButton);
                if (cityInfo.ContainsKey(checkedButton))
                {
                    cityInfo.Remove(checkedButton);
                }
                if (dateInfo.ContainsKey(checkedButton))
                {
                    dateInfo.Remove(checkedButton);
                }
                //need to find out which kind of location the spot is and remove from the list before saving
                String str = dic[checkedButton];
                String[] strings = Regex.Split(str, ",");
                if (strings[0] == "red")
                {

                    originX.Remove(strings[1]);
                    //originY.Remove(strings[2]);
                    originExists = false;

                }
                else if (strings[0] == "blue")
                {
                   // exhibitX.Remove(strings[1]);
                   // exhibitY.Remove(strings[2]);

                }
                else if (strings[0] == "yellow")
                {
                    // Console.Out.WriteLine("called yellow");
                    currentX.Remove(strings[1]);
                    //currentY.Remove(strings[2]);
                    currLocationExists = false;
                }
                date.IsReadOnly = true;
                city.IsReadOnly = true;

            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please select the location you want to remove!");
            }
        }

        private void date_TextChanged(object sender, TextChangedEventArgs e)
        {
            Boolean isChecked = false;
            SurfaceRadioButton checkedButton = new SurfaceRadioButton();
            foreach (SurfaceRadioButton rb in radioButtons)
            {
                if (rb.IsChecked == true)
                {
                    isChecked = true;
                    checkedButton = rb;
                }

            }
            if (isChecked)
            {

                if (dateInfo.ContainsKey(checkedButton))
                {
                    if (dateTo.Text != "")
                    {
                        if (date.Text != "")
                        {
                            dateInfo[checkedButton] = date.Text + "/" + dateTo.Text;
                        }
                        else
                        {
                            dateInfo[checkedButton] = "null/" + dateTo.Text;
                        }
                    }
                    else
                    {
                        if (date.Text != null)
                        {
                            dateInfo[checkedButton] = date.Text + "/null";
                        }
                        else
                        {
                            dateInfo[checkedButton] = "";
                        }
                    }
                }
                else
                {
                    if (date.Text != "" || dateTo.Text!="")
                    {
                        if(dateTo.Text =="")
                        {
                            
                            String newStr = date.Text+ "/null";
                            dateInfo.Add(checkedButton, newStr);
                        }
                        else
                        {
                            String newStr = date.Text + dateTo.Text;
                            dateInfo.Add(checkedButton,newStr);
                        }
                    }
                }
            }
            
        }

        private void city_TextChanged(object sender, TextChangedEventArgs e)
        {
            Boolean isChecked = false;
            SurfaceRadioButton checkedButton = new SurfaceRadioButton();
            foreach (SurfaceRadioButton rb in radioButtons)
            {
                if (rb.IsChecked == true)
                {
                    isChecked = true;
                    checkedButton = rb;
                }

            }
            if (isChecked)
            {
                
                if (cityInfo.ContainsKey(checkedButton))
                {
                    cityInfo[checkedButton] = city.Text;
                }
                else
                {
                    if (city.Text != "")
                    {
                        String newStr = city.Text;
                        cityInfo.Add(checkedButton, newStr);
                    }
                }
            }
        }

        private void dateTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            Boolean isChecked = false;
            SurfaceRadioButton checkedButton = new SurfaceRadioButton();
            foreach (SurfaceRadioButton rb in radioButtons)
            {
                if (rb.IsChecked == true)
                {
                    isChecked = true;
                    checkedButton = rb;
                }

            }
            if (isChecked)
            {

                if (dateInfo.ContainsKey(checkedButton))
                {
                    if (date.Text != "")
                    {
                        if (dateTo.Text != "")
                        {
                            dateInfo[checkedButton] = date.Text + "/" + dateTo.Text;
                        }
                        else
                        {
                            dateInfo[checkedButton] = date.Text;
                        }
                    }
                    else
                    {
                        if (dateTo.Text != "")
                        {
                            dateInfo[checkedButton] = "null/" + dateTo.Text;
                        }
                        else
                        {
                            dateInfo[checkedButton] = "";
                        }
                    }
                }
                else
                {
                    if (dateTo.Text != "" || date.Text!="")
                    {
                        if (date.Text != "")
                        {
                            String newStr = date.Text + "/" + dateTo.Text;
                            dateInfo.Add(checkedButton, newStr);
                        }
                        else
                        {
                            String newStr = "null/" + dateTo.Text;
                            dateInfo.Add(checkedButton, newStr);
                        }
                    }
                }
            }
        }

    }
}
