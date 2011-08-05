using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Xml;

namespace GCNav
{
    /// <summary>
    /// Interaction logic for newMap.xaml.This class models the deepzoom map in the navigator
    /// </summary>
    public partial class newMap : UserControl
    {
        private int CanvasLeft, CanvasTop, CanvasBottom;
        private Dictionary<SurfaceRadioButton, String> locButtons;
        private Dictionary<SurfaceRadioButton, Ellipse> ellipses, backEllipses;
        private ImageData data;
        public Double tranScaleX, tranScaleY;
        private Double mapWidth, mapHeight;

        public newMap()
        {
            InitializeComponent();
            this.Width = 1080;
            locButtons = new Dictionary<SurfaceRadioButton, String>();
            ellipses = new Dictionary<SurfaceRadioButton, Ellipse>();
            backEllipses = new Dictionary<SurfaceRadioButton, Ellipse>();
            this.loadImageBlur();
            Canvas.SetZIndex(Location, 20);
            this.findImageSize();
        }

        //This checks if the mapButtons are clicked.
        void Location_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            foreach (UIElement ele in Location.Children)
            {
                Ellipse ell = ele as Ellipse;
                if (ell != null)
                {
                    Double canvasLeft = Canvas.GetLeft(ell);
                    Double canvasTop = Canvas.GetTop(ell);
                    Point newPoint = e.MouseDevice.GetPosition(Location);
                    Double width = ell.Width;
                    Double height = ell.Height;

                    if (ell.Width != 26)
                    {
                        Point p = (sender as UIElement).TranslatePoint(e.GetPosition(sender as UIElement), Location);
                        if (p.X > canvasLeft && p.X < canvasLeft + width && p.Y > canvasTop && p.Y < canvasTop + height)
                        {
                            foreach (SurfaceRadioButton rb in ellipses.Keys)
                            {
                                if (ellipses[rb] == ell)
                                {
                                    newButton_Click(rb);

                                }
                            }
                        }
                    }
                }
            }
        }

        //This is to add the faded edge effect
        public void loadImageBlur()
        {

            BitmapImage blurImage = new BitmapImage();
            blurImage.BeginInit();
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\data\\";
            String imagePath = dataUri + "Map\\Images\\Blur.png";
            blurImage.UriSource = new Uri(imagePath);
            blurImage.EndInit();
            blur.Source = blurImage;
            ScaleTransform newT = new ScaleTransform();
            newT.ScaleX = 1.78;

            blur.RenderTransform = newT;
            Canvas.SetZIndex(blur, 20);
            blur.Visibility = Visibility.Hidden;

        }

        //This load the deepzoom map
        public void loadMap()
        {

            //MultiScaleImage newImage = new MultiScaleImage();
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\data\\";
            String mapUri = dataUri + "Map/MAP.png/dz.xml";

            mapImage.SetImageSource(mapUri);
            mapImage.UpdateLayout();
            Canvas.SetZIndex(mapImage, -10);

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));

            ZoomableCanvas msi = mapImage.GetZoomableCanvas;
            dpd.AddValueChanged(msi, LocationChanged);

        }

        //fade map and blur when filter box is down
        public void fadeMap()
        {
            //change opacity of blur and map
            mapImage.Opacity = .5;
        }

        public void unfadeMap()
        {
            mapImage.Opacity = 1;
            blur.Opacity = 1;
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


        //This sets the mapButton onto the right place when the map is being zoomed in and out
        public void LocationChanged(Object sender, EventArgs e)
        {
            foreach (SurfaceRadioButton rb in locButtons.Keys)
            {
                Ellipse newEllipse = ellipses[rb];
                Ellipse backEllipse = backEllipses[rb];
                rb.Visibility = Visibility.Visible;
                newEllipse.Visibility = Visibility.Visible;
                backEllipse.Visibility = Visibility.Visible;
                String str = locButtons[rb];
                String[] locInfo = Regex.Split(str, "///");
                double lon = Convert.ToDouble(locInfo[1]);
                double lat = Convert.ToDouble(locInfo[2]);

                double long1 = (lon - 0.0028) * mapWidth;
                double lat1 = (lat - 0.002) * mapHeight;

                Double screenPosX = (mapImage.GetZoomableCanvas.Scale * long1) - mapImage.GetZoomableCanvas.Offset.X; //need to reset the location thing
                Double screenPosY = (mapImage.GetZoomableCanvas.Scale * lat1) - mapImage.GetZoomableCanvas.Offset.Y;

                Canvas.SetLeft(backEllipse, screenPosX - 1 / mapImage.GetZoomableCanvas.Scale);
                Canvas.SetTop(backEllipse, screenPosY - 0.5 / mapImage.GetZoomableCanvas.Scale);

                Canvas.SetLeft(newEllipse, screenPosX + 2 - 1 / mapImage.GetZoomableCanvas.Scale);
                Canvas.SetTop(newEllipse, screenPosY + 2 - 0.5 / mapImage.GetZoomableCanvas.Scale);

                Canvas.SetLeft(rb, screenPosX - 1 / mapImage.GetZoomableCanvas.Scale);
                Canvas.SetTop(rb, screenPosY - 0.5 / mapImage.GetZoomableCanvas.Scale);


                //Make sure all the map buttons are displayed within the map image.
                if (screenPosX < 20 || screenPosX > Location.Width - 20)
                {
                    rb.Visibility = Visibility.Collapsed;
                    newEllipse.Visibility = Visibility.Collapsed;
                    backEllipse.Visibility = Visibility.Collapsed;
                }
                if (screenPosY < 10 || screenPosY > Location.Height - 25)
                {
                    rb.Visibility = Visibility.Collapsed;
                    newEllipse.Visibility = Visibility.Collapsed;
                    backEllipse.Visibility = Visibility.Collapsed;
                }
            }
        }

        //This makes sure the display is resolution independent on screens of any resolution.
        public void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {

            Size newSize = e.NewSize;
            Size oldSize = e.PreviousSize;

            CanvasLeft = (int)(newSize.Width * 0.316);

            Canvas.SetLeft(this, CanvasLeft);
            Canvas.SetTop(this, 30);

            Double mapAspectRatio = mapImage.Height / mapImage.Width;

            CanvasBottom = CanvasTop + (int)(newSize.Width / 2 / mapAspectRatio);

            ScaleTransform tran = new ScaleTransform();

            double scale = Math.Max(newSize.Width / 1600, newSize.Height / 900);
            tran.ScaleX = newSize.Width / 1600; //scale according to 1600* 900 resolution
            tran.ScaleY = newSize.Height / 900;

            this.RenderTransform = tran;
            tranScaleX = tran.ScaleX;
            tranScaleY = tran.ScaleY;
        }

        //When the new image is selected on the navigator, mapButtons of the previous image should be clear
        public void hideButtons()
        {
            if (Location.Children.Count != 0)
            {
                foreach (SurfaceRadioButton rb in locButtons.Keys)
                {
                    Location.Children.Remove(rb);
                }
                foreach (Ellipse ell in ellipses.Values)
                {
                    Location.Children.Remove(ell);
                }
                foreach (Ellipse backEll in backEllipses.Values)
                {
                    Location.Children.Remove(backEll);
                }
            }
        }

        //Creates the button and shows them up on the right place
        public void createButtons(String str)
        {
            String[] buttonsInfo = System.Text.RegularExpressions.Regex.Split(str, "///");
            String lon = buttonsInfo[1];
            String lat = buttonsInfo[2];

            double db1 = Convert.ToDouble(lon);
            double db2 = Convert.ToDouble(lat);

            Ellipse newEllipse = new Ellipse();
            Ellipse backEllipse = new Ellipse();
            SurfaceRadioButton newButton = new SurfaceRadioButton();

            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            if (buttonsInfo[0] == "yellow")//yellow
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                locButtons.Add(newButton, str);
            }
            else if (buttonsInfo[0] == "blue")//blue
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 0, 0, 255);
                locButtons.Add(newButton, str);

            }
            else if (buttonsInfo[0] == "red")//red
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);
                locButtons.Add(newButton, str);
            }

            newEllipse.Width = 22;
            newEllipse.Height = 22;
            backEllipse.Width = 26;
            backEllipse.Height = 26;

            SolidColorBrush newBrush = new SolidColorBrush();
            newBrush.Color = Color.FromRgb(0, 255, 127);
            newEllipse.Fill = mySolidColorBrush;
            backEllipse.Fill = newBrush;

            Location.Children.Add(newEllipse);
            Location.Children.Add(newButton);
            Location.Children.Add(backEllipse);

            double mapActualWidth = mapWidth * mapImage.GetZoomableCanvas.Scale;
            double mapActualHeight = mapHeight * mapImage.GetZoomableCanvas.Scale;
            double long1 = db1 * mapActualWidth - mapImage.GetZoomableCanvas.Offset.X;
            double lat1 = db2 * mapActualHeight - mapImage.GetZoomableCanvas.Offset.Y;

            ellipses.Add(newButton, newEllipse);
            backEllipses.Add(newButton, backEllipse);
            Canvas.SetLeft(newButton, long1 - 5 - 1 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetTop(newButton, lat1 - 3 - 0.5 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetZIndex(newButton, 10);
            Canvas.SetLeft(newEllipse, long1 - 3 - 1 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetTop(newEllipse, lat1 - 1 - 0.5 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetZIndex(newEllipse, 10);
            Canvas.SetLeft(backEllipse, long1 - 5 - 1 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetTop(backEllipse, lat1 - 3 - 0.5 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetZIndex(backEllipse, 8);

            //Make sure the buttons are within the map image.
            if (long1 < 0 || long1 > Location.Width)
            {
                newButton.Visibility = Visibility.Collapsed;
                newEllipse.Visibility = Visibility.Collapsed;
                backEllipse.Visibility = Visibility.Collapsed;
            }
            if (lat1 < 0 || lat1 > Location.Height)
            {
                newButton.Visibility = Visibility.Collapsed;
                newEllipse.Visibility = Visibility.Collapsed;
                backEllipse.Visibility = Visibility.Collapsed;
            }
        }

        //Displays the infomation of the mapButtons
        public void newButton_Click(SurfaceRadioButton sender)
        {
            String str = locButtons[(SurfaceRadioButton)sender];
            sender.IsChecked = true;
            String name = data.title;
            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
                name = name + "...";
            }

            String labelText = name + " ";
            String[] displayInfo = Regex.Split(str, "///");
            String locCategory = displayInfo[0];
            String date = displayInfo[3];
            String city = displayInfo[4];

            if (locCategory == "yellow")
            {
                if (city == "")
                {
                    labelText += "is currently kept here";
                }
                else
                {
                    labelText += "is currently at" + " " + city;
                }
                if (date != "")
                {
                    String[] strings = Regex.Split(date, "/");
                    if (strings[0] != "null" && strings[1] != "null")
                    {
                        labelText += ", " + "from" + " " + strings[0] + " to " + strings[1];
                    }
                    else
                    {
                        if (strings[0] == "null")
                        {
                            labelText += ", " + strings[1];
                        }
                        else
                        {
                            labelText += ", " + strings[0];
                        }
                    }
                }

            }
            else if (locCategory == "blue")
            {
                if (city == "")
                {
                    labelText += " was displayed here";
                }
                else
                {
                    labelText += " was displayed in" + " " + city;
                }
                if (date != "")
                {
                    String[] strings = Regex.Split(date, "/");
                    if (strings[0] != "null" && strings[1] != "null")
                    {
                        labelText += ", " + "from" + " " + strings[0] + " to " + strings[1];
                    }
                    else
                    {
                        if (strings[0] == "null")
                        {
                            labelText += ", " + strings[1];
                        }
                        else
                        {
                            labelText += ", " + strings[0];
                        }
                    }
                }
            }
            else
            {
                if (city == "")
                {
                    labelText += " was created here";
                }
                else
                {
                    labelText += " was created in" + " " + city;
                }
                if (date != "")
                {
                    String[] strings = Regex.Split(date, "/");
                    if (strings[0] != "null" && strings[1] != "null")
                    {
                        labelText += ", " + "from" + " " + strings[0] + " to " + strings[1];
                    }
                    else
                    {
                        if (strings[0] == "null")
                        {
                            labelText += ", " + strings[1];
                        }
                        else
                        {
                            labelText += ", " + strings[0];
                        }
                    }
                }
            }
            infoLabel.Text = labelText;
            Canvas.SetZIndex(infoLabel, 20);
        }

        //This method is called when the user select a new image
        public void HandleImageSelectedEvent(Object sender, Helpers.ImageSelectedEventArgs e)
        {
            data = e.getImage();
            this.hideButtons();
            infoLabel.Text = "";
            // infoLabel1.Text = "";
            List<String> locInfo = e.getImage().getLocButtonInfo();

            foreach (String info in locInfo)
            {
                this.createButtons(info);
            }
        }
        //Class for the mapButtons on the map
        public class newMapButton
        {
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
                            return " was created here";
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
            public newMapButton(double x, double y, int type, ImageData img)
            {
                _x = x;
                _y = y;
                _type = type;
                _canvas = new Canvas();
                Canvas.SetZIndex(_canvas, -1);
                _ellipse = new Ellipse();
                switch (type)
                {
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
                _ellipse.StrokeThickness = 0;
                _ellipse.Width = 20;
                _ellipse.Height = 20;
                _canvas.Children.Add(_ellipse);
                _canvas.Visibility = Visibility.Collapsed;
                _imageData = img;
            }
        }

        //This is to check if any mapButtons on the mapImage has been checked
        private void mapImage_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            foreach (UIElement ele in Location.Children)
            {
                SurfaceRadioButton t = ele as SurfaceRadioButton;
                Ellipse ell = ele as Ellipse;
                if (ell != null)
                {
                    Double canvasLeft = Canvas.GetLeft(ell);
                    Double canvasTop = Canvas.GetTop(ell);
                    Double width = ell.Width;
                    Double height = ell.Height;

                    if (ell.Width != 26)
                    {
                        TouchPoint p = e.GetTouchPoint(Location);
                        if (p.Position.X > canvasLeft && p.Position.X < canvasLeft + width && p.Position.Y > canvasTop && p.Position.Y < canvasTop + height)
                        {
                            foreach (SurfaceRadioButton rb in ellipses.Keys)
                            {
                                if (ellipses[rb] == ell)
                                {
                                    newButton_Click(rb);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
