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
using Microsoft.Surface.Presentation.Generic;
using System.Windows.Threading;
using System.Threading;


namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for HotspotDetailsControl.xaml
    /// </summary>
    public partial class HotspotDetailsControl : ScatterViewItem
    {
        MediaElement myMediaElement;
        ScatterView m_parentScatterView;
        Canvas m_parentCanvas;
        public Hotspot m_hotspotData;
        Boolean hasVideo;
        MediaElement videoElement;
        Boolean _dragging;
        private const int SLIDER_TIMER_RESOLUTION = 20; //how often we update the slider based on the video position, in milliseconds
        private System.Windows.Threading.DispatcherTimer _sliderTimer;
        Boolean _hasBeenOpened;
        double minX;
        bool firstTime = false;
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

        public double _volume;
        Size windowSize;
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
            this.PreviewMouseWheel += new MouseWheelEventHandler(HotspotDetailsControl_PreviewMouseWheel);
            isOnScreen = false;
            hasVideo = false;
            _hasBeenOpened = false;
            _volume = .5;
           // windowSize = new Size();
            sizeChanged = true;
            firstTime = true;
        }

        
      
        //when not muted
        public void showAudioIcon()
        {
            BitmapImage volume = new BitmapImage();
            volume.BeginInit();
            String volumePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\volume.png";
            volume.UriSource = new Uri(@volumePath);
            volume.EndInit();
            Volume.Source = volume;
        }

        //when audio is muted
        public void showMuteIcon()
        {
            BitmapImage mute = new BitmapImage();
            mute.BeginInit();
            String mutePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\muteIcon1.png";
            mute.UriSource = new Uri(@mutePath);
            mute.EndInit();
            Volume.Source = mute;
        }
        /// <summary>
        /// Called when the closed button is clicked.
        /// remove from screen.
        /// </summary>
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
           
            m_parentScatterView.Items.Remove(this);
                      
            isOnScreen = false;
            if (hasVideo)
            {

                (videoElement.Parent as Panel).Children.Remove(videoElement);
                videoElement.Pause();
                videoTimer.Stop();
                videoElement = null;
                              
            }
            if (m_hotspotData.Type.ToLower().Contains("audio")) {
                (myMediaElement.Parent as Panel).Children.Remove(myMediaElement);
                myMediaElement.Pause();
                _sliderTimer.Stop();
                myMediaElement.Position = new TimeSpan(0, 0, 0, 0, 0);
                timelineSlider.Value = 0;
                myMediaElement = null;

            }

            sizeChanged = false;
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
        /// Called when the play button is clicked.
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Position = new TimeSpan(0,0,0,0,(int)timelineSlider.Value);
            myMediaElement.Play();
            _sliderTimer.Start();
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
        /// 
        public void updateScreenLocation(MultiScaleImage msi)
        {
            Double[] size = this.findImageSize();

            screenPosX = (msi.GetZoomableCanvas.Scale * m_hotspotData.PositionX * size[0]) - msi.GetZoomableCanvas.Offset.X;
            screenPosY = (msi.GetZoomableCanvas.Scale * m_hotspotData.PositionY * size[1]) - msi.GetZoomableCanvas.Offset.Y;
            this.Center = new Point(screenPosX + this.Width / 2.0, screenPosY + this.Height / 2.0); // uncomment this to make the detail control move as the user pans

        }

        private DispatcherTimer videoTimer;

        /// <summary>
        /// Load data inside the hotspot detail control, can be text or image.
        /// </summary>
        /// 
       
        private void scatterItem_Loaded(object sender, RoutedEventArgs e)
        {
            firstTime = true;
            //if (m_hotspotData.Type.ToLower().Contains("image"))
            //{
            //    MinX = 402;
            //    MinY = 320;
            //    BitmapImage img = new BitmapImage();
            //    String imgUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + m_hotspotData.Description;
            //    img.BeginInit();
            //    img.UriSource = new Uri(imgUri, UriKind.Absolute);
            //    img.CacheOption = BitmapCacheOption.OnLoad;
            //    img.EndInit();
            //    HotspotImage.Source = img;
            //    HotspotImage.Visibility = Visibility.Visible;
            //    HotspotImage.IsEnabled = true;
            //    double maxWidth = 800.0;
            //    if (img.PixelWidth > maxWidth)
            //    {
            //        HotspotImage.SetCurrentValue(HeightProperty, maxWidth * img.PixelHeight / img.PixelWidth);
            //        HotspotImage.SetCurrentValue(WidthProperty, maxWidth);
            //    }
            //    else
            //    {
            //        HotspotImage.SetCurrentValue(HeightProperty, (double) img.PixelHeight);
            //        HotspotImage.SetCurrentValue(WidthProperty, (double) img.PixelWidth);
            //    }
            //    this.SetCurrentValue(HeightProperty, HotspotImage.Height + 47.0);
            //    this.SetCurrentValue(WidthProperty, HotspotImage.Width + 24.0);
            //    hotspotCanvas.Width = HotspotImage.Width + 24.0;
            //    hotspotCanvas.Height = HotspotImage.Height + 47.0;
                
            //    this.Width = hotspotCanvas.Width;
            //    this.Height = hotspotCanvas.Height;
            //    HotspotTextBox.Visibility = Visibility.Hidden;
            //    //textBoxScroll.Visibility = Visibility.Hidden;
            //    VideoStackPanel.Visibility = Visibility.Collapsed;
            //    AudioScroll.Visibility = Visibility.Hidden;
            //}
            if (m_hotspotData.Type.ToLower().Contains("text"))
            {
                HotspotTextBox.Text = m_hotspotData.Description;
                HotspotTextBox.Visibility = Visibility.Visible;
                //textBoxScroll.Visibility = Visibility.Visible;
                HotspotImage.Visibility = Visibility.Hidden;
                HotspotImage.IsEnabled = false;
                VideoStackPanel.Visibility = Visibility.Collapsed;
                AudioScroll.Visibility = Visibility.Hidden;
                this.CanScale = false;
            }
            else  if (m_hotspotData.Type.ToLower().Contains("audio"))
            {
                Width = 300;
                Height = 200;

                hotspotCanvas.Width = Width - 8;
                hotspotCanvas.Height = Height - 8;
                VideoStackPanel.Visibility = Visibility.Collapsed;
                //textBoxScroll.Visibility = Visibility.Collapsed;

                //creates new audio
                myMediaElement = new MediaElement();
                String newaudioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + m_hotspotData.Description;
                myMediaElement.Source = new Uri(newaudioUri);
                AudioScroll.Children.Add(myMediaElement);
              
                myMediaElement.Width = VideoStackPanel.Width;
                myMediaElement.Height = VideoStackPanel.Height - 30;
                Name.Width = Width - (422 - 335);

                VideoStackPanel.Visibility = Visibility.Collapsed;
                videoCanvas.Visibility = Visibility.Collapsed;
                //textBoxScroll.Visibility = Visibility.Collapsed;

                HotspotTextBox.Visibility = Visibility.Hidden;
                myMediaElement.MediaOpened += new RoutedEventHandler(myMediaElement_MediaOpened);
                myMediaElement.MediaEnded += new RoutedEventHandler(myMediaElement_MediaEnded); //need to fill in method
                myMediaElement.LoadedBehavior = MediaState.Manual;
                String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + m_hotspotData.Description;
                myMediaElement.Source = new Uri(audioUri);
                AudioScroll.Visibility = Visibility.Visible;
                timelineSlider.Visibility = Visibility.Visible;
                VideoStackPanel.Visibility = Visibility.Collapsed;

                AudioScroll.Width = hotspotCanvas.Width - 24;
                AudioScroll.Height = hotspotCanvas.Height + AudioTextbox.ActualHeight - 47;

                
                if (m_hotspotData.fileDescription == "")
                {
                    AudioTextbox.Visibility = Visibility.Hidden;
                    
                }
                else
                {
                    AudioTextbox.Content = "Description:  " + m_hotspotData.fileDescription;
                    AudioTextbox.Width = hotspotCanvas.Width;
                }
                this.UpdateLayout();

                AudioTextbox.Width = this.ActualWidth-8;
               
                hotspotCanvas.SetCurrentValue(HeightProperty, hotspotCanvas.Height + AudioTextbox.ActualHeight+40);
                this.SetCurrentValue(HeightProperty, hotspotCanvas.Height+8);
                //fire Media Opened in order to set the actual length
                myMediaElement.Play();
                myMediaElement.Pause();
                this.CanScale = false;
                PlayButton.Click += new RoutedEventHandler(PlayButton_Click);
                PauseButton.Click += new RoutedEventHandler(PauseButton_Click);
                StopButton.Click += new RoutedEventHandler(StopButton_Click);
            
                this.showAudioIcon();
              
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
                videoElement = new MediaElement();
                videoElement.Source = new Uri(videoUri);
                videoTimer = new DispatcherTimer();
                videoTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                videoTimer.Tick += new EventHandler(videoTimer_Tick);
                videoElement.MediaOpened += new RoutedEventHandler(videoElement_MediaOpened);
                videoElement.MediaEnded += VideoElementDone;
                videoElement.LoadedBehavior = MediaState.Manual;
                videoElement.Play();
                videoElement.Pause();

                VideoStackPanel.Children.Insert(1,videoElement);
                //textBoxScroll.Visibility = Visibility.Collapsed;
                AudioScroll.Visibility = Visibility.Collapsed;
                hasVideo = true;
                Grid g = new Grid();
                g.Height = 30;
                g.Width = 100;
                g.HorizontalAlignment = HorizontalAlignment.Left;
                Polygon p = new Polygon();
                PointCollection ppoints = new PointCollection();
                ppoints.Add(new System.Windows.Point(4, 5));
                ppoints.Add(new System.Windows.Point(4, 26));
                ppoints.Add(new System.Windows.Point(23, 14));
                p.Points = ppoints;
                p.Fill = Brushes.Green;
                //p.Opacity = 1;
                p.Visibility = Visibility.Visible;
                p.Margin = new Thickness(0, 0, 0, 0);
                p.Stroke = Brushes.Black;
                p.StrokeThickness = 1;
                p.HorizontalAlignment = HorizontalAlignment.Left;
                p.VerticalAlignment = VerticalAlignment.Center;
                p.Height = 36;
                p.Width = 30;

                Polygon pause = new Polygon();
                PointCollection pausepoints = new PointCollection();
                pausepoints.Add(new System.Windows.Point(29, -1));
                pausepoints.Add(new System.Windows.Point(29, 22));
                pausepoints.Add(new System.Windows.Point(37, 22));
                pausepoints.Add(new System.Windows.Point(37, -1));
                pause.Points = pausepoints;
                pause.Fill = Brushes.Blue;
                //p.Opacity = 1;
                pause.Visibility = Visibility.Visible;
                pause.Margin = new Thickness(0, 0, 0, 0);
                pause.Stroke = Brushes.Black;
                pause.StrokeThickness = 1;
                pause.HorizontalAlignment = HorizontalAlignment.Left;
                pause.VerticalAlignment = VerticalAlignment.Center;

                Polygon pause2 = new Polygon();
                PointCollection pausepoints2 = new PointCollection();
                pausepoints2.Add(new System.Windows.Point(43, -1));
                pausepoints2.Add(new System.Windows.Point(43, 22));
                pausepoints2.Add(new System.Windows.Point(51, 22));
                pausepoints2.Add(new System.Windows.Point(51, -1));
                pause2.Points = pausepoints2;
                pause2.Fill = Brushes.Blue;
                //p.Opacity = 1;
                pause2.Visibility = Visibility.Visible;
                pause2.Margin = new Thickness(0, 0, 0, 0);
                pause2.Stroke = Brushes.Black;
                pause2.StrokeThickness = 1;
                pause2.HorizontalAlignment = HorizontalAlignment.Left;
                pause2.VerticalAlignment = VerticalAlignment.Center;

                g.Children.Add(p);
                g.Children.Add(pause);
                g.Children.Add(pause2);
                g.Visibility = Visibility.Visible;
                SurfacePlayButton.Content = g;

                HotspotTextBox.Visibility = Visibility.Hidden;
                Canvas.SetTop(VideoStackPanel, 35);
                if (m_hotspotData.fileDescription == "")
                {
                    VideoText.Visibility = Visibility.Hidden;
                }
                else
                {
                    VideoText.Content = "Description:  " + m_hotspotData.fileDescription;
                }
               
            }
            else if((m_hotspotData.Type.ToLower().Contains("image")))
            {
                sizeChanged = false;
                BitmapImage img = new BitmapImage();
                String imgUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + m_hotspotData.Description;
                img.BeginInit();
                img.UriSource = new Uri(imgUri, UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                HotspotImageMix.Source = img;
                HotspotImageMix.Visibility = Visibility.Visible;
                HotspotImageMix.IsEnabled = true;
                double maxWidth = 600.0;
                if (img.PixelWidth > maxWidth)
                {
                    HotspotImageMix.SetCurrentValue(HeightProperty, maxWidth * img.PixelHeight / img.PixelWidth);
                    HotspotImageMix.SetCurrentValue(WidthProperty, maxWidth);
                }
                else
                {
                    HotspotImageMix.SetCurrentValue(HeightProperty, (double)img.PixelHeight);
                    HotspotImageMix.SetCurrentValue(WidthProperty, (double)img.PixelWidth);
                }
                
                if (m_hotspotData.fileDescription == "")
                {
                    HotspotTextBoxMix.Visibility = Visibility.Hidden;
                    
                }
                else
                {
                    HotspotTextBoxMix.Content = "Description:  "+m_hotspotData.fileDescription;
                    HotspotTextBoxMix.Width = HotspotImageMix.Width;
                }
               
                this.UpdateLayout();
                this.SetCurrentValue(HeightProperty, HotspotImageMix.Height + 10.0 + HotspotTextBoxMix.ActualHeight);
                this.SetCurrentValue(WidthProperty, HotspotImageMix.Width + 10.0);
                HotspotTextBox.Visibility = Visibility.Hidden;
                
                hotspotCanvas.Width = HotspotImageMix.Width;
                

                hotspotCanvas.Height = HotspotImageMix.Height + HotspotTextBoxMix.ActualHeight;
                
                
                //textBoxScroll.Visibility = Visibility.Visible;
                Mix.Visibility = Visibility.Visible;
                Canvas.SetZIndex(Mix, 100);
                this.UpdateLayout();

                this.CanScale = true;

                Canvas.SetZIndex(closeButton, 100);
                //HotspotTextBox.Visibility = Visibility.Hidden;
                //textBoxScroll.Visibility = Visibility.Hidden;
                VideoStackPanel.Visibility = Visibility.Collapsed;
                AudioScroll.Visibility = Visibility.Hidden;
               // windowSize = new Size(this.Width, this.Height);
             
            }
            Name.Content = m_hotspotData.Name;
            Double[] size = this.findImageSize();

            screenPosX = (m_msi.GetZoomableCanvas.Scale * m_hotspotData.PositionX *size[0]) - m_msi.GetZoomableCanvas.Offset.X;
            screenPosY = (m_msi.GetZoomableCanvas.Scale * m_hotspotData.PositionY *size[1]) - m_msi.GetZoomableCanvas.Offset.Y;
           
            this.Center = new Point(screenPosX + this.Width/2.0, screenPosY + this.Height/2.0);
            sizeChanged = true;
            firstTime = false;
        }


        void SurfaceTimelineSlider_ManipulationStarted(object sender, EventArgs e)
        {
            videoTimer.Tick -= videoTimer_Tick;
        }
        void SurfaceTimelineSlider_ManipulationCompleted(object sender, EventArgs e)
        {
            videoTimer.Tick += videoTimer_Tick;
        }
        void videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            videoTimer.Start();
            SurfaceTimelineSlider.Maximum = videoElement.NaturalDuration.TimeSpan.TotalMilliseconds - 1;
            SurfaceTimelineSlider.ManipulationStarted+= SurfaceTimelineSlider_ManipulationStarted;
            SurfaceTimelineSlider.ManipulationCompleted += SurfaceTimelineSlider_ManipulationCompleted;
            double aspectratio = (double)videoElement.NaturalVideoHeight / (double)videoElement.NaturalVideoWidth;
            if (aspectratio > .8)
            {
                videoElement.Height = 453;
                videoElement.Width = 453.0 / aspectratio;
            }
            else
            {
                videoElement.Width = 476;
                videoElement.Height = 476.0 * aspectratio;
            }
            VideoStackPanel.Width = videoElement.Width;
            VideoStackPanel.Height = videoElement.Height+30.0;
            hotspotCanvas.Width = VideoStackPanel.Width + 24;
            hotspotCanvas.Height = VideoStackPanel.Height + 47;
            Canvas.SetLeft(VideoStackPanel, 12);
            Width = hotspotCanvas.Width + 8;
            Height = hotspotCanvas.Height + 8;
            minX = Width / 2;
            minY = Height / 2;
        }

        void videoTimer_Tick(object sender, EventArgs e)
        {
            SurfaceTimelineSlider.ValueChanged -= SurfaceTimelineSlider_ValueChanged;
            SurfaceTimelineSlider.Value = videoElement.Position.TotalMilliseconds;
            SurfaceTimelineSlider.ValueChanged += SurfaceTimelineSlider_ValueChanged;
        }

        void newVideo_Loaded(object sender, RoutedEventArgs e)
        {
            (video as LADSVideoBubble).Resize(366,272);
            hotspotCanvas.Width = video.ActualWidth + 24;
            hotspotCanvas.Height = video.ActualHeight + 47;
            minX = hotspotCanvas.Width + 8;
            minY = hotspotCanvas.Height + 8;
            this.Width = hotspotCanvas.Width + 8;
            this.Height = hotspotCanvas.Height + 8;
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
            if (!_hasBeenOpened)
            {
                String audioUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + m_hotspotData.Description;
                myMediaElement.Source = new Uri(audioUri);
                myMediaElement.ScrubbingEnabled = true;
                timelineSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                myMediaElement.Play();
                myMediaElement.Pause();
                _hasBeenOpened = true;
            }
        }
        private void myMediaElement_MediaEnded(object sender, EventArgs e)
        {
            myMediaElement.Position = new TimeSpan(0, 0, 0, 0, 0);
            _sliderTimer.Stop();
            myMediaElement.Pause();
            _sliderTimer.Stop();
          
        }
    

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try 
            { 
                myMediaElement.Volume = (double)volumeSlider.Value;
                   
            }
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
                newValue = newValue * timelineSlider.Maximum; //should give number between 0 and Maximum
               
                if (newValue < 0)
                {
                   
                    newValue = 0;
                }
                if (newValue >= (timelineSlider.Maximum - 470))
                {
                    newValue = (timelineSlider.Maximum - 480);
                    
                }
                timelineSlider.Value = newValue; //this should be in milliseconds
              
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
           
            if (!IsDragging())
            {
                double newValue = e.GetPosition(timelineSlider).X / timelineSlider.ActualWidth; //yields number between 0 and 1
                newValue = newValue * timelineSlider.Maximum; //gives number between 0 and Maximum 
             
                if (newValue < 0)
                {
                    newValue = 0;
                }
                if (newValue >= (timelineSlider.Maximum - 470))
                {
                    newValue = (timelineSlider.Maximum - 480);
                }
                timelineSlider.Value = newValue; //this should be in milliseconds
            }
           
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
            if (myMediaElement.Position.TotalMilliseconds >= (timelineSlider.Maximum - 500))
            {
                myMediaElement.Pause();
                this.Testing();
            
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
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size actual = new Size();
         
            if (arrangeBounds.Width > 800)
            {
                arrangeBounds.Width = 800;
                arrangeBounds.Height = 615;

            }
            else if (arrangeBounds.Width < 400)
            {
                arrangeBounds.Width = 400;
                arrangeBounds.Height = 300;
            }
            
            actual.Width = arrangeBounds.Width;
            actual.Height = arrangeBounds.Height;
            
            //Console.Out.WriteLine("height" + arrangeBounds.Height);
            //Console.Out.WriteLine("width" + arrangeBounds.Width);
           
            if (m_hotspotData.Type.ToLower().Contains("video"))
            {
                VideoText.Width = actual.Width;
                this.updateLocation();
                //VideoStackPanel.Width = actual.Width - 6;
                //VideoStackPanel.Measure(new Size(actual.Width - 6, actual.Height - 10));
                //VideoStackPanel.Arrange(new Rect(3, 10, actual.Width - 12, actual.Height - 20));

                //VideoText.Visibility = Visibility.Visible;

                if (videoElement != null)
                {
                    //VideoStackPanel.Width = actual.Width - 10;
                    //VideoStackPanel.Height = actual.Height - 10;
                    //this.UpdateLayout();
                    //VideoStackPanel.Measure(new Size(VideoStackPanel.ActualWidth, VideoStackPanel.ActualHeight));
                    //VideoStackPanel.Arrange(new Rect(3, 8, VideoStackPanel.ActualWidth, VideoStackPanel.ActualHeight));
                    //this.UpdateLayout();
                    videoElement.Width = actual.Width - 20;
                    videoElement.Height = actual.Height - 15;
                    this.UpdateLayout();
                    videoElement.Measure(new Size(videoElement.ActualWidth, videoElement.ActualHeight));
                    videoElement.Arrange(new Rect(10, 10, videoElement.ActualWidth, videoElement.ActualHeight));
                    this.UpdateLayout();

                    VideoText.Width = actual.Width - 8;
                    this.UpdateLayout();

                    VideoStackPanel.Width = actual.Width - 20;
                    VideoStackPanel.Height = actual.Height + VideoText.ActualHeight + 40;
                    this.UpdateLayout();
                    VideoStackPanel.Measure(new Size(VideoStackPanel.ActualWidth, VideoStackPanel.ActualHeight));
                    VideoStackPanel.Arrange(new Rect(10, 8, VideoStackPanel.ActualWidth, VideoStackPanel.ActualHeight));
                    this.UpdateLayout();

                    VideoText.Measure(new Size(VideoText.ActualWidth, VideoText.ActualHeight));
                    VideoText.Arrange(new Rect(4, VideoStackPanel.ActualHeight - VideoText.ActualHeight - 30, VideoText.ActualWidth, VideoText.ActualHeight));
                    this.UpdateLayout();

                }
                hotspotCanvas.Height = VideoStackPanel.ActualHeight + 40;
                hotspotCanvas.Width = actual.Width;
                this.UpdateLayout();
                hotspotCanvas.Measure(new Size(hotspotCanvas.ActualWidth, hotspotCanvas.ActualHeight));
                hotspotCanvas.Arrange(new Rect(0, 0, hotspotCanvas.ActualWidth, hotspotCanvas.ActualHeight));

            }
            //if (m_hotspotData.Type.ToLower().Contains("image"))
            else
            {

                HotspotImageMix.Height = actual.Height - 20;
                HotspotImageMix.Width = actual.Width - 10;
                HotspotImageMix.Measure(new Size(actual.Width - 10, actual.Height - 20));

                HotspotImageMix.UpdateLayout();
                this.UpdateLayout();

                HotspotTextBoxMix.Width = actual.Width;

                this.UpdateLayout();

                hotspotCanvas.Height = actual.Height;
                hotspotCanvas.Width = actual.Width;
                hotspotCanvas.Measure(new Size(actual.Width, actual.Height));
                hotspotCanvas.Arrange(new Rect(0, 0, actual.Width, actual.Height));
                //this.UpdateLayout();
                //HotspotTextBoxMix.Measure(new Size(HotspotTextBoxMix.ActualWidth, HotspotTextBoxMix.ActualHeight));
                HotspotTextBoxMix.UpdateLayout();
                HotspotImageMix.Arrange(new Rect(4, 30, actual.Width - 12, actual.Height - 20));
                HotspotTextBoxMix.Arrange(new Rect(0, actual.Height - 3, HotspotTextBoxMix.ActualWidth, HotspotTextBoxMix.ActualHeight));
                //updateLocation();
                HotspotTextBoxMix.UpdateLayout();
                this.UpdateLayout();
            }
            //else
            //{
            //    hotspotCanvas.Width = actual.Width;
            //    hotspotCanvas.Height = actual.Height;
            //}
                //for (int i = 0; i < VideoStackPanel.Children.Count; i++)
                //{
                //    VideoStackPanel.Measure(new Size(actual.Width - 6, actual.Height - 10));
                //    VideoStackPanel.Children[i].Arrange(new Rect(3, 10, actual.Width - 12, actual.Height - 20));
                //}
            //}
            //else
            //{
                //VideoText.Width = actual.Width;
                //this.UpdateLayout();
                //hotspotCanvas.Height = actual.Height;
                //hotspotCanvas.Width = actual.Width;
                //hotspotCanvas.Measure(new Size(actual.Width, actual.Height));
                //hotspotCanvas.Arrange(new Rect(0, 0, actual.Width, actual.Height));
                //VideoStackPanel.Height = actual.Height - 10;
                //VideoStackPanel.Width = actual.Width - 6;
                //VideoStackPanel.Measure(new Size(actual.Width - 6, actual.Height - 10));
                //VideoStackPanel.Arrange(new Rect(3, 10, actual.Width - 12, actual.Height - 20));
                //VideoText.Arrange(new Rect(0, actual.Height - 3, VideoText.ActualWidth, VideoText.ActualHeight));
                //if (!firstTime)
                //{
                //    videoElement.Width = VideoStackPanel.Width;
                //}
                //this.UpdateLayout();

            //}
            //HotspotTextBoxMix.Refresh();
            //Thread.Sleep(500);
            //double actualHeight = HotspotTextBoxMix.ActualHeight;
            //HotspotImageMix.Measure(new Size(hotspotCanvas.Width - 12, hotspotCanvas.Height - actualHeight - 12));
            //HotspotImageMix.Arrange(new Rect(4, 4, hotspotCanvas.Width - 12, hotspotCanvas.Height - HotspotTextBoxMix.ActualHeight - 12));
            //HotspotTextBoxMix.Measure(new Size(hotspotCanvas.Width,actualHeight));
            //HotspotTextBoxMix.Arrange(new Rect(0,hotspotCanvas.Height-10-HotspotImageMix.Height,actual.Width-12,actualHeight));
            //Mix.Measure(new Size(hotspotCanvas.Width - 12, hotspotCanvas.Height - 12));
            //Mix.Arrange(new Rect(4, 4, hotspotCanvas.Width - 12, hotspotCanvas.Height - 12));
            //windowSize = actual;
            //this.Refresh();
            //Thread.Sleep(500);
            return actual;
        }
        void updateLocation()
        {
            Canvas.SetLeft(HotspotTextBoxMix, 0);
        }
        bool sizeChanged = false;
        private void scatterItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            //if (e.NewSize.Height > 400 && e.NewSize.Width > 400)
            //{
            //    this.Height = e.NewSize.Height;
            //    this.Width = e.NewSize.Width;
            //}
            //else
            //{
            //    this.Width = 400;
            //    this.Height =300;

            //}
            //AllowUIToUpdate();
            //this.UpdateLayout();
            //this.Center = new Point(this.Center.X+1, this.Center.Y+1);

            //if (e.NewSize.Height > 800 || e.NewSize.Width > 800 || e.NewSize.Height < MinY || e.NewSize.Width < MinX)
            //{
            //    Width = e.PreviousSize.Width;
            //    Height = e.PreviousSize.Height;
            //}

            //if (m_hotspotData.Type.ToLower().Contains("video"))
            //{
            //    if (videoElement != null && !sizeChanged)
            //    {
            //        hotspotCanvas.Width = Width - 8;
            //        hotspotCanvas.Height = Height - 8;
            //        VideoStackPanel.Width = hotspotCanvas.Width - 24;
            //        VideoStackPanel.Height = hotspotCanvas.Height + VideoText.Height - 47;
            //        videoElement.Width = VideoStackPanel.Width;
            //        videoElement.Height = VideoStackPanel.Height - 30 - VideoText.Height;

            //        SurfaceTimelineSlider.Width = hotspotCanvas.Width - 180;
            //        Name.Width = Width - (422 - 335);

            //        this.UpdateLayout();
            //        hotspotCanvas.Height = hotspotCanvas.Height + VideoText.ActualHeight;
            //        this.Height = hotspotCanvas.Height + 8;
            //        windowSize = new Size(this.Width, this.Height);
            //    }
            //}

            //if (m_hotspotData.Type.ToLower().Contains("image"))
            //{
            //    //hotspotCanvas.Width = e.NewSize.Width - 8;
            //    //hotspotCanvas.Height = e.NewSize.Height - 8;
            //    //HotspotImage.Height = hotspotCanvas.Height - 47.0;
            //    //HotspotImage.Width = hotspotCanvas.Width - 24.0;
            //    //Name.Width = Width - (422 - 335);
            //    //Canvas.SetLeft(HotspotImage, 12);
            //    //Canvas.SetTop(HotspotImage, 35);
            //    //windowSize = new Size(this.Width, this.Height);
            //    //ScaleTransform tran = new ScaleTransform();
            //    //tran.ScaleY = e.NewSize.Height/e.PreviousSize.Height;
            //    //tran.ScaleX = e.NewSize.Width / e.PreviousSize.Width;
            //    //tran.ScaleY = 1.1;
            //    //tran.ScaleX = 1.1;
            //    //this.RenderTransform = tran;
            //}
        }
        void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate(object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }

            bool mousewheel = false;
        void HotspotDetailsControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            mousewheel = true;
            this.Refresh();
            Thread.Sleep(500);
            if (m_hotspotData.Type.ToLower().Contains("video") || m_hotspotData.Type.ToLower().Contains("image"))
            {

                if (this.Height > 350 && this.Width > 350)
                {
                    if (e.Delta < 0)
                    {
                        this.Width *= 0.95;
                        this.Height *= 0.95;
                        if (m_hotspotData.Type.ToLower().Contains("image"))
                        {
                           // HotspotTextBoxMix.UpdateLayout();
                            //this.UpdateLayout();
                            this.Height = this.Height;
                            this.Width = this.Width;
                            //ScaleTransform tran = new ScaleTransform();
                            //tran.ScaleY = 0.95;
                            //tran.ScaleX = 0.95;
                            //hotspotCanvas.RenderTransform = tran;

                            //HotspotImageMix.Height *= 0.95;
                            //HotspotImageMix.Width *= 0.95;

                            //HotspotTextBoxMix.Width *= 0.95;
                            //hotspotCanvas.Height *= 0.95;
                            //hotspotCanvas.Width *= 0.95;
                            //this.UpdateLayout();
                            //this.SetCurrentValue(HeightProperty, HotspotImageMix.Height + 8.0 + HotspotTextBoxMix.ActualHeight);
                            //windowSize = new Size(this.Width, this.Height);
                        }
                        else
                        {
                            // SurfacePlayButton.Height *=0.95;
                            // SurfacePlayButton.Width *=0.95;
                            //SurfaceTimelineSlider.Height *=0.95;
                            //SurfaceTimelineSlider.Width *= 0.95;
                            //hotspotCanvas.Height *= 0.95;
                            //hotspotCanvas.Width *= 0.95;
                            //VideoStackPanel.Height *= 0.95;
                            //VideoStackPanel.Width *= 0.95;
                            //videoElement.Height *= 0.95;
                            //videoElement.Width *= 0.95;

                            //VideoText.Width *= 0.95;
                            //this.UpdateLayout();
                            //hotspotCanvas.SetCurrentValue(HeightProperty, VideoStackPanel.ActualHeight + 18);
                            //this.Height = hotspotCanvas.Height + 12;
                            //windowSize = new Size(this.Width, this.Height);
                        }
                    }
                }


                if (e.Delta > 0)
                {
                    if (this.Height < 800 && this.Width < 800)
                    {

                        this.Width /= 0.95;
                        this.Height /= 0.95;
                        if (m_hotspotData.Type.ToLower().Contains("image"))
                        {
                            //ScaleTransform tran = new ScaleTransform();
                            //tran.ScaleY = 1.1;
                            //tran.ScaleX = 1.1;
                            //hotspotCanvas.RenderTransform = tran;

                            //this.Height = hotspotCanvas.Height + 30;
                            //this.Width = hotspotCanvas.Width + 30;
                            //HotspotImageMix.Height /= 0.95;
                            //HotspotImageMix.Width /= 0.95;

                            //HotspotTextBoxMix.Width /= 0.95;
                            //this.UpdateLayout();
                            //this.SetCurrentValue(HeightProperty, HotspotImageMix.Height + 8.0 + HotspotTextBoxMix.ActualHeight);
                            //Canvas.SetLeft(HotspotImage, 12);
                            //Canvas.SetTop(HotspotImage, 35);
                            //windowSize = new Size(this.Width, this.Height);
                        }
                        else
                        {
                            //hotspotCanvas.Height /= 0.95;
                            //hotspotCanvas.Width /= 0.95;
                            //SurfaceTimelineSlider.Width /= 0.95;
                            //VideoStackPanel.Height /= 0.95;
                            //VideoStackPanel.Width /= 0.95;
                            //videoElement.Height /= 0.95;
                            //videoElement.Width /= 0.95;
                            //VideoText.Width /= 0.95;
                            //this.UpdateLayout();
                            //hotspotCanvas.SetCurrentValue(HeightProperty, VideoStackPanel.ActualHeight + 18);
                            //this.Height = hotspotCanvas.Height + 12;
                            //windowSize = new Size(this.Width, this.Height);
                        }

                    }
                }
            }
            
        }


        bool isPlaying;

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            //Unmute
            if (myMediaElement.Volume == 0)
            {
                myMediaElement.Volume = _volume;
                volumeSlider.Value = _volume;
                showAudioIcon();
            }

            //Mute
            else
            {
                _volume = myMediaElement.Volume;
                myMediaElement.Volume = 0;
                volumeSlider.Value = 0;
                showMuteIcon();
                
            }
        }

        private void SurfacePlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlaying)
            {
                videoElement.Play();
                videoTimer.Start();
                isPlaying = true;
            }
            else
            {
                videoElement.Pause();
                videoTimer.Stop();
                isPlaying = false;
            }

        }

        private void VideoElementDone(object sender, EventArgs e)
        {
            videoElement.Position = new TimeSpan(0, 0, 0, 0, 0);
            videoElement.Pause();
            isPlaying = false;
        }

        private void SurfaceTimelineSlider_MouseUp(object sender, EventArgs e)
        {
            videoElement.Position = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(SurfaceTimelineSlider.Value));
        }

        private void SurfaceTimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoElement.Position = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(SurfaceTimelineSlider.Value));
        }

      
    }
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate() { };
        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

    }

}
