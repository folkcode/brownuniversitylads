using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for AddImageWindow.xaml. This is the main window for adding new artworks
    /// </summary>
    public partial class AddImageWindow : SurfaceWindow
    {

        public MainWindow mainWindow;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AddImageWindow()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            this.setWindowSize();
            mapWindow newMapWindow = new mapWindow();
            hotspotWindow newHotWindow = new hotspotWindow();
            big_window1.setHotspotWindow(newHotWindow);
            big_window1.setMapWindow(newMapWindow);
          
        }

        //Changes the window size to make the content authoring tool fit in any size of screen
        public void setWindowSize()
        {

            Double width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            Double height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            Double ratio = height / width;
            ScaleTransform tran = new ScaleTransform();

            if (width < 1024 || height < 850)
            {
                if (width / 1024 > height / 800)
                {
                    
                    this.Height = height - 60;
                    this.Width = this.Height / 800 * 1024;
                    tran.ScaleY = this.Height / 800;
                    tran.ScaleX = this.Width / 1024;
                 
                }
                else
                {
                    this.Width = width - 60;
                   
                    this.Height = this.Width / 1024 * 800;
                    tran.ScaleX = this.Width / 1024;
                    tran.ScaleY = this.Height / 800;
                    
                }
              mainCanvas.RenderTransform = tran;
            }

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

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //this.setWindowSize();
            
        }

        

       
    }
}

