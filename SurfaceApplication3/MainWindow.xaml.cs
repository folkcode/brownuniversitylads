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
using System.IO;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       private Helpers _helpers;
        public MainWindow()
        {
            _helpers = new Helpers();
            InitializeComponent();
            this.load();
        
            this.setWindowSize();
        }
        public void setWindowSize()
        {
            
            Double width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            Double height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            Console.Out.WriteLine("height" + height);
            Console.Out.WriteLine("width" + width);
            Double ratio = height / width;
            ScaleTransform tran = new ScaleTransform();
               
            if (width < 1024 || height < 800)
                {
                    if(width / 1024 > height / 800)
                    {
                        this.Height = height - 100;
                        this.Width = this.Height / 800 * 1024;
                       // this.Width = this.Height/ratio;
                        tran.ScaleY = this.Height / 800;
                        tran.ScaleX = this.Width/ 1024;
                    }
                    else
                    {
                        this.Width = width -100 ;
                        this.Height = this.Width / 1024 * 800;
                        tran.ScaleX = this.Width / 1024;
                        tran.ScaleY = this.Height / 800;
                      //  this.Height = this.Width * ratio;
                    }
                    //Console.Out.WriteLine("width" + this.Width);
                    //Console.Out.WriteLine("height" + this.Height);
                     //scale according to 1600* 900 resolution
                    
                    mainCanvas.RenderTransform = tran;
                }
            
            
            
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

                                Image wpfImage = new Image();
                                FileStream stream = new FileStream(fullPath, FileMode.Open);
                                System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
                                wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
                                stream.Close();

                                /*BitmapImage myBitmapImage = new BitmapImage();
                                myBitmapImage.BeginInit();
                                myBitmapImage.UriSource = new Uri(@fullPath);
                                myBitmapImage.EndInit();
                                newEntry.setImage(myBitmapImage);*/



                                //Utils.setAspectRatio(newEntry.imageCanvas, newEntry.imageRec, newEntry.image1, myBitmapImage, 4);

                                //set image source
                                newEntry.image1.Source = wpfImage.Source;
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

            newWindow.WindowState = System.Windows.WindowState.Normal;
            newWindow.ShowDialog();
            //newWindow.big_window1.setMainWindow(this);
        }

        private void addEvent_Click(object sender, RoutedEventArgs e)
        {
            EventWindow evWin = new EventWindow();
            evWin.WindowState = System.Windows.WindowState.Normal;
            evWin.ShowDialog();

        }

    }
}
