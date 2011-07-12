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
using System.Windows.Forms;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for AddImageWindow.xaml
    /// </summary>
    public partial class AddImageWindow : SurfaceWindow
    {
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AddImageWindow()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
           // big_window1.setUserControl(HotspotControl);
            mapWindow newMapWindow = new mapWindow();
            hotspotWindow newHotWindow = new hotspotWindow();
            big_window1.setHotspotWindow(newHotWindow);
            big_window1.setMapWindow(newMapWindow);
            this.setWindowSize();
           // big_window1.setMapControl(mapControl);
           // mapControl.setBigWindow(big_window1);
           // mapControl.Visibility = Visibility.Collapsed;
            // big_window1.MetaDataList.Items.Add(new MetaDataEntry(big_window1));
        }
         public void setWindowSize()
        {

            Double width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            Double height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            Double ratio = height / width;
            ScaleTransform tran = new ScaleTransform();

            if (width < 1024 || height < 800)
            {
                if (width / 1024 > height / 800)
                {
                    this.Height = height - 100;
                    this.Width = this.Height / 800 * 1024;
                    // this.Width = this.Height/ratio;
                    tran.ScaleY = this.Height / 800;
                    tran.ScaleX = this.Width / 1024;
                  //  Console.Out.WriteLine("width" + this.Width);
                }
                else
                {
                    this.Width = width - 100;
                    this.Height = this.Width / 1024 * 800;
                    tran.ScaleX = this.Width / 1024;
                    tran.ScaleY = this.Height / 800;
                    //  this.Height = this.Width * ratio;
                }
                //Console.Out.WriteLine("width" + this.Width);
                //Console.Out.WriteLine("height" + this.Height);
                //scale according to 1600* 900 resolution

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

        }

        private void SurfaceRadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {

        }

        private void big_window_Loaded(object sender, RoutedEventArgs e)
        {


        }

        private void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {
        }



        private void HotspotControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void mapControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

       
    }
}

