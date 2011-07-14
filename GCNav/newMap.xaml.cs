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
using DeepZoom;
using DeepZoom.Controls;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace GCNav
{
    /// <summary>
    /// Interaction logic for newMap.xaml
    /// </summary>
    public partial class newMap : UserControl
    {
        private String mapImageLocations;
        private Size windowSize;
        private int CanvasLeft, CanvasRight, CanvasTop, CanvasBottom;
        ImageData currentImage;
        private Dictionary<SurfaceRadioButton, String> locButtons;
        private Dictionary<SurfaceRadioButton, Ellipse> ellipses,backEllipses;
        private ImageData data;
     
        public newMap()
        {
            InitializeComponent();
            this.Width = 1080;
            //this.loadMap();
            locButtons = new Dictionary<SurfaceRadioButton, String>();
            ellipses = new Dictionary<SurfaceRadioButton, Ellipse>();
            backEllipses = new Dictionary<SurfaceRadioButton, Ellipse>();
            this.loadImageBlur();
            
          //  Location.TouchUp +=new EventHandler<System.Windows.Input.TouchEventArgs>(Location_TouchUp);
          //  Location.TouchDown +=new EventHandler<System.Windows.Input.TouchEventArgs>(Location_TouchDown);
           // map.Source = "D://newMap.dzi";
        }
        public void loadImageBlur()
        {
            //This is to add the faded edge effect
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
            
        }

        public void loadMap()
        {
            
            //MultiScaleImage newImage = new MultiScaleImage();
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\data\\";
            String mapUri = dataUri + "Map/newMap.jpg/dz.xml";
            
            mapImage.SetImageSource(mapUri);
            mapImage.UpdateLayout();
            Canvas.SetZIndex(mapImage, -10);
            
            //MapCanvas.Children.Add(newImage);
           // newImage.Source = new MultiScaleTileSource();
            //mapImage.Loaded +=new RoutedEventHandler(mapImage_Loaded);
            
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
            //dpd.RemoveValueChanged(mapImage.GetZoomableCanvas, LocationChanged);

            ZoomableCanvas msi = mapImage.GetZoomableCanvas;
            dpd.AddValueChanged(msi, LocationChanged);

           
            
           // MessageBox.Show("scale" + mapImage.GetZoomableCanvas.Scale);
           
        }
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
                String[] locInfo = Regex.Split(str, "/");
                double lon = Convert.ToDouble(locInfo[1]);
                double lat = Convert.ToDouble(locInfo[2]);

                double long1 = (lon-0.0028) * 11527;
                double lat1 = (lat-0.002) *6505;
                
                Double screenPosX = (mapImage.GetZoomableCanvas.Scale * long1) - mapImage.GetZoomableCanvas.Offset.X; //need to reset the location thing
                Double screenPosY = (mapImage.GetZoomableCanvas.Scale * lat1) - mapImage.GetZoomableCanvas.Offset.Y;

                Canvas.SetLeft(backEllipse, screenPosX -1/mapImage.GetZoomableCanvas.Scale);
                Canvas.SetTop(backEllipse, screenPosY - 0.5 / mapImage.GetZoomableCanvas.Scale);

                Canvas.SetLeft(newEllipse, screenPosX +2 - 1 / mapImage.GetZoomableCanvas.Scale);
                Canvas.SetTop(newEllipse, screenPosY + 2 - 0.5 / mapImage.GetZoomableCanvas.Scale);

                Canvas.SetLeft(rb, screenPosX - 1 / mapImage.GetZoomableCanvas.Scale);
                Canvas.SetTop(rb, screenPosY - 0.5 / mapImage.GetZoomableCanvas.Scale);

                //Console.Out.WriteLine("canvas width" + blur.Width);

                //Console.Out.WriteLine(screenPosX);
                //Make sure all the map buttons are displayed within the map image

                Console.Out.Write("screen" +screenPosX);

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

        public void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
          
           Size newSize = e.NewSize;
           Size oldSize = e.PreviousSize;

           CanvasLeft = (int)(newSize.Width / 4);

         //  Console.Out.WriteLine("map" + CanvasLeft);
           Canvas.SetLeft(this, CanvasLeft);
           Canvas.SetTop(this, 30);

           Double mapAspectRatio = mapImage.Height / mapImage.Width;

           CanvasBottom = CanvasTop + (int)(newSize.Width / 2 / mapAspectRatio);

           ScaleTransform tran = new ScaleTransform();

           double scale = Math.Max(newSize.Width / 1600, newSize.Height / 900);
           tran.ScaleX = scale;// newSize.Width / 1600; //scale according to 1600* 900 resolution
           tran.ScaleY = scale;// newSize.Height / 900;

          this.RenderTransform = tran;

        }

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

        public void createButtons(String str)
        {
            //MessageBox.Show("scale" + mapImage.GetZoomableCanvas.Scale);
            String[] buttonsInfo = System.Text.RegularExpressions.Regex.Split(str, "/");
            String lon = buttonsInfo[1];
            String lat = buttonsInfo[2];
            
            double db1 = Convert.ToDouble(lon);
            double db2 = Convert.ToDouble(lat);
            //Console.Out.WriteLine(db1);

            Ellipse newEllipse = new Ellipse();
            Ellipse backEllipse = new Ellipse();
            SurfaceRadioButton newButton = new SurfaceRadioButton();
           
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            if (buttonsInfo[0]=="yellow")//yellow
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                locButtons.Add(newButton, str);
               
            }
            else if (buttonsInfo[0]=="blue")//blue
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

            newButton.Click +=new RoutedEventHandler(newButton_Click);

            Location.Children.Add(newEllipse);
            Location.Children.Add(newButton);
            Location.Children.Add(backEllipse);
            
            double mapActualWidth = 11527 * mapImage.GetZoomableCanvas.Scale;
            double mapActualHeight = 6505 * mapImage.GetZoomableCanvas.Scale;
            double long1 = db1 * mapActualWidth - mapImage.GetZoomableCanvas.Offset.X;
            double lat1 = db2 * mapActualHeight - mapImage.GetZoomableCanvas.Offset.Y;


            ellipses.Add(newButton,newEllipse);
            backEllipses.Add(newButton, backEllipse);
            Canvas.SetLeft(newButton, long1 -5 - 1 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetTop(newButton, lat1-3 - 0.5 / mapImage.GetZoomableCanvas.Scale );
            Canvas.SetZIndex(newButton, 10);
            Canvas.SetLeft(newEllipse, long1 -3 - 1 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetTop(newEllipse, lat1 -1 - 0.5 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetZIndex(newEllipse, 10);
            Canvas.SetLeft(backEllipse, long1-5 - 1 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetTop(backEllipse, lat1-3- 0.5 / mapImage.GetZoomableCanvas.Scale);
            Canvas.SetZIndex(backEllipse, 8);

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
        public void newButton_Click(Object sender, RoutedEventArgs e) 
        {
            String str = locButtons[(SurfaceRadioButton)sender];
            String name = data.title;
            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
                name = name + "...";
            }
            //Console.Out.WriteLine(str);

          //  Ellipse backEllipse = backEllipses[(SurfaceRadioButton)sender];
         //   SolidColorBrush newBrush = new SolidColorBrush();
          //  newBrush.Color = Color.FromRgb(219,219,112);
          //  backEllipse.Fill = newBrush;

            String labelText = name + " ";
            String[] displayInfo = Regex.Split(str, "/");
            String locCategory = displayInfo[0];
            String date = displayInfo[3];
            String city = displayInfo[4];

            //Console.Out.WriteLine(displayInfo[0]);
            if (locCategory == "yellow")
            {
                if (city == "")
                {
                    labelText += "is currently kept here";
                }
                else
                {
                    labelText += "is currently at" +" "+ city;
                }
                if (date != "")
                {
                    labelText += ", " + date;
                }
              //  newColor.Color = Color.FromRgb(244, 234, 150);
                
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
                    labelText += ", " + date;
                }
              //  newColor.Color = Color.FromRgb(0,169,184);
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
                    labelText += ", " + date;
                }
                //newColor.Color = Color.FromRgb(244, 234, 150);
            }
            //Console.Out.WriteLine(labelText);
            infoLabel.Text = labelText;
            //infoLabel1.Text = labelText;
          
        }

        //This method is called when the user select a new image
        public void HandleImageSelectedEvent(Object sender, Helpers.ImageSelectedEventArgs e)
        {
           
            data = e.getImage();
            this.hideButtons();
            infoLabel.Text = "";
           // infoLabel1.Text = "";
            List<String> locInfo = e.getImage().getLocButtonInfo();

            //Console.Out.WriteLine(locInfo.Count);
            foreach (String info in locInfo)
            {
                this.createButtons(info);

            }
           

        }
        
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
