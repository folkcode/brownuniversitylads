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
using Microsoft.Surface.Presentation.Controls;
using DeepZoom;
using DeepZoom.Controls;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Media;
using System.Text.RegularExpressions;
using System.Xml;


namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for HotspotDetailsControl.xaml
    /// </summary>
    public partial class HotspotDetailsControl : ScatterViewItem
    {
        ScatterView m_parentScatterView;
        Canvas m_parentCanvas;
        Hotspot m_hotspotData;
        Boolean hasVideo;
        LADSVideoBubble video;
       
        double screenPosX;

        public double ScreenPosX
        {
            get { return screenPosX; }
            set { screenPosX = value; }
        }

        double screenPosY;

        public double ScreenPosY
        {
            get { return screenPosY; }
            set { screenPosY = value; }
        }

        bool isOnScreen;

        public bool IsOnScreen
        {
            get { return isOnScreen; }
            set { isOnScreen = value; }
        }

        XpsDocument m_xpsdocument;

        public XpsDocument Xpsdocument
        {
            get { return m_xpsdocument; }
            set { m_xpsdocument = value; }
        }

        MultiScaleImage m_msi;

        public MultiScaleImage Msi
        {
            get { return m_msi; }
            set { m_msi = value; }
        }
        //ScatterViewItem scatterItem;

        /// <summary>
        /// Constructor
        /// </summary>
        public HotspotDetailsControl(Canvas parentCanvas, ScatterView parentScatterView, Hotspot hotspotData, MultiScaleImage msi)
        {
            InitializeComponent();
            m_hotspotData = hotspotData;
            m_parentCanvas = parentCanvas;
            m_parentScatterView = parentScatterView;
            m_msi = msi;
            //ScatterViewItem
            
            isOnScreen = false;
            hasVideo = false;
            
        }
        public void showAudioIcon()
        {
           // BitmapImage newImage = new BitmapImage();
           // newImage.BeginInit();
           // String filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\audio.png";
           // newImage.UriSource = new Uri(@filePath);
           // newImage.EndInit();
           // audioIcon.Source = newImage;
            BitmapImage play = new BitmapImage();
            play.BeginInit();
            String playPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\playbutton.png";
            play.UriSource = new Uri(@playPath);
            play.EndInit();
            PlayButton.Source = play;

            BitmapImage pause = new BitmapImage();
            pause.BeginInit();
            String pausePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\pausebutton.png";
            pause.UriSource = new Uri(@pausePath);
            pause.EndInit();
            PauseButton.Source = pause;

            BitmapImage stop = new BitmapImage();
            stop.BeginInit();
            String stopPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\stopbutton.png";
            stop.UriSource = new Uri(@stopPath);
            stop.EndInit();
            StopButton.Source = stop;

            BitmapImage volume = new BitmapImage();
            volume.BeginInit();
            String volumePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\volume.png";
            volume.UriSource = new Uri(@volumePath);
            volume.EndInit();
            Volume.Source = volume;
            // newImage.UriSource = new Uri(@filePath);
            // newImage.EndInit();
        }
        /// <summary>
        /// Called when the closed button is clicked.
        /// remove from screen.
        /// </summary>
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            //m_xpsdocument.Close();
            //m_parentCanvas.Children.Remove(this);
            m_parentScatterView.Items.Remove(this);
            isOnScreen = false;
            if (hasVideo)
            {
                video.pauseVideo();
            }
        }

        /// <summary>
        /// remove from screen.
        /// not used
        /// </summary>
        public void selfDelete()
        {
           m_parentCanvas.Children.Remove(this);
            //HotspotsContent = null;
            isOnScreen = false;
        }

        /// <summary>
        /// Called when a user chooses to display details about a hotspot.
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           // HotspotsContent.Text = m_hotspotData.Description;
          /*  String fileName = m_hotspotData.Description;
           // String fullpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\HotspotDataFiles\\" + fileName;
            String fullpath = null;
            try
            {
                fullpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\HotspotDataFiles\\" + fileName;
            }
            catch
            {
                fullpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\HotspotDataFiles\\" + "test.xps";
            }
            //MessageBox.Show(fullpath);
            m_xpsdocument = new XpsDocument(fullpath, FileAccess.ReadWrite);
            HotspotsContent.Document = m_xpsdocument.GetFixedDocumentSequence();*/
            //HotspotTextBox.Text = m_hotspotData.Type + " - " + m_hotspotData.Description;
            if (m_hotspotData.Type.ToLower().Contains("image"))
            {
                BitmapImage img = new BitmapImage();
                String imgUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + m_hotspotData.Description;
                img.BeginInit();
                img.UriSource = new Uri(imgUri, UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                HotspotImage.Source = img;
                HotspotImage.Visibility = Visibility.Visible;
                imageScroll.Visibility = Visibility.Visible;
                HotspotImage.IsEnabled = true;
                imageScroll.IsEnabled = true;
                double maxWidth = 600.0;
                if (img.Width > maxWidth)
                {
                    HotspotImage.SetCurrentValue(HeightProperty, maxWidth * img.Height / img.Width);
                    HotspotImage.SetCurrentValue(WidthProperty, maxWidth);
                }
                else
                {
                    HotspotImage.SetCurrentValue(HeightProperty,img.Height);
                    HotspotImage.SetCurrentValue(WidthProperty, img.Width);
                }
                this.SetCurrentValue(HeightProperty, HotspotImage.Height + 47.0);
                this.SetCurrentValue(WidthProperty, HotspotImage.Width + 24.0);
                hotspotCanvas.Width = HotspotImage.Width + 24.0;
                hotspotCanvas.Height = HotspotImage.Height + 47.0;
                
                this.Width = hotspotCanvas.Width;
                this.Height = hotspotCanvas.Height;
                Canvas.SetLeft(closeButton, hotspotCanvas.Width - 52.0);
                imageScroll.Width = HotspotImage.Width;
                imageScroll.Height = HotspotImage.Height;
                HotspotTextBox.Visibility = Visibility.Hidden;
                textBoxScroll.Visibility = Visibility.Hidden;
                //this.Width = img.Width;
            }
            if (m_hotspotData.Type.ToLower().Contains("text"))
            {
                HotspotTextBox.Text = m_hotspotData.Description;
                HotspotImage.Visibility = Visibility.Hidden;
                imageScroll.Visibility = Visibility.Hidden;
                HotspotImage.IsEnabled = false;
                imageScroll.IsEnabled = false;
                HotspotTextBox.Visibility = Visibility.Visible;
                textBoxScroll.Visibility = Visibility.Visible;
            }
            else
            {
                Console.Out.WriteLine("audio is being called");
                Storyboard audioResourceWav;
                String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\GaribaldiScene43Tour_mastered.mp3";
                audioResourceWav = (Storyboard)this.Resources[audioUri];
                audioResourceWav.Begin(this);

            }
            
            Name.Content = m_hotspotData.Name;
        }


        /// <summary>
        /// Update the screen location of the control with respect to the artwork.
        /// </summary>
        public void updateScreenLocation(MultiScaleImage msi)
        {
            Double[] size = this.findImageSize();

            screenPosX = (msi.GetZoomableCanvas.Scale * m_hotspotData.PositionX * size[0]) - msi.GetZoomableCanvas.Offset.X;
            screenPosY = (msi.GetZoomableCanvas.Scale * m_hotspotData.PositionY * size[1]) - msi.GetZoomableCanvas.Offset.Y;
            this.Center = new Point(screenPosX + this.Width / 2.0, screenPosY + this.Height / 2.0); // uncomment this to make the detail control move as the user pans
           
            //Canvas.SetLeft(this, screenPosX);
            //Canvas.SetTop(this, screenPosY);
        }
      
        /// <summary>
        /// Load data inside the hotspot detail control, can be text or image.
        /// </summary>
        private void scatterItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_hotspotData.Type.ToLower().Contains("image"))
            {
                BitmapImage img = new BitmapImage();
                String imgUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + m_hotspotData.Description;
                img.BeginInit();
                img.UriSource = new Uri(imgUri, UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                HotspotImage.Source = img;
                HotspotImage.Visibility = Visibility.Visible;
                imageScroll.Visibility = Visibility.Visible;
                HotspotImage.IsEnabled = true;
                imageScroll.IsEnabled = true;
                double maxWidth = 500.0;
                if (img.PixelWidth > maxWidth)
                {
                    HotspotImage.SetCurrentValue(HeightProperty, maxWidth * img.PixelHeight / img.PixelWidth);
                    HotspotImage.SetCurrentValue(WidthProperty, maxWidth);
                }
                else
                {
                    HotspotImage.SetCurrentValue(HeightProperty, (double) img.PixelHeight);
                    HotspotImage.SetCurrentValue(WidthProperty, (double) img.PixelWidth);
                }
                this.SetCurrentValue(HeightProperty, HotspotImage.Height + 47.0);
                this.SetCurrentValue(WidthProperty, HotspotImage.Width + 24.0);
                hotspotCanvas.Width = HotspotImage.Width + 24.0;
                hotspotCanvas.Height = HotspotImage.Height + 47.0;

                this.Width = hotspotCanvas.Width;
                this.Height = hotspotCanvas.Height;
                Canvas.SetLeft(closeButton, hotspotCanvas.Width - 52.0);
                imageScroll.Width = HotspotImage.Width;
                imageScroll.Height = HotspotImage.Height;
                HotspotTextBox.Visibility = Visibility.Hidden;
                textBoxScroll.Visibility = Visibility.Hidden;
                //this.Width = img.Width;
            }
            else if (m_hotspotData.Type.ToLower().Contains("text"))
            {
                HotspotTextBox.Text = m_hotspotData.Description;
                HotspotImage.Visibility = Visibility.Hidden;
                imageScroll.Visibility = Visibility.Hidden;
                HotspotImage.IsEnabled = false;
                imageScroll.IsEnabled = false;
                HotspotTextBox.Visibility = Visibility.Visible;
                textBoxScroll.Visibility = Visibility.Visible;
            }
            else  if (m_hotspotData.Type.ToLower().Contains("audio"))
            {
               // Console.Out.WriteLine("audio is selected!");
                AudioScroll.Visibility = Visibility.Visible;
                String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + m_hotspotData.Description;
                //mediaElement.Source = new Uri(audioUri);
                audioName.Content = m_hotspotData.Description;
                //String description = 
               // MediaName.Content = m_hotspotData.Description.Substring(0,m_hotspotData.Description.Length-4); //will display the name of the media according to where the file saves
                myMediaElement.Source = new Uri(audioUri);
                this.showAudioIcon();
                mediaTimeLine.Source = new Uri(audioUri);
                
               // mediaElement.Play();
            }
            else
            {
                String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Videos\\" + m_hotspotData.Description;
                LADSVideoBubble newVideo = new LADSVideoBubble(audioUri,372,268);
                newVideo.setPreferredSize(372, 268);
                VideoPanel.Children.Add(newVideo);
                Canvas.SetLeft(newVideo, 5);
                Canvas.SetTop(newVideo, 5);
                VideoPanel.Visibility = Visibility.Visible;
                video = newVideo;
                hasVideo = true;


            }
            Name.Content = m_hotspotData.Name;
            Double[] size = this.findImageSize();

            screenPosX = (m_msi.GetZoomableCanvas.Scale * m_hotspotData.PositionX *size[0]) - m_msi.GetZoomableCanvas.Offset.X;
            screenPosY = (m_msi.GetZoomableCanvas.Scale * m_hotspotData.PositionY *size[1]) - m_msi.GetZoomableCanvas.Offset.Y;
           
            this.Center = new Point(screenPosX + this.Width/2.0, screenPosY + this.Height/2.0);
           // Console.Out.WriteLine("center1" + this.Center);
           // m_parentScatterView.Items.Add(this);
        }

        public Double[] findImageSize()
        {
            Double[] sizes = new Double[2];
            XmlDocument newDoc = new XmlDocument();
            String imageFolder = "Data/Images/DeepZoom/" + m_hotspotData.artworkName + "/" + "dz.xml";
            newDoc.Load(imageFolder);
            if (newDoc.HasChildNodes)
            {
                foreach (XmlNode image in newDoc.ChildNodes)
                {
                    if (image.Name == "Image")
                    {
                        XmlNode size = image.FirstChild;
                        String width = size.Attributes.GetNamedItem("Width").InnerText;
                        String height = size.Attributes.GetNamedItem("Height").InnerText;
                        Double width1 = Convert.ToDouble(width);
                        Double height1 = Convert.ToDouble(height);
                        sizes[0] = width1;
                        sizes[1] = height1;

                    }
                }

            }
            return sizes;
        }
        // When the media opens, initialize the "Seek To" slider maximum value
        // to the total number of miliseconds in the length of the media clip.
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            timelineSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
           // Console.Out.WriteLine("duration" + myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds);
        }

        private void MediaTimeChanged(object sender, EventArgs e)
        {
            timelineSlider.Value = myMediaElement.Position.TotalMilliseconds;
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            myMediaElement.Volume = (double)volumeSlider.Value;
        }

        /**
        private void mediaPlay(Object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
            
        }
        private void mediaPause(Object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }
        private void mediaStop(Object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
        }
       private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
       {
         mediaElement.Volume = (double)volumeChange.Value;
       }
       private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
       {
           int SliderValue = (int)mediaPositionChange.Value;
           
           // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds.
           // Create a TimeSpan with miliseconds equal to the slider value.
           TimeSpan ts = new TimeSpan(0, 0, 0, SliderValue);
           mediaElement.Position = ts;
       }
         * */
    }
}
