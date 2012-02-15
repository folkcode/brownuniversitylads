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
using System.Windows.Threading;

namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for VideoItem.xaml
    /// </summary>
    public partial class VideoItem : UserControl
    {
        private const double MAX_ALPHA = .75;

        private DispatcherTimer _fadeTimer;

        private uint _fadeCounterMax;
        private uint _fadeCounter;
        private bool _dragging;
        public VideoItem()
        {
            InitializeComponent();

            _fadeTimer = new DispatcherTimer();
            _fadeTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            _fadeTimer.Tick += new EventHandler(fadeHandler);
            _dragging = false;
        }
        /// <summary>
        /// Handles switching between pause and play icons
        /// </summary>
        public void ShowPlay()
        {
            playIcon.Visibility = Visibility.Visible;
            pauseIconLeft.Visibility = Visibility.Hidden;
            pauseIconRight.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// Handles switching between play and pause icons
        /// </summary>
        public void ShowPause()
        {
            playIcon.Visibility = Visibility.Hidden;
            pauseIconLeft.Visibility = Visibility.Visible;
            pauseIconRight.Visibility = Visibility.Visible;
        }

        public void SetWidth(double newWidth)
        {
            if (newWidth < 100)
                newWidth = 100;

            container.Width = newWidth;
            playButton.Margin = new Thickness(0, 0, newWidth - 25, 0);
            stopButton.Margin = new Thickness(30, 0, newWidth - 55, 0);
            videoSlider.Margin = new Thickness(60, 1, 5, 0);
        }

        public void Hide()
        {
            container.Visibility = Visibility.Hidden;
            container.IsHitTestVisible = false;
            _fadeTimer.Stop();
        }

        public void Show()
        {
            container.Visibility = Visibility.Visible; ;
            container.Opacity = MAX_ALPHA;
            container.IsHitTestVisible = true;
            _fadeTimer.Stop();
        }

        public void FadeOut(uint millis, uint persistTime = 0)
        {
            _fadeCounter = millis + persistTime;
            _fadeCounterMax = millis;
            _fadeTimer.Start();
        }

        private void fadeHandler(object sender, EventArgs e)
        {
            if (_fadeCounter <= 0)
            {
                _fadeTimer.Stop();
                Hide();
            }
            else
            {
                _fadeCounter -= 10;
                double factor = (double)_fadeCounter / (double)_fadeCounterMax;
                container.Opacity = MAX_ALPHA * (factor > 1 ? 1 : factor);
            }
        }

        //this section may seem useless but it is not.

        private void videoSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _dragging = true;
        }

        private void videoSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _dragging = false;
        }

        public bool IsDragging()
        {
            return _dragging;
        }
    }
}
