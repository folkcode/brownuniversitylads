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
using System.ComponentModel;
using System.Windows.Media.Animation;
using Knowledge_Web;
using DeepZoom;
using System.Windows.Threading;
using System.Collections;
using LADSArtworkMode.TourEvents;
using System.Diagnostics;
using DeepZoom.Controls;
using Microsoft.Surface.Presentation.Generic;
using System.IO;
using System.Windows.Ink;
using System.Xml;
using System.Net;


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

        public String currentArtworkFileName;
        public String currentArtworkTitle;

        bool leftPanelVisible;
        public bool bottomPanelVisible;
        bool scatteremoved;
        public int dockedItems;
        bool knowledgeWebOn = false;
        KnowledgeWeb newWeb;
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

        private TourSystem tourSystem; // tour authoring & playback system



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

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArtworkModeWindow(string currentArtworkFName)
        {
            InitializeComponent();
            String[] c = Environment.GetCommandLineArgs();
            Console.Out.WriteLine("command" + c[0]);

            if (c.Length != 1)
            {
                if (c[1].Contains("noauthoring"))
                {
                    //tourAuthoring.Visibility = Visibility.Hidden;
                    Main.Children.Remove(tourAuthoringButton);
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

            currentArtworkFileName = currentArtworkFName;
            newMeta = new metadata_lists(this, currentArtworkFileName);
            authToolsVisible = true;

            //newWeb = new KnowledgeWeb(ImageArea, ImageArea.Height, ImageArea.Width, "", this);

            //newWeb.Hide();

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            //dpd.AddValueChanged(MainScatterViewItem, ScatterViewCenterChanged);
            //this.LayoutArtworkMode();

            // Hotspots
            m_hotspotCollection = new HotspotCollection();

            tourSystem = new TourSystem(this);

            // event handlers for tour testing buttons

            // event handlers for tour playback/authoring

            tourControlButton.Click += tourSystem.TourControlButton_Click;
            tourStopButton.Click += tourSystem.TourStopButton_Click;
            tourAuthoringDoneButton.Click += tourSystem.TourAuthoringDoneButton_Click;
            tourAuthoringDeleteButton.Click += tourSystem.TourAuthoringDeleteButton_Click;
            drawPaths.Click += tourSystem.drawPaths_Click;
            metaData.Click += new RoutedEventHandler(metaData_Click);
            tourAuthoringSaveButton.Click += TourAuthoringSaveButton_Click;
            addAudioButton.Click += tourSystem.grabSound;
            addAudioButton.Visibility = Visibility.Collapsed;
        }


        #region initialization

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // HotspotModel hotspotModel = this.Resources["HotspotBindingData"] as HotspotModel;
            // this.m_hotspotCollection.Initialize(hotspotModel);

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

        public void InitTourLayout()
        {
            msi.Visibility = Visibility.Hidden;
            msi_thumb.Visibility = Visibility.Hidden;
            tourAuthoringButton.Visibility = Visibility.Collapsed;
            switchToCatalogButton.Visibility = Visibility.Collapsed;
            //artModeWin.activateKW.Visibility = Visibility.Collapsed;
            resetArtworkButton.Visibility = Visibility.Collapsed;
            exitButton.Visibility = Visibility.Collapsed;
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
            if (Main.ActualWidth / 1920.0 > Main.ActualHeight / 1080.0)
            {
                //We have to do more scaling to get height, so we scale to the width (else we'll overscale width)
                Console.WriteLine("Scaling to Width:" + (Main.ActualWidth));
                ImageArea.RenderTransform = new ScaleTransform((Main.ActualWidth - 1) / 1920.0, (Main.ActualWidth - 1) / 1920.0);
                //artModeWin.MainScatterView.RenderTransform = new ScaleTransform((System.Windows.SystemParameters.WorkArea.Width) / 1920.0, (System.Windows.SystemParameters.WorkArea.Width) / 1920.0);
            }
            else
            {
                Console.WriteLine("Scaling to height:" + (Main.ActualHeight));
                ImageArea.RenderTransform = new ScaleTransform((Main.ActualHeight - 1) / 1080.0, (Main.ActualHeight - 1) / 1080.0);
                //artModeWin.MainScatterView.RenderTransform = new ScaleTransform((System.Windows.SystemParameters.WorkArea.Height) / 1080.0, (System.Windows.SystemParameters.WorkArea.Height) / 1080.0);
            }
        }

        public void NewImageSelected_Handler(object sender, EventArgs e)
        {
            this.Show();
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
            }
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
            msi.Visibility = Visibility.Visible;
            msi_thumb.Visibility = Visibility.Visible;
            tourAuthoringButton.Visibility = Visibility.Visible;
            switchToCatalogButton.Visibility = Visibility.Visible;
            //artModeWin.activateKW.Visibility = Visibility.Collapsed;
            resetArtworkButton.Visibility = Visibility.Visible;
            exitButton.Visibility = Visibility.Visible;
            MainScatterView.Visibility = Visibility.Visible;
            HotspotOverlay.Visibility = Visibility.Visible;
            ImageArea.RenderTransform = null;
        }
        /// <summary>
        /// Called when the user choose an artwork from the artwork selection mode. Initialize necessary properties.
        /// </summary>
        public void LayoutArtworkMode(String filename)
        {
            //Console.WriteLine("LayoutArworkMode called");
            double MainHeight = this.Height;// -30; // 1094 - 14
            double MainWidth = this.Width;// -30;  // 1934 - 14

            //Console.WriteLine("MainWidth = " + MainWidth + ", MainHeight = " + MainHeight);
            //Console.WriteLine(this.ActualHeight);
            //Console.WriteLine(this.ActualWidth);
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

            //sTextBoxDocsSearch.Width = sectionWidth - sBDocsSearch.ActualWidth * 2.0;
            //sTextBoxHotSpotSearch.Width = sectionWidth - sBHotSpotSearch.ActualWidth *2.0;
            SectionListBoxMaster.SetCurrentValue(Canvas.TopProperty, labelSize * 2.1);
            //SectionListBoxMaster.Height = sectionHeight - labelSize * 2.1;
            SectionListBoxMaster.Width = sectionWidth - 10;

            //LeftPanelButtonMaster.Height = labelSize * .75;
            LeftPanelButtonMaster.Width = 10;
            LeftPanelButtonMaster.FontSize = SectionTitlesMaster.FontSize * .75;
            LeftPanelButtonMaster.Height = labelSize * .75;

            sBDocsSearch.SetCurrentValue(Canvas.TopProperty, labelSize);
            sBHotSpotSearch.SetCurrentValue(Canvas.TopProperty, labelSize);

            sTextBoxDocsSearch.SetCurrentValue(Canvas.TopProperty, labelSize);
            sTextBoxHotSpotSearch.SetCurrentValue(Canvas.TopProperty, labelSize);

            sTextBoxDocsSearch.Height = labelSize * .8;
            sTextBoxHotSpotSearch.Height = labelSize * .8;
            sTextBoxDocsSearch.Width = sectionWidth - sBDocsSearch.ActualWidth * 1.5;
            sTextBoxHotSpotSearch.Width = sectionWidth - sBDocsSearch.ActualWidth * 1.5;

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

            loadMetadata(filename);
            // m_hotspotCollection = new HotspotCollection();
            if (m_hotspotCollection.loadDocument(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\XMLFiles\\" + filename + ".xml"))
                fillHotspotNavListBox();
            this.initWorkspace();
        }

        public void loadMetadata(string filename)
        {
            string dataDir = "data/";
            Helpers helpers = new Helpers();
            XmlDocument doc = new XmlDocument();
            doc.Load("data/NewCollection.xml");
            //new AssociatedDocListBoxItem("video", "Data\\Videos\\Metadata\\lightning.avi", "c:\\Users\\Public\\Documents\\3rdLADS\\GCNav\\bin\\Debug\\Data\\Videos\\Metadata\\lightning.avi", this); 
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {
                        int startY = 1;
                        int endY = 0;
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
                                                Console.WriteLine("metadataFilename: " + metadatafilename);
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
        }

        public void reloadMetadata(string filename)
        {
            treeDocs.Items.Clear();
            Console.WriteLine("Reload Meta: Filename = "+filename);
            this.loadMetadata(filename);
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            this.reloadMetadata(currentArtworkFileName);
        }

        public void initWorkspace()
        {
            /*
            images = new WorkspaceElement[num_images];

            double spaceWidth = this.ActualWidth / num_images;
            Console.WriteLine("spaceWidth: " + spaceWidth);
            Console.WriteLine("Bar.ActualHeight: " + Bar.ActualHeight);
            for (int i = 0; i < num_images; i++)
            {
                images[i] = new WorkspaceElement();
                
                Bar.Children.Add(images[i]);
                //images[i].Background = Brushes.Fuchsia;
                images[i].Visibility = Visibility.Visible;
                images[i].Center = new Point(spaceWidth * i + spaceWidth / 2.0 + this.ActualWidth * .2, this.ActualHeight * .8 + Bar.ActualHeight / 2.0);
                images[i].SetCurrentValue(ScatterViewItem.WidthProperty, spaceWidth - 2 * spaceBuffer); 
                images[i].SetCurrentValue(ScatterViewItem.HeightProperty, Bar.ActualHeight - 2 * spaceBuffer);
                images[i].SetCurrentValue(Canvas.LeftProperty, spaceWidth * i);
                images[i].SetCurrentValue(Canvas.TopProperty, spaceBuffer);
                
            }
             */
        }

        #endregion

        #region hotspots/associated documents/tools

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            mainArtwork = new Artwork();

            /*try
            {
                ImageSourceConverter imgConv = new ImageSourceConverter();
                String path = "C:/temp/Penguins.jpg";
                ImageSource imageSource = (ImageSource)imgConv.ConvertFromString(path);
                mainArtwork.addImage("C:/temp/Penguins.jpg");

                mainImage.Source = imageSource;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }*/
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
                //m_hotspotCollection.loadAllHotspotsIcon(HotspotOverlay, MainScatterView,msi);
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
            // m_hotspotOnOff = false;
            //toggleHotspots.Content = "Hotspots On";
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
                            //m_hotspotCollection.loadHotspotIcon(i, HotspotOverlay, MainScatterView, msi);
                            m_hotspotCollection.loadHotspotIcon(i, HotspotOverlay, MSIScatterView, msi);
                            //m_hotspotCollection.HotspotIcons[i].changeToHighLighted();
                        }
                    }
                }
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
                sBDocsSearch.BeginAnimation(OpacityProperty, opacityAnim);
                sTextBoxDocsSearch.BeginAnimation(OpacityProperty, opacityAnim);

                treeDocs.Height = 0;
                listHotspotNav.Height = 335;
                HotspotNav.Height = 80;
                sBHotSpotSearch.Visibility = Visibility.Visible;
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
                //opacityAnim.Completed += new EventHandler(opacityAnim_Completed);

                treeDocs.BeginAnimation(HeightProperty, assocDocHeightAnim);
                treeDocs.BeginAnimation(OpacityProperty, opacityAnim);
                listHotspotNav.BeginAnimation(HeightProperty, hotspotHeightAnim);
                sBDocsSearch.BeginAnimation(OpacityProperty, opacityAnim);
                sTextBoxDocsSearch.BeginAnimation(OpacityProperty, opacityAnim);


                treeDocs.Height = 140;
                listHotspotNav.Height = 140;
                HotspotNav.Height = 80;

                sBDocsSearch.Visibility = Visibility.Visible;
                sTextBoxDocsSearch.Visibility = Visibility.Visible;
                treeDocs.Visibility = Visibility.Visible;
            }

        }

        public void HSopacityAnim_Completed(object sender, EventArgs e)
        {
            sBDocsSearch.Visibility = Visibility.Collapsed;
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
                sBHotSpotSearch.BeginAnimation(OpacityProperty, opacityAnim);
                sTextBoxHotSpotSearch.BeginAnimation(OpacityProperty, opacityAnim);
                HotspotNav.BeginAnimation(HeightProperty, hotspotCanvasHeightAnim);

                listHotspotNav.Height = 0;
                treeDocs.Height = 315;
                HotspotNav.Height = 40;

                sBDocsSearch.Visibility = Visibility.Visible;
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
                //opacityAnim.Completed += new EventHandler(opacityAnim_Completed);

                listHotspotNav.BeginAnimation(HeightProperty, hotspotHeightAnim);
                listHotspotNav.BeginAnimation(OpacityProperty, opacityAnim);
                treeDocs.BeginAnimation(HeightProperty, assocDocHeightAnim);
                sBHotSpotSearch.BeginAnimation(OpacityProperty, opacityAnim);
                sTextBoxHotSpotSearch.BeginAnimation(OpacityProperty, opacityAnim);
                HotspotNav.BeginAnimation(HeightProperty, hotspotCanvasHeightAnim);


                listHotspotNav.Height = 140;
                treeDocs.Height = 140;
                HotspotNav.Height = 80;

                sBHotSpotSearch.Visibility = Visibility.Visible;
                sTextBoxHotSpotSearch.Visibility = Visibility.Visible;
                listHotspotNav.Visibility = Visibility.Visible;
            }

        }

        public void ADopacityAnim_Completed(object sender, EventArgs e)
        {
            sBHotSpotSearch.Visibility = Visibility.Collapsed;
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
            // testButtons.Text = "Reset All clicked"; 
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
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// Called when a user chooses 'Search' to filter hotspots
        /// Hotspots get filtered based on keywords.
        /// </summary>
        private void sBHotSpotSearch_Click(object sender, RoutedEventArgs e)
        {
            //loadHotspots();
            //Hotspot hs = new Hotspot();
            m_hotspotCollection.search(sTextBoxHotSpotSearch.Text.ToLower());
            fillHotspotNavListBox();
            //hs.LoadDocument("abc");

            //hs.LoadDocument("abc"); // jcchin - commented out for now so I can compile LADS
        }


        /// <summary>
        /// Not used.
        /// </summary>
        private void loadHotspots()
        {
            //HotspotIconControl hotspotIcon = new HotspotIconControl(HotspotOverlay, MainScatterView);
            //HotspotOverlay.Children.Add(hotspotIcon);
        }


        /// <summary>
        /// Call when a user chooses 'Search' to filter associated documents.
        /// Not implemented yet
        /// </summary>
        private void sBDocsSearch_Click(object sender, RoutedEventArgs e)
        {
            //  testButtons.Text = "Docs search clicked";
            String keyword = sTextBoxDocsSearch.Text;
            //treeViewSearch(keyword, treeDocs.Items); // jcchin - commented out to build LADS
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
        /// Called when a value in the Brightness slider is changed
        /// Not used
        /// </summary>
        private void sliderBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            /* if (!sliderBrightnessDragStarted)
            {
                if (mainArtwork != null)
                {
                    try
                    {
                        mainArtwork.captureScreen(384, 0, 1536, 864);
                        mainArtwork.Tools.adjustBrightness((int)sliderBrightness.Value);
                        ImageSource imgs = (ImageSource)mainArtwork.returnImage();
                        mainImage.Source = imgs;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            } */
        }


        /// <summary>
        /// Called when complete dragging the Brightness slider
        /// </summary>
        private void sliderBrightness_DragCompleted(object sender, EventArgs e)
        {
            /*//  if (mainArtwork != null)
              {
                  try
                  {
                      //mainArtwork.captureScreen(384, 0, 1536, 864);
                      mainArtwork.captureScreen(msi, 1);
                      //mainArtwork.Tools.adjustBrightness((int)sliderBrightness.Value);
                      //ImageSource imgs = (ImageSource)mainArtwork.returnImage();
                     // mainImage.Source = imgs;


                      Byte[] result = mainArtwork.Tools.applyFilter("Brightness", (int)sliderBrightness.Value, (int)sliderContrast.Value, (int)sliderSaturation.Value);
                      mainArtwork.Tools.Modified = true;
                      BitmapSource myNewImage = BitmapSource.Create(mainArtwork.Tools.ImageWidth, mainArtwork.Tools.ImageHeight, 96, 96, PixelFormats.Bgra32, null, result, mainArtwork.Tools.ImageWidth * 4);
                      mainImage.Height = msi.RenderSize.Height;
                      mainImage.Width = msi.RenderSize.Width;
                      mainImage.Source = myNewImage;

                      TransformGroup transformGroup = new TransformGroup();
                      transformGroup.Children.Add(new ScaleTransform(1 * (1), -1 * (1)));
                     // transformGroup.Children.Add(new TranslateTransform(1832, -70));
                      //MessageBox.Show(msi.GetZoomableCanvas.Scale.ToString());

                      transformGroup.Children.Add(new TranslateTransform(0,  msi.RenderSize.Height));
                      mainImage.RenderTransform = transformGroup;

                      MultiScaleImageSpatialItemsSource ss = msi.SpatialSource;
                      //ss.applyTools();
                  }
                  catch (Exception ex)
                  {
                      MessageBox.Show(ex.Message);
                  }
                 // msi.Brightness = (int)sliderBrightness.Value;
                 // msi.Modified = true;
                  //msi.LoadCurrentZoomLevelTiles();
              }
             // sliderBrightnessDragStarted = false;*/
        }


        /// <summary>
        /// Called when start dragging Brightness slider
        /// Not used
        /// </summary>
        private void sliderBrightness_DragStarted(object sender, EventArgs e)
        {
        }


        /// <summary>
        /// Called when a value in the Contrast slider is changed
        /// Not used
        /// </summary>
        private void sliderContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            /* if (!sliderContrastDragStarted)
            {
                if (mainArtwork != null)
                {
                    try
                    {
                        mainArtwork.captureScreen(384, 0, 1536, 864);
                        mainArtwork.Tools.adjustContrast((int)sliderContrast.Value);
                        ImageSource imgs = (ImageSource)mainArtwork.returnImage();
                        mainImage.Source = imgs;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            } */
        }

        /// <summary>
        /// Called when complete dragging Contrast slider
        /// </summary>
        private void sliderContrast_DragCompleted(object sender, EventArgs e)
        {
            /*if (mainArtwork != null)
            {
                try
                {
                    //mainArtwork.captureScreen(384, 0, 1536, 864);
                    mainArtwork.captureScreen(msi, 1);
                   // mainArtwork.Tools.adjustContrast((int)sliderContrast.Value);
                   // ImageSource imgs = (ImageSource)mainArtwork.returnImage();
                   // mainImage.Source = imgs;

                    Byte[] result = mainArtwork.Tools.applyFilter("Contrast", (int)sliderBrightness.Value, (int)sliderContrast.Value, (int)sliderSaturation.Value);
                    mainArtwork.Tools.Modified = true;
                    BitmapSource myNewImage = BitmapSource.Create(mainArtwork.Tools.ImageWidth, mainArtwork.Tools.ImageHeight, 96, 96, PixelFormats.Bgra32, null, result, mainArtwork.Tools.ImageWidth * 4);
                    mainImage.Height = msi.RenderSize.Height;
                    mainImage.Width = msi.RenderSize.Width;
                    mainImage.Source = myNewImage;

                    TransformGroup transformGroup = new TransformGroup();
                    transformGroup.Children.Add(new ScaleTransform(1 * (1), -1 * (1)));
                    // transformGroup.Children.Add(new TranslateTransform(1832, -70));
                    transformGroup.Children.Add(new TranslateTransform(0, msi.RenderSize.Height));
                    mainImage.RenderTransform = transformGroup;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }*/
        }

        /// <summary>
        /// Called when start dragging Contrast slider
        /// Not used
        /// </summary>
        private void sliderContrast_DragStarted(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called when a value in the Saturation slider is changed
        /// Not used
        /// </summary>
        private void sliderSaturation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            /* if (!sliderSaturationDragStarted)
            {
                if (mainArtwork != null)
                {
                    try
                    {
                        mainArtwork.captureScreen(384, 0, 1536, 864);
                        mainArtwork.Tools.adjustSaturation((int)sliderSaturation.Value);
                        ImageSource imgs = (ImageSource)mainArtwork.returnImage();
                        mainImage.Source = imgs;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            } */
        }

        /// <summary>
        /// Called when complete dragging Saturation slider
        /// </summary>
        private void sliderSaturation_DragCompleted(object sender, EventArgs e)
        {
            /*if (mainArtwork != null)
            {
                try
                {
                    //mainArtwork.captureScreen(384, 0, 1536, 864);
                    mainArtwork.captureScreen(msi, 1);
                    //mainArtwork.Tools.adjustSaturation((int)sliderSaturation.Value);
                    //ImageSource imgs = (ImageSource)mainArtwork.returnImage();
                    //mainImage.Source = imgs;

                    Byte[] result = mainArtwork.Tools.applyFilter("Saturation", (int)sliderBrightness.Value, (int)sliderContrast.Value, (int)sliderSaturation.Value);
                    mainArtwork.Tools.Modified = true;
                    BitmapSource myNewImage = BitmapSource.Create(mainArtwork.Tools.ImageWidth, mainArtwork.Tools.ImageHeight, 96, 96, PixelFormats.Bgra32, null, result, mainArtwork.Tools.ImageWidth * 4);
                    mainImage.Height = msi.RenderSize.Height;
                    mainImage.Width = msi.RenderSize.Width;
                    mainImage.Source = myNewImage;

                    // jcchin - fixes mirroring issue as well, but still just an overlay
                    TransformGroup transformGroup = new TransformGroup();
                    transformGroup.Children.Add(new ScaleTransform(1 * (1), -1 * (1)));
                    // transformGroup.Children.Add(new TranslateTransform(1832, -70));
                    transformGroup.Children.Add(new TranslateTransform(0, msi.RenderSize.Height));
                    mainImage.RenderTransform = transformGroup;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }*/
        }

        /// <summary>
        /// Called when start dragging Saturation slider
        /// Not used
        /// </summary>
        private void sliderSaturation_DragStarted(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called when a user chooses another item in the hotspot listbox.
        /// Change the hotspot icons to highlight corresponding hotspots based on user selection.
        /// </summary>
        int m_currentSelectedHotspotIndex = -1;
        private void listHotspotNav_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SurfaceListBoxItem item;
            int index;
            if (listHotspotNav.SelectedIndex != -1)
            {
                Hotspot selectedHotspot = m_hotspotCollection.Hotspots[listHotspotNav.SelectedIndex];
                if (m_hotspotOnOff == true)
                {
                    // if (m_currentSelectedHotspotIndex != listHotspotNav.SelectedIndex )
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
                    //m_hotspotCollection.loadHotspotIcon(index, HotspotOverlay, MainScatterView, msi);
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
            //resetAll(); 
            Rect viewbox = msi.GetZoomableCanvas.ActualViewbox;

            /* SIZE OF OVERLAY */
            msi_thumb_rect.Width = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * viewbox.Width) / msi.GetImageActualWidth;
            msi_thumb_rect.Height = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * viewbox.Height) / msi.GetImageActualHeight;

            /* POSITION OF OVERLAY */
            double msi_thumb_rect_centerX = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * viewbox.GetCenter().X) / msi.GetImageActualWidth;
            double msi_thumb_rect_centerY = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * viewbox.GetCenter().Y) / msi.GetImageActualHeight;

            ZoomableCanvas msi_thumb_zc = msi_thumb.GetZoomableCanvas;

            double msi_thumb_centerX = msi_thumb_zc.Offset.X + (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * 0.5);
            double msi_thumb_centerY = msi_thumb_zc.Offset.Y + (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * 0.5);

            double msi_thumb_rect_centerX_dist = msi_thumb_rect_centerX - msi_thumb_centerX + msi_thumb_zc.Offset.X;
            double msi_thumb_rect_centerY_dist = msi_thumb_rect_centerY - msi_thumb_centerY + msi_thumb_zc.Offset.Y;

            msi_thumb_rect.RenderTransform = new TranslateTransform(msi_thumb_rect_centerX_dist, msi_thumb_rect_centerY_dist);

            /* HOTSPOTS */
            double HotspotOverlay_centerX = (msi.GetZoomableCanvas.Scale * msi.GetImageActualWidth * 0.5) - msi.GetZoomableCanvas.Offset.X;
            double HotspotOverlay_centerY = (msi.GetZoomableCanvas.Scale * msi.GetImageActualHeight * 0.5) - msi.GetZoomableCanvas.Offset.Y;

            double msi_clip_centerX = msi.GetZoomableCanvas.ActualWidth * 0.5;
            double msi_clip_centerY = msi.GetZoomableCanvas.ActualHeight * 0.5;

            double HotspotOverlay_centerX_dist = HotspotOverlay_centerX - msi_clip_centerX;
            double HotspotOverlay_centerY_dist = HotspotOverlay_centerY - msi_clip_centerY;

            m_hotspotCollection.updateHotspotLocations(HotspotOverlay, MSIScatterView, msi);
        }

        /// <summary>
        /// (initial setup) - viewbox of msi --> msi_thumb_rect: synchronizes highlighted region of artwork thumbnail navigator with MSI viewbox (the part of the artwork that the user is looking at)
        /// </summary>
        public void msi_ViewboxUpdate()
        {
            /* set initial size and position of zoom navigator */
            Rect viewbox = msi.GetZoomableCanvas.ActualViewbox;

            msi_thumb_rect.Width = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * viewbox.Width) / msi.GetImageActualWidth;
            msi_thumb_rect.Height = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * viewbox.Height) / msi.GetImageActualHeight;

            double msi_thumb_rect_centerX = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * viewbox.GetCenter().X) / msi.GetImageActualWidth;
            double msi_thumb_rect_centerY = (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * viewbox.GetCenter().Y) / msi.GetImageActualHeight;

            ZoomableCanvas msi_thumb_zc = msi_thumb.GetZoomableCanvas;

            double msi_thumb_centerX = msi_thumb_zc.Offset.X + (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualWidth * 0.5);
            double msi_thumb_centerY = msi_thumb_zc.Offset.Y + (msi_thumb.GetZoomableCanvas.Scale * msi_thumb.GetImageActualHeight * 0.5);

            double msi_thumb_rect_centerX_dist = msi_thumb_rect_centerX - msi_thumb_centerX + msi_thumb_zc.Offset.X;
            double msi_thumb_rect_centerY_dist = msi_thumb_rect_centerY - msi_thumb_centerY + msi_thumb_zc.Offset.Y;

            msi_thumb_rect.RenderTransform = new TranslateTransform(msi_thumb_rect_centerX_dist, msi_thumb_rect_centerY_dist);

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
            /*base.OnManipulationDelta(e);

            msi_thumb_rect.RenderTransform = new TranslateTransform(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);

            e.Handled = true;*/
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

            this.msi_ViewboxUpdate();
        }

        #endregion

        #region inter-mode navigation event handlers

        private void switchToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //this.Hide();
        }


        private void ResetArtworkButton_Click(object sender, RoutedEventArgs e)
        {
            msi.ResetArtwork();
            msi_thumb.ResetArtwork();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void activateKW_Click(object sender, RoutedEventArgs e)
        {
            if (knowledgeWebOn)
            {
                knowledgeWebOn = false;
                DeepZoomGrid.Visibility = Visibility.Visible;
                //MainScatterView.Visibility = Visibility.Visible;
                newWeb.Hide();
                toggleLeftSide();

                //activateKW.ClearValue(BackgroundProperty);
            }
            else
            {
                knowledgeWebOn = true;
                DeepZoomGrid.Visibility = Visibility.Collapsed;
                //MainScatterView.Visibility = Visibility.Collapsed;
                newWeb.Show();
                toggleLeftSide();

                //activateKW.Background = (Brush)new BrushConverter().ConvertFrom("#4e765c");
            }
        }

        public void goBack()
        {
            knowledgeWebOn = false;
            DeepZoomGrid.Visibility = Visibility.Visible;
            MainScatterView.Visibility = Visibility.Visible;
            newWeb.Hide();
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
                Console.WriteLine("whats gong on here? w.item.ActualCenter.X is: " + w.item.ActualCenter.X);
                w.item.SetCurrentValue(ScatterViewItem.CenterProperty, new Point(w.item.ActualCenter.X - delta, w.item.ActualCenter.Y));
            }

        }

        public void addKnowledgeGroup(String str)
        {
            newWeb.addGroup(str);
        }

        public DoubleAnimation getDoubleAnimation(double from, double to, double duration)
        {
            DoubleAnimation anim = new DoubleAnimation();
            //anim.Completed += anim2Completed;
            anim.From = from;
            anim.To = to; // barVersion.Width;
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
                //DoubleAnimation heightAnim = getDoubleAnimation(Tools.ActualHeight, Tools.ActualHeight * 1.2, LeftPanelAnimationDuration);
                DoubleAnimation widthAnim = getDoubleAnimation(Tools.ActualWidth, Tools.ActualWidth * expansionRatio, LeftPanelAnimationDuration);
                DoubleAnimation sliderAnim = getDoubleAnimation(sliderContrast.ActualWidth, sliderContrast.ActualWidth * expansionRatio, LeftPanelAnimationDuration);
                //Tools.BeginAnimation(HeightProperty, heightAnim);

                Tools.BeginAnimation(WidthProperty, widthAnim);
                sliderBrightness.BeginAnimation(WidthProperty, sliderAnim);
                sliderContrast.BeginAnimation(WidthProperty, sliderAnim);
                sliderSaturation.BeginAnimation(WidthProperty, sliderAnim);
                //Tools.Height = Tools.ActualHeight * 1.2;
                Tools.Width = Tools.ActualWidth * expansionRatio;
                sliderBrightness.Width = sliderContrast.ActualWidth * expansionRatio;
                sliderContrast.Width = sliderContrast.ActualWidth * expansionRatio;
                sliderSaturation.Width = sliderContrast.ActualWidth * expansionRatio;
            }
            else
            {
                toolsExpanded = false;
                Tools.ClipToBounds = true;
                //DoubleAnimation heightAnim = getDoubleAnimation(Tools.ActualHeight, toolsOriginalHeight, LeftPanelAnimationDuration);
                DoubleAnimation widthAnim = getDoubleAnimation(Tools.ActualWidth, toolsOriginalWidth, LeftPanelAnimationDuration);
                DoubleAnimation sliderAnim = getDoubleAnimation(sliderContrast.ActualWidth, toolsOriginalSliderWidth, LeftPanelAnimationDuration);
                //Tools.BeginAnimation(HeightProperty, heightAnim);
                Tools.BeginAnimation(WidthProperty, widthAnim);
                sliderBrightness.BeginAnimation(WidthProperty, sliderAnim);
                sliderContrast.BeginAnimation(WidthProperty, sliderAnim);
                sliderSaturation.BeginAnimation(WidthProperty, sliderAnim);

                //Tools.Height = toolsOriginalHeight;
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

        public void BottomButtonClick(object sender, RoutedEventArgs e)
        {
            if (bottomPanelVisible)
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = (this.Height) * .8;
                //////
                da.To = (this.Height) * .8 + Bar.ActualHeight;
                //da.From = BottomPanel.ActualHeight * .5 + BottomPanel.;
                //da.To
                da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                BottomPanel.BeginAnimation(Canvas.TopProperty, da);
                foreach (WorkspaceElement wke in DockedItems)
                {
                    if (wke.isDocked) wke.item.IsEnabled = false;
                }
            }
            else
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
            }
            bottomPanelVisible = !bottomPanelVisible;

        }

        private void Tours_MouseEnter(object sender, MouseEventArgs e)
        {
            //Tours.Width = 800;
            ////Tours.Background = Brushes.Red;
            ////Tours.Background.Opacity = .5;
            //Tours.ClipToBounds = false;

        }

        private void Tours_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Tours.Width = 800;
            ////Tours.Background.Opacity = .5;
            //Tours.ClipToBounds = false;

        }

        private void Tours_MouseLeave(object sender, MouseEventArgs e)
        {
            //Tours.Width = 344;
            //Tours.ClipToBounds = true;
        }
        /*
        public void ScatterViewCenterChanged(object sender, EventArgs e)
        {
            if (MainScatterViewItem.ActualCenter.Y > 830.0 && !scatteremoved)
            {
                ScatterViewItem move = new ScatterViewItem();
                ScatterViewItem i = MainScatterView.Items.GetItemAt(MainScatterView.Items.IndexOf(MainScatterViewItem)) as ScatterViewItem;
                move.Background = i.Background;
                //move.Content = i.Content;
                MainScatterView.Items.Remove(MainScatterViewItem);
                scatteremoved = true;
                Bar.Children.Add(move);
                //Bar.ItemsSource = items;
            }
        }
         */

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
            //mainArtwork.captureScreen(msi, 1);
        }

        private void labelTools_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void mainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            resetAll();

        }

        private void mainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //  mainArtwork.captureScreen(msi, 1);
        }

        private void mainImage_TouchUp(object sender, TouchEventArgs e)
        {
            // mainArtwork.captureScreen(msi, 1);
        }

        #endregion

        private void tourAuthoring_Click(object sender, RoutedEventArgs e)
        {
            String filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                "\\Data\\Tour\\XML\\" + currentArtworkFileName + ".xml";

            if (!System.IO.File.Exists(filePath))
            {
                //tourSystem.CreateNewBlankTour(filePath, currentArtworkFileName, currentArtworkTitle, "No title");
                String fileContentString =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +

                "<TourStoryboard duration=\"40\" displayName=\"" + currentArtworkTitle + "\" description=\"" + "No Title" + "\">\r\n" +
                "<TourParallelTL type=\"artwork\" displayName=\"Main Artwork\" file=\"" + currentArtworkFileName + "\">\r\n" +
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
            InitTourLayout();


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
            //metaData.Content = "Assets list";
        }
        public void showMetaList()
        {
            if (Main.Children.Contains(newMeta))
            {
                Main.Children.Remove(newMeta);
                return;
            }
            Main.Children.Add(newMeta);
            //MainScatterView.Items.Add(newMeta);
            Canvas.SetZIndex(newMeta, 75);
            Canvas.SetLeft(newMeta, 410);
            Canvas.SetTop(newMeta, 100);

            //metaData.Content = "Fold list";
        }


        public void TourAuthoringSaveButton_Click(object sender, RoutedEventArgs e)
        {
            String filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                "\\Data\\Tour\\XML\\" + currentArtworkFileName + ".xml";
            tourSystem.SaveDictToXML(filePath);
            tourSystem.SaveInkCanvases();
            DoubleAnimation saveAnim = new DoubleAnimation();
            saveAnim.From = 1.0;
            saveAnim.To = 0.0;// barVersion.Width;
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
                if (!tourSystem.resetTourLength(newDuration))
                    MessageBox.Show("A tour cannot be shorter than the end of it's last event");
            }
            catch (Exception exc)
            {
            }


            TourLengthBox.Visibility = Visibility.Collapsed;



        }

        private void cancelTimeTourButton_Click(object sender, RoutedEventArgs e)
        {
            TourLengthBox.Visibility = Visibility.Collapsed;
            shortTimeLabel.Visibility = Visibility.Hidden;
        }


        public void updateWebImages()
        {
            WebClient webClient = new WebClient();

            XmlDocument doc = new XmlDocument();
            doc.Load("Data/NewCollection.xml");
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
                                if (currentArtworkFileName != node.Attributes.GetNamedItem("path").InnerText)
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
                                                            Console.WriteLine("This URL does not exist or you are not connected to the internet");
                                                            urlExists = false;
                                                        }
                                                        if (urlExists)
                                                        {
                                                            string filepath = "Data/Images/Metadata/" + metadatafilename;
                                                            DateTime lastModifiedTime = File.GetLastWriteTimeUtc(@filepath);

                                                            Console.WriteLine("File last modified at: " + lastModifiedTime);
                                                            DateTime currUtcTime = DateTime.UtcNow;
                                                            Console.WriteLine("Current Time: " + currUtcTime);
                                                            TimeSpan span = currUtcTime.Subtract(lastModifiedTime);
                                                            Console.WriteLine("It has been " + span.Hours + " hours, " + span.Minutes + " minutes, and " + span.Seconds + " seconds since the image was last modified");
                                                            //if (span.Minutes > 1)
                                                            if (span.Seconds > 20)
                                                            {
                                                                Console.WriteLine("URL = " + url + ", Filepath = " + filepath);
                                                                //webClient.DownloadFile(url, @filepath);
                                                                downloadAndSaveImage(url, @filepath);
                                                                Console.WriteLine("Updating");
                                                                //webClient.Dispose();
                                                                reloadMetadata(currentArtworkFileName);
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
    }




    public class WorkspaceElement : SurfaceListBoxItem
    {
        public bool isDocked = false;
        public Point Center;
        public DockableItem item;

    }


}