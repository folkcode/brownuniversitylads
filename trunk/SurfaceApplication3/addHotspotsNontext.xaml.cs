using System;
using System.Windows;

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
        
        //Open file dialog to allow user to select the file
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

           
        }

        //When Ok is clicked, close the window and save the information to the hospotAdd window
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
                  
                }
                hotspotsControl.newWindowIsOpened = false;
                this.Close();
            }
            else
            {
                MessageBox.Show("Caption and URL cannot be empty!");
                return;
            }
        }

        //Cancel all the changes 
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
