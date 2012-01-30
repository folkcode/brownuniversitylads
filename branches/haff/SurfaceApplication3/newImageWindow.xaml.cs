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
    /// Interaction logic for newImageWindow.xaml
    /// </summary>
    public partial class newImageWindow : Window
    {
        public newImageWindow()
        {
            InitializeComponent();
            this.load();
            
        }

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


                                String fullPath = dataDir + "Images\\" + "Thumbnail\\" + path;

                                BitmapImage myBitmapImage = new BitmapImage();
                                myBitmapImage.BeginInit();
                                myBitmapImage.UriSource = new Uri(@fullPath);
                                myBitmapImage.EndInit();

                                //set image source
                                newEntry.image1.Source = myBitmapImage;
                                newEntry.year_tag.Text = year;
                                newEntry.artist_tag.Text = artist;
                                newEntry.title_tag.Text = title;
                                newEntry.setImagePath(fullPath);
                                newEntry.setImageTitle(title);

                                EntryListBox.Items.Add(newEntry);
                            }
                               
                        }
                    }
                }
            }
        }

        private void addImage_Click(object sender, RoutedEventArgs e)
        {
            SurfaceWindow1 newWindow = new SurfaceWindow1();
            newWindow.big_window1.setImageProperty(false);
            newWindow.ShowDialog();
        }
                               
    }
}
