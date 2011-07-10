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
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.DeepZoomTools;


namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for big_window.xaml
    /// </summary>
    public partial class big_window : UserControl
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

        public big_window()
        {
            InitializeComponent();
            small_window newSmall = new small_window();
            newSmall.setBigWindow(this);
            MetaDataList.Items.Add(newSmall);
        }

        public void setImageName(String name)
        {
            imageName = name;

        }

        //Would pass the image to the map
        public String getImageName() {
            return imageName;
        }

        public void setImagePath(String path)
        {
            imagePath = path;
        }

        //To see if the image already exists in the artwork collection
        public void setImageProperty(Boolean property)
        {
            newOld = property;
        }

        private void small_window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        public void setUserControl(hotspotAdd hotspot)
        {
            control = hotspot;

        }
        public void setMapControl(map map)
        {
            mapControl = map;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = true;

            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"; //Is there a limit on type of metedata?

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] filePath = ofd.FileNames;
                string[] safeFilePath = ofd.SafeFileNames;

                for (int i = 0; i < safeFilePath.Length; i++)
                {
                    // FileInfo info = new FileInfo(safeFilePath[i]);
                    //Check what type of the metedata
                    //   String fileName = info.Name;

                    BitmapImage myBitmapImage = new BitmapImage();
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(@filePath[i]);
                    myBitmapImage.EndInit();

                    //set image source
                    image1.Source = myBitmapImage;
                    //control.showImage(myBitmapImage)
                    //Console.Out.WriteLine(safeFilePath[i]);
                    this.setImageName(safeFilePath[i]);
                    this.setImagePath(filePath[i]);
                    // this.setName(fileName);
                    // FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(filePath[i]);
                    // Console.Out.WriteLine(Environment.SystemDirectory);
                    // Console.Out.WriteLine(myFileVersionInfo.FileDescription);

                    //  summary.Text = "File description: " + myFileVersionInfo.Comments;
                    // Console.Out.WriteLine("Version" + myFileVersionInfo);

                }

            }

            // MeteDataList.Items.
            //String fileName = Path.GetFileName(ofd);



        }


        /**
        public void setName(String filename)
        {
            fileName = filename;

        }
        public String getName()
        {
            return fileName;
        }

        public void setAttribute(String attribute)
        {
            fileAttribute = attribute;
        }
        public String getAttribute()
        {
            return fileAttribute;
        }
        public void setTime(String time)
        {
            fileCreationTime = time;
        }
        public String getTime()
        {
            return fileCreationTime;
        }
        public void setArtist(String artist)
        {
            fileArtist = artist;
        }
        public String getArtist()
        {
            return fileArtist;
        }
        */
        public void setImage(BitmapImage importImage)
        {
            fileImage = importImage;

        }
        public BitmapImage getImage()
        {
            return fileImage;
        }




        /**
        private void import_multiple_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = true;

            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"; //Is there a limit on type of metedata?

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] filePath = ofd.FileNames;
                string[] safeFilePath = ofd.SafeFileNames;

                for (int i = 0; i < safeFilePath.Length; i++)
                {
                   // Console.Out.WriteLine(filePath[i]);
                   // Console.Out.WriteLine(safeFilePath[i]);

                    FileInfo info = new FileInfo(safeFilePath[i]);
                    
                    //Check what type of the metedata
                    String fileName = info.Name;
                    String time = "";
                    String attributes = "";
                    time += string.Format("{0}\n", info.CreationTime);
                    //attributes += string.Format("{0}\n", info.FileDescription);
                    //attributes += string.Format("{0}\n", info.Attributes);
                    this.setName(fileName);
                    this.setTime(time);
                    this.setAttribute(attributes);
                    
                    FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(filePath[i]);
                   // Console.Out.WriteLine(Environment.SystemDirectory);
                   // Console.Out.WriteLine(myFileVersionInfo.FileDescription);

                    summary.Text = "File description: " + myFileVersionInfo.Comments;
                    Console.Out.WriteLine("Version" + myFileVersionInfo);
                   // Console.Out.WriteLine("File Description" + myFileVersionInfo.FileDescription);
                   // Console.Out.WriteLine("File Description" + myFileVersionInfo.Comments);
                   // Console.Out.WriteLine("File Description" + myFileVersionInfo.FileVersion);

                    // Create source
                    BitmapImage myBitmapImage = new BitmapImage();
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(@filePath[i]);
                    myBitmapImage.EndInit();
                                       
                    //set image source
                    image1.Source = myBitmapImage;
                    control.showImage(myBitmapImage);
                    //set image of the hotspot window
                    
                }
               

                title_tag.Text = this.getName();
                year_tag.Text = this.getTime();
                
            }
           
        }
        */
        /*
        private void hot_spot_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Content.ToString();

            int index = content.IndexOf("Edit");

            if (index > -1)
            {


                string newContent = "Add Meatadata";
                hot_spot.Content = newContent;
                hot_spot.Width = 115;
                MeteDataList.Visibility = System.Windows.Visibility.Visible;

                string newContent1 = "Create Hotspots";
                Preview.Content = newContent1;
                Preview.Width = 115;

                string newContent2 = "Save Edits";
                Save_edit.Content = newContent2;


                //hot_spot.Content = 
            }
            else 
            {
                MeteDataList.Items.Add(new small_window());
            
            }         
        }

        */


        private void hotspot_Click(object sender, RoutedEventArgs e)
        {
            Console.Out.WriteLine("url"+imagePath);
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(imagePath);
            myBitmapImage.EndInit();
            control.setImage(myBitmapImage);
            control.setImagePath(imageName);
            control.showImage();
            control.LoadHotsptos();
            control.Show();

        }

        private void addMetadata_Click(object sender, RoutedEventArgs e)
        {
            small_window newSmallWindow = new small_window();

            MetaDataList.Items.Add(newSmallWindow);
            newSmallWindow.setBigWindow(this);

        }

        private void mapButton_Click(object sender, RoutedEventArgs e)
        {
            mapControl.loadPositions();
            mapControl.Show();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            //Write in Xml file
            String dataDir = "Data/";
            //String dataDir = "C://LADS-yc60/";
            //String dataDir = "E://";
            XmlDocument doc = new XmlDocument();
            doc.Load(dataDir + "NewCollection.xml");
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
                                        String title = node.Attributes.GetNamedItem("title").InnerText;
                                        String artist = node.Attributes.GetNamedItem("artist").InnerText;
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
                                                    if (tags.Text != keyword)
                                                    {
                                                        imgnode.RemoveAll();
                                                        //Split the keywords and write them into xml seperately
                                                        //keywd.Attributes.RemoveNamedItem();
                                                        String[] keywords = tags.Text.Split(new Char[] { ',' });
                                                        foreach (String kword in keywords)
                                                        {
                                                            XmlElement value = doc.CreateElement("Keyword");
                                                            value.SetAttribute("Value", "" + kword);
                                                            imgnode.AppendChild(value);
                                                            //XmlAttribute newAttr = doc.CreateAttribute("Keywords Value");
                                                            //keywd.Attributes.SetNamedItem(newAttr);
                                                            //imgnode.AppendChild()
                                                            //imgnode.SetAttribute("Keyword Value", kword);
                                                            //imgnode.AppendChild(elem);
                                                        }
                                                    }

                                                }
                                            }
                                            if (imgnode.Name == "Metadata") {
                                                node.RemoveChild(imgnode);
                                            
                                            }

                                        }
                                        XmlElement newMeta = doc.CreateElement("Metadata");
                                        XmlElement newGroup = doc.CreateElement("Group");
                                        newGroup.SetAttribute("name", "A"); //arbitrarily set the group as A
                                        foreach (small_window smallWindow in MetaDataList.Items) {
                                            XmlElement newFile = doc.CreateElement("Item");
                                            String name = smallWindow.title_tag.Text;
                                            newFile.SetAttribute("FileName", name);
                                            newGroup.AppendChild(newFile);
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
            else
            {
                if (doc.HasChildNodes)
                {
                    foreach (XmlNode docNode in doc.ChildNodes)
                    {
                        if (docNode.Name == "Collection")
                        {

                            XmlElement newEntry = doc.CreateElement("Image");
                            newEntry.SetAttribute("path", "" + imageName);
                            newEntry.SetAttribute("title", "" + title_tag.Text);
                            newEntry.SetAttribute("year", "" + year_tag.Text);
                            newEntry.SetAttribute("artist", "" + artist_tag.Text);
                            //doeesn't have medium included
                            newEntry.SetAttribute("medium", "oil");
                            newEntry.SetAttribute("description", "" + summary.Text);

                            XmlElement newKeyWords = doc.CreateElement("KeyWords");
                            String[] keywords = tags.Text.Split(new Char[] { ',' });
                            foreach (String kword in keywords)
                            {
                                XmlElement value = doc.CreateElement("Keyword");
                                value.SetAttribute("Value", "" + kword);
                                newKeyWords.AppendChild(value);

                            }
                            newEntry.AppendChild(newKeyWords);
                            

                            XmlElement newMeta = doc.CreateElement("Metadata");
                            XmlElement newGroup = doc.CreateElement("Group");
                            newGroup.SetAttribute("name", "A"); //arbitrarily set the group as A
                            foreach (small_window smallWindow in MetaDataList.Items)
                            {
                                XmlElement newFile = doc.CreateElement("Item");
                                String name = smallWindow.title_tag.Text;
                                newFile.SetAttribute("FileName", name);
                                newGroup.AppendChild(newFile);
                            }

                            newMeta.AppendChild(newGroup);
                            newEntry.AppendChild(newMeta);
                            docNode.AppendChild(newEntry);
                        }
                    }

                }
                string path = imagePath;
                String newPath = "data/Images/Thumbnail/" + imageName;
                String newPath2 = "data/Images/" + imageName;
                // String newPath = "C://LADS-yc60/data/Images/Thumbnail/" + imageName;//Should be changed
                //string newPath = "F://lads_data/Images/Thumbnail/" + imageName;//Should be changed
                string path2 = newPath;
                //Copy the image intothe Thumbnail and copy the folder into the deepzoom folder
                // Create the file and clean up handles.
                //using (FileStream fs = File.Create(path)) 

                // Ensure that the target does not exist.
                File.Delete(path2);
                File.Delete(newPath2);
                // Copy the file.
                File.Copy(path, path2);
                File.Copy(path, newPath2);


                //Copy the folders
                string[] imagePathComplete = Regex.Split(imagePath, imageName);
                String sourcePath1 = imagePathComplete[0];
                
                string sourcePath = sourcePath1;
                string targetPath = "data/Images/Deepzoom/" + imageName;
                //string targetPath = "C://LADS-yc60/data/Images/DeepZoom/" + imageName;
                // string targetPath = "F://lads_data/Images/DeepZoom/" + imageName;

               
                this.copyFolder(sourcePath, targetPath);
             
            }
            doc.Save(dataDir + "NewCollection.xml");
        }


        public void copyFolder(String sourceFolder, String desFolder) { 
        
        
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
                    Console.Out.WriteLine(sourceFolder);
                    //Console.Out.WriteLine(folders[0]);
                    foreach (String folder in folders)
                    {
                        String name = System.IO.Path.GetFileName(folder);
                        String dest = System.IO.Path.Combine(desFolder, name);
                        this.copyFolder(folder, dest);
                    }
                }
        
        }

        private void createDZ_Click(object sender, RoutedEventArgs e)
        {
            String[] strings = Regex.Split(imagePath, imageName);
            String srcFolderPath = strings[0];
            String destFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\DeepZoom";
            ImageCreator ic = new ImageCreator();

            ic.TileFormat = ImageFormat.Jpg;
            ic.TileOverlap = 1;
            ic.TileSize = 256;
            ic.ImageQuality = 0.92;
            Directory.CreateDirectory(destFolderPath + "\\" + imageName);

            string target = destFolderPath + "\\" + imageName + "\\dz.xml";

            ic.Create(imagePath, target);
            MessageBox.Show("Create Deep Zoom image successfully!");


        }



    }
}

      
    /*
        private void Preview_Click(object sender, RoutedEventArgs e)
       { 
            string content = (sender as Button).Content.ToString();

            int index = content.IndexOf("Create");

            if (index > -1)
            {
                control.Show();

            }
            else {
                this.knowledgeWeb(); //when clicking the preview button
            }
        }*/
     

   


