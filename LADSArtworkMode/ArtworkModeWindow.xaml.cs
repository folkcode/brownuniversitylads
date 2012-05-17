using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Collections;
using System.IO;
using System.Windows.Ink;
using System.Xml;
using System.Net;
using System.Windows.Threading;


namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for ArtworkModeWindow.xaml
    /// </summary>
    public partial class ArtworkModeWindow : SurfaceWindow
    {
        #region global variables

        // -------------DEFINE GLOBAL VARIABLES (ATTRIBUTE) OF THE CLASS ArtworkModeWindow: -------------------------//

        Artwork mainArtwork;

        public Artwork MainArtwork
        {
            get { return mainArtwork; }
            set { mainArtwork = value; }
        }

        public NameInfo CurrNameInfo;
        public bool leftPanelVisible;
        public bool bottomPanelVisible;
        bool scatteremoved;
        public int dockedItems;
        bool knowledgeWebOn = false;
        public ArrayList DockedItems;
        public int num_images = 20;
        public double spaceBuffer = 10.0;
        public double BarOffset;
        metadata_lists newMeta;
        StrokeCollection collection;
        public HotspotCollection m_hotspotCollection;
        bool m_hotspotOnOff = false;
        bool authToolsVisible;
        System.Windows.Forms.Timer _timer;
        EventHandler _timerHandler;
        private bool _noHotspots;
        private bool _searchedHotspots, _searchedAssets;
        public List<DockableItem> DockedDockableItems = new List<DockableItem>();
        
        
        public List<DockedItemInfo> SavedDockedItems = new List<DockedItemInfo>();

        // A hash map of filename -> opened assets (replaces the deprecated AssociatedDocListBoxItem.opened field)
        public Dictionary<String, DockableItem> _openedAssets = new Dictionary<string, DockableItem>();


        TourSystem tourSystem; // tour authoring & playback system
        public bool IsTourOn { get { return tourSystem.IsExploreMode || tourSystem.tourPlaybackOn; } }
        public bool IsExploreOn { get { return tourSystem.IsExploreMode; } }

        public bool isTourPlayingOrAuthoring()
        {
            return (tourSystem.tourAuthoringOn || tourSystem.tourPlaybackOn);
        }


        public DeepZoom.Controls.MultiScaleImage MultiImage
        {
            get
            {
                return msi;
            }
            set
            {
                msi = value;
            }
        }

        public DeepZoom.Controls.MultiScaleImage MultiImageThumb
        {
            get
            {
                return msi_thumb;
            }

            set
            {
                msi_thumb = value;
            }
        }

        // ------------------------------ END OF GLOBAL VARIABLES DEFINITION --------------------------------------- //

        #endregion

        private DispatcherTimer _resetTimer = new DispatcherTimer();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArtworkModeWindow(NameInfo currName)
        {
            InitializeComponent();
            String[] c = Environment.GetCommandLineArgs();

            if (c.Length != 1)
            {
                if (c[1].Contains("noauthoring"))
                {
                    Main.Children.Remove(tourAuthoringButton);
                    Main.Children.Remove(exitButton);
                    
                }
            }
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            leftPanelVisible = true;
            bottomPanelVisible = true;
            scatteremoved = false;
            dockedItems = 0;
            DockedItems = new ArrayList();
            BarOffset = 0;
            _noHotspots = true;

            CurrNameInfo = currName;
            newMeta = new metadata_lists(this, CurrNameInfo.PanelNumber);
            authToolsVisible = true;
            _searchedAssets = false;
            _searchedHotspots = false;

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));

            // Hotspots
            m_hotspotCollection = new HotspotCollection();

            tourSystem = new TourSystem(this);

            // event handlers for tour testing buttons

            // event handlers for tour playback/authoring
            tourExploreButton.Click += tourSystem.TourExploreButton_Click;
            tourControlButton.Click += tourSystem.TourControlButton_Click;
            tourStopButton.Click += tourSystem.TourStopButton_Click;
            tourAuthoringDoneButton.Click += tourSystem.TourAuthoringDoneButton_Click;
            tourAuthoringDeleteButton.Click += tourSystem.TourAuthoringDeleteButton_Click;
            //drawPaths.Click += tourSystem.drawPaths_Click;
            //metaData.Click += new RoutedEventHandler(metaData_Click);
            tourAuthoringSaveButton.Click += TourAuthoringSaveButton_Click;
            //addAudioButton.Click += tourSystem.grabSound;
            //addAudioButton.Visibility = Visibility.Collapsed;
            //Canvas.SetTop(downButton, 648);

            //Canvas.SetLeft(collapseButtonDown, (collapseBar.Width - msi_thumb.ActualWidth) / 2 + msi_thumb.ActualWidth);
            Canvas.SetTop(collapseButtonLeft, 400);
            Canvas.SetTop(collapseButtonRight, 400);

            _resetTimer.Interval = TimeSpan.FromSeconds(120);
            _resetTimer.Tick += new EventHandler(_resetTimer_Tick);
            _resetTimer.Start();

            String dzPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/data/Images/DeepZoom/dz.xml";
            MultiImage.SetImageSource(dzPath);
            MultiImageThumb.SetImageSource(@dzPath);
            LayoutArtworkMode();
        }

        void _resetTimer_Tick(object sender, EventArgs e)
        {
            _resetTimer.Stop();
            this.Close();
        }

        private void SurfaceWindow_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _resetTimer.Stop();
            e.Handled = false;
        }

        private void SurfaceWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _resetTimer.Stop();
            e.Handled = false;
        }

        private void SurfaceWindow_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            _resetTimer.Start();
            e.Handled = false;
        }

        private void SurfaceWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _resetTimer.Start();
            e.Handled = false;
        }


        #region initialization

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            fillHotspotNavListBox();
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
            Helpers helper = new Helpers();
            foreach (DockableItem item in MainScatterView.Items)
            {
                if (item.Visibility == Visibility.Visible)
                {
                    if (helper.IsVideoFile(item.scatteruri))
                    {
                        item.stopVideo();
                    }
                }
            }
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

        public void LoadDockedItems(List<DockedItemInfo> SavedDockedItemsFromNav)
        {
            SavedDockedItems = SavedDockedItemsFromNav;
            Helpers _helpers=new Helpers();
            foreach (DockedItemInfo info in SavedDockedItemsFromNav)
            {
                //if it's an image, do this:
                if (_helpers.IsImageFile(info.scatteruri))
                {
                    DockableItem item = new DockableItem(MainScatterView, this, Bar, info.scatteruri, null);
                    item.AddtoDockFromSaved(info.savedOldWidth, info.savedOldHeight, info.savedWKEWidth, info);
                }
                else if (_helpers.IsVideoFile(info.scatteruri))
                {
                    //perhaps the initializatoin of this bubble should have the height and width of the thumbnail... if I could extract one...
                    DockableItem item = new DockableItem(MainScatterView, this, Bar, info.scatteruri, null, new LADSVideoBubble(info.scatteruri, 500, 500), new VideoItem());
                    item.AddtoDockFromSaved(info.savedOldWidth, info.savedOldHeight, info.savedWKEWidth, info);//video-specific constructor
                }
                else
                { //not image or video...
                }
            }
                //for all, do this:
        }

            
        

        public void TourLayout()
        {
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
            dpd.RemoveValueChanged(msi.GetZoomableCanvas, msi_ViewboxChanged);
            msi.Visibility = Visibility.Hidden;
            msi_thumb.Visibility = Visibility.Hidden;
            tourAuthoringButton.Visibility = Visibility.Collapsed;
            switchToCatalogButton.Visibility = Visibility.Collapsed;
            resetArtworkButton.Visibility = Visibility.Collapsed;
            //exitButton.Visibility = Visibility.Collapsed;
            MainScatterView.Visibility = Visibility.Collapsed;
            HotspotOverlay.Visibility = Visibility.Collapsed;
            ImageArea.Width = 1920;
            ImageArea.Height = 1080;
            MSIScatterView.Width = 1920;
            MSIScatterView.Height = 1080;
            MainScatterView.Width = 1920;
            MainScatterView.Height = 1080;
            DeepZoomGrid.Width = 1920;
            DeepZoomGrid.Height = 1080;
            HotspotOverlay.Width = 1920;
            HotspotOverlay.Height = 1080;
            if (Main.ActualWidth / 1920.0 > Main.ActualHeight / 1080.0)
            {
                //We have to do more scaling to get height, so we scale to the width (else we'll overscale width)
                ImageArea.RenderTransform = new ScaleTransform((Main.ActualWidth - 1) / 1920.0, (Main.ActualWidth - 1) / 1920.0);
            }
            else
            {
                ImageArea.RenderTransform = new ScaleTransform((Main.ActualHeight - 1) / 1080.0, (Main.ActualHeight - 1) / 1080.0);
            }
        }

        public void NewImageSelected_Handler(object sender, EventArgs e)
        {
            /*this.Show();
            string newImageFilename = sender as string;
            if (newImageFilename != currentArtworkFileName)
            {
                if (MessageBox.Show("Are you sure you want to switch artworks? You will lose what you have been working on.","Switch", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ArtworkModeWindow newWindow = new ArtworkModeWindow(newImageFilename);
                    newWindow.Show();
                    this.Close();
                }
                else
                {
                    return;
                }
            }*/
        }

        public void ArtModeLayout()
        {
            double MainHeight = this.Height;// -30; // 1094 - 14
            double MainWidth = this.Width;// -30;  // 1934 - 14
            ImageArea.SetCurrentValue(HeightProperty, MainHeight);
            ImageArea.SetCurrentValue(WidthProperty, MainWidth);
            DeepZoomGrid.SetCurrentValue(HeightProperty, MainHeight);
            DeepZoomGrid.SetCurrentValue(WidthProperty, MainWidth);
            MainScatterView.SetCurrentValue(HeightProperty, MainHeight);
            MainScatterView.SetCurrentValue(WidthProperty, MainWidth);
            MSIScatterView.SetCurrentValue(HeightProperty, MainHeight);
            MSIScatterView.SetCurrentValue(WidthProperty, MainWidth);
            MainScatterView.SetCurrentValue(HeightProperty,MainHeight);
            MainScatterView.SetCurrentValue(WidthProperty, MainWidth);
            DeepZoomGrid.SetCurrentValue(HeightProperty, MainHeight);
            DeepZoomGrid.SetCurrentValue(WidthProperty, MainWidth);
            HotspotOverlay.SetCurrentValue(HeightProperty, MainHeight);
            HotspotOverlay.SetCurrentValue(WidthProperty, MainWidth);
            msi.Visibility = Visibility.Visible;
            msi_thumb.Visibility = Visibility.Visible;
            tourAuthoringButton.Visibility = Visibility.Visible;
            switchToCatalogButton.Visibility = Visibility.Visible;
            resetArtworkButton.Visibility = Visibility.Visible;
            exitButton.Visibility = Visibility.Visible;
            MainScatterView.Visibility = Visibility.Visible;
            HotspotOverlay.Visibility = Visibility.Visible;
            ImageArea.RenderTransform = null;
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
            dpd.AddValueChanged(msi.GetZoomableCanvas, msi_ViewboxChanged);
        }
        /// <summary>
        /// Called when the user choose an artwork from the artwork selection mode. Initialize necessary properties.
        /// </summary>
        public void LayoutArtworkMode()
        {
            double MainHeight = this.Height;// -30; // 1094 - 14
            double MainWidth = this.Width;// -30;  // 1934 - 14

            ImageArea.SetCurrentValue(HeightProperty, MainHeight);
            ImageArea.SetCurrentValue(WidthProperty, MainWidth);

            DeepZoomGrid.SetCurrentValue(HeightProperty, MainHeight);
            DeepZoomGrid.SetCurrentValue(WidthProperty, MainWidth);
            MainScatterView.SetCurrentValue(HeightProperty, MainHeight);
            MainScatterView.SetCurrentValue(WidthProperty, MainWidth);
            BottomPanel.SetCurrentValue(Canvas.TopProperty, MainHeight * .8);
            BottomPanel.SetCurrentValue(HeightProperty, MainHeight * .2);
            BottomPanel.SetCurrentValue(WidthProperty, MainWidth);
            tourAuthoringUICanvas.SetCurrentValue(Canvas.TopProperty, MainHeight * .8);
            tourAuthoringUICanvas.SetCurrentValue(HeightProperty, MainHeight * .2);
            tourAuthoringUICanvas.SetCurrentValue(WidthProperty, MainWidth);
            NavPanel.SetCurrentValue(WidthProperty, MainWidth * .2);
            NavPanel.SetCurrentValue(HeightProperty, MainHeight * .2);

            ThumbSVI.Center = new Point(ThumbSV.Width / 2, ThumbSV.Height / 2);

            msi.UpdateLayout(); // man...took me forever to figure out that I needed to call this method
            msi.ResetArtwork();
            msi_thumb.UpdateLayout(); // man...took me forever to figure out that I needed to call this method
            msi_thumb.ResetArtwork();

            tourSeekBar.SetCurrentValue(WidthProperty, MainWidth * .8);
            tourSeekBarSlider.SetCurrentValue(WidthProperty, tourSeekBar.Width - tourSeekBarTimeDisplayBackground.Width);

            LeftPanel.SetCurrentValue(HeightProperty, MainHeight * .8);
            LeftPanel.SetCurrentValue(WidthProperty, MainWidth * .2);
            double sectionWidth = MainWidth * .2 - 32;
            SectionBoxMaster.Width = MainWidth * .2 - 32;
            double sectionHeight = MainHeight * .2;
            SectionBoxMaster.Height = sectionHeight;
            SectionTitlesMaster.FontSize = sectionHeight / 9.0;
            double labelSize = labelTools.ActualHeight;

            AuthLeftPanel.SetCurrentValue(HeightProperty, MainHeight * .8);
            AuthLeftPanel.SetCurrentValue(WidthProperty, MainWidth * .2);

            SectionListBoxMaster.SetCurrentValue(Canvas.TopProperty, labelSize * 2.1);
            SectionListBoxMaster.Width = sectionWidth - 10;

            LeftPanelButtonMaster.Width = 10;
            LeftPanelButtonMaster.FontSize = SectionTitlesMaster.FontSize * .75;
            LeftPanelButtonMaster.Height = labelSize * .75;

            sBDocsSearchClear.SetCurrentValue(Canvas.TopProperty, labelSize);
            sBHotSpotClear.SetCurrentValue(Canvas.TopProperty, labelSize);

            sTextBoxDocsSearch.SetCurrentValue(Canvas.TopProperty, labelSize);
            sTextBoxHotSpotSearch.SetCurrentValue(Canvas.TopProperty, labelSize);

            sTextBoxDocsSearch.Height = labelSize * .8;
            sTextBoxHotSpotSearch.Height = labelSize * .8;
            sTextBoxDocsSearch.Width = sectionWidth - sBDocsSearchClear.ActualWidth * 1.5;
            sTextBoxHotSpotSearch.Width = sectionWidth - sBDocsSearchClear.ActualWidth * 1.5;

            treeDocs.Height = sectionHeight - labelSize * 2.1;
            TourScroll.Height = sectionHeight - labelSize * 1.2;
            TourScroll.SetCurrentValue(Canvas.TopProperty, labelSize * 1.2);
            listHotspotNav.Height = sectionHeight - labelSize * 2.1;

            labelBrightness.SetCurrentValue(Canvas.TopProperty, sectionHeight * 0.7 / 4.0);
            labelContrast.SetCurrentValue(Canvas.TopProperty, sectionHeight * 1.7 / 4.0);
            labelSaturation.SetCurrentValue(Canvas.TopProperty, sectionHeight * 2.7 / 4.0);

            sliderBrightness.SetCurrentValue(Canvas.TopProperty, sectionHeight / 4.0);
            sliderContrast.SetCurrentValue(Canvas.TopProperty, sectionHeight * 2.0 / 4.0);
            sliderSaturation.SetCurrentValue(Canvas.TopProperty, sectionHeight * 3.0 / 4.0);

            sliderBrightness.Width = sectionWidth - textBlockBrightness.ActualWidth;
            sliderContrast.Width = sectionWidth - textBlockBrightness.ActualWidth;
            sliderSaturation.Width = sectionWidth - textBlockBrightness.ActualWidth;

            textBlockBrightness.SetCurrentValue(Canvas.TopProperty, sectionHeight / 4.0);
            textBlockContrast.SetCurrentValue(Canvas.TopProperty, sectionHeight * 2.0 / 4.0);
            textBlockSaturation.SetCurrentValue(Canvas.TopProperty, sectionHeight * 3.0 / 4.0);


            treeDocs.Items.Clear();

            loadMetadata(CurrNameInfo.PanelNumber);
            if (m_hotspotCollection.loadDocument(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\XMLFiles\\" + CurrNameInfo.PanelNumber + ".xml"))
                fillHotspotNavListBox();
            this.initWorkspace();
        }

        public void loadMetadata(string filename)
        {
            /*int count = 0;
            string dataDir = "data/";
            Helpers helpers = new Helpers();
            XmlDocument doc = new XmlDocument();
            doc.Load("data/AnnenbergCollection.xml");
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {
                        foreach (XmlNode node in docNode.ChildNodes)
                        {
                            if (node.Name == "Image")
                            {
                                if (filename != node.Attributes.GetNamedItem("path").InnerText)
                                    continue;
                                foreach (XmlNode imgnode in node.ChildNodes)
                                {
                                    if (imgnode.Name == "Metadata")
                                    {
                                        foreach (XmlNode group in imgnode.ChildNodes)
                                        {
                                            foreach (XmlNode file in group.ChildNodes)
                                            {
                                                string metadatafilename = file.Attributes.GetNamedItem("Filename").InnerText;
                                                count++;
                                                string name;
                                                try
                                                {
                                                    name = file.Attributes.GetNamedItem("Name").InnerText;
                                                }
                                                catch (Exception exc)
                                                {
                                                    name = "Untitled";
                                                }
                                                if (helpers.IsImageFile(metadatafilename))
                                                {
                                                    new AssociatedDocListBoxItem(name, "Data\\Images\\Metadata\\" + metadatafilename, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Metadata\\" + metadatafilename, this);
                                                }
                                                else if (helpers.IsVideoFile(metadatafilename))
                                                {
                                                    new AssociatedDocListBoxItem(name, "Data\\Videos\\Metadata\\" + metadatafilename, "Data\\Videos\\Metadata\\" + metadatafilename, this); 
                                                }
                                            }

                                        }
                                    }

                                }

                            }
                        }
                    }
                }
            }
            if (count == 0)
            {
                SurfaceListBoxItem item = new SurfaceListBoxItem();
                item.Content = "(No assets for this artwork)";
                treeDocs.Items.Add(item);
            }*/
        }

        public void reloadMetadata(string filename)
        {
            treeDocs.Items.Clear();
            this.loadMetadata(filename);
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            this.reloadMetadata(CurrNameInfo.PanelNumber);
        }

        public void initWorkspace()
        {
        }

        #endregion

        #region hotspots/associated documents/tools

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            mainArtwork = new Artwork();
        }
        private void toggleHotspot()
        {
            if (m_hotspotOnOff == false)
            {
                if (m_currentSelectedHotspotIndex != -1)
                {
                    SurfaceListBoxItem item = (SurfaceListBoxItem)listHotspotNav.Items.GetItemAt(m_currentSelectedHotspotIndex);
                    int index = (int)item.Tag;
                    m_hotspotCollection.unloadHotspotIcon(index);
                }
                m_hotspotCollection.loadAllHotspotsIcon(HotspotOverlay, MSIScatterView, msi);
                if (m_currentSelectedHotspotIndex != -1)
                {
                    SurfaceListBoxItem item = (SurfaceListBoxItem)listHotspotNav.Items.GetItemAt(m_currentSelectedHotspotIndex);
                    int index = (int)item.Tag;
                    m_hotspotCollection.HotspotIcons[index].changeToHighLighted();
                }
                toggleHotspots.Content = "Hotspots Off";

            }
            else
            {
                m_hotspotCollection.unloadAllHotspotsIcon();
                toggleHotspots.Content = "Hotspots On";
            }
            m_hotspotOnOff = !m_hotspotOnOff;
        }

        /// <summary>
        /// Called a user chooses to turn on/off hotspot icons.
        /// </summary>
        /// 
        private void toggleHotspots_Click(object sender, RoutedEventArgs e)
        {
            if (m_hotspotOnOff == false)
            {
                if (m_currentSelectedHotspotIndex != -1)
                {
                    SurfaceListBoxItem item = (SurfaceListBoxItem)listHotspotNav.Items.GetItemAt(m_currentSelectedHotspotIndex);
                    int index = (int)item.Tag;
                    m_hotspotCollection.unloadHotspotIcon(index);
                }
                m_hotspotCollection.loadAllHotspotsIcon(HotspotOverlay, MSIScatterView, msi);
                if (m_currentSelectedHotspotIndex != -1)
                {
                    SurfaceListBoxItem item = (SurfaceListBoxItem)listHotspotNav.Items.GetItemAt(m_currentSelectedHotspotIndex);
                    int index = (int)item.Tag;
                    m_hotspotCollection.HotspotIcons[index].changeToHighLighted();
                }
                toggleHotspots.Content = "Hotspots Off";

            }
            else
            {
                m_hotspotCollection.unloadAllHotspotsIcon();
                toggleHotspots.Content = "Hotspots On";
            }
            m_hotspotOnOff = !m_hotspotOnOff;
        }

        /// <summary>
        /// Feed the hotspot navigator listbox with proper contents.
        /// Which hotspots are displayed are based on the search box above the listbox.
        /// </summary>
        private void fillHotspotNavListBox()
        {
            listHotspotNav.Items.Clear();
            m_hotspotCollection.unloadAllHotspotsIcon();
            if (m_hotspotCollection.Hotspots != null)
            {
                for (int i = 0; i < m_hotspotCollection.Hotspots.Length; i++)
                {
                    if (m_hotspotCollection.IsSelected[i] == true)
                    {
                        SurfaceListBoxItem item = new SurfaceListBoxItem();
                        item.Tag = i; // the tag stores the index (position of the hotspot in hotspotCollection)
                        item.Content = m_hotspotCollection.Hotspots[i].Name;
                        listHotspotNav.Items.Add(item);
                        if (m_hotspotOnOff == true)
                        {
                            m_hotspotCollection.loadHotspotIcon(i, HotspotOverlay, MSIScatterView, msi);
                        }
                    }
                }
                _noHotspots = false;
                if (m_hotspotCollection.Hotspots.Length == 0)
                {
                    SurfaceListBoxItem item = new SurfaceListBoxItem();
                    item.Content = "(No hotspots for this artwork)";
                    listHotspotNav.Items.Add(item);
                }
            }
            else
            {
                SurfaceListBoxItem item = new SurfaceListBoxItem();
                item.Content = "(No hotspots for this artwork)";
                listHotspotNav.Items.Add(item);
            }

        }

        private bool hotspotNavExpanded = false;
        private bool assocDocNavExpanded = false;
        private bool hotspotAnimating = false;
        public void expandHotspotNav(object sender, EventArgs e)
        {
            if (!hotspotNavExpanded && !hotspotAnimating)
            {
                hotspotAnimating = true;
                hotspotNavExpanded = true;
                assocDocNavExpanded = false;
                DoubleAnimation hotspotHeightAnim = getDoubleAnimation(listHotspotNav.ActualHeight, 335, .3);
                hotspotHeightAnim.Completed += new EventHandler(hotspotHeightAnim_Completed);
                DoubleAnimation assocDocHeightAnim = getDoubleAnimation(treeDocs.ActualHeight, 0, .2);
                DoubleAnimation opacityAnim = getDoubleAnimation(1, 0, .25);
                opacityAnim.Completed += new EventHandler(HSopacityAnim_Completed);

                treeDocs.BeginAnimation(HeightProperty, assocDocHeightAnim);
                listHotspotNav.BeginAnimation(HeightProperty, hotspotHeightAnim);
                sBDocsSearchClear.BeginAnimation(OpacityProperty, opacityAnim);
                sTextBoxDocsSearch.BeginAnimation(OpacityProperty, opacityAnim);

                treeDocs.Height = 0;
                listHotspotNav.Height = 335;
                HotspotNav.Height = 80;
                sBHotSpotClear.Visibility = Visibility.Visible;
                sTextBoxHotSpotSearch.Visibility = Visibility.Visible;
                listHotspotNav.Visibility = Visibility.Visible;
            }
            else if (hotspotNavExpanded && !hotspotAnimating)
            {
                hotspotAnimating = true;
                hotspotNavExpanded = false;
                assocDocNavExpanded = false;
                DoubleAnimation hotspotHeightAnim = getDoubleAnimation(listHotspotNav.ActualHeight, 140, .3);
                hotspotHeightAnim.Completed += new EventHandler(hotspotHeightAnim_Completed);
                DoubleAnimation assocDocHeightAnim = getDoubleAnimation(treeDocs.ActualHeight, 140, .3);
                DoubleAnimation opacityAnim = getDoubleAnimation(0, 1, .3);

                treeDocs.BeginAnimation(HeightProperty, assocDocHeightAnim);
                treeDocs.BeginAnimation(OpacityProperty, opacityAnim);
                listHotspotNav.BeginAnimation(HeightProperty, hotspotHeightAnim);
                sBDocsSearchClear.BeginAnimation(OpacityProperty, opacityAnim);
                sTextBoxDocsSearch.BeginAnimation(OpacityProperty, opacityAnim);


                treeDocs.Height = 140;
                listHotspotNav.Height = 140;
                HotspotNav.Height = 80;

                sBDocsSearchClear.Visibility = Visibility.Visible;
                sTextBoxDocsSearch.Visibility = Visibility.Visible;
                treeDocs.Visibility = Visibility.Visible;
            }
        }

        public void HSopacityAnim_Completed(object sender, EventArgs e)
        {
            sBDocsSearchClear.Visibility = Visibility.Collapsed;
            sTextBoxDocsSearch.Visibility = Visibility.Collapsed;
            treeDocs.Visibility = Visibility.Collapsed;
        }

        public void hotspotHeightAnim_Completed(object sender, EventArgs e)
        {
            hotspotAnimating = false;
        }

        public void expandAssocDocNav(object sender, EventArgs e)
        {
            if (!assocDocNavExpanded && !hotspotAnimating)
            {
                hotspotAnimating = true;
                assocDocNavExpanded = true;
                hotspotNavExpanded = false;
                DoubleAnimation hotspotHeightAnim = getDoubleAnimation(treeDocs.ActualHeight, 315, .3);
                DoubleAnimation hotspotCanvasHeightAnim = getDoubleAnimation(HotspotNav.ActualHeight, 40, .3);
                hotspotHeightAnim.Completed += new EventHandler(hotspotHeightAnim_Completed);
                DoubleAnimation assocDocHeightAnim = getDoubleAnimation(listHotspotNav.ActualHeight, 0, .3);
                DoubleAnimation opacityAnim = getDoubleAnimation(1, 0, .3);
                opacityAnim.Completed += new EventHandler(ADopacityAnim_Completed);

                listHotspotNav.BeginAnimation(HeightProperty, assocDocHeightAnim);
                treeDocs.BeginAnimation(HeightProperty, hotspotHeightAnim);
                sBHotSpotClear.BeginAnimation(OpacityProperty, opacityAnim);
                sTextBoxHotSpotSearch.BeginAnimation(OpacityProperty, opacityAnim);
                HotspotNav.BeginAnimation(HeightProperty, hotspotCanvasHeightAnim);

                listHotspotNav.Height = 0;
                treeDocs.Height = 315;
                HotspotNav.Height = 40;

                sBDocsSearchClear.Visibility = Visibility.Visible;
                sTextBoxDocsSearch.Visibility = Visibility.Visible;
                treeDocs.Visibility = Visibility.Visible;
            }
            else if (assocDocNavExpanded && !hotspotAnimating)
            {
                hotspotAnimating = true;
                assocDocNavExpanded = false;
                hotspotNavExpanded = false;
                DoubleAnimation assocDocHeightAnim = getDoubleAnimation(treeDocs.ActualHeight, 140, .3);
                assocDocHeightAnim.Completed += new EventHandler(hotspotHeightAnim_Completed);
                DoubleAnimation hotspotHeightAnim = getDoubleAnimation(listHotspotNav.ActualHeight, 140, .3);
                DoubleAnimation opacityAnim = getDoubleAnimation(0, 1, .3);
                DoubleAnimation hotspotCanvasHeightAnim = getDoubleAnimation(HotspotNav.ActualHeight, 80, .3);

                listHotspotNav.BeginAnimation(HeightProperty, hotspotHeightAnim);
                listHotspotNav.BeginAnimation(OpacityProperty, opacityAnim);
                treeDocs.BeginAnimation(HeightProperty, assocDocHeightAnim);
                sBHotSpotClear.BeginAnimation(OpacityProperty, opacityAnim);
                sTextBoxHotSpotSearch.BeginAnimation(OpacityProperty, opacityAnim);
                HotspotNav.BeginAnimation(HeightProperty, hotspotCanvasHeightAnim);


                listHotspotNav.Height = 140;
                treeDocs.Height = 140;
                HotspotNav.Height = 80;

                sBHotSpotClear.Visibility = Visibility.Visible;
                sTextBoxHotSpotSearch.Visibility = Visibility.Visible;
                listHotspotNav.Visibility = Visibility.Visible;
            }

        }

        public void ADopacityAnim_Completed(object sender, EventArgs e)
        {
            sBHotSpotClear.Visibility = Visibility.Collapsed;
            sTextBoxHotSpotSearch.Visibility = Visibility.Collapsed;
            listHotspotNav.Visibility = Visibility.Collapsed;
        }



        private void sBResetAll_Click(object sender, RoutedEventArgs e)
        {
            resetAll();
        }


        /// <summary>
        /// Reset the artwork into original condition, wipe out all image manipulation tools applied
        /// </summary>
        public void resetAll()
        {
            try
            {

                mainImage.Source = null;
                sliderBrightness.Value = 0;
                sliderContrast.Value = 0;
                sliderSaturation.Value = 0;
                mainArtwork.Tools.CurrentBrightness = 0;
                mainArtwork.Tools.CurrentContrast = 0;
                mainArtwork.Tools.CurrentSaturation = 0;
                mainArtwork.Tools.Modified = false;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// Called when a user chooses 'Search' to filter hotspots
        /// Hotspots get filtered based on keywords.
        /// </summary>
        private void sBHotSpotSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //loadHotspots();
                //Hotspot hs = new Hotspot();
                m_hotspotCollection.search(sTextBoxHotSpotSearch.Text.ToLower());
                fillHotspotNavListBox();
            }
            catch (Exception exc)
            {
            }
        }

        /// <summary>
        /// Call when a user searches associated media
        /// </summary>
        private void sBDocsSearch_Click(object sender, RoutedEventArgs e)
        {
            String keyword = sTextBoxDocsSearch.Text;
            try
            {
                foreach (AssociatedDocListBoxItem currentItem in treeDocs.Items)
                {
                    string lowercaseLabel = currentItem.getLabel().ToLower();
                    string lowercaseKeyword = keyword.ToLower();
                    if (lowercaseLabel.Contains(lowercaseKeyword))
                        currentItem.Visibility = Visibility.Visible;
                    else
                        currentItem.Visibility = Visibility.Collapsed;
                }
            }
            catch(Exception ex)
            {
            }

        }


        /// <summary>
        /// search the tree based on the keyword provided
        /// </summary>
        /// <param name="keyword">The keyword used to search/</param>
        /// <param name="tree">Item collection in which search is performed</param>
        private void treeViewSearch(String keyword, ItemCollection tree)
        {
            foreach (TreeViewItem currentItem in tree)
            {
                if (currentItem.Header.ToString().Contains(keyword))
                    currentItem.Visibility = Visibility.Visible;
                else
                    currentItem.Visibility = Visibility.Hidden;
                treeViewSearch(keyword, currentItem.Items);
            }

        }

        /// <summary>
        /// Called when a user chooses another item in the hotspot listbox.
        /// Change the hotspot icons to highlight corresponding hotspots based on user selection.
        /// </summary>
        int m_currentSelectedHotspotIndex = -1;
        private void listHotspotNav_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_noHotspots)
            {
              
                SurfaceListBoxItem item;
                int index;
                if (listHotspotNav.SelectedIndex != -1)
                {
                    Hotspot selectedHotspot = m_hotspotCollection.Hotspots[listHotspotNav.SelectedIndex];
                    if (m_hotspotOnOff == true)
                    {
                        {
                            if (m_currentSelectedHotspotIndex != -1)
                            {
                                item = (SurfaceListBoxItem)listHotspotNav.Items.GetItemAt(m_currentSelectedHotspotIndex);
                                index = (int)item.Tag;
                                m_hotspotCollection.HotspotIcons[index].changeToNormal();
                            }
                            item = (SurfaceListBoxItem)listHotspotNav.Items.GetItemAt(listHotspotNav.SelectedIndex);
                            index = (int)item.Tag;
                            m_hotspotCollection.HotspotIcons[index].changeToHighLighted();
                        }
                    }
                    else
                    {
                        m_hotspotCollection.unloadAllHotspotsIcon();
                        item = (SurfaceListBoxItem)listHotspotNav.Items.GetItemAt(listHotspotNav.SelectedIndex);
                        index = (int)item.Tag;
                        m_hotspotCollection.loadHotspotIcon(index, HotspotOverlay, MSIScatterView, msi);
                        m_hotspotCollection.HotspotIcons[index].changeToHighLighted();
                    }
                    m_currentSelectedHotspotIndex = listHotspotNav.SelectedIndex;

                    // pan to the current selected hotspot:
                    Double[] size = this.findImageSize(m_hotspotCollection.Hotspots[index].artworkName);
                    Point dest = new Point(m_hotspotCollection.Hotspots[index].PositionX * size[0], m_hotspotCollection.Hotspots[index].PositionY * size[1]);
                    Point targetOffset = new Point(dest.X * msi.GetZoomableCanvas.Scale - msi.GetZoomableCanvas.ActualWidth * 0.5, dest.Y * msi.GetZoomableCanvas.Scale - msi.GetZoomableCanvas.ActualHeight * 0.5);
                    var duration = TimeSpan.FromMilliseconds(0.5 * 1000);

                    var easing = new QuadraticEase();
                    msi.GetZoomableCanvas.BeginAnimation(ZoomableCanvas.OffsetProperty, new PointAnimation(targetOffset, duration) { EasingFunction = easing }, HandoffBehavior.Compose);
                }
            }
        }

        public Double[] findImageSize(String artworkName)
        {
            Double[] sizes = new Double[2];
            XmlDocument newDoc = new XmlDocument();
            String imageFolder = "Data/Images/DeepZoom/" +artworkName + "/" + "dz.xml";
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

        #endregion

        #region msi navigator event handlers

        /// <summary>
        /// viewbox of msi --> msi_thumb_rect: synchronizes highlighted region of artwork thumbnail navigator with MSI viewbox (the part of the artwork that the user is looking at)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void msi_ViewboxChanged(Object sender, EventArgs e)
        {
            if (!_isInteractionOnThumb)
            {
                Rect viewbox = msi.GetZoomableCanvas.ActualViewbox;

                /* SIZE OF OVERLAY */
                ThumbSVI.Width = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * viewbox.Width) / msi.GetImageActualWidth;
                ThumbSVI.Height = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * viewbox.Height) / msi.GetImageActualHeight;

                /* POSITION OF OVERLAY */
                double msi_thumb_rect_centerX = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * viewbox.GetCenter().X) / msi.GetImageActualWidth;
                double msi_thumb_rect_centerY = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * viewbox.GetCenter().Y) / msi.GetImageActualHeight;

                ZoomableCanvas msi_thumb_zc = msi_thumb.GetZoomableCanvas;

                double msi_thumb_centerX = msi_thumb_zc.Offset.X + (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * 0.5);
                double msi_thumb_centerY = msi_thumb_zc.Offset.Y + (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * 0.5);

                double msi_thumb_rect_centerX_dist = msi_thumb_rect_centerX - msi_thumb_centerX + msi_thumb_zc.Offset.X;
                double msi_thumb_rect_centerY_dist = msi_thumb_rect_centerY - msi_thumb_centerY + msi_thumb_zc.Offset.Y;

                ThumbSVI.Center = new Point(msi_thumb_rect_centerX_dist + ThumbSV.Width / 2, msi_thumb_rect_centerY_dist + ThumbSV.Height / 2);
                //msi_thumb_rect.RenderTransform = new TranslateTransform(msi_thumb_rect_centerX_dist, msi_thumb_rect_centerY_dist);

                /* HOTSPOTS */
                double HotspotOverlay_centerX = (msi.GetZoomableCanvas.Scale * msi.GetImageActualWidth * 0.5) - msi.GetZoomableCanvas.Offset.X;
                double HotspotOverlay_centerY = (msi.GetZoomableCanvas.Scale * msi.GetImageActualHeight * 0.5) - msi.GetZoomableCanvas.Offset.Y;

                double msi_clip_centerX = msi.GetZoomableCanvas.ActualWidth * 0.5;
                double msi_clip_centerY = msi.GetZoomableCanvas.ActualHeight * 0.5;

                double HotspotOverlay_centerX_dist = HotspotOverlay_centerX - msi_clip_centerX;
                double HotspotOverlay_centerY_dist = HotspotOverlay_centerY - msi_clip_centerY;

                m_hotspotCollection.updateHotspotLocations(HotspotOverlay, MSIScatterView, msi);

            }
        }

        /// <summary>
        /// (initial setup) - viewbox of msi --> msi_thumb_rect: synchronizes highlighted region of artwork thumbnail navigator with MSI viewbox (the part of the artwork that the user is looking at)
        /// </summary>
        public void msi_ViewboxUpdate()
        { /* set initial size and position of zoom navigator */
            Rect viewbox = msi.GetZoomableCanvas.ActualViewbox;

            ThumbSVI.Width = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * viewbox.Width) / msi.GetImageActualWidth;
            ThumbSVI.Height = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * viewbox.Height) / msi.GetImageActualHeight;

            double msi_thumb_rect_centerX = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * viewbox.GetCenter().X) / msi.GetImageActualWidth;
            double msi_thumb_rect_centerY = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * viewbox.GetCenter().Y) / msi.GetImageActualHeight;

            ZoomableCanvas msi_thumb_zc = msi_thumb.GetZoomableCanvas;

            double msi_thumb_centerX = msi_thumb_zc.Offset.X + (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * 0.5);
            double msi_thumb_centerY = msi_thumb_zc.Offset.Y + (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * 0.5);

            double msi_thumb_rect_centerX_dist = msi_thumb_rect_centerX - msi_thumb_centerX + msi_thumb_zc.Offset.X;
            double msi_thumb_rect_centerY_dist = msi_thumb_rect_centerY - msi_thumb_centerY + msi_thumb_zc.Offset.Y;

            ThumbSVI.Center = new Point(msi_thumb_rect_centerX_dist + ThumbSV.Width / 2, msi_thumb_rect_centerY_dist + ThumbSV.Height / 2);
            //msi_thumb_rect.RenderTransform = new TranslateTransform(msi_thumb_rect_centerX_dist, msi_thumb_rect_centerY_dist);

            /* HOTSPOTS */

            double HotspotOverlay_centerX = (msi.GetZoomableCanvas.Scale * msi.GetImageActualWidth * 0.5) - msi.GetZoomableCanvas.Offset.X;
            double HotspotOverlay_centerY = (msi.GetZoomableCanvas.Scale * msi.GetImageActualHeight * 0.5) - msi.GetZoomableCanvas.Offset.Y;

            double msi_clip_centerX = msi.GetZoomableCanvas.ActualWidth * 0.5;
            double msi_clip_centerY = msi.GetZoomableCanvas.ActualHeight * 0.5;

            double HotspotOverlay_centerX_dist = HotspotOverlay_centerX - msi_clip_centerX;
            double HotspotOverlay_centerY_dist = HotspotOverlay_centerY - msi_clip_centerY;

            HotspotOverlay.RenderTransform = new TranslateTransform(HotspotOverlay_centerX_dist, HotspotOverlay_centerY_dist);

            m_hotspotCollection.updateHotspotLocations(HotspotOverlay, MSIScatterView, msi);

            /* disable msi_thumb's event handlers */
            msi_thumb.DisableEventHandlers();
        }

        /// <summary>
        /// TODO: msi_thumb_rect --> viewbox of msi: should synchronize artwork MSI viewbox (the part of the artwork that the user is looking at) with highlighted region of artwork thumbnail navigator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void msi_thumb_rect_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
        }

        /// <summary>
        /// (initial setup) - tells msi_ViewboxChanged to listen for changes to viewbox of msi 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void msi_thumb_rect_Loaded(object sender, RoutedEventArgs e)
        {
            /* Listen for changes in ActualViewboxProperty */
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
            dpd.AddValueChanged(msi.GetZoomableCanvas, msi_ViewboxChanged);

            DependencyPropertyDescriptor dpd2 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd2.AddValueChanged(ThumbSVI, ThumbSVI_CenterChanged);

            this.msi_ViewboxUpdate();

            ThumbSVI.SizeChanged += new SizeChangedEventHandler(ThumbSVI_SizeChanged);
            ThumbSVI.MouseWheel += new MouseWheelEventHandler(ThumbSVI_MouseWheel);
        }

        #endregion

        #region inter-mode navigation event handlers

        private void switchToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void ResetArtworkButton_Click(object sender, RoutedEventArgs e)
        {
            msi.ResetArtwork();
            msi_thumb.ResetArtwork();
            resetAll();
        }

        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "Are you sure you want to quit LADS?";
            string caption = "Quit LADS";
            System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
            System.Windows.Forms.DialogResult result;

            result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {

                Application.Current.Shutdown();
            }
        }

        public void goBack()
        {
            DeepZoomGrid.Visibility = Visibility.Visible;
            MainScatterView.Visibility = Visibility.Visible;
            toggleLeftSide();
        }

        #endregion

        #region UI methods

        public void msi_ChangeImage(String xml)
        {
            msi.Source = new DeepZoom.DeepZoomImageTileSource(new Uri(xml, UriKind.Relative));
            msi_thumb.Source = new DeepZoom.DeepZoomImageTileSource(new Uri(xml, UriKind.Relative));
        }

        public void BottomBarScroll(object sender, RoutedEventArgs e)
        {
            ScrollChangedEventArgs s = e as ScrollChangedEventArgs;
            double delta = s.HorizontalChange;
            foreach (WorkspaceElement w in DockedItems)
            {
                w.item.SetCurrentValue(ScatterViewItem.CenterProperty, new Point(w.item.ActualCenter.X - delta, w.item.ActualCenter.Y));
            }

        }

        public DoubleAnimation getDoubleAnimation(double from, double to, double duration)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = from;
            anim.To = to;
            anim.Duration = new Duration(TimeSpan.FromSeconds(duration));
            anim.FillBehavior = FillBehavior.Stop;
            return anim;
        }

        private double LeftPanelAnimationDuration = .3;
        private double toolsOriginalHeight;
        private double toolsOriginalWidth;
        private bool toolsExpanded = false;
        private double toolsOriginalSliderWidth;

        public void expandTools(object sender, EventArgs e)
        {
            double expansionRatio = 1.9;
            if (!toolsExpanded)
            {
                toolsExpanded = true;
                Tools.ClipToBounds = false;
                toolsOriginalHeight = Tools.ActualHeight;
                toolsOriginalWidth = Tools.ActualWidth;
                toolsOriginalSliderWidth = sliderBrightness.ActualWidth;
                DoubleAnimation widthAnim = getDoubleAnimation(Tools.ActualWidth, Tools.ActualWidth * expansionRatio, LeftPanelAnimationDuration);
                DoubleAnimation sliderAnim = getDoubleAnimation(sliderContrast.ActualWidth, sliderContrast.ActualWidth * expansionRatio, LeftPanelAnimationDuration);

                Tools.BeginAnimation(WidthProperty, widthAnim);
                sliderBrightness.BeginAnimation(WidthProperty, sliderAnim);
                sliderContrast.BeginAnimation(WidthProperty, sliderAnim);
                sliderSaturation.BeginAnimation(WidthProperty, sliderAnim);
                Tools.Width = Tools.ActualWidth * expansionRatio;
                sliderBrightness.Width = sliderContrast.ActualWidth * expansionRatio;
                sliderContrast.Width = sliderContrast.ActualWidth * expansionRatio;
                sliderSaturation.Width = sliderContrast.ActualWidth * expansionRatio;
            }
            else
            {
                toolsExpanded = false;
                Tools.ClipToBounds = true;
                DoubleAnimation widthAnim = getDoubleAnimation(Tools.ActualWidth, toolsOriginalWidth, LeftPanelAnimationDuration);
                DoubleAnimation sliderAnim = getDoubleAnimation(sliderContrast.ActualWidth, toolsOriginalSliderWidth, LeftPanelAnimationDuration);
                Tools.BeginAnimation(WidthProperty, widthAnim);
                sliderBrightness.BeginAnimation(WidthProperty, sliderAnim);
                sliderContrast.BeginAnimation(WidthProperty, sliderAnim);
                sliderSaturation.BeginAnimation(WidthProperty, sliderAnim);

                Tools.Width = toolsOriginalWidth;
                sliderBrightness.Width = toolsOriginalSliderWidth;
                sliderContrast.Width = toolsOriginalSliderWidth;
                sliderSaturation.Width = toolsOriginalSliderWidth;
            }
        }

        public void toggleLeftSide()
        {
            if (leftPanelVisible)
            {

                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = -leftPaneContent.ActualWidth;
                da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                LeftPanel.BeginAnimation(Canvas.LeftProperty, da);
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = -leftPaneContent.ActualWidth;
                da.To = 0;
                da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                LeftPanel.BeginAnimation(Canvas.LeftProperty, da);

            }
            leftPanelVisible = !leftPanelVisible;
        }

        public void LeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!tourSystem.TourPlaybackOn)
            {
                if (leftPanelVisible)
                {

                    DoubleAnimation da = new DoubleAnimation();
                    da.From = 0;
                    da.To = -leftPaneContent.ActualWidth;
                    da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                    LeftPanel.BeginAnimation(Canvas.LeftProperty, da);
                    collapseButtonRight.Visibility = Visibility.Visible;
                    collapseButtonLeft.Visibility = Visibility.Hidden;
                }
                else
                {
                    DoubleAnimation da = new DoubleAnimation();
                    da.From = -leftPaneContent.ActualWidth;
                    da.To = 0;
                    da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                    LeftPanel.BeginAnimation(Canvas.LeftProperty, da);
                    collapseButtonRight.Visibility = Visibility.Hidden;
                    collapseButtonLeft.Visibility = Visibility.Visible;

                }
                leftPanelVisible = !leftPanelVisible;
            }
            else
            {
                DoubleAnimation da2 = new DoubleAnimation();
                da2.From = 0;
                da2.To = 1;
                da2.Duration = new Duration(TimeSpan.FromSeconds(1.4));
                da2.AutoReverse = true;
                Text.BeginAnimation(OpacityProperty, da2);
                verticalMessage.BeginAnimation(OpacityProperty, da2);
            }
        }

        public void BottomButtonClick(object sender, RoutedEventArgs e)
        {
            if (!tourSystem.TourPlaybackOn && !tourSystem.IsExploreMode)
            {
                if (bottomPanelVisible)
                {
                    DoubleAnimation da = new DoubleAnimation();
                    da.From = (this.Height) * .8;
                    da.To = (this.Height) * .8 + Bar.ActualHeight;
                    da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                    BottomPanel.BeginAnimation(Canvas.TopProperty, da);
                    foreach (WorkspaceElement wke in DockedItems)
                    {
                        if (wke.isDocked) wke.item.IsEnabled = false;
                    }
                    collapseButtonDown.Visibility = Visibility.Hidden; //change the direction of the triangle
                    collapseButtonUp.Visibility = Visibility.Visible;
                }
                else
                {
                    DoubleAnimation da = new DoubleAnimation();
                    da.From = (this.Height) * .8 + Bar.ActualHeight;
                    da.To = (this.Height) * .8;
                    da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                    BottomPanel.BeginAnimation(Canvas.TopProperty, da);
                    foreach (WorkspaceElement wke in DockedItems)
                    {
                        if (wke.isDocked) wke.item.IsEnabled = true;
                    }
                    collapseButtonDown.Visibility = Visibility.Visible; //change the direction of the triangle
                    collapseButtonUp.Visibility = Visibility.Hidden;
                }
               
                
                bottomPanelVisible = !bottomPanelVisible;
            }
            else
            {//display text
               // textMessage.Opacity = 1;
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = 1;
                da.Duration = new Duration(TimeSpan.FromSeconds(1.4));
                da.AutoReverse = true;
                Text.BeginAnimation(OpacityProperty, da);
                textMessage.BeginAnimation(OpacityProperty, da);
            }
        }

        public void ShowBottomPanel ()
        {
            DoubleAnimation da = new DoubleAnimation();
            /////
            da.From = (this.Height) * .8 + Bar.ActualHeight;
            da.To = (this.Height) * .8;
            da.Duration = new Duration(TimeSpan.FromSeconds(.4));
            BottomPanel.BeginAnimation(Canvas.TopProperty, da);
            foreach (WorkspaceElement wke in DockedItems)
            {
                if (wke.isDocked) wke.item.IsEnabled = true;
            }
            bottomPanelVisible = !bottomPanelVisible;
        }

        public Canvas getMain()
        {
            return Main;
        }

        public SurfaceListBox getBar()
        {
            return Bar;
        }

        public SurfaceListBox getAssociatedDocToolBar()
        {
            return treeDocs;
        }

        public ScatterView getMainScatterView()
        {
            return MainScatterView;
        }

        private void mainImage_TouchDown(object sender, TouchEventArgs e)
        {
            resetAll();
        }

        private void mainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            resetAll();

        }

        #endregion


        private void sBResumeTour_Click(object sender, RoutedEventArgs e)
        {
            tourSystem.resumeExploringTour();
        }

        // Tour explore mode asset management for when an item is docked.
        public void tourExploreManageDock(DockableItem item)
        {
            if (tourSystem.IsExploreMode)
            {
                tourSystem.exploreAssetsInDock.Add(item.scatteruri, item);
                if (tourSystem.exploreAssetsOnCanvas.ContainsKey(item.scatteruri))
                    tourSystem.exploreAssetsOnCanvas.Remove(item.scatteruri);
                if (tourSystem.exploreDisposableAssets.ContainsKey(item.scatteruri))
                    tourSystem.exploreDisposableAssets.Remove(item.scatteruri);
            }
        }

        // Tour explore mode asset management for when an item is undocked.
        public void tourExploreManageUndock(DockableItem item)
        {
            if (tourSystem.IsExploreMode)
            {
                tourSystem.exploreAssetsOnCanvas.Add(item.scatteruri, item);
                if (tourSystem.exploreAssetsInDock.ContainsKey(item.scatteruri)) // it should...
                    tourSystem.exploreAssetsInDock.Remove(item.scatteruri);
            }
        }

        // Tour explore mode asset management for when an item is deleted.
        public void tourExploreManageDelete(DockableItem item)
        {
            if (tourSystem.IsExploreMode)
            {
                if (tourSystem.exploreAssetsInDock.ContainsKey(item.scatteruri))
                    tourSystem.exploreAssetsInDock.Remove(item.scatteruri);
                if (tourSystem.exploreAssetsOnCanvas.ContainsKey(item.scatteruri))
                    tourSystem.exploreAssetsOnCanvas.Remove(item.scatteruri);
                if (tourSystem.exploreDisposableAssets.ContainsKey(item.scatteruri))
                    tourSystem.exploreDisposableAssets.Remove(item.scatteruri);
            }
        }

        // Tour explore mode asset management for when an item should be added.
        // Takes the uri string for the potential item.
        // If it should be added, does asset management and returns a new DockableItem. It it shouldn't, just returns null.
        // Note that this method actually creates a new DockableItem, which is otherwise only done in AssociatedDocListBoxItem.onTouch.
        public DockableItem tourExploreManageAdd(String scatteruri, AssociatedDocListBoxItem adlbi)
        {
            Helpers _helpers = new Helpers();  // Stupid.
            if (tourSystem.IsExploreMode)
            {
                if (!tourSystem.exploreAssetsOnCanvas.ContainsKey(scatteruri) &&
                    !tourSystem.exploreAssetsInDock.ContainsKey(scatteruri) &&
                    !tourSystem.exploreDisposableAssets.ContainsKey(scatteruri))
                {
                    // Make the Dockable item (currently only images supported)
                    if (_helpers.IsImageFile(scatteruri))
                    {
                        DockableItem asset = new DockableItem(getMainScatterView(), this, getBar(), scatteruri, adlbi);
                        _openedAssets.Add(scatteruri, asset);
                        tourSystem.exploreAssetsOnCanvas.Add(scatteruri, asset);
                        return asset;
                    }

                }
            }
            return null;
        }

        private void tourAuthoring_Click(object sender, RoutedEventArgs e)
        {
            //hide the buttons
            collapseButtonRight.Visibility = Visibility.Hidden;
            collapseButtonLeft.Visibility = Visibility.Hidden;
            collapseButtonUp.Visibility = Visibility.Hidden;
            collapseButtonDown.Visibility = Visibility.Hidden;

            String filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                "\\Data\\Tour\\XML\\" + CurrNameInfo.PanelNumber + ".xml";

            if (!System.IO.File.Exists(filePath))
            {
                String fileContentString =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +

                "<TourStoryboard duration=\"40\" displayName=\"" + CurrNameInfo.PanelNumber + "\" description=\"" + "No Title" + "\">\r\n" +
                "<TourParallelTL type=\"artwork\" displayName=\"Main Artwork\" file=\"" + CurrNameInfo.PanelNumber + "\">\r\n" +
                "<TourEvent beginTime=\"-1\" type=\"ZoomMSIEvent\" scale=\"0.2\" toMSIPointX=\"0\" toMSIPointY=\"0\" duration=\"1\"></TourEvent>\r\n" +
                "</TourParallelTL>\r\n" +
                "</TourStoryboard>";
                tourSystem.LoadDictFromString(fileContentString);
            }
            else
                tourSystem.LoadDictFromXML(filePath);

            tourSystem.LoadTourPlaybackFromDict();
            tourSystem.LoadTourAuthoringUIFromDict();
            tourSystem.loadAuthoringGUI();
            TourLayout();
        }


        private void metaData_Click(object sender, RoutedEventArgs e)
        {
            SurfaceButton button = sender as SurfaceButton;

            if (button.Content.Equals("Assets list"))
            {
                this.showMetaList();
            }
            else
            {
                this.hideMetaList();
            }
        }

        public void hideMetaList()
        {
            Main.Children.Remove(newMeta);
        }
        public void showMetaList()
        {
            if (Main.Children.Contains(newMeta))
            {
                Main.Children.Remove(newMeta);
                return;
            }
            Main.Children.Add(newMeta);
            Canvas.SetZIndex(newMeta, 75);
            Canvas.SetLeft(newMeta, 410);
            Canvas.SetTop(newMeta, 100);
        }

        public void TourAuthoringSaveButton_Click(object sender, RoutedEventArgs e)
        {
            String filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                "\\Data\\Tour\\XML\\" + CurrNameInfo.PanelNumber + ".xml";
            tourSystem.SaveDictToXML(filePath);
            tourSystem.SaveInkCanvases();
            DoubleAnimation saveAnim = new DoubleAnimation();
            saveAnim.From = 1.0;
            saveAnim.To = 0.0;
            saveAnim.Duration = new Duration(TimeSpan.FromSeconds(2.0));
            saveAnim.FillBehavior = FillBehavior.Stop;
            tourSystem.getSaveSuccessfulLabel().BeginAnimation(OpacityProperty, saveAnim);
        }

        private void addAudioButton_Click(object sender, RoutedEventArgs e)
        {
        }

        public void newMediaTimeLine(String filePath, String fileName)
        {
            tourSystem.undoableActionPerformed();
            tourSystem.AddNewMetaDataTimeline(filePath, fileName);
        }

        public void LeftButtonAuthClick(object sender, RoutedEventArgs e)
        {
            if (authToolsVisible)
            {

                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = -authPanelContent.ActualWidth;
                da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                AuthLeftPanel.BeginAnimation(Canvas.LeftProperty, da);
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = -authPanelContent.ActualWidth;
                da.To = 0;
                da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                AuthLeftPanel.BeginAnimation(Canvas.LeftProperty, da);

            }
            authToolsVisible = !authToolsVisible;
        }

        private void applyTimeTourButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double newDuration = double.Parse(TourLengthTextBox.Text);
                if (newDuration < 15)
                {
                    MessageBox.Show("Tours must be longer than 15 seconds.");
                    return;
                }
                if (!tourSystem.resetTourLength(newDuration))
                    MessageBox.Show("A tour cannot be shorter than the end of its last event.");
            }
            catch (Exception exc)
            {
            }

            TourLengthBox.Visibility = Visibility.Collapsed;
            TimeBorder.Visibility = Visibility.Collapsed;
        }

        private void cancelTimeTourButton_Click(object sender, RoutedEventArgs e)
        {
            TourLengthBox.Visibility = Visibility.Collapsed;
            TimeBorder.Visibility = Visibility.Collapsed;

            shortTimeLabel.Visibility = Visibility.Hidden;
        }


        public void updateWebImages()
        {
            WebClient webClient = new WebClient();

            XmlDocument doc = new XmlDocument();
            doc.Load("Data/AnnenbergCollection.xml");
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {
                        foreach (XmlNode node in docNode.ChildNodes)
                        {
                            if (node.Name == "Image")
                            {
                                if (CurrNameInfo.PanelNumber != node.Attributes.GetNamedItem("path").InnerText)
                                    continue;
                                foreach (XmlNode imgnode in node.ChildNodes)
                                {
                                    if (imgnode.Name == "Metadata")
                                    {
                                        foreach (XmlNode group in imgnode.ChildNodes)
                                        {
                                            foreach (XmlNode file in group.ChildNodes)
                                            {
                                                bool urlExists = false;
                                                string metadatafilename = file.Attributes.GetNamedItem("Filename").InnerText;
                                                if( file.Attributes.GetNamedItem("Type")!=null){
                                                    string fileType = file.Attributes.GetNamedItem("Type").InnerText;
                                                    if (fileType == "Web")
                                                    {
                                                        string url = file.Attributes.GetNamedItem("URL").InnerText;
                                                        try
                                                        {
                                                            String response = webClient.DownloadString(url);
                                                            urlExists = true;
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            urlExists = false;
                                                        }
                                                        if (urlExists)
                                                        {
                                                            string filepath = "Data/Images/Metadata/" + metadatafilename;
                                                            DateTime lastModifiedTime = File.GetLastWriteTimeUtc(@filepath);
                                                            DateTime currUtcTime = DateTime.UtcNow;
                                                            TimeSpan span = currUtcTime.Subtract(lastModifiedTime);
                                                            if (span.Days > 0 || span.Hours > 0 || span.Minutes > 5)
                                                            {
                                                                downloadAndSaveImage(url, @filepath);
                                                                reloadMetadata(CurrNameInfo.PanelNumber);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void downloadAndSaveImage(string url, string filepath)
        {
            System.Drawing.Image drawingImage = null;
            System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
            try
            {
                System.Net.HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                httpWebRequest.AllowWriteStreamBuffering = true;
                httpWebRequest.Timeout = 20000;
                System.Net.WebResponse response = httpWebRequest.GetResponse();
                System.IO.Stream stream = response.GetResponseStream();
                drawingImage = System.Drawing.Image.FromStream(stream);

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(drawingImage);
                IntPtr hBitmap = bmp.GetHbitmap();
                System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                wpfImage.Source = WpfBitmap;
                wpfImage.Width = 500;
                wpfImage.Height = 600;
                wpfImage.Stretch = System.Windows.Media.Stretch.Fill;
                bmp.Save(@filepath);
                stream.Close();
                response.Close();
                response.Close();
            }

            catch (Exception e)
            {
            }
        }

        private void sTextBoxHotSpotSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                m_hotspotCollection.search(sTextBoxHotSpotSearch.Text.ToLower());
                fillHotspotNavListBox();
            }
            catch (Exception exc)
            {
            }
        }

        private void sBHotSpotClear_Click(object sender, RoutedEventArgs e)
        {
            sTextBoxHotSpotSearch.Text = "";
            m_hotspotCollection.search("");
            fillHotspotNavListBox();
        }

        private void sBDocsSearchClear_Click(object sender, RoutedEventArgs e)
        {
            sTextBoxDocsSearch.Text = "";
            sBDocsSearch_Click(sender, e);
        }

        private void sTextBoxDocsSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                sBDocsSearch_Click(sender, e);
            }
            catch (Exception exc)
            {
            }
        }

        private void sTextBoxDocsSearch_Click(object sender, EventArgs e)
        {
            if (!_searchedAssets)
            {
                sTextBoxDocsSearch.Text = "";
                _searchedAssets = true;
            }
        }

        private void sTextBoxHotSpotSearch_Click(object sender, EventArgs e)
        {
            if (!_searchedHotspots)
            {
                sTextBoxHotSpotSearch.Text = "";
                _searchedHotspots = true;
            }
        }

        public void addDockedItems(List<DockableItem> docked)
        {
            if (docked == null) return;
            foreach (object item in docked)
            {
                DockableItem dockitem = item as DockableItem;
                (dockitem.Parent as Microsoft.Surface.Presentation.Controls.ScatterView).Items.Remove(dockitem);
                dockitem.resetValues(MainScatterView, this, Bar);
                MainScatterView.Items.Add(dockitem);
                Random rnd = new Random();
                Point pt = new Point(rnd.Next((int)(dockitem.win.ActualWidth * .4), (int)(dockitem.win.ActualWidth*.6)),
                                                              rnd.Next((int)(dockitem.win.ActualHeight * .4), (int)(dockitem.win.ActualWidth * .6)));
                dockitem.Center = pt;
                dockitem.Orientation = rnd.Next(-20, 20);
                if (dockitem.image != null)
                {
                    dockitem.SetCurrentValue(HeightProperty, dockitem.image.Height);
                    dockitem.SetCurrentValue(WidthProperty, dockitem.image.Width);
                }
                dockitem.AddtoDock(dockitem, null);
            }
        }

        private void BottomButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (!tourSystem.TourPlaybackOn && !tourSystem.IsExploreMode)
            {
                if (bottomPanelVisible)
                {
                    DoubleAnimation da = new DoubleAnimation();
                    da.From = (this.Height) * .8;
                    da.To = (this.Height) * .8 + Bar.ActualHeight;
                    da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                    BottomPanel.BeginAnimation(Canvas.TopProperty, da);
                    foreach (WorkspaceElement wke in DockedItems)
                    {
                        if (wke.isDocked) wke.item.IsEnabled = false;
                    }
                    collapseButtonDown.Visibility = Visibility.Hidden; //change the direction of the triangle
                    collapseButtonUp.Visibility = Visibility.Visible;
                }
                else
                {
                    DoubleAnimation da = new DoubleAnimation();
                    da.From = (this.Height) * .8 + Bar.ActualHeight;
                    da.To = (this.Height) * .8;
                    da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                    BottomPanel.BeginAnimation(Canvas.TopProperty, da);
                    foreach (WorkspaceElement wke in DockedItems)
                    {
                        if (wke.isDocked) wke.item.IsEnabled = true;
                    }
                    collapseButtonDown.Visibility = Visibility.Visible; //change the direction of the triangle
                    collapseButtonUp.Visibility = Visibility.Hidden;
                }


                bottomPanelVisible = !bottomPanelVisible;
            }
            else
            {//display text
                // textMessage.Opacity = 1;
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = 1;
                da.Duration = new Duration(TimeSpan.FromSeconds(1.4));
                da.AutoReverse = true;
                Text.BeginAnimation(OpacityProperty, da);
                textMessage.BeginAnimation(OpacityProperty, da);
            }
        }

        private void ThumbSVI_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _isInteractionOnThumb = true;

            double scale;

            if (e.Delta > 0)
            {
                scale = 1.1;
            }
            else
            {
                scale = 0.9;
            }
            double newW = ThumbSVI.Width* scale;
            double newH = ThumbSVI.Height * scale;

            //if (newW <= ThumbSVI.MaxWidth && newW >= ThumbSVI.MinWidth && newH <= ThumbSVI.MaxHeight && newH >= ThumbSVI.MinHeight)
            {
                ThumbSVI.Width = newW;
                ThumbSVI.Height = newH;
            }
        }

        private bool _ignoreReverse = false;
        private void ThumbSVI_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isInteractionOnThumb)
            {
                if (msi.GetZoomableCanvas != null)
                {
                    ZoomableCanvas zc = msi.GetZoomableCanvas;

                    double oldW = zc.ActualViewbox.Width;
                    double oldH = zc.ActualViewbox.Height;

                    double zoomscale = e.NewSize.Height / e.PreviousSize.Height;
                    
                    Point center = new Point(-zc.Offset.X + (zc.Scale * msi.GetImageActualWidth) / 2, -zc.Offset.Y + (zc.Scale * msi.GetImageActualHeight) / 2);

                    msi.ScaleCanvas(1.0/zoomscale, center);

                    double scale = msi.GetZoomableCanvas.Scale * msi.GetImageActualWidth / (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth);
                    double x = (ThumbSVI.Center.X - ThumbSVI.Width / 2) - (ThumbSV.Width - (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth)) / 2;
                    double y = (ThumbSVI.Center.Y - ThumbSVI.Height / 2) - (ThumbSV.Height - (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight)) / 2;
                    msi.GetZoomableCanvas.Offset = new Point(x*scale, y*scale);

                    if (!_ignoreReverse)
                    {
                        if (zc.Viewbox.Height == oldH || zc.Viewbox.Width == oldW)
                        {
                            ThumbSVI.Width = e.PreviousSize.Width;
                            ThumbSVI.Height = e.PreviousSize.Height;
                            _ignoreReverse = true;
                        }
                        else
                        {
                            _ignoreReverse = false;
                        }
                    }
                }
            }
        }

        private Point preCenter;
        private void ThumbSVI_CenterChanged(Object sender, EventArgs e)
        {
            if (_isInteractionOnThumb)
            {
                if (preCenter == null)
                {
                    preCenter = ThumbSVI.Center;
                }
                //if (ThumbSVI.Center.X > ThumbSV.Width / 2 - msi_thumb.ActualWidth / 2 && ThumbSVI.Center.X > ThumbSV.Width / 2 + msi_thumb.ActualWidth / 2 && ThumbSVI.Center.Y > ThumbSV.Height / 2 - msi_thumb.ActualHeight / 2 && ThumbSVI.Center.Y > ThumbSV.Height / 2 + msi_thumb.ActualHeight / 2)
                //{
                    if (msi.GetZoomableCanvas != null)
                    {
                        double scale = msi.GetZoomableCanvas.Scale * msi.GetImageActualWidth / (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth);
                        Console.Out.WriteLine("scale: " + scale);
                        Console.Out.WriteLine("precenter: " + preCenter);
                        Console.Out.WriteLine("newcenter: " + ThumbSVI.Center);
                        Console.Out.WriteLine("offset before: "+msi.GetZoomableCanvas.Offset);
                        msi.GetZoomableCanvas.Offset = new Point(msi.GetZoomableCanvas.Offset.X + (ThumbSVI.Center.X - preCenter.X) * scale, msi.GetZoomableCanvas.Offset.Y + (ThumbSVI.Center.Y - preCenter.Y) * scale);
                        Console.Out.WriteLine("offset after: "+msi.GetZoomableCanvas.Offset);
                        //preCenter = ThumbSVI.Center;
                    }
                //}
                //else
                //{
                //    ThumbSVI.Center = preCenter;
                //}
            }
            preCenter = ThumbSVI.Center;
        }

        private bool _isInteractionOnThumb = false;
        private void Rectangle_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _isInteractionOnThumb = true;
            e.Handled = false;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isInteractionOnThumb = true;
            e.Handled = false;
        }

        private void msi_TouchDown(object sender, TouchEventArgs e)
        {
            _isInteractionOnThumb = false;
            e.Handled = false;
        }

        private void msi_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isInteractionOnThumb = false;
            e.Handled = false;
        }

        private void msi_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _isInteractionOnThumb = false;
            e.Handled = false;
        }

        private void help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.showHelp();
        }

        private void help_TouchDown(object sender, TouchEventArgs e)
        {
            this.showHelp();
        }

        private void showHelp()
        {
            helpWindow.ShowHelp(false);
            Canvas.SetZIndex(helpWindow, 100);
        }
    
    }

    public class WorkspaceElement : SurfaceListBoxItem
    {
        public bool isDocked = false;
        public Point Center;
        public DockableItem item;
        public ArtworkModeWindow artmodewin;
        public SurfaceListBox bar;
        public string scatteruri;
        public DockedItemInfo info;
        public Point trackedCenter;
        DependencyPropertyDescriptor dpd;

        public WorkspaceElement()
        {
            this.PreviewMouseDown += new MouseButtonEventHandler(WorkspaceElement_PreviewMouseDown);
            this.PreviewTouchDown += new EventHandler<TouchEventArgs>(WorkspaceElement_PreviewTouchDown);
        }

        public void releaseItem() {
                
                item.Opacity = 1.0;
                item.CanRotate = true;
                item.isDocked = false;
                item.CanMove = true;

                Helpers helper = new Helpers();
                DoubleAnimation heightAnim = new DoubleAnimation();
                
                heightAnim.From = item.barImageHeight;
                heightAnim.To = item.oldHeight;
                heightAnim.Duration = new Duration(TimeSpan.FromSeconds(.3));
                heightAnim.FillBehavior = FillBehavior.Stop;
                DoubleAnimation widthAnim = new DoubleAnimation();
                widthAnim.From = item.barImageWidth;
                widthAnim.To = item.oldWidth;
                widthAnim.Duration = new Duration(TimeSpan.FromSeconds(.3));
                widthAnim.FillBehavior = FillBehavior.Stop;
                item.BeginAnimation(HeightProperty, heightAnim);
                item.BeginAnimation(WidthProperty, widthAnim);

                this.Opacity = 0;
                DoubleAnimation dockwidthAnim = new DoubleAnimation();
                dockwidthAnim.Completed += anim2Completed;
                dockwidthAnim.From = item.barImageWidth;
                dockwidthAnim.To = 0;
                dockwidthAnim.Duration = new Duration(TimeSpan.FromSeconds(.3));
                dockwidthAnim.FillBehavior = FillBehavior.Stop;
                this.BeginAnimation(WidthProperty, dockwidthAnim);

                item.isDocked = false;
                artmodewin.DockedDockableItems.Remove(item);
                artmodewin.SavedDockedItems.Remove(info);

                artmodewin.BarOffset -= this.ActualWidth;
                int dex = artmodewin.DockedItems.IndexOf(this);
                for (int i = dex + 1; i < artmodewin.DockedItems.Count; i++)
                {
                    WorkspaceElement w = artmodewin.DockedItems[i] as WorkspaceElement;
                    w.item.Center = new Point(w.item.Center.X - this.ActualWidth, w.item.Center.Y);
                }

        }
        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            
            string message = "Are you sure you want to quit LADS?";
            string caption = "Quit LADS";
            System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
            System.Windows.Forms.DialogResult result;

            result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {

                Application.Current.Shutdown();
            }
        }

        // Same as releaseItem(), but without the animation, and returns the released item.
        // It makes sense to use this if you are undocking items to be further manipulated programmatically.
        public DockableItem quickReleaseItem()
        {
            item.Opacity = 1.0;
            item.CanRotate = true;
            item.isDocked = false;
            item.CanMove = true;
            item.Visibility = Visibility.Visible;
            item.Height = item.oldHeight;
            item.Width = item.oldWidth;
            Helpers helper = new Helpers();
            this.Opacity = 0;

            // Should we keep the dock width animation?
            // Will this break when multiple things are undocked simultaneously?
            DoubleAnimation dockwidthAnim = new DoubleAnimation();
            //dockwidthAnim.Completed += anim2Completed;
            dockwidthAnim.From = item.barImageWidth;
            dockwidthAnim.To = 0;
            dockwidthAnim.Duration = new Duration(TimeSpan.FromSeconds(.3));
            dockwidthAnim.FillBehavior = FillBehavior.Stop;
            this.BeginAnimation(WidthProperty, dockwidthAnim);

            item.isDocked = false;
            artmodewin.DockedDockableItems.Remove(item);
            artmodewin.SavedDockedItems.Remove(info);
            artmodewin.BarOffset -= this.ActualWidth;
            int dex = artmodewin.DockedItems.IndexOf(this);
            for (int i = dex + 1; i < artmodewin.DockedItems.Count; i++)
            {
                WorkspaceElement w = artmodewin.DockedItems[i] as WorkspaceElement;
                w.item.Center = new Point(w.item.Center.X - this.ActualWidth, w.item.Center.Y);
            }
            artmodewin.Bar.Items.Remove(this);
            artmodewin.DockedItems.Remove(this);

            // From anim2Completed.
            this.Visibility = Visibility.Collapsed;
            this.IsHitTestVisible = true;
            item.MouseMove -= WorkspaceElement_MouseMove;

            return item;
        }

        public void WorkspaceElement_PreviewMouseDown(object sender, MouseEventArgs e)
        {

            if (artmodewin.IsTourOn)
            {
                if (artmodewin.IsExploreOn)
                {
                    artmodewin.tourExploreManageUndock(item);
                }
                else
                {
                    return;
                }
            }
            item.touchDown = true;
            if (item.isDocked && item.Center.X > artmodewin.ActualWidth * .2 && !item.isAnimating && artmodewin.bottomPanelVisible)
            {
                this.IsHitTestVisible = false;
                item.isAnimating = true;
                item.Visibility = Visibility.Visible;
                item.CaptureMouse();
                item.Center = new Point(e.MouseDevice.GetPosition(artmodewin).X, e.MouseDevice.GetPosition(artmodewin).Y);


                trackedCenter = item.Center;
                item.MouseMove += new MouseEventHandler(WorkspaceElement_MouseMove);

                this.releaseItem();
            }
        }


        public void WorkspaceElement_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (artmodewin.IsTourOn)
            {
                if (artmodewin.IsExploreOn)
                {
                    artmodewin.tourExploreManageUndock(item);
                }
                else
                {
                    return;
                }
            }
            item.touchDown = true;
            if (item.isDocked && item.Center.X > artmodewin.ActualWidth * .2 && !item.isAnimating && artmodewin.bottomPanelVisible)
            {
                item.isAnimating = true;
                item.Visibility = Visibility.Visible;
                item.CaptureTouch(e.TouchDevice);
                item.Center = new Point(e.TouchDevice.GetPosition(artmodewin).X, e.TouchDevice.GetPosition(artmodewin).Y);

                trackedCenter = item.Center;
                item.TouchMove += new EventHandler<TouchEventArgs>(WorkspaceElement_TouchMove);

                this.releaseItem();
            }
        }


        public void anim2Completed(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            artmodewin.DockedItems.Remove(this);
            item.isDocked = false;

            this.IsHitTestVisible = true;
            item.MouseMove -= WorkspaceElement_MouseMove;
            item.Center = trackedCenter;
            item.Height = item.oldHeight;
            item.Width = item.oldWidth;
            dpd = null;

            item.isAnimating = false;

        }

        void WorkspaceElement_MouseMove(object sender, MouseEventArgs e)
        {
            item.Center = new Point(e.MouseDevice.GetPosition(artmodewin).X, e.MouseDevice.GetPosition(artmodewin).Y);
            trackedCenter = item.Center;
        }

        void WorkspaceElement_TouchMove(object sender, TouchEventArgs e)
        {
            item.Center = new Point(e.TouchDevice.GetPosition(artmodewin).X, e.TouchDevice.GetPosition(artmodewin).Y);
            trackedCenter = item.Center;

        }

        void TrackCenterListener(object sender, EventArgs e)
        {
            item.Center = trackedCenter;
        }

    }



    public class DockedItemInfo
    {
        public string scatteruri;
        public double savedOldHeight;
        public double savedOldWidth;
        public double savedWKEWidth;

    }


}