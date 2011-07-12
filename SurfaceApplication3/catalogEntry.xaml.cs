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
using System.Xml;
using System.IO;
using System.Windows.Forms;
using DexterLib;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for catalogEntry.xaml
    /// </summary>
    public partial class catalogEntry : System.Windows.Controls.UserControl
    {
        public String imagePath;
        public String imageTitle;
        public String imageName;
        public MainWindow _newWindow;
        public ImageSource imageToDispose;
        private Helpers _helpers;
       

        public catalogEntry(MainWindow newWindow) //Would have to pass it a window so it can tell the window when to delete or add it
        {
            InitializeComponent();
            _newWindow = newWindow;
            _helpers = new Helpers();
        }

        public void setImagePath(String imPath) {
            imagePath = imPath;

        }
        public void setImageTitle(String title) {
            imageTitle = title;
        }
        public void setImageName(String name)
        {
            imageName = name;
        }

        /// <summary>
        /// Loads and opens the addImageWindow
        /// </summary>
        private void edit_Click(object sender, EventArgs e)
        {
            AddImageWindow newBigWindow = new AddImageWindow();
            newBigWindow.WindowState = System.Windows.WindowState.Normal;
            //newBigWindow.setWindowSize();
            newBigWindow.big_window1.browse.Visibility = Visibility.Hidden;
            newBigWindow.big_window1.createDZ.Visibility = Visibility.Hidden;
            newBigWindow.big_window1.setMainWindow(_newWindow); //pass the main window to the addImage Control
            //BitmapImage myBitmapImage = new BitmapImage();
            //myBitmapImage.BeginInit();
            //myBitmapImage.UriSource = new Uri(imagePath);
            //myBitmapImage.EndInit();

            

            Image wpfImage = new Image();
            FileStream stream = new FileStream(imagePath, FileMode.Open);
            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            stream.Close();

            //The image of the window is set according to its ratio of length and width
            Utils.setAspectRatio(newBigWindow.big_window1.imageCanvas, newBigWindow.big_window1.imageRec, newBigWindow.big_window1.image1, wpfImage, 7);
           
            //set image source
            newBigWindow.big_window1.image1.Source = wpfImage.Source;
            String dataDir = "Data/";
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
            // String dataDir = "C:\\Users\\MISATRAN\\Desktop\\LADS-new\\GCNav\\bin\\Debug\\Data\\";
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
                                String title = node.Attributes.GetNamedItem("title").InnerText;
                                if (imageTitle == title)
                                {
                                    String artist = node.Attributes.GetNamedItem("artist").InnerText;
                                    String year = node.Attributes.GetNamedItem("year").InnerText;
                                    String path = node.Attributes.GetNamedItem("path").InnerText;
                                    String medium = node.Attributes.GetNamedItem("medium").InnerText;
                                    
                                    //Loads the information about the imgae
                                    newBigWindow.big_window1.year_tag.Text = year;
                                    newBigWindow.big_window1.artist_tag.Text = artist;
                                    newBigWindow.big_window1.title_tag.Text = title;
                                    newBigWindow.big_window1.medium_tag.Text = medium;
                                    newBigWindow.big_window1.setImageName(path);
                                    newBigWindow.big_window1.setImagePath(dataUri + "Images\\" + "Thumbnail\\" + path);
                                    newBigWindow.big_window1.setImageProperty(true);
                                   
                                    if(node.Attributes.GetNamedItem("description")!=null){
                                        String summary = node.Attributes.GetNamedItem("description").InnerText;
                                        newBigWindow.big_window1.summary.Text = summary;
                                    }
                                    //Loads the keywords
                                    String keyword = "";
                                    foreach (XmlNode imgnode in node.ChildNodes)
                                    {
                                        if (imgnode.Name == "Keywords")
                                        {
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
                                            }
                                        }
                                        //Load metadatas if there is any
                                        if (imgnode.Name == "Metadata")
                                        {
                                            //newBigWindow.big_window1.MetaDataList.Items.RemoveAt(0);
                                            foreach (XmlNode meta in imgnode.ChildNodes)
                                            {
                                                if (meta.Name == "Group")
                                                { //This needs to specify group A,B,C or D
                                                    foreach (XmlNode file in meta)
                                                    {
                                                        String fileName = file.Attributes.GetNamedItem("Filename").InnerText; //this is to save the imageName
                                                        Console.WriteLine("FILE NAME " + fileName);
                                                        

                                                        //need to special case this for videos images and audio files
                                                        System.Windows.Controls.Image wpfImage2 = new System.Windows.Controls.Image();
                                                        _helpers = new Helpers();
                                                        if (_helpers.IsImageFile(fileName))
                                                        {
                                                            String fullPath = dataUri + "Images/" + "Metadata/" + fileName;
                                                            FileStream stream2 = new FileStream(fullPath, FileMode.Open);
                                                            System.Drawing.Image dImage2 = System.Drawing.Image.FromStream(stream2);
                                                            wpfImage2 = _helpers.ConvertDrawingImageToWPFImage(dImage2);
                                                            stream2.Close();

                                                            //set image source
                                                            MetaDataEntry newSmallwindow = new MetaDataEntry();
                                                            //Utils.setAspectRatio(newSmallwindow.imageCanvas, newSmallwindow.imageRec, newSmallwindow.image1, metaBitmapImage, 4);
                                                            newSmallwindow.image1.Source = wpfImage2.Source;
                                                            newSmallwindow.title_tag.Text = fileName;

                                                            if (file.Attributes.GetNamedItem("Name") != null)
                                                            {
                                                                String name = file.Attributes.GetNamedItem("Name").InnerText;
                                                                newSmallwindow.name_tag.Text = name;
                                                            }
                                                            if (file.Attributes.GetNamedItem("Description") != null)
                                                            {
                                                                String description = file.Attributes.GetNamedItem("Description").InnerText;
                                                                newSmallwindow.summary.Text = description;
                                                            }
                                                            newSmallwindow.tags.Text = keyword;
                                                            newBigWindow.big_window1.MetaDataList.Items.Add(newSmallwindow);
                                                            newSmallwindow.setBigWindow(newBigWindow.big_window1);
                                                        }
                                                        //loads videos
                                                        else if (_helpers.IsVideoFile(fileName))
                                                        {
                                                            String fullPath = dataUri + "Videos/" + "Metadata/" + fileName;
                                                            BitmapImage metaBitmapImage = new BitmapImage();
                                                            metaBitmapImage.BeginInit();
                                                            String thumbPath = fullPath;
                                                            Console.WriteLine("thumbpath" + thumbPath);
                                                            Console.WriteLine("fileName" + fileName);

                                                            int decrement = System.IO.Path.GetExtension(thumbPath).Length;
                                                            thumbPath = thumbPath.Remove(thumbPath.Length - decrement, decrement);

                                                            //thumbPath = thumbPath.Remove(thumbPath.Length - 4, 4);
                                                            thumbPath += ".bmp";
                                                            metaBitmapImage.UriSource = new Uri(thumbPath);
                                                            metaBitmapImage.EndInit();

                                                            //set image source
                                                            MetaDataEntry newSmallwindow = new MetaDataEntry();
                                                            //Utils.setAspectRatio(newSmallwindow.imageCanvas, newSmallwindow.imageRec, newSmallwindow.image1, metaBitmapImage, 4);

                                                            newSmallwindow.image1.Source = metaBitmapImage;
                                                            newSmallwindow.title_tag.Text = fileName;

                                                            if (file.Attributes.GetNamedItem("Name") != null)
                                                            {
                                                                String name = file.Attributes.GetNamedItem("Name").InnerText;
                                                                newSmallwindow.name_tag.Text = name;
                                                            }
                                                            if (file.Attributes.GetNamedItem("description") != null)
                                                            {
                                                                String description = file.Attributes.GetNamedItem("description").InnerText;
                                                                newSmallwindow.summary.Text = description;
                                                            }
                                                            newSmallwindow.tags.Text = keyword;
                                                            newBigWindow.big_window1.MetaDataList.Items.Add(newSmallwindow);
                                                            newSmallwindow.setBigWindow(newBigWindow.big_window1);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    newBigWindow.big_window1.tags.Text = keyword;
                                    }
                                }
                             }
                        }
                    }
                }
            if (newBigWindow.big_window1.MetaDataList.Items.Count == 0) {
                MetaDataEntry newSmallwindow = new MetaDataEntry();
                newBigWindow.big_window1.MetaDataList.Items.Add(newSmallwindow);
                newSmallwindow.setBigWindow(newBigWindow.big_window1);
            }
            newBigWindow.Show();
        }
        public void setImage(BitmapImage image)
        {
            imageToDispose = image;
           
        }
        public void removeFolders(String folder)
        {
            if (System.IO.Directory.Exists(folder))
            {
                String[] files = System.IO.Directory.GetFiles(folder);
                
                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    File.Delete(s);
                }
                String[] folders = Directory.GetDirectories(folder);
                Console.Out.WriteLine("folder1"+folders[0]);
                foreach (String smallFolder in folders)
                {
                    Directory.Delete(smallFolder,true);
                }
                Directory.Delete(folder);
            }
        
        }
        /// <summary>
        /// Remove the image from the XML file and the list in MainWindow
        /// </summary>
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            
            DialogResult  result = System.Windows.Forms.MessageBox.Show("Are you sure you want to remove this artwork" + " " + imageName +" " +"from the collection?","Remove the entry",MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                //imageToDispose.UriSource = null;
                _newWindow.EntryListBox.Items.Remove(this);
                //Would need to remove from the xml file as well
                XmlDocument doc = new XmlDocument();
                String dataDir = "Data/";
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
                                    String title = node.Attributes.GetNamedItem("title").InnerText;
                                    if (imageTitle == title)
                                    {
                                        docNode.RemoveChild(node);
                                        doc.Save(dataDir + "NewCollection.xml");
                                    }
                                }
                            }
                        }
                    }
                }

                String imageToRemove = "Data/Images/" + imageName;
                String thumbnaiToRemove = "Data/Images/Thumbnail/" + imageName;
                Console.Out.WriteLine("imageTitle" + imageName);
                if (File.Exists(imageToRemove))
                {
                    Console.Out.WriteLine("imageToRemove exists");
                    //image1 = new Image();

                    File.Delete(imageToRemove);
                    File.Delete(thumbnaiToRemove);
                }
                /*
                if (File.Exists(thumbnaiToRemove))
                {
                    Console.Out.WriteLine("thumbnail exists!");
                    try
                    {
                        File.Delete(thumbnaiToRemove);
                    }
                    catch (Exception imageBeingUsed) {
                        
                    }
                }
                */
                //Remove the deepzoom folder.
                String folderToRemove = "Data/Images/DeepZoom/" + imageName;
                
                this.removeFolders(folderToRemove);
                //Need to remove its hotspots
                if (File.Exists("Data/XMLFiles/" + imageName + "." + "xml"))
                {
                    Console.Out.WriteLine("hotspots exists");
                    File.Delete("Data/XMLFiles/" + imageName + "." + "xml");
                }
                //In case other files are using the same metadata, keep the metadata here.

            }
            else
            {
                return;
            }

        }
        ///// <summary>
        ///// The next two code blocks together check to see if file is an image
        ///// </summary>
        //static string[] imageExtensions = {
        //    ".BMP", ".JPG", ".GIF"
        //};

        //public bool IsImageFile(string filename)
        //{
        //    return -1 != Array.IndexOf(imageExtensions, System.IO.Path.GetExtension(filename).ToUpperInvariant());
        //}

        ///// <summary>
        ///// The next two code blocks together check to see if file is a video
        ///// </summary>
        //static string[] videoExtensions = {
        //                                      ".MOV", ".AVI"
        //    //".WMV", ".ASF", ".ASX", ".AVI", ".FLV",
        //    //".MOV", ".MP4", ".MPG", ".RM", ".SWF", ".VOB"
        //};

        //public bool IsVideoFile(string filename)
        //{
        //    return -1 != Array.IndexOf(videoExtensions, System.IO.Path.GetExtension(filename).ToUpperInvariant());
        //}
    }
}
