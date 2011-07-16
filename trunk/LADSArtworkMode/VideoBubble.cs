using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Windows.Input;
//using UICommon;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Shapes;

namespace LADSArtworkMode
{

    public class LADSVideoBubble : UserControl
    {
        private const int SLIDER_TIMER_RESOLUTION = 50; //how often we update the slider based on the video position, in milliseconds
        private const int CONTROLS_PERSIST_TIME = 100; //how long the controls hang around before they start to fade, in milliseconds
        private const uint CONTROLS_FADE_TIME = 200; //how long the controls take to fade, in milliseconds

        private Grid _layoutRoot;

        private String _filename;
        public MediaElement _video;
        public double _aspectRatio;
        private VideoItem _controls;
        private DispatcherTimer _sliderTimer;

        private Size _preferredSize;

        private delegate void ButtonFunction();
        private ButtonFunction playButtonFunction;

        public LADSVideoBubble(String filename, double width, double height)
        {
            _filename = filename;
            _video = new MediaElement();
            _controls = new VideoItem();
            _sliderTimer = new DispatcherTimer();
            _preferredSize = new Size(width, height);

            _layoutRoot = new Grid();

            _video.MediaOpened += new RoutedEventHandler(video_MediaOpened);
            _video.MediaEnded += new RoutedEventHandler(video_MediaEnded);
            _video.ScrubbingEnabled = true;
            _video.LoadedBehavior = MediaState.Manual;
            _video.Source = new Uri(_filename, UriKind.RelativeOrAbsolute);

            //fire the MediaOpened event
            _video.Play();
            _video.Pause();

            _controls.HorizontalAlignment = HorizontalAlignment.Center;
            _controls.VerticalAlignment = VerticalAlignment.Center;
            _controls.Hide();

            _controls.playButton.Click += new RoutedEventHandler(playButton_Click);
            _controls.stopButton.Click += new RoutedEventHandler(stopButton_Click);
            _controls.videoSlider.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(videoSlider_PreviewMouseLeftButtonDown);
            _controls.videoSlider.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(videoSlider_PreviewMouseLeftButtonUp);
            _controls.videoSlider.PreviewTouchDown += new System.EventHandler<TouchEventArgs>(videoSlider_PreviewTouchDown);
            _controls.videoSlider.PreviewTouchUp += new System.EventHandler<TouchEventArgs>(videoSlider_PreviewTouchUp);
            //_controls.videoSlider += videoSlider_ValueChanged;

            _controls.videoSlider.IsMoveToPointEnabled = false;
            _controls.videoSlider.SmallChange = 0;
            _controls.videoSlider.LargeChange = 0;

            _sliderTimer.Interval = new TimeSpan(0, 0, 0, 0, SLIDER_TIMER_RESOLUTION);
            _sliderTimer.Tick += new EventHandler(sliderTimer_Tick);

            _layoutRoot.RowDefinitions.Add(new RowDefinition());
            _layoutRoot.RowDefinitions.Add(new RowDefinition());
            _layoutRoot.RowDefinitions[1].Height = new GridLength(50);
            Grid.SetRow(_controls, 1);
            Grid.SetRow(_video, 0);
            _layoutRoot.Children.Add(_video);

            this.AddChild(_layoutRoot);
            this.MouseEnter += new MouseEventHandler(VideoBubble_MouseEnter);
            this.MouseLeave += new MouseEventHandler(VideoBubble_MouseLeave);
            this.SizeChanged += new SizeChangedEventHandler(LADSVideoBubble_SizeChanged);
        }

        void LADSVideoBubble_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > _video.ActualWidth)
            {
                Width = _video.ActualWidth;
            }
            if (e.NewSize.Height > _video.ActualHeight)
            {
                Height = _video.ActualHeight;
            }
        }

        #region Video Event Handlers

        bool hasBeenOpened = false;
        private void video_MediaOpened(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("trying to open video");
            if (!hasBeenOpened)
            {
                _aspectRatio = (double)_video.NaturalVideoWidth / (double)_video.NaturalVideoHeight;
                Resize(_preferredSize.Width,_preferredSize.Height);
                Console.Out.WriteLine("width" + _preferredSize.Width);
                Console.Out.WriteLine("height" + _preferredSize.Height);
                
                _layoutRoot.Children.Add(_controls);
                Grid.SetRow(_controls, 1);

                _controls.videoSlider.Maximum = 1;
                _controls.Show();

                //unless you want to fade out the controls whenever the user isn't hovering over them WHICH I SERIOUSLY DOUBT don't uncommment this. But just in case, here ya go.
                //if for some reason you WANT the controls to fade out, just call controls.fadeout
                //if (!this.IsMouseOver)
                //  _controls.FadeOut(CONTROLS_FADE_TIME, CONTROLS_PERSIST_TIME);

                //MediaElement has this weird bug where it always starts from 0:00 the first time it is played, regardless of its actual position - this prevents that
                _video.Play();
                _video.Pause();

                playButtonFunction = playVideo;
                hasBeenOpened = true;
            }
        }

        private void video_MediaEnded(object sender, RoutedEventArgs e)
        {
            _video.Position = new TimeSpan(0, 0, 0, 0, 0);
            pauseVideo();
            Console.WriteLine("VideoEnded");
        }

        public void hideControls()
        {
            _controls.Hide();
            _controls.FadeOut(1, 0);
        }

        #endregion

        #region Fading Events

        private void VideoBubble_MouseLeave(object sender, MouseEventArgs e)
        {
            //uncomment this if you want the controls to fade out when you aren't moused over then, which given your touchscreen context makes no frikken sense but hey I wrote the code for it so you can have it
            //_controls.FadeOut(CONTROLS_FADE_TIME, CONTROLS_PERSIST_TIME);
        }

        private void VideoBubble_MouseEnter(object sender, MouseEventArgs e)
        {
            //this is also for the fading
            //_controls.Show();
        }

        #endregion

        #region Button Event Handlers

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            playButtonFunction();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            pauseVideo();
            _video.Position = new TimeSpan(0, 0, 0, 0, 0);
            _controls.videoSlider.Value = 0;
        }

        #endregion

        #region Slider/SliderTimer Event Handlers

        /*
         * some of this code is working around the fact that sliders in windows suck. since they're tiny and can't be resized, you'll
         * probably end up making your own anyways. questions? just email me. -nz
         * (nmzimmt@gmail.com)
         * */
        //public void sliderOpacity_ValueChanged(object sender, RoutedEventArgs e)
        //{
        //    tourSystem.ChangeOpacity(((SurfaceSlider)sender).Value);
        //}

        public void videoSlider_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _sliderTimer.Stop();
            double tp = e.GetTouchPoint(_controls.videoSlider).Position.X;
            _controls.videoSlider.Value = tp / _controls.videoSlider.ActualWidth;
            _controls.videoSlider.PreviewTouchMove += new System.EventHandler<TouchEventArgs>(videoSlider_PreviewTouchMove);
        }

        public void videoSlider_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            _controls.videoSlider.PreviewTouchMove -= new EventHandler<TouchEventArgs>(videoSlider_PreviewTouchMove);
            _video.Position = new TimeSpan(0, 0, 0, 0, (int)Math.Floor(_video.NaturalDuration.TimeSpan.TotalMilliseconds * _controls.videoSlider.Value));
            _sliderTimer.Start();
        }

        private void videoSlider_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (!_controls.IsDragging())
            {
                double newValue = e.GetTouchPoint(_controls.videoSlider).Position.X / _controls.videoSlider.ActualWidth;
                newValue = newValue < 0 ? 0 : newValue >= 1 ? .999 : newValue; //.999 as opposed to 1 because the MediaEnded event does not fire if the movie is scrolled to 100% manually. This stuff is WACKY.
                _controls.videoSlider.Value = newValue;
            }
        }

        private void videoSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _sliderTimer.Stop();
            _controls.videoSlider.Value = e.GetPosition(_controls.videoSlider).X / _controls.videoSlider.ActualWidth;
            _controls.videoSlider.PreviewMouseMove += new MouseEventHandler(videoSlider_PreviewMouseMove);
        }

        private void videoSlider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_controls.IsDragging())
            {
                double newValue = e.GetPosition(_controls.videoSlider).X / _controls.videoSlider.ActualWidth;
                newValue = newValue < 0 ? 0 : newValue >= 1 ? .999 : newValue; //.999 as opposed to 1 because the MediaEnded event does not fire if the movie is scrolled to 100% manually. This stuff is WACKY.
                _controls.videoSlider.Value = newValue;
            }
        }

        private void videoSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _controls.videoSlider.PreviewMouseMove -= new MouseEventHandler(videoSlider_PreviewMouseMove);
            _video.Position = new TimeSpan(0, 0, 0, 0, (int)Math.Floor(_video.NaturalDuration.TimeSpan.TotalMilliseconds * _controls.videoSlider.Value));
            _sliderTimer.Start();
        }

        private void sliderTimer_Tick(object sender, EventArgs e)
        {
            _controls.videoSlider.Value = _video.Position.TotalSeconds / _video.NaturalDuration.TimeSpan.TotalSeconds;
        }
       

        #endregion

        #region Video Functions

        private void playVideo()
        {
            _video.Play();
            _sliderTimer.Start();
            _controls.ShowPause();
            playButtonFunction = pauseVideo;
        }

        //is there anything wrong with this being public?
        public void pauseVideo()
        {
            _video.Pause();
            _sliderTimer.Stop();
            _controls.ShowPlay();
            playButtonFunction = playVideo;
        }

        #endregion

        #region Stuff

        public double CurrentVideoPosition
        {
            get { return _video.Position.TotalSeconds; }
            set
            {
                int seconds = (int)value;
                int millis = (int)(1000 * (value - seconds));

                _video.Position = new TimeSpan(0, 0, 0, seconds, millis);
            }
        }

        //tested this as best I could seeing as I basically hacked it together for y'all (Humbub does resizing stuff for you, mostly)
        public void Resize(double newWidth, double newHeight, bool forceAspectRatio = true)
        {
            if (forceAspectRatio)
            {
                Resize(_aspectRatio > 1 ? newWidth : newWidth * _aspectRatio,
                       _aspectRatio > 1 ? newHeight / _aspectRatio : newHeight, false);
            }
            else
            {
                _layoutRoot.Width = newWidth;
                _layoutRoot.Height = newHeight;
                Width = newWidth;
                Height = newHeight;
                _video.Width = newWidth;
                _video.Height = newHeight;
            }

            UpdateControls();
        }

        //call this every time you resize the video
        public void UpdateControls()
        {
            //double vMargin = _video.ActualHeight * .05;
            double vMargin = 0;
            double hMargin = _video.ActualWidth * .025;

            _controls.SetWidth(_video.ActualWidth - 2 * hMargin);
            _controls.Margin = new Thickness(hMargin, 0, hMargin, vMargin);

            _layoutRoot.UpdateLayout();
        }

        #endregion

        public double getWidth()
        {
            Console.WriteLine("ActualVideoWodth : " + _video.ActualWidth);
            return _video.ActualWidth;   
        }

        public double getHeight()
        {
            Console.WriteLine("ActualVidHeight" + _video.ActualHeight);
            return _video.ActualHeight;
        }

        public MediaElement getVideo()
        {
            return _video;
        }

        public void setPreferredSize(double width, double height)
        {
            _preferredSize.Width = width;
            _preferredSize.Height = height;
        }
        //hills.alex@gmail.com | marguerite_pace@brown.edu | yuting_chen@brown.edu
    }
}
