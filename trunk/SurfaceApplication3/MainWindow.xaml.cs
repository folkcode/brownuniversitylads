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
using System.Windows.Shapes;
using System.Xml;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.load();


        }

        /// <summary>
        /// Load data from XML file and store into a list
        /// </summary>
        public void load()
        {
            //String dataDir = "Data\\";
            String dataDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
            //String dataDir = "F://lads_data/";
            //String dataDir = "C://LADS-yc60/data/";
            Console.WriteLine("DataDir: " + dataDir);
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
                                catalogEntry newEntry = new catalogEntry(this);
                                String path = node.Attributes.GetNamedItem("path").InnerText;
                                String artist = node.Attributes.GetNamedItem("artist").InnerText;
                                String title = node.Attributes.GetNamedItem("title").InnerText;
                                String year = node.Attributes.GetNamedItem("year").InnerText;
                                String medium = node.Attributes.GetNamedItem("medium").InnerText;

                               

                                String fullPath = dataDir + "Images\\" + "Thumbnail\\" + path;

                                BitmapImage myBitmapImage = new BitmapImage();
                                myBitmapImage.BeginInit();
                                myBitmapImage.UriSource = new Uri(@fullPath);
                                myBitmapImage.EndInit();
                                newEntry.setImage(myBitmapImage);

                                Utils.setAspectRatio(newEntry.imageCanvas, newEntry.imageRec, newEntry.image1, myBitmapImage, 4);

                                //set image source
                                newEntry.image1.Source = myBitmapImage;
                                newEntry.year_tag.Text = year;
                                newEntry.artist_tag.Text = artist;
                                newEntry.title_tag.Text = title;
                                newEntry.medium_tag.Text = medium;

                                if (node.Attributes.GetNamedItem("description") != null)
                                {
                                    String description = node.Attributes.GetNamedItem("description").InnerText;
                                    newEntry.summary.Text = description;
                                }
                                newEntry.setImagePath(fullPath);
                                newEntry.setImageTitle(title);
                                newEntry.setImageName(path);
                                EntryListBox.Items.Add(newEntry);
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Open a new window for users to add new image to the collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addImage_Click(object sender, RoutedEventArgs e)
        {
            AddImageWindow newWindow = new AddImageWindow();
            newWindow.big_window1.mainWindow = this;
            newWindow.big_window1.setImageProperty(false);
            MetaDataEntry newSmall = new MetaDataEntry();
            newWindow.big_window1.MetaDataList.Items.Add(newSmall);
            newSmall.setBigWindow(newWindow.big_window1);
            //Initially set the two buttons disabled, after the user save to the catalog then will these two be abled
            newWindow.big_window1.mapButton.IsEnabled = false;
            newWindow.big_window1.hotspot.IsEnabled = false;

            Canvas.SetLeft(newWindow.big_window1.save, 349);
            Canvas.SetLeft(newWindow.big_window1.mapButton, 513);
            Canvas.SetLeft(newWindow.big_window1.hotspot, 713);
                        
            newWindow.ShowDialog();
            //newWindow.big_window1.setMainWindow(this);
        }

        private void addEvent_Click(object sender, RoutedEventArgs e)
        {
            EventWindow evWin = new EventWindow();
            evWin.ShowDialog();

        }

    }
}
