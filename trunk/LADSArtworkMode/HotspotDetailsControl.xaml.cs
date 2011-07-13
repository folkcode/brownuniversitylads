﻿using System;
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
using Microsoft.Surface.Presentation.Generic;


namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for HotspotDetailsControl.xaml
    /// </summary>
    public partial class HotspotDetailsControl : ScatterViewItem
    {
        //MediaElement myMediaElement;
        ScatterView m_parentScatterView;
        Canvas m_parentCanvas;
        public Hotspot m_hotspotData;
        Boolean hasVideo;
        //LADSVideoBubble video;
        //MediaElement _audio;
        Boolean _dragging;
        private const int SLIDER_TIMER_RESOLUTION = 20; //how often we update the slider based on the video position, in milliseconds
        private System.Windows.Threading.DispatcherTimer _sliderTimer;
        Boolean _hasBeenOpened;
        double minX;
        public double MinX
        {
            get { return minX; }
            set { minX = value; }
        }

        double minY;
        public double MinY
        {
            get { return minY; }
            set { minY = value; }
        }

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
            _hasBeenOpened = false;
            
        }
        public void showAudioIcon()
        {
           // BitmapImage newImage = new BitmapImage();
           // newImage.BeginInit();
           // String filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\audio.png";
           // newImage.UriSource = new Uri(@filePath);
           // newImage.EndInit();
           // audioIcon.Source = newImage;

            //BitmapImage play = new BitmapImage();
            //play.BeginInit();
            //String playPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\playbutton.png";
            //play.UriSource = new Uri(@playPath);
            //play.EndInit();

            //PlayButton.Source = play;

            //BitmapImage pause = new BitmapImage();
            //pause.BeginInit();
            //String pausePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\pausebutton.png";
            //pause.UriSource = new Uri(@pausePath);
            //pause.EndInit();
            ////PauseButton.Source = pause;

            //BitmapImage stop = new BitmapImage();
            //stop.BeginInit();
            //String stopPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\stopbutton.png";
            //stop.UriSource = new Uri(@stopPath);
            //stop.EndInit();
            ////StopButton.Source = stop;

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
                (video as LADSVideoBubble).pauseVideo();
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
            Console.WriteLine("_Loaded");
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
                //Canvas.SetLeft(closeButton, hotspotCanvas.Width - 52.0);
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
                //Storyboard audioResourceWav;
                String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\GaribaldiScene43Tour_mastered.mp3";
                //myMediaElement = new MediaElement();
                //audioResourceWav = (Storyboard)this.Resources[audioUri];
                //audioResourceWav.Begin(this);
            }
            
            Name.Content = m_hotspotData.Name;
        }

        /// <summary>
        /// Called when the play button is clicked.
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("timelineslider.Value is: " + timelineSlider.Value);
            myMediaElement.Position = new TimeSpan(0,0,0,0,(int)timelineSlider.Value);
            myMediaElement.Play();
            _sliderTimer.Start();
            Console.WriteLine("Play clicked");
            Console.WriteLine("media element is: " + myMediaElement.IsLoaded);
            Console.WriteLine("max is : " + timelineSlider.Maximum);
            //timelineSlider.Start();
        }
        /// <summary>
        /// Called when the pause button is clicked.
        /// </summary>
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Pause();
            _sliderTimer.Stop();
        }

        /// <summary>
        /// Called when the stop button is clicked. only difference from pause is that it sets playhead back to zero.
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Pause();
            _sliderTimer.Stop();
            myMediaElement.Position = new TimeSpan(0, 0, 0, 0, 0);
            timelineSlider.Value = 0;
        }

        private void Testing()
        {
            myMediaElement.Pause();
            _sliderTimer.Stop();
            myMediaElement.Position = new TimeSpan(0, 0, 0, 0, 0);
            timelineSlider.Value = 0;
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
                MinX = 402;
                MinY = 320;
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
                double maxWidth = 800.0;
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
                //Canvas.SetLeft(closeButton, hotspotCanvas.Width - 52.0);
                imageScroll.Width = HotspotImage.Width;
                imageScroll.Height = HotspotImage.Height;
                HotspotTextBox.Visibility = Visibility.Hidden;
                textBoxScroll.Visibility = Visibility.Hidden;
                AudioScroll.Visibility = Visibility.Hidden;
                VideoScroll.Visibility = Visibility.Hidden;
                //this.Width = img.Width;
            }
            else if (m_hotspotData.Type.ToLower().Contains("text"))
            {
                HotspotTextBox.Text = m_hotspotData.Description;
                HotspotImage.Visibility = Visibility.Hidden;
                imageScroll.Visibility = Visibility.Hidden;
                HotspotImage.IsEnabled = false;
                imageScroll.IsEnabled = false;
                //HotspotTextBox.Visibility = Visibility.Visible;
                //textBoxScroll.Visibility = Visibility.Visible;
                AudioScroll.Visibility = Visibility.Hidden;
                VideoScroll.Visibility = Visibility.Hidden;
               // video.Visibility = Visibility.Hidden;
            }
            else  if (m_hotspotData.Type.ToLower().Contains("audio"))
            {
                //scatterItem.Height = 220;
                //hotspotCanvas.Height = 200;
                //InitializeComponent();
                //myMediaElement = new MediaElement();
                myMediaElement.MediaOpened += new RoutedEventHandler(myMediaElement_MediaOpened);
                myMediaElement.MediaEnded += new RoutedEventHandler(myMediaElement_MediaEnded); //need to fill in method
               // myMediaElement.ScrubbingEnabled = true;
                myMediaElement.LoadedBehavior = MediaState.Manual;
                String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + m_hotspotData.Description;
                myMediaElement.Source = new Uri(audioUri);
                AudioScroll.Visibility = Visibility.Visible;
                timelineSlider.Visibility = Visibility.Visible;

                //fire Media Opened
                myMediaElement.Play();
                myMediaElement.Pause();

                PlayButton.Click += new RoutedEventHandler(PlayButton_Click);
                PauseButton.Click += new RoutedEventHandler(PauseButton_Click);
                StopButton.Click += new RoutedEventHandler(StopButton_Click);
                //myMediaElement.ScrubbingEnabled = true;
                //audioName.Content = m_hotspotData.Description;
                //String description = 
                // MediaName.Content = m_hotspotData.Description.Substring(0,m_hotspotData.Description.Length-4); //will display the name of the media according to where the file saves

                this.showAudioIcon();
                //myMediaElement.ScrubbingEnabled = true;
                //mediaTimeLine.Source = new Uri(audioUri);


                _dragging = false;
                _sliderTimer = new System.Windows.Threading.DispatcherTimer();


                timelineSlider.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(timelineSlider_PreviewMouseLeftButtonDown);
                timelineSlider.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(timelineSlider_PreviewMouseLeftButtonUp);
                timelineSlider.PreviewTouchDown += new System.EventHandler<TouchEventArgs>(timelineSlider_PreviewTouchDown);
                timelineSlider.PreviewTouchUp += new System.EventHandler<TouchEventArgs>(timelineSlider_PreviewTouchUp);

                timelineSlider.IsMoveToPointEnabled = false;
                timelineSlider.SmallChange = 0;
                timelineSlider.LargeChange = 0;

                _sliderTimer.Interval = new TimeSpan(0, 0, 0, 0, SLIDER_TIMER_RESOLUTION);
                _sliderTimer.Tick += new EventHandler(sliderTimer_Tick);
                _sliderTimer.Stop();

            }
            else if (m_hotspotData.Type.ToLower().Contains("video"))
            {
                String videoUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Videos\\" + m_hotspotData.Description;
                LADSVideoBubble newVideo = new LADSVideoBubble(videoUri,500,500);
                newVideo._video.MediaOpened += new RoutedEventHandler(newVideo_Loaded);
                //newVideo.setPreferredSize(372, 268);
                //VideoScroll.Content = newVideo;
                //VideoScroll.Add(newVideo);

                //VideoScroll.Visibility = Visibility.Visible;
                video = newVideo;
                hotspotCanvas.Children.Add(video);
                Canvas.SetTop(video, 35);
                //Canvas.SetLeft(video, -30);
                hasVideo = true;

                this.SetCurrentValue(HeightProperty, newVideo.Height);
                this.SetCurrentValue(WidthProperty, newVideo.Width);
                hotspotCanvas.Width = newVideo.Width;
                hotspotCanvas.Height = newVideo.Height;
                this.Width = hotspotCanvas.Width;
                this.Height = hotspotCanvas.Height;
                //Canvas.SetLeft(closeButton, hotspotCanvas.Width - 52.0);
                VideoScroll.Width = newVideo.Width;
                VideoScroll.Height = newVideo.Height;
                HotspotTextBox.Visibility = Visibility.Hidden;
                textBoxScroll.Visibility = Visibility.Hidden;
            }
            Name.Content = m_hotspotData.Name;
            Double[] size = this.findImageSize();

            screenPosX = (m_msi.GetZoomableCanvas.Scale * m_hotspotData.PositionX *size[0]) - m_msi.GetZoomableCanvas.Offset.X;
            screenPosY = (m_msi.GetZoomableCanvas.Scale * m_hotspotData.PositionY *size[1]) - m_msi.GetZoomableCanvas.Offset.Y;
           
            this.Center = new Point(screenPosX + this.Width/2.0, screenPosY + this.Height/2.0);
           // Console.Out.WriteLine("center1" + this.Center);
           // m_parentScatterView.Items.Add(this);
        }

        void newVideo_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetCurrentValue(HeightProperty, (video as LADSVideoBubble)._video.Height);
            this.SetCurrentValue(WidthProperty, (video as LADSVideoBubble)._video.Width);
            hotspotCanvas.Width = (video as LADSVideoBubble)._video.Width;// +24.0;
            hotspotCanvas.Height = (video as LADSVideoBubble)._video.Height;// +47.0;
            VideoScroll.Width = hotspotCanvas.Width -24.0;
            VideoScroll.Height = hotspotCanvas.Height - 47.0;

            this.Width = hotspotCanvas.Width+8;
            this.Height = hotspotCanvas.Height+8;
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
        private void myMediaElement_MediaOpened(object sender, EventArgs e)
        {
            Console.WriteLine("trying to OPEN");
            if (!_hasBeenOpened)
            {
                String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + m_hotspotData.Description;
                myMediaElement.Source = new Uri(audioUri);
                Console.WriteLine("OPENED");
                myMediaElement.ScrubbingEnabled = true;
                timelineSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                myMediaElement.Play();
                myMediaElement.Pause();
                // Console.Out.WriteLine("duration" + myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                _hasBeenOpened = true;
            }
        }
        private void myMediaElement_MediaEnded(object sender, EventArgs e)
        {
            Console.WriteLine("MEDIA ENDED");
            myMediaElement.Position = new TimeSpan(0, 0, 0, 0, 0);
            _sliderTimer.Stop();
            myMediaElement.Pause();

            //hacky code
            //myMediaElement = null;
            //myMediaElement = new MediaElement();
            //_hasBeenOpened = false;
            //_dragging = false;
            //// Console.Out.WriteLine("audio is selected!");
            //AudioScroll.Visibility = Visibility.Visible;
            //String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + m_hotspotData.Description;
            //Console.WriteLine("audiouri is : " + audioUri);
            //timelineSlider.Visibility = Visibility.Visible;
            ////_audio = new MediaElement();
            ////_audio.Source = new Uri(audioUri, UriKind.RelativeOrAbsolute);
            //PlayButton.Click += new RoutedEventHandler(PlayButton_Click);
            //PauseButton.Click += new RoutedEventHandler(PauseButton_Click);
            //StopButton.Click += new RoutedEventHandler(StopButton_Click);
            ////myMediaElement.ScrubbingEnabled = true;
            //audioName.Content = m_hotspotData.Description;
            ////String description = 
            //// MediaName.Content = m_hotspotData.Description.Substring(0,m_hotspotData.Description.Length-4); //will display the name of the media according to where the file saves
            //myMediaElement.Source = new Uri(audioUri);
            //this.showAudioIcon();
            ////myMediaElement.ScrubbingEnabled = true;
            ////mediaTimeLine.Source = new Uri(audioUri);
            //myMediaElement.LoadedBehavior = MediaState.Manual;

            //_dragging = false;
            //_sliderTimer = new System.Windows.Threading.DispatcherTimer();
            //myMediaElement.MediaOpened += new RoutedEventHandler(myMediaElement_MediaOpened);
            //myMediaElement.MediaEnded += new RoutedEventHandler(myMediaElement_MediaEnded); //need to fill in method

            ////fire Media Opened
            //myMediaElement.Play();
            //myMediaElement.Pause();

            //timelineSlider.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(timelineSlider_PreviewMouseLeftButtonDown);
            //timelineSlider.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(timelineSlider_PreviewMouseLeftButtonUp);
            //timelineSlider.PreviewTouchDown += new System.EventHandler<TouchEventArgs>(timelineSlider_PreviewTouchDown);
            //timelineSlider.PreviewTouchUp += new System.EventHandler<TouchEventArgs>(timelineSlider_PreviewTouchUp);

            //timelineSlider.IsMoveToPointEnabled = false;
            //timelineSlider.SmallChange = 0;
            //timelineSlider.LargeChange = 0;

            //_sliderTimer.Interval = new TimeSpan(0, 0, 0, 0, SLIDER_TIMER_RESOLUTION);
            //_sliderTimer.Tick += new EventHandler(sliderTimer_Tick);


            ////_dragging = false;
            ////_hasBeenOpened = false;
            ////myMediaElement.MediaOpened -= new RoutedEventHandler(myMediaElement_MediaOpened);
            ////myMediaElement.MediaEnded -= new RoutedEventHandler(myMediaElement_MediaEnded);
            ////myMediaElement.MediaOpened += new RoutedEventHandler(myMediaElement_MediaOpened);
            ////myMediaElement.MediaEnded += new RoutedEventHandler(myMediaElement_MediaEnded);
            //Console.WriteLine("media element is: " + myMediaElement.IsLoaded);
            ////timelineSlider.Value = 0;

            ////myMediaElement.Play();
            ////myMediaElement.Pause();
        }
        //private void MediaTimeChanged(object sender, EventArgs e)
        //{
        //    timelineSlider.Value = myMediaElement.Position.TotalMilliseconds;
        //}

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try { myMediaElement.Volume = (double)volumeSlider.Value; }
            catch { }
        }


        //2ndtry
        public void timelineSlider_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _sliderTimer.Stop();
            timelineSlider.Value = (e.GetTouchPoint(timelineSlider).Position.X / timelineSlider.ActualWidth) * timelineSlider.Maximum;
            timelineSlider.PreviewTouchMove += new System.EventHandler<TouchEventArgs>(timelineSlider_PreviewTouchMove);
        }

        public void timelineSlider_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            int toPosition = (int)Math.Floor(timelineSlider.Value);
            if (toPosition >= timelineSlider.Maximum - 470)
            {
                toPosition = (int)(timelineSlider.Maximum - 480);
            }
            timelineSlider.PreviewTouchMove -= new EventHandler<TouchEventArgs>(timelineSlider_PreviewTouchMove);
            myMediaElement.Position = new TimeSpan(0, 0, 0, 0, toPosition);
            _sliderTimer.Start();
        }

        private void timelineSlider_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            
            if (!IsDragging())
            {
                double newValue = e.GetTouchPoint(timelineSlider).Position.X / timelineSlider.ActualWidth; //yields number between 0 and 1
                newValue = newValue * timelineSlider.Maximum; //should give number between 0 and Maximum (about 300000)
                //newValue = newValue < 0 ? 0 : newValue >= 1 ? .999 : newValue; //.999 as opposed to 1 because the MediaEnded event does not fire if the movie is scrolled to 100% manually. This stuff is WACKY.
                //newValue = newValue < 0 ? 0 : newValue >= timelineSlider.Maximum ? (timelineSlider.Maximum - 1) : newValue; //.999 as opposed to 1 because the MediaEnded event does not fire if the movie is scrolled to 100% manually. This stuff is WACKY.
                if (newValue < 0)
                {
                    Console.WriteLine("less than zero");
                    newValue = 0;
                }
                if (newValue >= (timelineSlider.Maximum - 470))
                {
                    newValue = (timelineSlider.Maximum - 480);
                    Console.WriteLine("greater than max");
                }
                timelineSlider.Value = newValue; //this should be in milliseconds
                Console.WriteLine("newValue is " + newValue);
            }
            
        }

        private void timelineSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _sliderTimer.Stop();
            timelineSlider.Value = (e.GetPosition(timelineSlider).X / timelineSlider.ActualWidth) *timelineSlider.Maximum;
            timelineSlider.PreviewMouseMove += new MouseEventHandler(timelineSlider_PreviewMouseMove);
        }

        private void timelineSlider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse Move");
            //myMediaElement.Pause();
            //_sliderTimer.Stop();
            if (!IsDragging())
            {
                Console.WriteLine("Not Dragging");
                double newValue = e.GetPosition(timelineSlider).X / timelineSlider.ActualWidth; //yields number between 0 and 1
                newValue = newValue * timelineSlider.Maximum; //should give number between 0 and Maximum (about 300000)
                //newValue = newValue < 0 ? 0 : newValue >= 1 ? .999 : newValue; //.999 as opposed to 1 because the MediaEnded event does not fire if the movie is scrolled to 100% manually. This stuff is WACKY.
                //newValue = newValue < 0 ? 0 : newValue >= (timelineSlider.Maximum - 480) ? (timelineSlider.Maximum -500) : newValue; //.999 as opposed to 1 because the MediaEnded event does not fire if the movie is scrolled to 100% manually. This stuff is WACKY.
                if (newValue < 0)
                {
                    Console.WriteLine("less than zero");
                    newValue = 0;
                }
                if (newValue >= (timelineSlider.Maximum - 470))
                {
                    newValue = (timelineSlider.Maximum - 480);
                    Console.WriteLine("greater than max");
                }
                Console.WriteLine("Is this being read?");
                timelineSlider.Value = newValue; //this should be in milliseconds
                //if (newValue >= (timelineSlider.Maximum - 101))
                //{
                //    this.Testing();
                //    timelineSlider.Value = 0;
                //}
                Console.WriteLine("newValue is " + newValue);
            }
            //double newValue2 = e.GetPosition(timelineSlider).X / timelineSlider.ActualWidth; //yields number between 0 and 1
            //newValue2 = newValue2 * timelineSlider.Maximum; //should give number between 0 and Maximum (about 300000)
            ////newValue = newValue < 0 ? 0 : newValue >= 1 ? .999 : newValue; //.999 as opposed to 1 because the MediaEnded event does not fire if the movie is scrolled to 100% manually. This stuff is WACKY.
            ////newValue = newValue < 0 ? 0 : newValue >= (timelineSlider.Maximum - 480) ? (timelineSlider.Maximum -500) : newValue; //.999 as opposed to 1 because the MediaEnded event does not fire if the movie is scrolled to 100% manually. This stuff is WACKY.
            //if (newValue2 < 0)
            //{
            //    Console.WriteLine("less than zero");
            //    newValue2 = 0;
            //}
            //if (newValue2 >= (timelineSlider.Maximum - 470))
            //{
            //    newValue2 = (timelineSlider.Maximum - 480);
            //    Console.WriteLine("greater than max");
            //}
            ////myMediaElement.Play();
            ////_sliderTimer.Start();
        }

        private void timelineSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int toPosition = (int)Math.Floor(timelineSlider.Value);
            if (toPosition >= timelineSlider.Maximum - 470)
            {
                toPosition = (int)(timelineSlider.Maximum - 480);
            }
            timelineSlider.PreviewMouseMove -= new MouseEventHandler(timelineSlider_PreviewMouseMove);
            myMediaElement.Position = new TimeSpan(0, 0, 0, 0, toPosition);
            _sliderTimer.Start();
        }

        private void sliderTimer_Tick(object sender, EventArgs e)
        {
            _sliderTimer.Stop();
            timelineSlider.Value = myMediaElement.Position.TotalMilliseconds;
            Console.WriteLine("myMediaElement.Position.TotalMilliseconds is: " + myMediaElement.Position.TotalMilliseconds);
            Console.WriteLine("slider value is: " + timelineSlider.Value);
            Console.WriteLine("slider timeer max is: " + timelineSlider.Maximum);
            if (myMediaElement.Position.TotalMilliseconds >= (timelineSlider.Maximum - 500))
            {
                myMediaElement.Pause();
                Console.WriteLine("HSHOULD WORK GODDAMMIT");
                this.Testing();
                //myMediaElement.Stop();
                //_sliderTimer.Stop();
                //myMediaElement.Position = new TimeSpan(0, 0, 0, 0, 21);
                //timelineSlider.Value = 21;
                timelineSlider.Value = 0;
                return;
            }
            _sliderTimer.Start();
        }

        //#endregion

        private void timelineSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _dragging = true;
        }

        private void timelineSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _dragging = false;
        }

        public bool IsDragging()
        {
            return _dragging;
        }

        private void scatterItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 800 || e.NewSize.Width>800 || e.NewSize.Height < MinY || e.NewSize.Width < MinX)
            {
                Width = e.PreviousSize.Width;
                Height = e.PreviousSize.Height;
            }
            if (m_hotspotData.Type.ToLower().Contains("image"))
            {
                hotspotCanvas.Width = e.NewSize.Width-8;
                hotspotCanvas.Height = e.NewSize.Height-8;
                HotspotImage.Height = hotspotCanvas.Height - 47.0;
                HotspotImage.Width = hotspotCanvas.Width - 24.0;
                imageScroll.Height = hotspotCanvas.Height - 47.0;
                imageScroll.Width = hotspotCanvas.Width - 24.0; 
                
            }
            if (hasVideo)
            {
                (video as LADSVideoBubble).Resize(e.NewSize.Width, e.NewSize.Height, true);
            }
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
