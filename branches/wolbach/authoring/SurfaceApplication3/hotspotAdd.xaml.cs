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
using System.IO;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Xml;
using DeepZoom;
using Microsoft.DeepZoomTools;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class hotspotAdd : UserControl
    {
        private BitmapImage curImage;
        private String imageName;
        private List<Ellipse> ellipses;
        private List<String> posX;
        private List<String> posY;
        private Boolean exists;
        private Boolean isImage;
        
            
        public hotspotAdd()
        {
            InitializeComponent();
            image1.TouchDown += new EventHandler<TouchEventArgs>(ImageTouchHandler);
            image1.MouseDown += new MouseButtonEventHandler(ImageMouseHandler);
            ellipses = new List<Ellipse>();
            posX = new List<String>();
            posY = new List<String>();
            exists = false;
            isImage = false;
        }
        
        private void ImageTouchHandler(object sender, TouchEventArgs e)
        {
            //((Canvas)sender).CaptureTouch(e.TouchDevice);
            Point newPoint = e.TouchDevice.GetCenterPosition(this);
            this.CreateNewPoints(newPoint);
            // Console.Out.WriteLine(newPoint);

        }
       
        private void ImageMouseHandler(object sender, MouseButtonEventArgs e)
        {
            // ((Image)sender).CaptureMouse();
            Point newPoint = e.MouseDevice.GetCenterPosition(this);
            this.CreateNewPoints(newPoint);

            // Console.Out.WriteLine(newPoint);


        }

        public void setImagePath(String path) {
            imageName = path;
        }

        public void LoadHotsptos() {
            String dataDir = "Data/XMLFiles/";
            // String dataDir = "C://LADS-yc60/data/XMLFiles/";
            
            //String dataDir = "F://lads_data/XMLFiles/";
          
          XmlDocument doc = new XmlDocument();
          //Console.Out.WriteLine(imageName);
          Console.Out.WriteLine(dataDir + imageName + "." + "xml");
          String path = dataDir + imageName + "." + "xml";
          if (File.Exists(dataDir + imageName + "." + "xml"))
          {
             // Console.Out.WriteLine("exists");
              exists = true;
              doc.Load(path);
              if (doc.HasChildNodes)
              {
                 foreach (XmlNode docNode in doc.ChildNodes)
                 {
                    if (docNode.Name == "hotspots")
                    {
                        Console.Out.WriteLine("called1");
                        if (docNode.HasChildNodes){
                            Console.Out.WriteLine("called2");
                            foreach (XmlNode hotspot in docNode.ChildNodes){
                                if(hotspot.Name == "hotspot"){
                                    String positionX = "";
                                    String positionY = "";
                                    Console.Out.WriteLine("called3");

                                    foreach (XmlNode posNode in hotspot.ChildNodes)
                                    {
                                        if (posNode.Name == "positionX")
                                        {
                                            String poX = posNode.InnerText;
                                            positionX = poX;
                                           // Console.Out.WriteLine(positionX);
                                        }
                                        if (posNode.Name == "positionY") 
                                        {
                                            String poY = posNode.InnerText;
                                            positionY = poY;
                                        
                                        }
                                        if (posNode.Name == "type") 
                                        {
                                            String type = posNode.InnerText;
 
                                        }
                                        if (posNode.Name == "description")
                                        {
                                            String description = posNode.InnerText;
                                        }

                                    }
                                    Point newPoint = new Point();
                                    
                                   
                                     BitmapImage newImage = new BitmapImage();
                                     String dataDir1 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
                                     String filePath = dataDir1+ "Images\\" + imageName; 
                                    //String filePath = "C://LADS-yc60/data/Images/" + imageName;
                                    
                                    // String filePath = "F:/lads_data/Images/" + imageName;
                                     try
                                     {
                                         newImage.BeginInit();
                                         newImage.UriSource = new Uri(@filePath);
                                         newImage.EndInit();
                                     }
                                     catch (Exception e)
                                     {
                                         MessageBox.Show("You must save edits before click on the hotspot and location button!");
                                         return;
                                     }

                                     this.setImage(newImage);
                           
                                     newPoint.X = Convert.ToDouble(positionX) / (newImage.Width * 9) * image1.Width + 28;
                                     newPoint.Y = Convert.ToDouble(positionY)/(newImage.Height* 9) * image1.Height + 48;
                                     Console.Out.WriteLine(Convert.ToDouble(positionX) / (newImage.Width * 32));
                                     Console.Out.WriteLine(newPoint.X); 
                                     Console.Out.WriteLine(newPoint.Y);
                                   
                                    this.CreateNewPoints(newPoint);
                                 }

                                }
                            }
                        }
                    }
                 }
                
          }
          //doc.Load(d);
          
          
        
        }

        public void CreateNewPoints(Point newPoint) {
            Double db1 = newPoint.X - 28;
            Double db2 = newPoint.Y - 48;

            Double PositionX = db1 / 434 * curImage.Width;
            Double PositionY = db2 / 344 * curImage.Height;
            String X = PositionX.ToString("N2");
            String Y = PositionY.ToString("N2");
            posX.Add(X);
            posY.Add(Y);

            Ellipse newEllipse = new Ellipse();
            ellipses.Add(newEllipse); //the list of newly added hotspots
            newEllipse.Width = 10;
            newEllipse.Height = 10;
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
            newEllipse.Fill = mySolidColorBrush;
            imageCover.Children.Add(newEllipse);


            //Set the location of the circle on the map               
            Canvas.SetLeft(newEllipse, db1);
            Canvas.SetTop(newEllipse, db2);


        }

        public void Show()
        {
            Visibility = Visibility.Visible;
                       
        }
        public void Hide()

        {

            Visibility = Visibility.Hidden;

        }

        public void save_close_click(object sender, RoutedEventArgs e)
        {
            this.save();
           // if(MessageBox.Show("Do you really wish to quit?","warning",MessageBoxButton.YesNo,
           //     MessageBoxImage.Question)==MessageBoxResult.No){
            
           // return;
            //}
            this.Hide();

        }
        public void save() {  //save the inforamtion of the hotspot
            if (exists == false) {

                XmlDocument doc = new XmlDocument();
                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);
                XmlElement hotspot = doc.CreateElement("hotspots");
                doc.AppendChild(hotspot);
                for (int i = 0; i < posX.Count;i++ )
                {
                    XmlElement spots = doc.CreateElement("hotspot");
                    XmlElement name = doc.CreateElement("name");
                    name.InnerText = text.Text;
                    XmlElement positionX = doc.CreateElement("positionX");
                    XmlElement positionY = doc.CreateElement("positionY");
                    XmlElement type = doc.CreateElement("type");
                    if (URL.Text != null)
                    {
                        isImage = true;
                        type.InnerText = "image";

                    }
                    else {
                        type.InnerText = "text";
                    }
                    XmlElement description = doc.CreateElement("description");
                    if (isImage == false)
                    {
                        description.InnerText = text.Text;
                    }
                    else {
                        description.InnerText = URL.Text;
                    }
                    spots.AppendChild(name);
                    spots.AppendChild(positionX);
                    spots.AppendChild(positionY);
                    spots.AppendChild(type);
                    spots.AppendChild(description);
                    hotspot.AppendChild(spots);
                }
                doc.Save("Data/XMLFiles/" + imageName + "." + "xml");
                //doc.Save("C://LADS-yc60/data/XMLFiles/" + imageName + "." + "xml");
                //doc.Save("F://lads_data/XMLFiles/" + imageName + "." + "xml");
            }


        }
        public void setImage(BitmapImage image) {
            curImage = image;
        }

        public void showImage() {
            
            image1.Source = curImage;
        
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            posX = new List<String>();
            posY = new List<String>();
            foreach (Ellipse ell in ellipses) 
            {
                imageCover.Children.Remove(ell);
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = true;

            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"; 

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] filePath = ofd.FileNames;
                string[] safeFilePath = ofd.SafeFileNames;

                for (int i = 0; i < safeFilePath.Length; i++)
                {
                    URL.Text = safeFilePath[i];
                 
                }

            }
        }
    }
}
