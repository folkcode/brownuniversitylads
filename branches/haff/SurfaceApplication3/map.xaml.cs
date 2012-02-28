using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Shapes;
using System.Xml;
using System.Text.RegularExpressions;
using System.ComponentModel;

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
        private List<string> currentX;
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
            map1.MouseUp += new MouseButtonEventHandler(MapMouseHandler);
            RadioColor = 0; //Means not chosen yet
            originX = new List<string>();
            currentX = new List<string>();
            radioButtons = new List<SurfaceRadioButton>();
            dic = new Dictionary<SurfaceRadioButton, string>();
            ellipses = new List<Ellipse>();
            dicRb = new Dictionary<SurfaceRadioButton, Ellipse>();
            dateInfo = new Dictionary<SurfaceRadioButton, String>() ;
            cityInfo = new Dictionary<SurfaceRadioButton, String>() ;

            originExists = false;
            currLocationExists = false;
            canAdd = true;
            currentMarker = "";

            //set the default map hotspot to "Exhibition locations"
            RadioColor = 2;
            currentMarker = "exhibit";
            buttonChecked = blue;//blue is a button
        }
        public void findImageSize()
        {
            
            XmlDocument newDoc = new XmlDocument();
            String imageFolder = "Data/Map/Map.png" + "/" + "dz.xml";
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
            
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\data\\";
            String mapUri = dataUri + "Map/Map.png/dz.xml";
            map1.SetImageSource(mapUri);
            map1.UpdateLayout();
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));

            ZoomableCanvas msi = map1.GetZoomableCanvas;
            dpd.AddValueChanged(msi, LocationChanged);
        }

        //This changed the mapButtons location when the map is zoomed in or out
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
                Double screenPosX = (lon * mapcurWidth ) - map1.GetZoomableCanvas.Offset.X;
                Double screenPosY = (lat * mapcurHeight) - map1.GetZoomableCanvas.Offset.Y;


                Canvas.SetLeft(newEllipse, screenPosX - 20.8);
                Canvas.SetTop(newEllipse, screenPosY - 5);

                Canvas.SetLeft(rb, screenPosX - 22.8);
                Canvas.SetTop(rb, screenPosY - 13);

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

                    if (newPoint.X > canvasLeft && newPoint.X < canvasLeft + width && newPoint.Y > canvasTop && newPoint.Y < canvasTop + height)
                    {
                        newMarker_Click(t);
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

                    if (newPoint.X > canvasLeft && newPoint.X < canvasLeft + width && newPoint.Y > canvasTop && newPoint.Y < canvasTop + height)
                    {
                        newMarker_Click(t);
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

                        //These operations are used to calculate the exact longitude and latitude before saving into the xml file.
                        this.findImageSize();
                        Double mapcurWidth = mapWidth * map1.GetZoomableCanvas.Scale;
                        Double mapcurHeight = mapHeight * map1.GetZoomableCanvas.Scale;

                        Point newP = map1.GetZoomableCanvas.Offset;
                        Double canvasLeft = (db1 + newP.X) / mapcurWidth;
                        Double canvasTop = (db2 + newP.Y) / mapcurHeight;
                        String lon = canvasLeft.ToString();
                        String lat = canvasTop.ToString();

                        Ellipse newEllipse = new Ellipse();
                        ellipses.Add(newEllipse);

                        SurfaceRadioButton newMarker = new SurfaceRadioButton();


                        SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                        if (currentMarker == "current")//yellow
                        {
                            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                            currentX.Add(lon);
                            dic.Add(newMarker, "yellow" + "," + lon + "," + lat);
                            currLocationExists = true;
                        }
                        else if (currentMarker == "exhibit")//blue
                        {
                            mySolidColorBrush.Color = Color.FromArgb(255, 0, 0, 255);
                            dic.Add(newMarker, "blue" + "," + lon + "," + lat);
                        }
                        else if (currentMarker == "origin")//red
                        {
                            mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
                            originX.Add(lon);
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
                        newMarker.IsChecked = true;

                        radioButtons.Add(newMarker);
                        dicRb.Add(newMarker, newEllipse);

                        Double screenPosX = canvasLeft - map1.GetZoomableCanvas.Offset.X; //need to reset the location thing
                        Double screenPosY = (map1.GetZoomableCanvas.Scale / (1 / 15) * db2) - map1.GetZoomableCanvas.Offset.Y;
                        Canvas.SetLeft(newEllipse, db1 - 20.8);
                        Canvas.SetTop(newEllipse, db2 - 5);

                        Canvas.SetLeft(newMarker, db1 - 22.8);
                        Canvas.SetTop(newMarker, db2 - 13);
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
            //Make sure that if it's the origin location, no date could be entered
            String str = dic[(SurfaceRadioButton)sender];
            String[] strs = Regex.Split(str, ",");
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
        /// Save the hostpots to XML file and close the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_close_click(object sender, RoutedEventArgs e)
        {
            this.save();
            
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

            Ellipse newEllipse = new Ellipse();
            ellipses.Add(newEllipse);

            SurfaceRadioButton newMarker = new SurfaceRadioButton();
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
                dic.Add(newMarker, "yellow" + "," + lon + "," + lat);

            }
            else if (marker == "exhibit")//blue
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 0, 0, 255);
                dic.Add(newMarker, "blue" + "," + lon + "," + lat);

            }
            else if (marker == "origin")//red
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
                originX.Add(lon);
                dic.Add(newMarker, "red" + "," + lon + "," + lat);
            }
            radioButtons.Add(newMarker);
            newEllipse.Width = 13.5;
            newEllipse.Height = 13.5;
            newEllipse.Fill = mySolidColorBrush;

            newMarker.Height = 1;
            newMarker.Width = 1;

            mapCover.Children.Add(newEllipse);
            mapCover.Children.Add(newMarker);

            //Set the location of the circle on the map    
            Point newP = map1.GetZoomableCanvas.Offset;
            this.findImageSize();
            Double mapcurWidth = mapWidth * map1.GetZoomableCanvas.Scale; //the size of the zoomed map
            Double mapcurHeight = mapHeight * map1.GetZoomableCanvas.Scale;


            Double screenPosX = (db1 * mapcurWidth) - map1.GetZoomableCanvas.Offset.X;
            Double screenPosY = (db2 * mapcurHeight) - map1.GetZoomableCanvas.Offset.Y;

            Canvas.SetLeft(newEllipse, screenPosX - 20.8);
            Canvas.SetTop(newEllipse, screenPosY - 5);

            Canvas.SetLeft(newMarker, screenPosX - 22.8);
            Canvas.SetTop(newMarker, screenPosY - 13);
            this.newMarker_Click(newMarker);
        }
        public Boolean checkDateValid()
        {
            bool isValid = true;
            foreach (SurfaceRadioButton rb in radioButtons)
            {
                if (!dateInfo.ContainsKey(rb)) continue;
                String dateInfos = dateInfo[rb];
                if (dateInfos != "")
                {
                    String[] strs = Regex.Split(dateInfos, "/");
                    int toYear;
                    int fromYear;
                    if (strs[0] != "null" && strs[1] != "null")
                    {

                        try
                        {
                            fromYear = int.Parse(strs[0]);
                        }
                        catch (Exception excep)
                        {
                            System.Windows.Forms.MessageBox.Show("Please input a valid 'From' year!");
                            isValid = false;
                            break;
                        }

                        try
                        {
                            toYear = int.Parse(strs[1]);
                        }
                        catch (Exception excep)
                        {
                            System.Windows.Forms.MessageBox.Show("Please input a valid 'To' year!");
                            isValid = false;
                            break;
                        }
                        if (fromYear < -9999 || fromYear > 9999 || toYear < -9999 || toYear > 9999)
                        {
                            System.Windows.Forms.MessageBox.Show("Please input valid numbers");
                            isValid = false;
                            break;
                        }
                        if (fromYear > toYear)
                        {
                            System.Windows.Forms.MessageBox.Show("'To' year must be after 'From' year");
                            isValid = false;
                        }

                    }
                    else if (strs[0] == "null")
                    {
                        try
                        {
                            toYear = int.Parse(strs[1]);
                        }
                        catch (Exception excep)
                        {
                            System.Windows.Forms.MessageBox.Show("Please input the valid 'To' year!");
                            isValid = false;
                            break;
                        }
                    }
                    else if (strs[1] == "null")
                    {
                        try
                        {
                            fromYear = int.Parse(strs[0]);
                        }
                        catch (Exception excep)
                        {
                            System.Windows.Forms.MessageBox.Show("Please input the valid 'From' year!");
                            isValid = false;
                            break;
                        }
                    }
                }
            }
            return isValid;
        }

        /// <summary>
        /// ` hotspots to XML file
        /// </summary>
        private void save()
        {
            
            //There can be only one point of origin and current location
            if (currentX.Count > 1 || originX.Count > 1)
            {
                System.Windows.Forms.MessageBox.Show("There can be only one point of origin and one current location");
                
                return;
            }
            else if (!checkDateValid())
            {
                
                //System.Windows.Forms.MessageBox.Show("To year must be after From year");
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
                newMapWindow.Close();
            }
        }
                            
             


        public void setBigWindow(AddNewImageControl window)
        {
            big = window;

        }

        public void Show()
        {
            Visibility = Visibility.Visible;
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
            currentX = new List<string>();
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
            dateTo.Text = "";

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
                    originExists = false;

                }
                else if (strings[0] == "blue")
                {

                }
                else if (strings[0] == "yellow")
                {
                    currentX.Remove(strings[1]);
                    currLocationExists = false;
                }
                date.IsReadOnly = true;
                city.IsReadOnly = true;
                date.Text = "";
                city.Text = "";
                dateTo.Text = "";
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
                            dateInfo[checkedButton] = date.Text +"/null";
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
