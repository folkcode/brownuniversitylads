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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace GCNav
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    /// 
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        private StartCard _startCard;
        private FilterTimelineBox filter;
       
        public SurfaceWindow1()
        {

            InitializeComponent();
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            this.SizeChanged += new SizeChangedEventHandler(SurfaceWindow1_SizeChanged);
            this.SizeChanged += Map.WindowSizeChanged;
            this.SizeChanged += nav.WindowSizeChanged;
            
           

            this.MouseUp += new MouseButtonEventHandler(MouseUp_Handler);

            _startCard = new StartCard();
            _startCard.HorizontalAlignment = HorizontalAlignment.Center;
            _startCard.VerticalAlignment = VerticalAlignment.Center;
            startCan.Children.Add(_startCard);

            panImg.Source = new BitmapImage(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Startup\\panning_startup.bmp", UriKind.Absolute));
         
            panImg.Width = 3598;
            panImg.Height = 1080;
            DoubleAnimation myAnimation = new DoubleAnimation();
            myAnimation.From = 0;
            myAnimation.To = -1080;
            myAnimation.AutoReverse = true;
            myAnimation.RepeatBehavior = RepeatBehavior.Forever;
            myAnimation.Duration = new Duration(TimeSpan.FromSeconds(45));
            TranslateTransform t = new TranslateTransform();
            panImg.HorizontalAlignment = HorizontalAlignment.Right;
            panImg.Opacity = 0.2;
            panCan.RenderTransform = t;
            t.BeginAnimation(TranslateTransform.XProperty, myAnimation);
           // Map.RegionSelected += nav.HandleMapSelectedEvent;
           // Map.RegionDeselected += nav.HandleMapDeselectedEvent;
           // nav.ImageLoaded += Map.HandleImageLoadedEvent;
            nav.HandleImageSelected += Map.HandleImageSelectedEvent;
            nav.setMapWidth(Map.Width);
            //nav.loadCollection();
            //nav.startAll();
            filter = new FilterTimelineBox();
            nav.filter = filter;
           
            map.Children.Add(filter);
           
            this.SizeChanged += SurfaceWindow1_SizeChanged;
           
        }

        void SurfaceWindow1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Double canvasLeft = e.NewSize.Width / 2 - filter.ActualWidth / 2;
            nav.setMapWidth(Map.ActualWidth);
            Double filterWidth = 420;
            if (e.NewSize.Width < 1600)
            {
                ScaleTransform tran = new ScaleTransform();
                tran.ScaleX = e.NewSize.Width / 1600;
                filter.RenderTransform = tran;
                canvasLeft = e.NewSize.Width / 2 - filterWidth * tran.ScaleX / 2;
                filterWidth = filterWidth * tran.ScaleX;
            }
            
            Double scaleX= Map.tranScaleX;
            Double scaleY = Map.tranScaleY;
            //Console.Out.WriteLine("filterwidth" + filter.ActualWidth);
           // Console.Out.WriteLine("mapwidth" + Map.Width * scale);
            Canvas.SetLeft(filter, canvasLeft);
            Canvas.SetZIndex(filter, 10);
            filter.Visibility = Visibility.Hidden;

            backRec.Width = map.Width*scaleX +10;
            backRec.Height = map.Height*scaleY + 30+10;
         //   Console.Out.WriteLine("width rec" + backRec.Width);
         //   Console.Out.WriteLine("REC HE" +backRec.Height);
            //Canvas.SetTop(backRec, 20);
            Canvas.SetLeft(backRec, e.NewSize.Width *0.316);
            Canvas.SetZIndex(backRec, -10);
          
            
        }


        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        /// <summary>
        /// The surface button serves 2 purposes:
        /// 1. Loading the images before the window finishes initializing causes resolution problem;
        /// 2. The about box shows up on start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void surfaceButton1_Click(object sender, RoutedEventArgs e)
        {
            nav.startAll();
            surfaceButton1.Visibility = Visibility.Collapsed;
            _startCard.Visibility = Visibility.Hidden;
            if (nav.collectionEmpty())
            {
                EmptyCollectionControl popup = new EmptyCollectionControl();
                popup.HorizontalAlignment = HorizontalAlignment.Center;
                popup.VerticalAlignment = VerticalAlignment.Center;
                startCan.Children.Add(popup);

            }
            else
            {
                panImg.Visibility = Visibility.Hidden;
                Map.loadMap();
                Map.blur.Visibility = Visibility.Visible;
                filter.Visibility = Visibility.Visible;
                backRec.Visibility = Visibility.Visible;
            }
        }

        private void addMapHandler(Helpers.MapEventHandler handler)
        {
        }

        public void MouseUp_Handler(object sender, EventArgs e)
        {
            nav.setTimelineMouseUpFalse();
        }
    }
}