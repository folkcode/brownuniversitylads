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

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for map.xaml
    /// </summary>
    public partial class map : System.Windows.Controls.UserControl
    {
        public int RadioColor; //Represents which radioButton is chosen
        public List<string> redX;
        public List<string> redY;
        public List<string> blueX;
        public List<string> blueY;
        public List<string> yellowX;
        public List<string> yellowY;
        public List<SurfaceRadioButton> radioButtons;
        public Dictionary<SurfaceRadioButton ,string> dic;
        public big_window big;
       
        public map()
        {
            InitializeComponent();
            map1.TouchDown += new EventHandler<TouchEventArgs>(MapTouchHandler);
            map1.MouseUp += new MouseButtonEventHandler(MapMouseHandler);
            this.showMap();
            RadioColor = 0; //Means not chosen yet
            redX = new List<string>();
            redY = new List<string>();
            blueX = new List<string>();
            blueY = new List<string>();
            yellowX = new List<string>();
            yellowY = new List<string>();
            radioButtons = new List<SurfaceRadioButton>();
            dic = new Dictionary<SurfaceRadioButton, string>();
        }

        public void showMap() {
            String mapPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\" +"fullmap.png";
            
            // String mapPath = "F://project/authoring/SurfaceApplication3/Resources/fullmap.png";
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(mapPath);
            myBitmapImage.EndInit();
  
            //set image source
            map1.Source = myBitmapImage;
        
        }

        private void MapTouchHandler(object sender, TouchEventArgs e)
        {
            //((Canvas)sender).CaptureTouch(e.TouchDevice);
             Point newPoint = e.TouchDevice.GetCenterPosition(this);
             this.CreateNewPoints(newPoint);
            // Console.Out.WriteLine(newPoint);
        
        }
        private void MapMouseHandler(object sender, MouseButtonEventArgs e) {
            //((Image)sender).CaptureMouse();
            
           
                Point newPoint = e.MouseDevice.GetCenterPosition(this);
                this.CreateNewPoints(newPoint);
                
            }
        
           // Console.Out.WriteLine(newPoint);
            
        
        
        public void CreateNewPoints(Point newPoint) {

            if (RadioColor != 0)
            {   //get the point clicked and calculate the longitute and latitude
                LengthConverter myLengthConverter = new LengthConverter();
                Double db1 = newPoint.X - 31;
                Double db2 = newPoint.Y - 51;

                Double longitude = db1 / 434;
                Double latitude = db2 / 344;
                String lon = longitude.ToString("N2");
                String lat = latitude.ToString("N2");
                //  Console.Out.WriteLine(lon);
                // Console.Out.WriteLine(lat);

                SurfaceRadioButton newButton = new SurfaceRadioButton();
                radioButtons.Add(newButton);
                //Ellipse newEllipse = new Ellipse();
                //ellipses.Add(newEllipse);
                //newEllipse.Width = 10;
                //newEllipse.Height = 10;
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                if (RadioColor == 3)//yellow
                {
                    mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                    yellowX.Add(lon);
                    yellowY.Add(lat);
                    dic.Add(newButton,"yellow" + ","+ lon + "," + lat);
                }
                else if (RadioColor == 2)//blue
                {
                    mySolidColorBrush.Color = Color.FromArgb(255, 0, 0, 255);
                    blueX.Add(lon);
                    blueY.Add(lat);
                    dic.Add(newButton, "blue" + "," + lon + "," + lat);
                }
                else if (RadioColor == 1)//red
                {
                    mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
                    redX.Add(lon);
                    redY.Add(lat);
                    dic.Add(newButton, "red" + "," + lon + "," + lat);
                }

                //newEllipse.Fill = mySolidColorBrush;
                //mapCover.Children.Add(newEllipse);
                
                
                
                //how to set the toggle button into round shape
               // SurfaceButton button = new SurfaceButton();
               // button.Visibility = System.Windows.Visibility.Visible;
              
                newButton.Click += new RoutedEventHandler(newButton_Click);
                
                mapCover.Children.Add(newButton);
                
                newButton.Width = 1;
                newButton.Height = 1;
                newButton.Background = mySolidColorBrush;
                //Set the location of the circle on the map  
                Canvas.SetLeft(newButton, db1);
                Canvas.SetTop(newButton, db2);

                             
            }
        }

        //Used to cancel one single location 
        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            mapCover.Children.Remove((SurfaceRadioButton)sender);
            radioButtons.Remove((SurfaceRadioButton)sender);
            //need to find out which kind of location the spot is and remove from the list before saving
            String str = dic[(SurfaceRadioButton)sender];
            string[] strings = Regex.Split(str,",");
            if (strings[0] == "red") {
              
                redX.Remove(strings[1]);
                redY.Remove(strings[2]);
            
            }
            else if (strings[1] == "blue")
            {
                blueX.Remove(strings[1]);
                blueY.Remove(strings[2]);

            }
            else {
               // Console.Out.WriteLine("called yellow");
                yellowX.Remove(strings[1]);
                yellowY.Remove(strings[2]);
            }
        
        }

        private void save_close_click(object sender, RoutedEventArgs e)
        {
            this.save();
            this.Hide();
        }

        public void loadPositions()
        {
            String dataDir = "Data/";
           // String dataDir = "C://LADS-yc60/";
            //String dataDir = "F://lads_data/";
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
                               // Console.Out.WriteLine(big.getImageName());
                                if (big.getImageName() == path)
                                {
                                   
                                    foreach (XmlNode imgnode in node.ChildNodes)
                                    {
                                        if (imgnode.Name == "Locations")
                                        {
                                            foreach (XmlNode location in imgnode.ChildNodes)
                                            {
                                                if (location.Name == "Purchase")
                                                {

                                                    String lon = location.Attributes.GetNamedItem("longitude").InnerText;
                                                    String lat = location.Attributes.GetNamedItem("latitude").InnerText;
                                                    this.createCircles(lon, lat, 3);
                                                }
                                                if (location.Name == "Display")
                                                {
                                                    foreach (XmlNode display in location.ChildNodes)
                                                    {
                                                        String lon1 = display.Attributes.GetNamedItem("longitude").InnerText;
                                                        String lat1 = display.Attributes.GetNamedItem("latitude").InnerText;
                                                        this.createCircles(lon1, lat1, 2);
                                                    }

                                                }
                                                if (location.Name == "Work")
                                                {
                                                    String lon2 = location.Attributes.GetNamedItem("longitude").InnerText;
                                                    String lat2 = location.Attributes.GetNamedItem("latitude").InnerText;
                                                    this.createCircles(lon2, lat2, 1);
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
                
        
        private void createCircles(String lon, String lat, int color) {
            double db1 = Convert.ToDouble(lon);
            double db2 = Convert.ToDouble(lat);
            Console.Out.WriteLine(db1);

            SurfaceRadioButton newButton = new SurfaceRadioButton();
            newButton.Height = 1;
            newButton.Width = 1;
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            if (color == 3)//yellow
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                
            }
            else if (color == 2)//blue
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 0, 0, 255);
             
            }
            else if (color == 1)//red
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
              
            }

            newButton.Background = mySolidColorBrush;
            mapCover.Children.Add(newButton);

            
            //Set the location of the circle on the map    
            double long1 = db1 * 434;
            double lat1 = db2 * 344;
            Canvas.SetLeft(newButton, long1);
            Canvas.SetTop(newButton, lat1);
        }



        private void save()
        {
            String dataDir = "Data/";
           // String dataDir = "C://LADS-yc60/";
            //String dataDir = "E://"; //should be changed
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
                                // Console.Out.WriteLine(big.getImageName());
                                //Boolean newImage = true;
                                if (big.getImageName() == path)
                                {
                                    //newImage = false;
                                    String oldWorkLong = "";
                                    String oldWorkLati = "";
                                    List<String> oldDisLong = new List<String>();
                                    List<String> oldDisLati = new List<String>();
                                    String oldPurLong = "";
                                    String oldPurLati = "";
                                    XmlElement newLocation = doc.CreateElement("Locations");
                                    foreach (XmlNode imgnode in node.ChildNodes)
                                    {

                                        if (imgnode.Name == "Locations")
                                        {

                                            foreach (XmlNode locnode in imgnode.ChildNodes)
                                            {
                                                if (locnode.Name == "Work")
                                                {
                                                    oldWorkLong = oldWorkLong + locnode.Attributes.GetNamedItem("longitude").InnerText;
                                                    oldWorkLati = oldWorkLati + locnode.Attributes.GetNamedItem("latitude").InnerText;
                                                }
                                                if (locnode.Name == "Display")
                                                {
                                                    foreach (XmlNode disNode in locnode.ChildNodes)
                                                    {
                                                        String oldDisLon = disNode.Attributes.GetNamedItem("longitude").InnerText;
                                                        String oldDisLat = disNode.Attributes.GetNamedItem("latitude").InnerText;
                                                        oldDisLong.Add(oldDisLon);
                                                        oldDisLati.Add(oldDisLat);
                                                    }
                                                }
                                                if (locnode.Name == "Purchase")
                                                {
                                                    oldPurLong = oldPurLong + locnode.Attributes.GetNamedItem("longitude").InnerText;
                                                    oldPurLati = oldPurLati + locnode.Attributes.GetNamedItem("latitude").InnerText;
                                                }

                                            }
                                            node.RemoveChild(imgnode);
                                        }
                                    }



                                        if (oldPurLong != "" || yellowX.Count != 0)
                                        {
                                            XmlElement newPurchase = doc.CreateElement("Purchase");
                                            if (oldPurLong != "")
                                            {
                                                newPurchase.SetAttribute("longitude", oldPurLong);
                                                newPurchase.SetAttribute("latitude", oldPurLati);
                                            }
                                            if (yellowX.Count != 0)
                                            {
                                                foreach (String yellow in yellowX)
                                                {
                                                    newPurchase.SetAttribute("longitude", yellow);
                                                }
                                                foreach (String yellow in yellowY)
                                                {
                                                    newPurchase.SetAttribute("latitude", yellow);
                                                }

                                            }
                                            newLocation.AppendChild(newPurchase);
                                        }

                                        if (oldDisLong.Count != 0 || blueX.Count != 0)
                                        {
                                            XmlElement newDisplay = doc.CreateElement("Display");

                                            if (oldDisLong.Count != 0)
                                            {
                                                XmlElement location = doc.CreateElement("Location");
                                                for (int i = 0; i < oldDisLong.Count; i++)
                                                {
                                                    location.SetAttribute("longitude", oldDisLong[i]);
                                                    location.SetAttribute("latitude", oldDisLati[i]);
                                                    newDisplay.AppendChild(location);
                                                }
                                            }
                                            if (blueX.Count != 0)
                                            {
                                                for (int i = 0; i < blueX.Count; i++)
                                                {
                                                    XmlElement location = doc.CreateElement("Location");
                                                    location.SetAttribute("longitude", blueX[i]);
                                                    location.SetAttribute("latitude", blueY[i]);

                                                    newDisplay.AppendChild(location);
                                                }


                                            }
                                            newLocation.AppendChild(newDisplay);
                                        }

                                        if (oldWorkLong != "" || redX.Count != 0)
                                        {
                                            XmlElement newWork = doc.CreateElement("Work");
                                            if (oldWorkLong != "")
                                            {
                                                newWork.SetAttribute("longitude", oldWorkLong);
                                                newWork.SetAttribute("latitude", oldWorkLati);
                                            }
                                            if (redX.Count != 0)
                                            
                                                {
                                                    foreach (String red in redX)
                                                    {
                                                        newWork.SetAttribute("longitude", red);
                                                    }
                                                    foreach (String red in redY)
                                                    {
                                                        newWork.SetAttribute("latitude", red);
                                                    }

                                                }
                                                newLocation.AppendChild(newWork);
                                            }
                                        node.AppendChild(newLocation);
                                        doc.Save(dataDir + "NewCollection.xml");

                                        }
                                        
                                    }
                                }

                            
                        }
                    }

                }

            }
        

        
        public void setBigWindow(big_window window) {
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

        }

        private void SurfaceRadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            RadioColor = 2;
        }

        private void SurfaceRadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            RadioColor = 3;
        }

       
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            Console.Out.WriteLine("called1");
            ((Canvas)sender).CaptureMouse();
           // Point newPoint = e.MouseDevice.GetCenterPosition(mapCover);
           // Console.Out.WriteLine(newPoint);
           // mapCover.TouchDown += new EventHandler<TouchEventArgs>(MapTouchHandler);
           // mapCover.MouseDown += new MouseButtonEventHandler(MapTouchHandler);
        }

        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //clear all the newly-added points
            foreach (SurfaceRadioButton toRemove in radioButtons) {
                mapCover.Children.Remove(toRemove);
            }

            //mapCover.Children.Add(map1);
            //Clear the stored points
            radioButtons = new List<SurfaceRadioButton>();
            redX = new List<string>();
            redY = new List<string>();
            blueX = new List<string>();
            blueY = new List<string>();
            yellowX = new List<string>();
            yellowY = new List<string>();
            dic = new Dictionary<SurfaceRadioButton,String>();
        }
        
    }
}
