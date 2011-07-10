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
using Microsoft.Surface.Presentation.Controls.Primitives;
using System.IO;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for MetaDataEntry.xaml
    /// </summary>
    public partial class MetaDataEntry : UserControl
    {
        public AddNewImageControl big;
        SurfaceToggleButton itemChecked;
        public String metaImagePath;
        private string webURL;
        private string type;
        private Helpers _helper;

        public MetaDataEntry()
        {
            InitializeComponent();
            metaImagePath = "";
            webURL = "";
            type = "Image";
            _helper = new Helpers();
        }

        public void setBigWindow(AddNewImageControl bigwindow)
        {
            big = bigwindow;
        }

        private void SurfaceToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (itemChecked != null)
                itemChecked.IsChecked = false;
            itemChecked = e.Source as SurfaceToggleButton;
            itemChecked.IsChecked = true;

        }

        /// <summary>
        /// Open an image associated with the current metadata
        /// </summary>
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            type = "Image";
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = false;

            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"; //Only allow image type metadata

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] filePath = ofd.FileNames;
                string[] safeFilePath = ofd.SafeFileNames;

                for (int i = 0; i < safeFilePath.Length; i++)
                {
                  
                    //BitmapImage myBitmapImage = new BitmapImage();
                    System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
                    try
                    {
                        //myBitmapImage.BeginInit();
                        //myBitmapImage.UriSource = new Uri(@filePath[i]);
                        //myBitmapImage.EndInit();
                        Console.WriteLine("Filepath: "+filePath[i]);
                        FileStream stream = new FileStream(@filePath[i], FileMode.Open);
                        
                        System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
                        //wpfImage = ConvertDrawingImageToWPFImage(dImage);
                        wpfImage = _helper.ConvertDrawingImageToWPFImage(dImage);
                        
                        stream.Close();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("The image is broken or invalid!");
                        return;
                    }

                    image1.Source = wpfImage.Source;
                    //Utils.setAspectRatio(imageCanvas, imageRec, image1, myBitmapImage, 4);
                    
                    //title_tag.Text = safeFilePath[i];
                    string filename = safeFilePath[i];
                    string extension = Path.GetExtension(safeFilePath[i]);
                    string tempFileName = Path.GetFileNameWithoutExtension(safeFilePath[i]);
                    Random random = new Random();
                    int randomNumber = random.Next(0, 100000000);
                    filename = tempFileName + randomNumber + extension;
                    while (File.Exists("Data/Images/Metadata/" + filename))
                    {
                        randomNumber = random.Next(0, 100000000);
                        filename = tempFileName + randomNumber + extension;
                    }
                    title_tag.Text = filename;
                    //title_tag.Text = safeFilePath[i];

                    metaImagePath = filePath[i];
                }

            }
            Console.WriteLine("Asset Type: "+type);
        }

        /// <summary>
        /// Remove the current metadata from the list
        /// </summary>
        private void remove_Click(object sender, RoutedEventArgs e)
        {
            big.MetaDataList.Items.Remove(this);
        }

        /// <summary>
        /// This allows users to select a word when double clicking on the words in the description textbox
        /// </summary>
        private void summary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        private void Web_Click(object sender, RoutedEventArgs e)
        {
            type = "Web";
            AddWebImage addWeb = new AddWebImage(this);
            addWeb.ShowDialog();
        }

        public void setURL(string url)
        {
            webURL = url;
            Console.WriteLine("URL: " + webURL);
            string filePath = "Data/Images/Metadata/" + Path.GetFileName(webURL);
            string filename = Path.GetFileName(url);
            string extension = Path.GetExtension(url);
            string tempFileName = Path.GetFileNameWithoutExtension(url);
            Random random = new Random();
            int randomNumber = random.Next(0, 100000000);
            filename = tempFileName + randomNumber + extension;
            while (File.Exists("Data/Images/Metadata/" + filename))
            {
                randomNumber = random.Next(0, 100000000);
                filename = tempFileName + randomNumber + extension;
            }
            
            Console.WriteLine("File path = " + filePath);

            System.Drawing.Image drawingImage = null;
            System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
            try
            {
                System.Net.HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(webURL);
                httpWebRequest.AllowWriteStreamBuffering = true;
                httpWebRequest.Timeout = 20000;
                System.Net.WebResponse response = httpWebRequest.GetResponse();
                System.IO.Stream stream = response.GetResponseStream();
                drawingImage = System.Drawing.Image.FromStream(stream);

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(drawingImage);
                IntPtr hBitmap = bmp.GetHbitmap();
                System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                image1.Source = WpfBitmap;
                
                wpfImage.Source = WpfBitmap;
                wpfImage.Width = 500;
                wpfImage.Height = 600;
                wpfImage.Stretch = System.Windows.Media.Stretch.Fill;

                string tempFilepath = System.IO.Path.GetTempPath() + Path.GetFileName(webURL);
                Console.WriteLine("TEMP FILE PATH = " + tempFilepath);
                bmp.Save(@tempFilepath);
                metaImagePath = tempFilepath;
                stream.Close();
                response.Close();
                response.Close();

                title_tag.Text = filename;
                type = "Web";

                //Utils.setAspectRatio(imageCanvas, imageRec, image1, myBitmapImage, 4);
            }

            catch (Exception e)
            {
                MessageBox.Show("The image is broken or invalid.");
            }
            //title_tag.Text = safeFilePath[i];
        }

        public string getType()
        {
            return type;
        }

        public string getURL()
        {
            return webURL;
        }


    }
}
