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
using DeepZoom.Controls;
using System.IO;

namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for HotspotIconControl.xaml
    /// </summary>
    public partial class HotspotIconControl : UserControl
    {
        Canvas m_parent;

        public Canvas Parent1
        {
            get { return m_parent; }
            set { m_parent = value; }
        }
        ScatterView m_parentScatterView;

        public ScatterView ParentScatterView
        {
            get { return m_parentScatterView; }
            set { m_parentScatterView = value; }
        }
        Hotspot m_hotspotData;

        internal Hotspot HotspotData
        {
            get { return m_hotspotData; }
            set { m_hotspotData = value; }
        }

        HotspotDetailsControl m_detailControl;

        public HotspotDetailsControl DetailControl
        {
            get { return m_detailControl; }
            set { m_detailControl = value; }
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

        BitmapImage normal;

        public BitmapImage Normal
        {
            get { return normal; }
            set { normal = value; }
        }

        BitmapImage highlighted;

        public BitmapImage Highlighted
        {
            get { return highlighted; }
            set { highlighted = value; }
        }
        WriteableBitmap test;
        private MultiScaleImage _msi;

        /// <summary>
        ///Constructor
        /// </summary>
        public HotspotIconControl(Canvas parent, ScatterView parentScatterView,Hotspot hotspotData, MultiScaleImage msi )
        {
            //hotspotCircle = new Image();
            //hotspotCanvas.Children.Add(hotspotCircle);
            InitializeComponent();
            m_hotspotData = hotspotData;
            m_parent = parent;
            _msi = msi;
            m_parentScatterView = parentScatterView;
            m_detailControl = new HotspotDetailsControl(m_parent, m_parentScatterView, m_hotspotData, msi);

            try
            {
                String imgUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +  "\\Data\\Hotspots\\Icons\\normal.png";
                normal = new BitmapImage();
                normal.BeginInit();
                normal.UriSource = new Uri(imgUri, UriKind.Relative);
                normal.CacheOption = BitmapCacheOption.OnLoad;
                normal.EndInit();

                highlighted = new BitmapImage();
                imgUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\highlighted.png";
                highlighted.BeginInit();
                highlighted.UriSource = new Uri(imgUri, UriKind.Relative);
                highlighted.CacheOption = BitmapCacheOption.OnLoad;
                highlighted.EndInit();

                WriteableBitmap wbmap = new WriteableBitmap(normal);
               /* wbmap.Lock();
                IntPtr bbuff = wbmap.BackBuffer;
                unsafe
                {
                    byte* pbuff = (byte*)bbuff.ToPointer();
                }*/
               // MessageBox.Show(normal.Format.ToString());


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

            }

        }


        /// <summary>
        /// Display the hotspot icon on screen.
        /// </summary>
        public void displayOnScreen(MultiScaleImage msi)
        {
            m_parent.Children.Add(this);
            try
            {
                hotspotCircle.Source = normal;
                //hotspotCircle.SetCurrentValue(DockPanel.DockProperty, Dock.Left);

                //hotspotCircle.SetCurrentValue(HeightProperty, 50.0);
                //hotspotCircle.SetCurrentValue(WidthProperty, 50.0);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            //Console.Out.WriteLine("display LOCATIONS");
            Double[] size = m_detailControl.findImageSize();

            screenPosX = (msi.GetZoomableCanvas.Scale * m_hotspotData.PositionX * size[0]) - msi.GetZoomableCanvas.Offset.X;
            screenPosY = (msi.GetZoomableCanvas.Scale * m_hotspotData.PositionY *size[1]) - msi.GetZoomableCanvas.Offset.Y;
            Canvas.SetLeft(this, screenPosX);
            Canvas.SetTop(this, screenPosY);
          

        }


        /// <summary>
        /// Update the icon screen location with respect to the artwork.
        /// </summary>
        public void updateScreenLocation(MultiScaleImage msi)
        {
           // Console.Out.WriteLine("update location");
            Double[] size = m_detailControl.findImageSize();
            screenPosX = (msi.GetZoomableCanvas.Scale * m_hotspotData.PositionX *size[0]) - msi.GetZoomableCanvas.Offset.X;
            screenPosY = (msi.GetZoomableCanvas.Scale * m_hotspotData.PositionY *size[1]) - msi.GetZoomableCanvas.Offset.Y;

            Canvas.SetLeft(this, screenPosX);
            Canvas.SetTop(this, screenPosY);
        }

        /// <summary>
        /// remove the icon from screen
        /// </summary>
        public void removeFromScreen()
        {
            m_parent.Children.Remove(this);
        }

        /// <summary>
        /// display the detail control on screen.
        /// </summary>
        private void showHotspotDetails()
        {
            //MessageBox.Show("hotpsot click");
            if (m_parentScatterView.Items.Contains(m_detailControl) == false)
            {
                if (m_detailControl.m_hotspotData.Type.ToLower().Contains("audio"))
                {
                    m_detailControl = new HotspotDetailsControl(m_parent, m_parentScatterView, m_hotspotData, _msi);
                    //m_detailControl = new HotspotDetailsControl(m_parent, m_parentScatterView,  

                    try
                    {
                        String imgUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\normal.png";
                        normal = new BitmapImage();
                        normal.BeginInit();
                        normal.UriSource = new Uri(imgUri, UriKind.Relative);
                        normal.CacheOption = BitmapCacheOption.OnLoad;
                        normal.EndInit();

                        highlighted = new BitmapImage();
                        imgUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Icons\\highlighted.png";
                        highlighted.BeginInit();
                        highlighted.UriSource = new Uri(imgUri, UriKind.Relative);
                        highlighted.CacheOption = BitmapCacheOption.OnLoad;
                        highlighted.EndInit();

                        WriteableBitmap wbmap = new WriteableBitmap(normal);
                        /* wbmap.Lock();
                         IntPtr bbuff = wbmap.BackBuffer;
                         unsafe
                         {
                             byte* pbuff = (byte*)bbuff.ToPointer();
                         }*/
                        // MessageBox.Show(normal.Format.ToString());


                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());

                    }
                }
                //m_detailControl = new HotspotDetailsControl(
                m_parentScatterView.Items.Add(m_detailControl);
               // Canvas.SetLeft(m_detailControl, Convert.ToDouble(screenPosX));
               // Canvas.SetTop(m_detailControl, Convert.ToDouble(screenPosY));
                //m_detailControl.ScreenPosX = screenPosX;
                //m_detailControl.ScreenPosY = ScreenPosY;

                m_detailControl.IsOnScreen = true;
            }
        }

        /// <summary>
        /// delete the hotspot
        /// </summary>
        public void selfDelete()
        {
            hotspotCircle = null;
            m_parent.Children.Remove(this);
        }

        /// <summary>
        /// change the state of the hotspot to 'highlighted'
        /// </summary>
        public void changeToHighLighted()
        {
            //hotspotCircle.Fill = new RadialGradientBrush(Color.FromRgb(200, 120, 170), Color.FromRgb(200, 120, 170));
            hotspotCircle.Source = highlighted;
        }

        /// <summary>
        /// change the state of the hotspot to 'normal'
        /// </summary>
        public void changeToNormal()
        {
           // hotspotCircle.Fill = new RadialGradientBrush(Color.FromRgb(255,255,255), Color.FromRgb(255,255,255));
            hotspotCircle.Source = normal;
        }

        /// <summary>
        /// Called when a user touches the hotspot icon.
        /// </summary>
        private void hotspotCircle_TouchDown(object sender, TouchEventArgs e)
        {
            showHotspotDetails();
        }


        /// <summary>
        /// Called when a user clicks on the hotspot icon.
        /// </summary>
        private void hotspotCircle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showHotspotDetails();
        }
    }
}
