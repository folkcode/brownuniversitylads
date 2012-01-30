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

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for catalogEntry.xaml
    /// </summary>
    public partial class catalogEntry : UserControl
    {
        public String imagePath;
        public String imageTitle;
        public newImageWindow _newWindow;

        public catalogEntry(newImageWindow newWindow) //Would have to pass it a window so it can tell the window when to delete or add it
        {
            InitializeComponent();
            _newWindow = newWindow;
        }

        public void setImagePath(String imPath) {
            imagePath = imPath;

        }
        public void setImageTitle(String title) {
            imageTitle = title;
        }
       
        private void edit_Click(object sender, RoutedEventArgs e)
        {
            SurfaceWindow1 newBigWindow = new SurfaceWindow1();
            newBigWindow.big_window1.browse.Visibility = Visibility.Hidden;
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(imagePath);
            myBitmapImage.EndInit();

            //set image source
            newBigWindow.big_window1.image1.Source = myBitmapImage;
            String dataDir = "Data/";
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
           //  String dataDir = "C://LADS-yc60/data/"; 
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

                                    
                                    newBigWindow.big_window1.year_tag.Text = year;
                                    newBigWindow.big_window1.artist_tag.Text = artist;
                                    newBigWindow.big_window1.title_tag.Text = title;
                                    newBigWindow.big_window1.setImageName(path);
                                    newBigWindow.big_window1.setImagePath(dataUri + "Images\\" + "Thumbnail\\" + path);
                                    newBigWindow.big_window1.setImageProperty(true);

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
                                        if(imgnode.Name == "Metadata")
                                        {
                                            newBigWindow.big_window1.MetaDataList.Items.RemoveAt(0);
                                            foreach (XmlNode meta in imgnode.ChildNodes)
                                            {
                                                if(meta.Name =="Group"){ //This needs to specify group A,B,C or D
                                                    foreach (XmlNode file in meta)
                                                    {
                                                     String fileName = file.Attributes.GetNamedItem("Filename").InnerText;
                                                     String fullPath = dataDir + "Images/" + "Metadata/" + fileName;
                                                        
                                                     BitmapImage metaBitmapImage = new BitmapImage();
                                                     metaBitmapImage.BeginInit();
                                                     metaBitmapImage.UriSource = new Uri(fullPath);
                                                     metaBitmapImage.EndInit();

                                                     //set image source
                                                    small_window newSmallwindow = new small_window();
                                                    newSmallwindow.image1.Source = metaBitmapImage;
                                                    newSmallwindow.title_tag.Text = fileName;
                                                    newSmallwindow.tags.Text = keyword;
                                                    newBigWindow.big_window1.MetaDataList.Items.Add(newSmallwindow);
                                                    newSmallwindow.setBigWindow(newBigWindow.big_window1);
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
            newBigWindow.Show();
        
            
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            _newWindow.EntryListBox.Items.Remove(this);
            //Would need to remove from the xml file as well
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            title_tag.IsReadOnly = true;
            year_tag.IsReadOnly = true;
            artist_tag.IsReadOnly = true;
            summary.IsReadOnly = true;
        }
    }
}
