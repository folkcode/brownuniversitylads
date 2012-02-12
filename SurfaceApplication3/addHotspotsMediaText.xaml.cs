using System;
using System.Windows;
using System.IO;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for addHotspotsContent.xaml
    /// </summary>
    public partial class addHotspotsMediaText : Window
    {
        public int hotspotContent; //represent different hotspot content categroy audio=1, image =2, video =3
        private hotspotAdd hotspotsControl;
        public String filePath; // jcchin
        public String contentFilename;
        public String existingContentFilename; // jcchin
        public int existingContentCategory; // jcchin

        public addHotspotsMediaText()
        {
            InitializeComponent();
            this.Closed +=new EventHandler(addHotspotsContent_Closed);

            filePath = ""; // path of new file (selected through browse window)
            contentFilename = ""; // name of new file 
            existingContentFilename = "";
            existingContentCategory = 0;
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
            Console.WriteLine("Hotspot Content = " + hotspotContent);
            if (hotspotContent == 4)
            {
                ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.TIFF;*.TIF;*.JPEG;*.PNG)|*.BMP;*.JPG;*.GIF;*.TIFF;*.TIF;*.JPEG;*.PNG";
            }
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //String filePath = ofd.FileName;
                filePath = ofd.FileName; // jcchin - replaced line above

                //String safePath = ofd.SafeFileName;
                contentFilename = ofd.SafeFileName; // jcchin

                url_tag.Text = filePath;
                
                //contentFilename = safePath; // jcchin - not needed anymore
                
                // jcchin - MOVED to OK_Click
                /*if (hotspotContent == 1)
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
                }*/
            }

           
        }

        //When Ok is clicked, close the window and save the information to the hospotAdd window
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (title.Text != "" && url_tag.Text != "" && Text.Text != "")
            {
                if (hotspotContent == 4) // image+text
                {
                    // jcchin - case - modifying content type
                    if((existingContentCategory != 0) && (existingContentCategory != hotspotContent))
                    {
                        if (existingContentCategory == 1) {
                            String hotspotAudioToRemove = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + existingContentFilename;
                            hotspotsControl.hotspotFilesToDelete.Add(hotspotAudioToRemove);
                        }
                        else if (existingContentCategory == 2) {
                            String hotspotImageToRemove = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + existingContentFilename;
                            String hotspotImageThumbToRemove = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\Thumbnail\\" + existingContentFilename;
                            hotspotsControl.hotspotFilesToDelete.Add(hotspotImageToRemove);
                            hotspotsControl.hotspotFilesToDelete.Add(hotspotImageThumbToRemove);
                        }
                        else if (existingContentCategory == 3) {
                            String hotspotVideoToRemove = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Videos\\" + existingContentFilename;
                            hotspotsControl.hotspotFilesToDelete.Add(hotspotVideoToRemove);
                        }
                        else if (existingContentCategory == 4)
                        {
                            String hotspotImageToRemove = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + existingContentFilename;
                            String hotspotImageThumbToRemove = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\Thumbnail\\" + existingContentFilename;
                            hotspotsControl.hotspotFilesToDelete.Add(hotspotImageToRemove);
                            hotspotsControl.hotspotFilesToDelete.Add(hotspotImageThumbToRemove);
                        }
                    }
                    // jcchin - case - modifying path of existing content
                    else if (existingContentFilename != "")
                    {
                        if (filePath == "") // no new file was selected via the Browse window
                        {
                            this.Close();
                            return;
                        }
                        else
                        {
                            String hotspotImageToRemove = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + existingContentFilename;
                            hotspotsControl.hotspotFilesToDelete.Add(hotspotImageToRemove);
                        }
                    }

                    // NEW - jcchin - randomize hotspot filenames
                    Random random = new Random();
                    int randomNumber = random.Next(0, 100000000);
                    string hotspotMediaFileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(contentFilename);
                    string hotspotMediaExtension = System.IO.Path.GetExtension(contentFilename);
                    string destContentFilename = hotspotMediaFileWithoutExtension + "_" + randomNumber + hotspotMediaExtension;
                    while (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + destContentFilename))
                    {
                        randomNumber = random.Next(0, 100000000);
                        destContentFilename = hotspotMediaFileWithoutExtension + "_" + randomNumber + hotspotMediaExtension;
                    }
                    // NEW_end - jcchin

                    hotspotsControl.hotImageNames.Add(destContentFilename); // jcchin
                    hotspotsControl.hotImagePaths.Add(filePath); // jcchin

                    hotspotsControl.setHotspotInfo(title.Text + "/" + "image+text" + "/" + destContentFilename + "/" + Text.Text); // jcchin - image+text

                    hotspotsControl.saveHotspotInfo();
                    hotspotsControl.AddImage.IsEnabled = true;
                    hotspotsControl.AddText.IsEnabled = true;
                    hotspotsControl.AddAudio.IsEnabled = true;
                    hotspotsControl.AddVideo.IsEnabled = true;
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
            contentFilename = "";
            hotspotsControl.newWindowIsOpened = false;
            this.Close();
        }
    }

    
}
