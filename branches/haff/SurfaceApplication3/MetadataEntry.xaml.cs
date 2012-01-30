using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Surface.Presentation.Controls.Primitives;
using System.IO;
using DexterLib;

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
            type = "";
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
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = false;

            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.JPEG)|*.BMP;*.JPG;*.GIF;*.JPEG|Audio and Video Files(*.MOV;*.AVI;*.MP4;*.WMV)|*MOV;*AVI;*.MP4;*.WMV|All files (*.*)|*.*"; //Only allow image and video type metadata

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] filePath = ofd.FileNames;
                string[] safeFilePath = ofd.SafeFileNames;
                for (int i = 0; i < safeFilePath.Length; i++)
                {
                    //if image
                    if (_helper.IsImageFile(filePath[i]))
                    {
                        type = "Image";
                        System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
                        try
                        {
                            FileStream stream = new FileStream(@filePath[i], FileMode.Open);

                            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
                          
                            wpfImage = _helper.ConvertDrawingImageToWPFImage(dImage);

                            stream.Close();
                            image1.Source = wpfImage.Source;
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show("The image is broken or invalid!");
                            return;
                        }
                    }
                    //if video
                    else if (_helper.IsVideoFile(filePath[i]))
                    {
                        type = "Video";
                        BitmapImage videoThumb = new BitmapImage();

                        if (_helper.IsDirShowFile(filePath[i]))
                        {
                            //creating and saving thumbnail image
                            DexterLib.MediaDet md = new MediaDet();
                            md.Filename = @filePath[i];
                            md.CurrentStream = 0;
                            string fBitmapName = @filePath[i];
                            fBitmapName = fBitmapName.Remove(fBitmapName.Length - 4, 4);
                            fBitmapName += ".bmp";
                            md.WriteBitmapBits(md.StreamLength / 2, 400, 240, fBitmapName);

                            Image wpfImage = new Image();
                            FileStream stream = new FileStream(fBitmapName, FileMode.Open);
                            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
                            wpfImage = _helper.ConvertDrawingImageToWPFImage(dImage);
                            stream.Close();

                            Utils.setAspectRatio(imageCanvas, imageRec, image1, wpfImage, 4);
                            image1.Source = videoThumb;
                                                      
                        }
                        else
                        {                            
                            String sImageText = Path.GetFileNameWithoutExtension(filePath[i]);
                            System.Drawing.Bitmap objBmpImage = new System.Drawing.Bitmap(1,1);

                            int intWidth = 0;
                            int intHeight = 0;

                            // Create the Font object for the image text drawing.
                            System.Drawing.Font objFont = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);

                            // Create a graphics object to measure the text's width and height.
                            System.Drawing.Graphics objGraphics = System.Drawing.Graphics.FromImage(objBmpImage);

                            // This is where the bitmap size is determined.
                            intWidth = (int)objGraphics.MeasureString(sImageText, objFont).Width;
                            intHeight = (int)objGraphics.MeasureString(sImageText, objFont).Height;
                            System.Drawing.Size newsize = new System.Drawing.Size(intWidth, intHeight);

                            // Create the bmpImage again with the correct size for the text and font.
                            objBmpImage = new System.Drawing.Bitmap(objBmpImage, newsize);

                            // Add the colors to the new bitmap.
                            objGraphics = System.Drawing.Graphics.FromImage(objBmpImage);

                            // Set Background color
                            objGraphics.Clear(System.Drawing.Color.White);
                            objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//SmoothingMode.AntiAlias;
                            objGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;//TextRenderingHint.AntiAlias;
                            objGraphics.DrawString(sImageText, objFont, new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(102, 102, 102)), 0, 0);
                            objGraphics.Flush();

                            string fBitmapName = @filePath[i];
                            fBitmapName = fBitmapName.Remove(fBitmapName.Length - 4, 4);
                            fBitmapName += ".bmp";
                            objBmpImage.Save(fBitmapName);

                            FileStream stream = new FileStream(fBitmapName, FileMode.Open);
                            System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
                            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
                            wpfImage = _helper.ConvertDrawingImageToWPFImage(dImage);

                            stream.Close();
                            image1.Source = wpfImage.Source;
                        } 
                      
                        title_tag.Text = safeFilePath[i];
                        metaImagePath = filePath[i];
                    }

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
                
                    metaImagePath = filePath[i];
                }
            }
           
        }

        /// <summary>
        /// Remove the current metadata from the list
        /// </summary>
        private void remove_Click(object sender, RoutedEventArgs e)
        {
            big.MetaDataList.Items.Remove(this);
            big.addImagesToDelete(title_tag.Text);
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

        /// <summary>
        /// sets the URL of a web asset
        /// </summary>
        public void setURL(string url)
        {
            webURL = url;
            try
            {
                string filePath = "Data/Images/Metadata/" + Path.GetFileName(webURL);
            }
            catch (Exception notvalid)
            {
                MessageBox.Show("This is not a valid URL");
                return;
            }
            string filename = Path.GetFileName(url);
            string extension = Path.GetExtension(url);
            string tempFileName = Path.GetFileNameWithoutExtension(url);
            //assign random characters after the name of the image to prevent duplicates
            Random random = new Random();
            int randomNumber = random.Next(0, 100000000);
            filename = tempFileName + randomNumber + extension;
            //check if name already exists
            while (File.Exists("Data/Images/Metadata/" + filename))
            {
                randomNumber = random.Next(0, 100000000);
                filename = tempFileName + randomNumber + extension;
            }

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
                
                bmp.Save(@tempFilepath);
                metaImagePath = tempFilepath;
                stream.Close();
                response.Close();
                response.Close();

                title_tag.Text = filename;
                type = "Web";

            }

            catch (Exception e)
            {
                MessageBox.Show("The image is broken or invalid.");
            }
        }

        public string getType()
        {
            return type;
        }

        public string getURL()
        {
            return webURL;
        }

        private void image1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
        
    }
}
