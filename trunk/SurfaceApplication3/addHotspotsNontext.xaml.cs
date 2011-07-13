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

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for addHotspotsContent.xaml
    /// </summary>
    public partial class addHotspotsContent : Window
    {
        public int hotspotContent; //represent different hotspot content categroy audio=1, image =2, video =3
        private hotspotAdd hotspotsControl;
        public String contentPath;
        public addHotspotsContent()
        {
            InitializeComponent();
            this.Closed +=new EventHandler(addHotspotsContent_Closed);
        }
        public void addHotspotsContent_Closed(object sender, EventArgs e)
        {
            hotspotsControl.newWindowIsOpened = false;
        }

        public void setParentControl(hotspotAdd add)
        {
            hotspotsControl = add;
        }
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = false;
            if (hotspotContent == 1)
            {
                ofd.Filter = "Audio Files(*.MP3;*.WMA;*.MID)|*.MP3;*.WMA;*.MID";
            }

            else if (hotspotContent == 2)
            {
                ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.TIFF;*.TIF;*.JPEG;*.PNG)|*.BMP;*.JPG;*.GIF;*.TIFF;*.TIF;*.JPEG;*.PNG";
            }
            else
            {
                ofd.Filter = "Video Files(*.AVI;*.MOV;*.WMV;*.MPEG;*.MP4)|*.AVI;*.MOV;*.WMV;*.MPG;*.MP4";
            }
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                String filePath = ofd.FileName;
                String safePath = ofd.SafeFileName;
                url_tag.Text = filePath;
                contentPath = safePath;
                if (hotspotContent == 1)
                {
                    hotspotsControl.hotAudioNames.Add(safePath);
                    hotspotsControl.hotAudioPaths.Add(filePath);
                }
                else if (hotspotContent == 2)
                {
                    hotspotsControl.hotImageNames.Add(safePath);
                    hotspotsControl.hotImagePaths.Add(filePath);
                }
                else
                {
                    hotspotsControl.hotVideoNames.Add(safePath);
                    hotspotsControl.hotVideoPaths.Add(filePath);
                }
            }

            /**
             ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files(*.*)|*.*";

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

                     try
                     {

                         myBitmapImage.BeginInit();
                         myBitmapImage.UriSource = new Uri(@filePath[i]);
                         myBitmapImage.EndInit();
                     }
                     catch (Exception exception)
                     {
                         MessageBox.Show("The image file is broken or not valid");
                         return;
                     }


                     Utils.setAspectRatio(imageCanvas, imageRec, image1, myBitmapImage, 7);

                     //set image source
                     image1.Source = myBitmapImage;
                     //control.showImage(myBitmapImage)
                     //Console.Out.WriteLine(safeFilePath[i]);
                     this.setImageName(safeFilePath[i]);
                     this.setImagePath(filePath[i]);

                     this.createThumbnailImage();
                     // this.setName(fileName);
                     // FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(filePath[i]);
                     // Console.Out.WriteLine(Environment.SystemDirectory);
                     // Console.Out.WriteLine(myFileVersionInfo.FileDescription);

                     //  summary.Text = "File description: " + myFileVersionInfo.Comments;
                     // Console.Out.WriteLine("Version" + myFileVersionInfo);
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
             */
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (title.Text != "" && url_tag.Text != "")
            {
                if (hotspotContent == 1)
                {
                    hotspotsControl.setHotspotInfo(title.Text + "/" + "audio" + "/" + contentPath);
                    hotspotsControl.saveHotspotInfo();

                    hotspotsControl.AddAudio.IsEnabled = false;
                    hotspotsControl.AddText.IsEnabled = true;
                    hotspotsControl.AddImage.IsEnabled = true;
                    hotspotsControl.AddVideo.IsEnabled = true;
                    hotspotsControl.Edit.IsEnabled = true;
                    //  hotspotsControl.ModifyAudio.IsEnabled = true;

                }
                else if (hotspotContent == 2)
                {
                    hotspotsControl.setHotspotInfo(title.Text + "/" + "image" + "/" + contentPath);
                    hotspotsControl.saveHotspotInfo();
                    hotspotsControl.AddImage.IsEnabled = false;
                    hotspotsControl.AddText.IsEnabled = true;
                    hotspotsControl.AddAudio.IsEnabled = true;
                    hotspotsControl.AddVideo.IsEnabled = true;
                    hotspotsControl.Edit.IsEnabled = true;
                    //   hotspotsControl.ModifyImage.IsEnabled = true;
                }
                else
                {
                    hotspotsControl.setHotspotInfo(title.Text + "/" + "video" + "/" + contentPath);
                    hotspotsControl.saveHotspotInfo();
                    hotspotsControl.AddVideo.IsEnabled = false;
                    hotspotsControl.AddText.IsEnabled = true;
                    hotspotsControl.AddAudio.IsEnabled = true;
                    hotspotsControl.AddImage.IsEnabled = true;
                    hotspotsControl.Edit.IsEnabled = true;
                    //     hotspotsControl.ModifyVideo.IsEnabled = true;
                }
                hotspotsControl.newWindowIsOpened = false;
                this.Close();
            }
            else
            {
                MessageBox.Show("Caption and URL can not be empty!");
                return;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            title.Text = "";
            url_tag.Text = "";
            contentPath = "";
            hotspotsControl.newWindowIsOpened = false;
            this.Close();
        }
    }

    
}
