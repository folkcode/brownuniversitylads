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
using System.Text.RegularExpressions;
using DeepZoom;
using Microsoft.DeepZoomTools;
using System.ComponentModel;

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
        private Boolean exists;
        public List<String> hotImagePaths;
        public List<String> hotAudioPaths;
        public List<String> hotVideoPaths;
        public List<String> hotImageNames;
        public List<String> hotAudioNames;
        public List<String> hotVideoNames;
        private List<SurfaceRadioButton> radioButtons;
        public Dictionary<SurfaceRadioButton, String> dic;
        private Dictionary<SurfaceRadioButton, String> dicPos;
        private String hotspotInfo;
        private SurfaceRadioButton buttonChecked;
        private hotspotWindow parentWindow;
        private Helpers _helpers;
        private Boolean addHotspotsEnabled, addHotspotValid;
        public Boolean newWindowIsOpened;
        /// <summary>
        /// Default constructor
        /// </summary>
        public hotspotAdd()
        {
            InitializeComponent();
            image1.MouseDown += new MouseButtonEventHandler(ImageMouseHandler);
            image1.TouchDown += new EventHandler<TouchEventArgs>(image1_TouchDown);
            ellipses = new List<Ellipse>();
            hotImageNames = new List<String>();
            hotAudioNames = new List<String>();
            hotVideoNames = new List<String>();
            hotImagePaths = new List<String>();
            hotAudioPaths = new List<String>();
            hotVideoPaths = new List<String>();
            exists = false;
            radioButtons = new List<SurfaceRadioButton>();
            dic = new Dictionary<SurfaceRadioButton, String>();
            dicPos = new Dictionary<SurfaceRadioButton, String>();
            _helpers = new Helpers();
            addHotspotsEnabled = false;
            addHotspotValid = true;
            newWindowIsOpened = false;
           
        }

        //This is to check if hotspots on the map is selected
        void image1_TouchDown(object sender, TouchEventArgs e)
        {
            Point newPoint = e.TouchDevice.GetCenterPosition(image1);
            this.hotspotValid(newPoint);
            if (addHotspotsEnabled && addHotspotValid)
            {
                addHotspotsEnabled = false;
                Enable.IsEnabled = true;
                this.CreateNewPoints(newPoint, null);

            }
            foreach (UIElement ele in imageCover.Children)
            {
                SurfaceRadioButton t = ele as SurfaceRadioButton;

                if (t != null)
                {
                    Double canvasLeft = Canvas.GetLeft(t);
                    Double canvasTop = Canvas.GetTop(t);
                    image1.UpdateLayout();
                    Double width = t.ActualWidth - 30;
                    Double height = t.ActualHeight - 30;

                    if (newPoint.X > canvasLeft && newPoint.X < canvasLeft + width && newPoint.Y > canvasTop && newPoint.Y < canvasTop + height)
                    {
                        newButton_Checked(t);
                       
                    }

                }
            }
        }

        /// <summary>
        /// Get the position of the click
        /// </summary>
        private void ImageMouseHandler(object sender, MouseButtonEventArgs e)
        {
            Point newPoint = e.MouseDevice.GetCenterPosition(image1);
            this.hotspotValid(newPoint);
            if (addHotspotsEnabled && addHotspotValid)
            {
                addHotspotsEnabled = false;
                Enable.IsEnabled = true;
                this.CreateNewPoints(newPoint, null);
                
            }
            foreach (UIElement ele in imageCover.Children)
            {
                SurfaceRadioButton t = ele as SurfaceRadioButton;

                if (t != null)
                {
                    Double canvasLeft = Canvas.GetLeft(t);
                    Double canvasTop = Canvas.GetTop(t);
                    image1.UpdateLayout();
                    Double width = t.ActualWidth-30;
                    Double height = t.ActualHeight-30;

                    if (newPoint.X > canvasLeft && newPoint.X < canvasLeft + width && newPoint.Y > canvasTop && newPoint.Y < canvasTop + height)
                    {
                        newButton_Checked(t);
                    
                    }

                }
            }
        }

        //This is to make sure that the point is within the image
        public void hotspotValid(Point newPoint)
        {
            Double[] size = this.findImageSize();
            Double imagecurWidth = size[0] * image1.GetZoomableCanvas.Scale; //the size of the zoomed image
            Double imagecurHeight = size[1] * image1.GetZoomableCanvas.Scale;

            Double left = - image1.GetZoomableCanvas.Offset.X;
            Double right = left + imagecurWidth;
            Double top =  - image1.GetZoomableCanvas.Offset.Y;
            Double bottom = top + imagecurHeight;

            Double x = newPoint.X;
            Double y = newPoint.Y;
       
            if (x > right|| x < left)
            {
                addHotspotValid = false;
            }
            else if (y < top || y > bottom)
            {
                addHotspotValid = false;
            }
            else
            {
                addHotspotValid = true;
            }

        }

        public void setImagePath(String path) {
            imageName = path;
        }

        //This shows the deepzoom artwork on which the hotspots are added to
        public void showImage()
        {
          
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\data\\";
            String imageUri = dataUri + "Images\\DeepZoom\\" + imageName+ "\\dz.xml";
            image1.SetImageSource(imageUri);
            image1.UpdateLayout();
          
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
          
            ZoomableCanvas msi = image1.GetZoomableCanvas;
            dpd.AddValueChanged(msi, LocationChanged);
        }

        //This adjusts the location of the hotspots when the artwork is zoomed in or out
        public void LocationChanged(Object sender, EventArgs e)
        {
            foreach (SurfaceRadioButton rb in radioButtons)
            {
                rb.Visibility = Visibility.Visible;
                String str = dicPos[rb];
                String[] locInfo = Regex.Split(str, "/");
                double lon = Convert.ToDouble(locInfo[0]);
                double lat = Convert.ToDouble(locInfo[1]);
             
                Double[] size = this.findImageSize();
                Double imagecurWidth = size[0] * image1.GetZoomableCanvas.Scale; //the size of the zoomed image
                Double imagecurHeight = size[1] * image1.GetZoomableCanvas.Scale;
                Double screenPosX = (lon * imagecurWidth) - image1.GetZoomableCanvas.Offset.X;
               
                Double screenPosY = (lat * imagecurHeight) - image1.GetZoomableCanvas.Offset.Y;

                Canvas.SetLeft(rb, screenPosX);
                Canvas.SetTop(rb, screenPosY);
                if (screenPosX < 0 || screenPosX > imageCover.Width)
                {
                    rb.Visibility = Visibility.Collapsed;
                }
                if (screenPosY < 0 || screenPosY > imageCover.Height)
                {
                    rb.Visibility = Visibility.Collapsed;
                }
            }


        }
        /// <summary>
        /// Load existing hotspots from XML file
        /// </summary>
        public void LoadHotspots()
        {
            Double[] sizes = this.findImageSize();
            Double width = sizes[0];
            Double height = sizes[1];
            String dataDir = "Data/XMLFiles/";

            XmlDocument doc = new XmlDocument();
            String path = dataDir + imageName + "." + "xml";
            if (File.Exists(dataDir + imageName + "." + "xml"))
            {
              
                exists = true;
                doc.Load(path);
                if (doc.HasChildNodes)
                {
                    foreach (XmlNode docNode in doc.ChildNodes)
                    {
                        if (docNode.Name == "hotspots")
                        {                           
                            if (docNode.HasChildNodes)
                            {                               
                                foreach (XmlNode hotspot in docNode.ChildNodes)
                                {
                                    if (hotspot.Name == "hotspot")
                                    {
                                        String positionX = "";
                                        String positionY = "";
                                        String name = "";
                                        String type = "";
                                        String description = "";
                                       
                                        foreach (XmlNode posNode in hotspot.ChildNodes)
                                        {
                                            if (posNode.Name == "name")
                                            {
                                                name = posNode.InnerText;
                                            }
                                            if (posNode.Name == "positionX")
                                            {
                                                String poX = posNode.InnerText;
                                                positionX = poX;
                                              
                                            }
                                            if (posNode.Name == "positionY")
                                            {
                                                String poY = posNode.InnerText;
                                                positionY = poY;

                                            }
                                            if (posNode.Name == "type")
                                            {
                                                type = posNode.InnerText;

                                            }
                                            if (posNode.Name == "description")
                                            {
                                                description = posNode.InnerText;
                                            }

                                        }
                                        String longString = name + "/" + type + "/" + description;
                                    
                                        BitmapImage newImage = new BitmapImage();
                                        String dataDir1 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
                                        String filePath = dataDir1 + "Images\\" + imageName;
                                      
                                        try
                                        {
                                            newImage.BeginInit();
                                            newImage.UriSource = new Uri(@filePath);
                                            newImage.EndInit();
                                        }
                                        catch (Exception e)
                                        {
                                            MessageBox.Show("You must save the artwork before you can add hotspots.");
                                            return;
                                        }

                                        this.setImage(newImage);

                                        Double PositionX = Convert.ToDouble(positionX);
                                        Double PositionY = Convert.ToDouble(positionY);
                                        this.createLoadingPoints(PositionX, PositionY, longString);
                                      
                                    }

                                }
                            }
                        }
                    }
                }

            }
          
        }

        //This creates the hotspots when the artwork is loaded
        public void createLoadingPoints(Double positionX, Double positionY, String longString)
        {

            Point newP = image1.GetZoomableCanvas.Offset;
            Double[] size = this.findImageSize();
            Double imageCurWidth = size[0] * image1.GetZoomableCanvas.Scale; //the size of the zoomed map
            Double imageCurHeight = size[1] * image1.GetZoomableCanvas.Scale;

            Point newPoint = new Point();
            newPoint.X = positionX * imageCurWidth - newP.X;
            newPoint.Y = positionY * imageCurHeight - newP.Y;
            this.CreateNewPoints(newPoint, longString);

        }


        /// <summary>
        /// Find the original image size in dz.xml in Deep Zoom folder of the image
        /// </summary>
        public Double[] findImageSize() 
        {
            Double[] sizes = new Double[2];
            XmlDocument newDoc = new XmlDocument();
            String imageFolder = "Data/Images/DeepZoom/" + imageName + "/" + "dz.xml";
            newDoc.Load(imageFolder);
            if (newDoc.HasChildNodes) {
                foreach (XmlNode image in newDoc.ChildNodes) 
                {
                    if (image.Name == "Image") 
                    {
                        XmlNode size = image.FirstChild;
                        String width = size.Attributes.GetNamedItem("Width").InnerText;
                        String height = size.Attributes.GetNamedItem("Height").InnerText;
                        Double width1 = Convert.ToDouble(width);
                        Double height1 = Convert.ToDouble(height);
                        sizes[0] = width1;
                        sizes[1] = height1;
                       
                    }
                }
            
            }
            return sizes;
        }

        //This creates the points when the users click on the artwork to create new hotspots
        public void CreateNewPoints(Point newPoint, String str)
        {
                Double x = newPoint.X;// -20;
                Double y = newPoint.Y;// -40;
                Double[] size = this.findImageSize();
                Double imagecurWidth = size[0] * image1.GetZoomableCanvas.Scale;
                Double imagecurHeight = size[1] * image1.GetZoomableCanvas.Scale;

                Point newP = image1.GetZoomableCanvas.Offset;
               
                Double canvasLeft = (x + newP.X) / imagecurWidth;
                Double canvasTop = (y + newP.Y) / imagecurHeight;

                String lon = canvasLeft.ToString();
                String lat = canvasTop.ToString();

                SurfaceRadioButton newButton = new SurfaceRadioButton();
                //Default the new button on the map as checked;
                if (str != null)
                {
                    dic.Add(newButton, str);
                }
            
                newButton.IsChecked = true;
                buttonChecked = newButton;

                newButton.Width = 1;
                newButton.Height = 1;

                foreach (SurfaceRadioButton rb in radioButtons)
                {
                    rb.IsChecked = false;
                }

                radioButtons.Add(newButton);
                dicPos.Add(newButton, lon + "/" + lat);

                imageCover.Children.Add(newButton);

                Canvas.SetLeft(newButton, x -3.5);
                Canvas.SetTop(newButton, y -3);
                Canvas.SetZIndex(newButton, 20);
           
        }

        //When a hotspot is checked, display the information and allow user to change the contents
        public void newButton_Checked(SurfaceRadioButton sender)
        {
            sender.IsChecked = true;
            AddText.IsEnabled = false;
            AddImage.IsEnabled = false;
            AddAudio.IsEnabled = false;
            AddVideo.IsEnabled = false;
            Edit.IsEnabled = false;
       
            buttonChecked = (SurfaceRadioButton)sender;
            if (dic.ContainsKey((SurfaceRadioButton)sender))
            {
                String info = dic[buttonChecked];
                String[] infos = Regex.Split(info, "/");
                String type = infos[1];
                Edit.IsEnabled = true;
                if (type == "text")
                {
                    AddImage.IsEnabled = true;
                    AddAudio.IsEnabled = true;
                    AddVideo.IsEnabled = true;
                }
                else if (type == "image")
                {
                    AddText.IsEnabled = true;
                    AddAudio.IsEnabled = true;
                    AddVideo.IsEnabled = true;
                }
                else if (type == "audio")
                {
                    AddImage.IsEnabled = true;
                    AddText.IsEnabled = true;
                    AddVideo.IsEnabled = true;
                }
                else
                {
                    AddImage.IsEnabled = true;
                    AddAudio.IsEnabled = true;
                    AddText.IsEnabled = true;
                }

            }
            else
            {
                AddText.IsEnabled = true;
                AddImage.IsEnabled = true;
                AddAudio.IsEnabled = true;
                AddVideo.IsEnabled = true;
             
            }
          
        }

        public void Enable_Click(Object sender, RoutedEventArgs e)
        {
            if (Enable.IsEnabled)
            {
                addHotspotsEnabled = true;
                Enable.IsEnabled = false;
            }
            else
            {
                addHotspotsEnabled = false;
                Enable.IsEnabled = true;
            }
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
            
        }

        /// <summary>
        /// Save hotspots info to XML file.
        /// </summary>
        public void save()
        {  //save the inforamtion of the hotspot
            
            if (exists == false)
            {
                if (radioButtons.Count != 0)
                {
                    XmlDocument doc = new XmlDocument();
                    XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    doc.AppendChild(docNode);
                    XmlElement hotspot = doc.CreateElement("hotspots");
                    doc.AppendChild(hotspot);
                    for (int i = 0; i < dicPos.Count; i++)
                    {
                        Console.Out.WriteLine("number of hotspots" + dicPos.Count);
                        SurfaceRadioButton button = dicPos.ElementAt(i).Key;
                        if (dic.ContainsKey(button))
                        {
                            XmlElement spots = doc.CreateElement("hotspot");
                            XmlElement name = doc.CreateElement("name");

                            String str = dic[button];

                            String[] strs = Regex.Split(str, "/");
                            if (strs[0] != "" && strs[1] != "")
                            {

                                name.InnerText = strs[0];
                                XmlElement type = doc.CreateElement("type");
                                XmlElement description = doc.CreateElement("description");
                                type.InnerText = strs[1];
                                description.InnerText = strs[2];

                                XmlElement positionX = doc.CreateElement("positionX");
                                XmlElement positionY = doc.CreateElement("positionY");


                                String newStr = dicPos.ElementAt(i).Value;
                                String[] strings = Regex.Split(newStr, "/");

                                Console.Out.WriteLine("lon" + strings[0]);
                                Console.Out.WriteLine("lat" + strings[1]);
                                positionX.InnerText = strings[0];
                                positionY.InnerText = strings[1];


                                spots.AppendChild(name);
                                spots.AppendChild(positionX);
                                spots.AppendChild(positionY);
                                spots.AppendChild(type);
                                spots.AppendChild(description);
                                hotspot.AppendChild(spots);
                            }
                            //This is to inform that parts of the information is not compelete
                            else
                            {
                                MessageBox.Show("Every hotspot must have a name and description.");
                                return;
                            }
                        }
                        // This is to inform that some hotspots do not have information
                        else
                        {
                            MessageBox.Show("Some hotspots do not have information!");
                            return;
                        }
                    }
                    doc.Save("Data/XMLFiles/" + imageName + "." + "xml");
                    parentWindow.Close();

                    //doc.Save("C://LADS-yc60/data/XMLFiles/" + imageName + "." + "xml");
                    //doc.Save("F://lads_data/XMLFiles/" + imageName + "." + "xml");
                }
                else
                {
                    parentWindow.Close();
                }
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Data/XMLFiles/" + imageName + "." + "xml");
                if (doc.HasChildNodes)
                {
                    foreach (XmlNode docNode in doc.ChildNodes)
                    {
                        if (docNode.Name == "hotspots")
                        {
                            docNode.RemoveAll();
                            if (radioButtons.Count != 0)
                            {

                                for (int i = 0; i < dicPos.Count; i++)
                                {
                                    Console.Out.WriteLine("NUMBER" + dicPos.Count);
                                    SurfaceRadioButton button = dicPos.ElementAt(i).Key;
                                    if (dic.ContainsKey(button))
                                    {
                                        XmlElement spots = doc.CreateElement("hotspot");
                                        XmlElement name = doc.CreateElement("name");

                                        String str = dic[button];
                                        String[] strs = Regex.Split(str, "/");

                                        if (strs[0] != "" && strs[2] != "")
                                        {
                                            name.InnerText = strs[0];
                                            XmlElement positionX = doc.CreateElement("positionX");
                                            XmlElement positionY = doc.CreateElement("positionY");
                                            XmlElement type = doc.CreateElement("type");
                                            XmlElement description = doc.CreateElement("description");
                                            type.InnerText = strs[1];
                                            description.InnerText = strs[2];
                                          
                                            String newStr = dicPos.ElementAt(i).Value;
                                           
                                            String[] strings = Regex.Split(newStr, "/");
                                            positionX.InnerText = strings[0];
                                            positionY.InnerText = strings[1];
                                
                                            spots.AppendChild(name);
                                            spots.AppendChild(positionX);
                                            spots.AppendChild(positionY);
                                            spots.AppendChild(type);
                                            spots.AppendChild(description);

                                            docNode.AppendChild(spots);
                                        }
                                        else
                                        {
                                            MessageBox.Show("Every hotspot must have a name and description.");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Some hotspots do not have information!");
                                        return;
                                    }
                                }
                            }
                 
                        }


                    }
                    doc.Save("Data/XMLFiles/" + imageName + "." + "xml");
                    parentWindow.Close();
                }

                

            }
            for (int k = 0; k < hotImagePaths.Count; k++)
            {

                String oldPath = hotImagePaths.ElementAt(k);

                String newName = hotImageNames.ElementAt(k);

                String newPath = "data/Hotspots/Images/" + newName;
                //Copy the image intothe Thumbnail and copy the folder into the deepzoom folder
                // Create the file and clean up handles.

                // Ensure that the target does not exist.
                if (oldPath != newPath)
                {
                    try
                    {
                        File.Delete(newPath);

                        // Copy the file.
                        File.Copy(oldPath, newPath);
                        this.createHotspotThumbnail(newPath, newName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("File is destroyed or does not exist!");
                        return;
                    }

                }
            }
            for (int l = 0; l < hotAudioPaths.Count; l++)
            {

                String oldPath = hotAudioPaths.ElementAt(l);

                String newName = hotAudioNames.ElementAt(l);

                
                String newPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\" + "Hotspots\\Audios\\" + newName;
               
                //Copy the image intothe Thumbnail and copy the folder into the deepzoom folder
                // Create the file and clean up handles.

                // Ensure that the target does not exist.
                if (oldPath != newPath)
                {
                    try
                    {
                        File.Delete(newPath);
                        File.Copy(oldPath, newPath);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("File is destroyed or does not exist!");
                        return;
                    }
                }
                // Copy the file.
                

            }
            for (int m = 0; m < hotVideoPaths.Count; m++)
            {

                String oldPath = hotVideoPaths.ElementAt(m);

                String newName = hotVideoNames.ElementAt(m);

                String newPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\" + "Hotspots\\Videos\\" + newName;
                //Copy the image intothe Thumbnail and copy the folder into the deepzoom folder
                // Create the file and clean up handles.

                // Ensure that the target does not exist.
                if (oldPath != newPath)
                {
                    try
                    {
                        File.Delete(newPath);

                        // Copy the file.
                        File.Copy(oldPath, newPath);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("File is destroyed or does not exist!");
                        return;
                    }
                }
            }
            this.cancel();
        }

       //Create thumbnails for hotspots images and save them
        public void createHotspotThumbnail(String imagePath, String filename)
        {
            
                string newPath = "data/Hotspots/Images/";

                System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);
                img = img.GetThumbnailImage(128, 128, null, new IntPtr());
                img.Save(newPath + "Thumbnail/" + filename);
            
            
        }
        /// <summary>
        /// Set the image
        /// </summary>
        /// <param name="image"></param>
        public void setImage(BitmapImage image) {
            curImage = image;
        }
      
       
        /// <summary>
        /// remove all hotspots created in the current session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.cancel();
            this.LoadHotspots();
            parentWindow.Close();
        }

        /// <summary>
        /// Called by the cancel_click function
        /// </summary>
        private void cancel() 
        {
            foreach (SurfaceRadioButton rb in radioButtons)
            {
                imageCover.Children.Remove(rb);
            }
        
            dic = new Dictionary<SurfaceRadioButton, String>();

            radioButtons = new List<SurfaceRadioButton>();
            dicPos = new Dictionary<SurfaceRadioButton, String>();
            buttonChecked = null;
            hotImageNames = new List<String>();
            hotImagePaths = new List<String>();
            hotAudioNames = new List<String>();
            hotAudioPaths = new List<String>();
            hotVideoNames = new List<String>();
            hotVideoPaths = new List<String>();
            addHotspotsEnabled = false;
            Edit.IsEnabled = false;
            AddText.IsEnabled = false;
            AddAudio.IsEnabled = false;
            AddImage.IsEnabled = false;
            AddVideo.IsEnabled = false;

        }


        public void setHotspotInfo(String info)
        {
            hotspotInfo = info;
        }

          
        /// <summary>
        /// See this.cancel()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            this.cancel();

        }

        private void AddAudio_Click(object sender, RoutedEventArgs e)
        {
            if (!newWindowIsOpened)
            {
                addHotspotsContent newContents = new addHotspotsContent();
                newContents.Show();
                newContents.hotspotContent = 1;
           
                newContents.setParentControl(this);
                newWindowIsOpened = true;
            }
        }
        //This opens the window for adding text to the hotspot
        private void AddText_Click(object sender, RoutedEventArgs e)
        {
            if (!newWindowIsOpened)
            {
                hotspotAddText newText = new hotspotAddText();
                newText.Show();
        
                newText.setParentControl(this);
                newWindowIsOpened = true;
            }
  
        }
        //This opens the window for adding image to the hotspot
        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            if (!newWindowIsOpened)
            {
                addHotspotsContent newContents = new addHotspotsContent();
                newContents.Show();
             
                newContents.setParentControl(this);
                newWindowIsOpened = true;
            }
        }
        //This opens the window for adding video to the hotspot
        private void AddVideo_Click(object sender, RoutedEventArgs e)
        {
            if (!newWindowIsOpened)
            {
                addHotspotsContent newContents = new addHotspotsContent();
                newContents.Show();
           
                newContents.setParentControl(this);
                newWindowIsOpened = true;
            }
        }
        //This opens the window for modifying text to the hotspot
        private void ModifyText(String caption, String description)
        {
            hotspotAddText newText = new hotspotAddText();

            newText.title.Text = caption;
            newText.Text.Text = description;
            newText.Show();
            newText.setParentControl(this);
        }

        //This opens the window for adding nontext to the hotspot
        private void ModifyNonText(int contentCatogory, String caption, String url)
        {
            addHotspotsContent newContents = new addHotspotsContent();

            newContents.title.Text = caption;
            newContents.url_tag.Text = url;
            newContents.Show();
            newContents.setParentControl(this);
            newContents.hotspotContent = contentCatogory;//need to set the contentPath as to display the correct URL info
        }


        private void Edit_Click(Object sender, RoutedEventArgs e)
        {
            String curSpotsInfo = dic[buttonChecked];
            String[] str = Regex.Split(curSpotsInfo, "/");
         
            if (str[1] == "text")
            {
                this.ModifyText(str[0],str[2]);
            }
            else if (str[1] == "audio")
            {
                this.ModifyNonText(1, str[0], str[2]);
            }
            else if (str[1] == "image")
            {
                this.ModifyNonText(2, str[0], str[2]);
            }
            else
            {
                this.ModifyNonText(3, str[0], str[2]);
            }
        }

        //This saves the hotspotInfo when the user enters the information
        public void saveHotspotInfo()
        {
        
            if (dic.ContainsKey(buttonChecked))
            {
                dic[buttonChecked] = hotspotInfo;
            }
            else
            {
                dic.Add(buttonChecked, hotspotInfo);
            }
        }
        /// <summary>
        /// Remove a hotspot from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void RemoveOne_Click(object sender, RoutedEventArgs e)
        {
         
            if (buttonChecked != null)
            {

                imageCover.Children.Remove(buttonChecked);
                radioButtons.Remove(buttonChecked);
                if (dic.ContainsKey(buttonChecked))
                {
                    String info = dic[buttonChecked];

                    String[] infos = Regex.Split(info, "/");
                    
                    if (infos[1] == "image")
                    {
                        hotImagePaths.Remove(infos[2]);
                    }
                    else if (infos[1] == "audio")
                    {
                        hotAudioPaths.Remove(infos[2]);
                    }
                    else if (infos[1] == "video")
                    {
                        hotVideoPaths.Remove(infos[2]);
                    }

                    dic.Remove(buttonChecked);
                   
                }
                dicPos.Remove(buttonChecked);
                 
                    AddText.IsEnabled = false;
                    AddImage.IsEnabled = false;
                    AddAudio.IsEnabled = false;
                    AddVideo.IsEnabled = false;
                    buttonChecked = null;
                    Edit.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Please select the hotspot you want to remove!");
            }
        }

        internal void setParentWindow(hotspotWindow hotspotWindow)
        {
            parentWindow = hotspotWindow;
        }

        private void image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
        
           
        }
    }
}
