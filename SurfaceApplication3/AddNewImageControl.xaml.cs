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
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.DeepZoomTools;
using System.Threading;
using System.ComponentModel;
using DexterLib;
using System.IO;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for AddNewImageControl.xaml
    /// </summary>
    public partial class AddNewImageControl : UserControl
    {
        public String file;
        public String[] tag;
        public String fileCreationTime;
        public BitmapImage fileImage;
        public hotspotAdd control;
        public map mapControl;
        public string imageName;
        public string imagePath;
        public Boolean newOld;
        private BackgroundWorker progressBarWorker;
        private Boolean DZcreated;
        public MainWindow mainWindow;
        private mapWindow newMapWindow;
        private Helpers _helpers;
        private List<string> imagesToDelete;
        private hotspotWindow newHotspotWindw;
        public bool mapWinOpened, hotspotWinOpened;
        private int catalogNumber;
        public bool imageSaved;
        /// <summary>
        /// Default constructor
        /// </summary>
        public AddNewImageControl()
        {
            InitializeComponent();
            DZcreated = false;
            imageName = "";
            // Create a Background Worker
            progressBarWorker = new BackgroundWorker();

            // Enable support for cancellation
            progressBarWorker.WorkerSupportsCancellation = true;

            progressBarWorker.DoWork += new DoWorkEventHandler(progressBarWorker_DoWork);
            progressBarWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(progressBarWorker_RunWorkerCompleted);

            //year_tag.MaxLength = 4; //set the limits on the year input
            _helpers = new Helpers();
            imagesToDelete = new List<string>();
            mapWinOpened = false;
            hotspotWinOpened = false;
            imageSaved = false; ;
        }

        /// <summary>
        /// Set the image name
        /// </summary>
        public void setImageName(String name)
        {
            imageName = name;

        }

        //Would pass the image to the map
        public String getImageName()
        {
            return imageName;
        }

        /// <summary>
        /// Set the absolute path to the image
        /// </summary>
        public void setImagePath(String path)
        {
            imagePath = path;
        }

        public void addImagesToDelete(String path)
        {
            imagesToDelete.Add(path);
        }
        /// <summary>
        /// To see if the image already exists in the artwork collection
        /// </summary>
        public void setImageProperty(Boolean property)
        {
            newOld = property;
        }


        private void small_window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Register the map control associated with this control
        /// </summary>
        public void setMapControl(map map)
        {
            mapControl = map;
        }

        public void setMapWindow(mapWindow window)
        {
            newMapWindow = window;
        }
        public void setHotspotWindow(hotspotWindow window)
        {
            newHotspotWindw = window;
        }
        /// <summary>
        /// When 'Browse' button for Main Artwork is clicked, open a 'file open' dialog for user to open an image file.
        /// </summary>
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = false;

            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.TIFF;*.TIF;*.JPEG;*.PNG)|*.BMP;*.JPG;*.GIF;*.TIFF;*.TIF;*.JPEG;*.PNG|All files(*.*)|*.*";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                string[] filePath = ofd.FileNames;
                string[] safeFilePath = ofd.SafeFileNames;

                for (int i = 0; i < safeFilePath.Length; i++)
                {
                    // FileInfo info = new FileInfo(safeFilePath[i]);
                    //Check what type of the metedata
                    //   String fileName = info.Name;
                    image1.Width = 186;
                    image1.Height = 136;
                    imageRec.Width = 200;
                    imageRec.Height = 150;
                    Canvas.SetTop(image1, 7);
                    Canvas.SetTop(imageRec, 0);
                    Canvas.SetLeft(image1, 7);
                    Canvas.SetLeft(imageRec, 0);
                    //BitmapImage myBitmapImage = new BitmapImage();
                    System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
                    
                    try
                    {

                        System.Drawing.Image img = System.Drawing.Image.FromFile(@filePath[i]);
                        
                        img = img.GetThumbnailImage(128, 128, null, new IntPtr());
                        String tempname = System.IO.Path.GetTempFileName();
                        img.Save(tempname);
                        img.Dispose();
                    
                        FileStream fstream = new FileStream(tempname, FileMode.Open);
                        System.Drawing.Image dImage = System.Drawing.Image.FromStream(fstream);
                        wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
                        fstream.Close();
                        File.Delete(tempname);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("The image file is broken or not valid");
                        return;
                    }

                    /*
                    try
                    {
                        FileStream fstream = new FileStream(@filePath[i], FileMode.Open);
                        System.Drawing.Image dImage = System.Drawing.Image.FromStream(fstream);
                        wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
                        fstream.Close();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("The image file is broken or not valid");
                        return;
                    }*/


                    Utils.setAspectRatio(imageCanvas, imageRec, image1, wpfImage, 7);

                    //set image source
                    image1.Source = wpfImage.Source;
                    string filename = safeFilePath[i];
                    string extension = System.IO.Path.GetExtension(safeFilePath[i]);
                    string tempFileName = System.IO.Path.GetFileNameWithoutExtension(safeFilePath[i]);
                    Random random = new Random();
                    int randomNumber = random.Next(0, 100000000);
                    filename = tempFileName + randomNumber + extension;
                    while (File.Exists("Data/Images/Metadata/" + filename))
                    {
                        randomNumber = random.Next(0, 100000000);
                        filename = tempFileName + randomNumber + extension;
                    }
                    this.setImageName(filename);
                    this.setImagePath(filePath[i]);

                    title_tag.Text = "";
                    year_tag.Text = "";
                    medium_tag.Text = "";
                    artist_tag.Text = "";
                    tags.Text = "";
                    summary.Text = "";
                    MetaDataList.Items.Clear();
                    MetaDataEntry newSmall = new MetaDataEntry();
                    newSmall.setBigWindow(this);
                    MetaDataList.Items.Add(newSmall);
                }
            }
        }


        //for image assets

        //used when an item is added to the dock at the bottom.
        public void createMetaThumbnail(MetaDataEntry mde)
        {
            string filename = mde.title_tag.Text;
            string path = mde.metaImagePath;

            if (_helpers.IsVideoFile(path))
            {
                string newPath = "data/Videos/Metadata/";
                string fBitmapName = path;

                int decrement = System.IO.Path.GetExtension(fBitmapName).Length;
                fBitmapName = fBitmapName.Remove(fBitmapName.Length - decrement, decrement);
                fBitmapName += ".bmp";

                System.Drawing.Image img = System.Drawing.Image.FromFile(fBitmapName);
                int decrement2 = System.IO.Path.GetExtension(filename).Length;
                filename = filename.Remove(filename.Length - decrement2, decrement2);

                filename += ".bmp";
                img.Save(newPath + filename);

            }
            else if (_helpers.IsImageFile(path))
            {
                string newPath = "data/Images/Metadata/";

                System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                img = img.GetThumbnailImage(128, 128, null, new IntPtr());
                img.Save(newPath + "Thumbnail/" + filename);
                img.Dispose();
                Console.WriteLine(newPath + "Thumbnail/" + filename + " IMAGE!!");
            }
        }

        public void setImage(BitmapImage importImage)
        {
            fileImage = importImage;

        }
        public BitmapImage getImage()
        {
            return fileImage;
        }

        /// <summary>
        /// Open a control to handle metadata processing.
        /// </summary>
        private void addMetadata_Click(object sender, RoutedEventArgs e)
        {
            MetaDataEntry newSmallWindow = new MetaDataEntry();
            MetaDataList.Items.Add(newSmallWindow);
            newSmallWindow.setBigWindow(this);
            Console.WriteLine("New Asset Type: "+newSmallWindow.getType());
        }

        /// <summary>
        /// Open a control to handle map hotspot processing
        /// </summary>
        private void mapButton_Click(object sender, RoutedEventArgs e)
        {
            if (!mapWinOpened)
            {
                newMapWindow.mapControl.setBigWindow(this);
                newMapWindow.Show();

                newMapWindow.mapControl.showMap();
                newMapWindow.mapControl.loadPositions();
                mapWinOpened = true;
            }
            else
            {
                newMapWindow = new mapWindow();
                newMapWindow.mapControl.setBigWindow(this);
                newMapWindow.Show();

                newMapWindow.mapControl.showMap();
                newMapWindow.mapControl.loadPositions();
            }
        }

        private void hotspot_Click(object sender, RoutedEventArgs e)
        {
            if (!hotspotWinOpened)
            {
                newHotspotWindw.Show();
                newHotspotWindw.hotspot.setImagePath(imageName);
                newHotspotWindw.hotspot.showImage();
                newHotspotWindw.hotspot.LoadHotspots();
                hotspotWinOpened = true;
            }
            else
            {
                newHotspotWindw = new hotspotWindow();
                newHotspotWindw.hotspot.setImagePath(imageName);

                newHotspotWindw.Show();
                newHotspotWindw.hotspot.showImage();
                newHotspotWindw.hotspot.LoadHotspots();

            }
        }
        /// <summary>
        /// Save the image to the XML file and create a deep zoom version of the image.
        /// </summary>
        private void save_Click(object sender, RoutedEventArgs e)
        {
            imageSaved = true;
            foreach (string name in imagesToDelete)
            {
                if(File.Exists("Data/Images/Metadata/" + name))
                {
                    File.Delete("Data/Images/Metadata/" + name);
                }
                if (File.Exists("Data/Images/Metadata/Thumbnail/" + name))
                {
                    File.Delete("Data/Images/Metadata/Thumbnail/" + name);
                }
            }
            hotspot.IsEnabled = true;
            mapButton.IsEnabled = true;
            //Write in Xml file
            String dataDir = "Data/";
            XmlDocument doc = new XmlDocument();
            doc.Load(dataDir + "NewCollection.xml");
            //If it's an old image
            if (newOld == true)
            {
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

                                    if (this.imageName == path)
                                    {
                                        int yearInput = 0;
                                        try
                                        {
                                            yearInput = Convert.ToInt32(year_tag.Text);
                                        }
                                        catch (Exception exc)
                                        {
                                            MessageBox.Show("The year must be a valid number.");
                                            return;
                                        }
                                        if (yearInput < -9999 || yearInput > 9999)
                                        {
                                            MessageBox.Show("Year must be between -9999 and 9999.");
                                            return;
                                        }

                                        String title = node.Attributes.GetNamedItem("title").InnerText;
                                        String artist = node.Attributes.GetNamedItem("artist").InnerText;
                                        String medium = node.Attributes.GetNamedItem("medium").InnerText;
                                        String year = node.Attributes.GetNamedItem("year").InnerText;

                                        if (title_tag.Text != title)
                                        {
                                            node.Attributes.GetNamedItem("title").InnerText = title_tag.Text;
                                        }
                                        if (year_tag.Text != year)
                                        {
                                            node.Attributes.GetNamedItem("year").InnerText = year_tag.Text;
                                        }
                                        if (artist_tag.Text != artist)
                                        {
                                            node.Attributes.GetNamedItem("artist").InnerText = artist_tag.Text;
                                        }
                                        if (medium_tag.Text != medium)
                                        {
                                            node.Attributes.GetNamedItem("medium").InnerText = medium_tag.Text;
                                        }
                                        if (node.Attributes.GetNamedItem("description") == null)
                                        {
                                            XmlAttribute newAttr = doc.CreateAttribute("description");
                                            newAttr.Value = "" + summary.Text;
                                            node.Attributes.SetNamedItem(newAttr);
                                        }
                                        else
                                        {
                                            if (node.Attributes.GetNamedItem("description").InnerText != summary.Text)
                                            {
                                                node.Attributes.GetNamedItem("description").InnerText = summary.Text;
                                            }
                                        }
                                        String keyword = "";
                                        int metaDataCount = 0;
                                        bool hasKeywords = false;
                                        foreach (XmlNode imgnode in node.ChildNodes)
                                        {
                                            if (imgnode.Name == "Keywords")
                                            {
                                                hasKeywords = true;
                                                foreach (XmlNode keywd in imgnode.ChildNodes)
                                                {
                                                    if (keywd.Name == "Keyword")
                                                    {
                                                        if (keyword == "")
                                                        {
                                                            keyword = keyword + keywd.Attributes.GetNamedItem("Value").InnerText;
                                                        }
                                                        else
                                                        {
                                                            keyword = keyword + "," + keywd.Attributes.GetNamedItem("Value").InnerText;
                                                        }
                                                    }
                                                    if (tags.Text != keyword)
                                                    {
                                                        imgnode.RemoveAll();
                                                        //Split the keywords and write them into xml seperately
                                                        String[] keywords = tags.Text.Split(new Char[] { ',' });
                                                        foreach (String kword in keywords)
                                                        {
                                                            XmlElement value = doc.CreateElement("Keyword");
                                                            value.SetAttribute("Value", "" + kword);
                                                            imgnode.AppendChild(value);
                                                        }
                                                    }

                                                }
                                            }

                                            if (imgnode.Name == "Metadata")
                                            {
                                                node.RemoveChild(imgnode);
                                            }
                                           


                                        }
                                        if (!hasKeywords)
                                        {
                                            XmlElement newKeywords = doc.CreateElement("Keywords");
                                            node.AppendChild(newKeywords);
                                            String[] keywords = tags.Text.Split(new Char[] { ',' });
                                            foreach (String kword in keywords)
                                            {
                                                XmlElement value = doc.CreateElement("Keyword");
                                                value.SetAttribute("Value", "" + kword);
                                                newKeywords.AppendChild(value);
                                            }
                                        }
                                        if (MetaDataList.Items.Count != 0)
                                        {
                                            MetaDataEntry newSmall = (MetaDataEntry)MetaDataList.Items.GetItemAt(0);
                                            if (newSmall.title_tag.Text != "")
                                            {
                                                XmlElement newMeta = doc.CreateElement("Metadata");
                                                XmlElement newGroup = doc.CreateElement("Group");
                                                newGroup.SetAttribute("name", "A"); //arbitrarily set the group as A
                                                foreach (MetaDataEntry smallWindow in MetaDataList.Items)
                                                {
                                                    if (smallWindow.title_tag.Text != "")
                                                    {
                                                        XmlElement newFile = doc.CreateElement("Item");
                                                        String fileName = smallWindow.title_tag.Text;
                                                        newFile.SetAttribute("Filename", fileName);
                                                        if (smallWindow.name_tag.Text != "")
                                                        {
                                                            String name = smallWindow.name_tag.Text;
                                                            newFile.SetAttribute("Name", name);
                                                        }
                                                        if (smallWindow.summary.Text != "")
                                                        {
                                                            String description = smallWindow.summary.Text;
                                                            newFile.SetAttribute("Description", description);
                                                        }
                                                        newFile.SetAttribute("Type", smallWindow.getType());
                                                        if (smallWindow.getType() == "Web")
                                                        {
                                                            newFile.SetAttribute("URL", smallWindow.getURL());
                                                        }

                                                        newGroup.AppendChild(newFile);

                                                        if (smallWindow.metaImagePath != "")
                                                        {
                                                            String oldPath = smallWindow.metaImagePath;
                                                            if (_helpers.IsImageFile(oldPath))
                                                            {
                                                                String newPath = "data/Images/Metadata/" + smallWindow.title_tag.Text;
                                                                String path2 = newPath;
                                                                // Ensure that the target does not exist.
                                                                   
                                                                    try
                                                                    {
                                                                        File.Delete(path2);
                                                                        createMetaThumbnail(smallWindow);
                                                                        Console.WriteLine("oldPath: " + oldPath + " path2: " + path2);
                                                                        // Copy the file.
                                                                        File.Copy(oldPath, path2);
                                                                    }
                                                                    catch (Exception exc) { }
                                                            }
                                                            else if (_helpers.IsVideoFile(oldPath))
                                                            {
                                                                String newPath = "data/Videos/Metadata/" + smallWindow.title_tag.Text;
                                                                String path2 = newPath;
                                                                Console.WriteLine("Video File save: " + newPath);
                                                                // Ensure that the target does not exist.
                                                                try
                                                                {
                                                                    File.Delete(path2);
                                                                    createMetaThumbnail(smallWindow);
                                                                    // Copy the file.
                                                                    File.Copy(oldPath, path2);
                                                                }
                                                                catch (Exception exc)
                                                                {
                                                                    Console.WriteLine("CATCH!!");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                newMeta.AppendChild(newGroup);
                                                node.AppendChild(newMeta);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //If it's a new image
            else
            {
                Boolean imageNameExist = false;
                if (imageName != "" && year_tag.Text != "" && title_tag.Text != "")
                {
                    newOld = false;
                    if (doc.HasChildNodes)
                    {
                        foreach (XmlNode docNode in doc.ChildNodes)
                        {
                            if (docNode.Name == "Collection")
                            {
                                if (docNode.HasChildNodes)
                                {
                                    foreach (XmlNode imageNode in docNode.ChildNodes)
                                    {
                                        if (imageNode.Name == "Image")
                                        {
                                            String oldImageName = imageNode.Attributes.GetNamedItem("path").InnerText;

                                            if (oldImageName == imageName)
                                            {
                                                imageNameExist = true;
                                            }
                                        }
                                    }
                                }
                                if (!imageNameExist)
                                {
                                    int yearInput = 0;
                                    try
                                    {
                                        yearInput = Convert.ToInt32(year_tag.Text);
                                    }
                                    catch (Exception exc)
                                    {
                                        MessageBox.Show("The year must be a valid number.");
                                        return;
                                    }
                                    if (yearInput < -9999 || yearInput > 9999)
                                    {
                                        MessageBox.Show("Year must be between -9999 and 9999.");
                                        return;
                                    }
                                    XmlElement newEntry = doc.CreateElement("Image");
                                    docNode.AppendChild(newEntry);
                                    newEntry.SetAttribute("path", "" + imageName);
                                    newEntry.SetAttribute("title", "" + title_tag.Text);
                                    newEntry.SetAttribute("year", "" + year_tag.Text);
                                    newEntry.SetAttribute("artist", "" + artist_tag.Text);
                                    newEntry.SetAttribute("medium", "" + medium_tag.Text);
                                    newEntry.SetAttribute("description", "" + summary.Text);
                                    if (tags.Text != "")
                                    {
                                        XmlElement newKeyWords = doc.CreateElement("Keywords");
                                        String[] keywords = tags.Text.Split(new Char[] { ',' });
                                        foreach (String kword in keywords)
                                        {
                                            XmlElement value = doc.CreateElement("Keyword");
                                            value.SetAttribute("Value", "" + kword);
                                            newKeyWords.AppendChild(value);

                                        }
                                        newEntry.AppendChild(newKeyWords);
                                    }
                                    if (MetaDataList.Items.Count != 0)
                                    {

                                        MetaDataEntry newSmall = (MetaDataEntry)MetaDataList.Items.GetItemAt(0);
                                        if (newSmall.title_tag.Text != "")
                                        {
                                            XmlElement newMeta = doc.CreateElement("Metadata");
                                            XmlElement newGroup = doc.CreateElement("Group");
                                            newGroup.SetAttribute("name", "A"); //arbitrarily set the group as A
                                            foreach (MetaDataEntry smallWindow in MetaDataList.Items)
                                            {
                                                if (smallWindow.title_tag.Text != "")
                                                {
                                                    XmlElement newFile = doc.CreateElement("Item");
                                                    String fileName = smallWindow.title_tag.Text;
                                                    newFile.SetAttribute("Filename", fileName);
                                                    if (newSmall.name_tag.Text != "")
                                                    {
                                                        String name = newSmall.name_tag.Text;
                                                        newFile.SetAttribute("Name", name);
                                                    }
                                                    if (newSmall.summary.Text != "")
                                                    {
                                                        String description = newSmall.summary.Text;
                                                        newFile.SetAttribute("Description", description);
                                                    }
                                                    newFile.SetAttribute("Type", newSmall.getType());
                                                    if (newSmall.getType() == "Web")
                                                    {
                                                        newFile.SetAttribute("URL", newSmall.getURL());
                                                    }

                                                    newGroup.AppendChild(newFile);
                                                    newGroup.AppendChild(newFile);
                                                    if (smallWindow.metaImagePath != "")
                                                    {
                                                        String oldMetaPath = smallWindow.metaImagePath;
                                                        if (_helpers.IsImageFile(oldMetaPath))
                                                        {
                                                            String newMetaPath = "data/Images/Metadata/" + smallWindow.title_tag.Text;

                                                            // Ensure that the target does not exist.
                                                            try
                                                            {
                                                                File.Delete(newMetaPath);

                                                                // Copy the file.
                                                                File.Copy(oldMetaPath, newMetaPath);
                                                            }
                                                            catch (Exception exception)
                                                            { }


                                                        }
                                                        else if (_helpers.IsVideoFile(oldMetaPath))
                                                        {
                                                            String newMetaPath = "data/Videos/Metadata/" + smallWindow.title_tag.Text;

                                                            // Ensure that the target does not exist.
                                                            try
                                                            {
                                                                File.Delete(newMetaPath);

                                                                // Copy the file.
                                                                File.Copy(oldMetaPath, newMetaPath);
                                                            }
                                                            catch (Exception e2)
                                                            {
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            newMeta.AppendChild(newGroup);
                                            newEntry.AppendChild(newMeta);
                                        }
                                    }
                                    mainWindow.addOneArtworkToCatalog();

                                    string path = imagePath;
                                    String newPath = "data/Images/Thumbnail/" + imageName;
                                    String newPath2 = "data/Images/" + imageName;

                                    string path2 = newPath;
                                    if (path == path2) {
                                        MessageBox.Show("The image already exists");
                                        return;
                                    }
                                    else
                                    {
                                        // Ensure that the target does not exist.
                                        try
                                        {
                                            File.Delete(path2);
                                            File.Delete(newPath2); //Going to delete the web image?
                                            // Copy the file.

                                            System.Drawing.Image img = Helpers.getThumbnail(path,800);
                                            img.Save(path2);
                                            img.Save(newPath2);
                                            img.Dispose();

                                        }
                                        catch (Exception ee)
                                        { }
                                    }


                                    statusLabel.Foreground = Brushes.Black;
                                    statusLabel.Content = "Saved to catalog, converting to Deep Zoom format...";

                                    // Create Deep Zoom Image:

                                    loadingProgressBar.IsIndeterminate = true;
                                    loadingProgressBar.Background = Brushes.WhiteSmoke;
                                    loadingProgressBar.Foreground = Brushes.LawnGreen;
                                    progressBarWorker.RunWorkerAsync();

                                    title_tag.BorderBrush = Brushes.DarkGreen;
                                    year_tag.BorderBrush = Brushes.DarkGreen;
                                    artist_tag.BorderBrush = Brushes.DarkGreen;
                                    medium_tag.BorderBrush = Brushes.DarkGreen;
                                    newOld = true;

                                    
                                }
                                else
                                {
                                    MessageBox.Show("The image already exists in the catalog");
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    statusLabel.Foreground = Brushes.DarkRed;
                    statusLabel.Content = "Some items are still missing:";
                    if (imageName == "")
                    {
                        imageRec.Stroke = Brushes.DarkRed;
                        statusLabel.Content += " Image;";
                    }
                    if (title_tag.Text == "")
                    {
                        title_tag.BorderBrush = Brushes.DarkRed;
                        statusLabel.Content += " Title;";
                    }
                    else
                    {
                        title_tag.BorderBrush = Brushes.DarkGreen;
                    }

                    if (year_tag.Text == "")
                    {
                        year_tag.BorderBrush = Brushes.DarkRed;
                        statusLabel.Content += " Year;";
                    }
                    else
                    {
                        year_tag.BorderBrush = Brushes.DarkGreen;
                    }
                  
                }
            }


            doc.Save(dataDir + "NewCollection.xml");
        }

        /// <summary>
        ///Copy files and folders in a directory to a new place
        /// </summary>
        public void copyFolder(String sourceFolder, String desFolder)
        {
            if (!System.IO.Directory.Exists(desFolder))
            {
                System.IO.Directory.CreateDirectory(desFolder);
            }

            // To copy all the files in one directory to another directory.
            // Get the files in the source folder. 

            if (System.IO.Directory.Exists(sourceFolder))
            {
                string[] files = System.IO.Directory.GetFiles(sourceFolder);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    String name = System.IO.Path.GetFileName(s);
                    String desFile = System.IO.Path.Combine(desFolder, name);
                    System.IO.File.Copy(s, desFile, true);
                }
                string[] folders = Directory.GetDirectories(sourceFolder);
                foreach (String folder in folders)
                {
                    String name = System.IO.Path.GetFileName(folder);
                    String dest = System.IO.Path.Combine(desFolder, name);
                    this.copyFolder(folder, dest);
                }
            }

        }

        /// <summary>
        /// Not Used
        /// </summary>
        private void createDZ_Click(object sender, RoutedEventArgs e)
        {
            loadingProgressBar.IsIndeterminate = true;
            loadingProgressBar.Background = Brushes.WhiteSmoke;
            loadingProgressBar.Foreground = Brushes.LawnGreen;
            progressBarWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Called when finishing creating deep zoom images to chang the state of the progress bar.
        /// </summary>
        private void progressBarWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Arrow;

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            // Reset the ProgressBar's IsInderterminate
            // property to false to stop the progress indicator
            loadingProgressBar.IsIndeterminate = false;
            statusLabel.Content = "Done.";
            DZcreated = true;
        }

        /// <summary>
        /// Create deep zoom image here
        /// </summary>
        private void progressBarWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            String[] strings = Regex.Split(imagePath, imageName);
            String srcFolderPath = strings[0];
            String destFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\DeepZoom";
            
            ImageCreator ic = new ImageCreator();

            ic.TileFormat = ImageFormat.Jpg;
            ic.TileOverlap = 1;
            ic.TileSize = 256;
            ic.ImageQuality = 0.92;
            ic.UseOptimizations = true;
            Directory.CreateDirectory(destFolderPath + "\\" + imageName);

            string target = destFolderPath + "\\" + imageName + "\\dz.xml";

            ic.Create(imagePath, target);
            ic = null;
            System.GC.Collect();
            System.GC.Collect();
            Thread.Sleep(0);

        }

        /// <summary>
        /// Close the window
        /// </summary>
        private void close_Click(object sender, RoutedEventArgs e)
        {
            Canvas par = (Canvas)this.Parent;
            AddImageWindow parr = (AddImageWindow)par.Parent;
            //Need to clear all the entries in the existing catalog first and then reload
            int p = 0;
            if (imageSaved)
            {
                this.loadOneArtwork();
            }
            parr.Close();

        }


        public void setCatalogNumber(int number)
        {
            catalogNumber = number;
        }

        public void setMainWindow(MainWindow main)
        {
            mainWindow = main;
        }

        public void loadOneArtwork()
        {
            catalogEntry entryToModify = (catalogEntry)mainWindow.EntryListBox.Items.GetItemAt(catalogNumber);
            entryToModify.title_tag.Text = title_tag.Text;
            entryToModify.year_tag.Text = year_tag.Text;
            entryToModify.medium_tag.Text = medium_tag.Text;
            entryToModify.artist_tag.Text = artist_tag.Text;
            entryToModify.summary.Text = summary.Text;

                String dataDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
                String fullPath = dataDir + "Images\\" + "Thumbnail\\" + imageName;

                System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
                FileStream stream = new FileStream(fullPath, FileMode.Open);
                System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
                wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
                stream.Close();
                Utils.setAspectRatio(entryToModify.imageCanvas, entryToModify.imageRec, entryToModify.image1, wpfImage, 4);
                entryToModify.image1.Source = wpfImage.Source;
                entryToModify.setImagePath(fullPath);
                entryToModify.setImageTitle(title_tag.Text);
                entryToModify.setImageName(imageName);
        }

        private void tagsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int cursorPosition = tags.SelectionStart;
            int nextSpace = tags.Text.IndexOf(' ', cursorPosition);
            int selectionStart = 0;
            string trimmedString = string.Empty;
            if (nextSpace != -1)
            {
                trimmedString = tags.Text.Substring(0, nextSpace);
            }
            else
            {
                trimmedString = tags.Text;
            }


            if (trimmedString.LastIndexOf(' ') != -1)
            {
                selectionStart = 1 + trimmedString.LastIndexOf(' ');
                trimmedString = trimmedString.Substring(1 + trimmedString.LastIndexOf(' '));
            }

            tags.SelectionStart = selectionStart;
            tags.SelectionLength = trimmedString.Length;


        }
        private void summaryMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int cursorPosition = summary.SelectionStart;
            int nextSpace = summary.Text.IndexOf(' ', cursorPosition);
            int selectionStart = 0;
            string trimmedString = string.Empty;
            if (nextSpace != -1)
            {
                trimmedString = summary.Text.Substring(0, nextSpace);
            }
            else
            {
                trimmedString = summary.Text;
            }


            if (trimmedString.LastIndexOf(' ') != -1)
            {
                selectionStart = 1 + trimmedString.LastIndexOf(' ');
                trimmedString = trimmedString.Substring(1 + trimmedString.LastIndexOf(' '));
            }

            summary.SelectionStart = selectionStart;
            summary.SelectionLength = trimmedString.Length;


        }

    }
}





