using System;
using System.Collections.Generic;
using DeepZoom.Controls;
using LADSArtworkMode.TourEvents;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using LADSArtworkMode.Tour;
using Microsoft.Win32;

namespace LADSArtworkMode
{
    /// <summary>
    /// 
    /// </summary>
    class TourSystem
    {
        private ArtworkModeWindow artModeWin;

        public bool tourPlaybackOn;
        public bool tourAuthoringOn;
        private bool drawingON;
        public BiDictionary<Timeline, BiDictionary<double, TourEvent>> tourBiDictionary;

        public Stack<BiDictionary<Timeline, BiDictionary<double, TourEvent>>> undoStack;
        public Stack<BiDictionary<Timeline, BiDictionary<double, TourEvent>>> redoStack;

        private Dictionary<DockableItem, Timeline> itemToTLDict;
        private Dictionary<MultiScaleImage, Timeline> msiToTLDict;


        public List<SurfaceInkCanvas> inkCanvases = new List<SurfaceInkCanvas>();

        private List<DockableItem> dockableItemsLoaded; // associated media that's loaded for a tour
        private List<MultiScaleImage> msiItemsLoaded; // msi's that are loaded for a tour (just one -- the artwork -- for now)

        public TourStoryboard tourStoryboard;
        private TimeSpan tourCurrentTime, tourDuration; // for [elapsed time]/[duration] display during tour authoring & playback
        private String tourCurrentTimeString, tourDurationString; // for [elapsed time]/[duration] display during tour authoring & playback
        private MediaElement tourAudio_element; // for playback of tour audio narration file

        // for dragging of seek bar during tour playback
        private int tourTimerCount;
        public TimeSpan authorTimerCountSpan { get; set; }
        private TimeSpan tourTimerCountSpan;
        private String tourTimerCountSpanString;
        private Point startDragPoint;
        private Point startMSIdrag;
        
        private SurfaceInkCanvas currentPathCanvas;
        private string currentPathCanvasFile;
        private SurfaceInkCanvas currentHighlightCanvas;
        private string currentHighlightCanvasFile;

        private TourAuthoringUI tourAuthoringUI;
        private bool mouseIsDown;

        public bool IsExploreMode { get { return _isExploreMode; } }
        private bool _isExploreMode = false;

        // Three Dictionaries used for managing assets in explore mode.
        // exploreAssetsOnCanvas: All assets that should remain on the artwork mode canvas when exploring is finished.
        // exploreAssetsInDock: All assets that should remain in the dock when exploring is finished.
        // exploreDisposableAssets: Assets that were created only for explore mode and should be removed.
        public Dictionary<String, DockableItem> exploreAssetsOnCanvas;
        public Dictionary<String, DockableItem> exploreAssetsInDock;
        public Dictionary<String, DockableItem> exploreDisposableAssets;

        public Rectangle yellowScrub;
        public TourSystem(ArtworkModeWindow artworkModeWindowParam)
        {
            artModeWin = artworkModeWindowParam;
            ///// Delete?
            tourAuthoringUI = new TourAuthoringUI(artModeWin, this);
            // event handlers for dragging of seek bar during tour playback
            artModeWin.tourSeekBarMarker.PreviewTouchDown += new EventHandler<TouchEventArgs>(TourSeekBarMarker_PreviewTouchDown);
            artModeWin.tourSeekBarMarker.PreviewMouseDown += new MouseButtonEventHandler(TourSeekBarMarker_PreviewMouseDown);
            artModeWin.tourSeekBarMarker.PreviewTouchMove += new EventHandler<TouchEventArgs>(TourSeekBarMarker_PreviewTouchMove);
            artModeWin.tourSeekBarMarker.PreviewMouseMove += new MouseEventHandler(TourSeekBarMarker_PreviewMouseMove);
            artModeWin.tourSeekBarMarker.PreviewTouchUp += new EventHandler<TouchEventArgs>(TourSeekBarMarker_PreviewTouchUp);
            artModeWin.tourSeekBarMarker.PreviewMouseUp += new MouseButtonEventHandler(TourSeekBarMarker_PreviewMouseUp);
            startDragPoint = new Point();
            loadTourButtons();
            tourPlaybackOn = false;
            tourAuthoringOn = false;
            mouseIsDown = false;
        }

        #region msi_tour navigator event handler
        /// <summary>
        /// viewbox of msi_tour --> msi_tour_thumb_rect: synchronizes highlighted region of artwork thumbnail navigator with MSI viewbox (the part of the artwork that the user is looking at)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void msi_tour_ViewboxChanged(Object sender, EventArgs e)
        {
            artModeWin.resetAll();

            Rect viewbox = artModeWin.msi_tour.GetZoomableCanvas.ActualViewbox;

            /* SIZE OF OVERLAY */
            artModeWin.msi_thumb_rect.Width = (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualWidth * viewbox.Width) / artModeWin.msi_tour.GetImageActualWidth;
            artModeWin.msi_thumb_rect.Height = (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualHeight * viewbox.Height) / artModeWin.msi_tour.GetImageActualHeight;

            /* POSITION OF OVERLAY */
            double msi_thumb_rect_centerX = (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualWidth * viewbox.GetCenter().X) / artModeWin.msi_tour.GetImageActualWidth;
            double msi_thumb_rect_centerY = (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualHeight * viewbox.GetCenter().Y) / artModeWin.msi_tour.GetImageActualHeight;

            ZoomableCanvas msi_thumb_zc = artModeWin.msi_tour_thumb.GetZoomableCanvas;

            double msi_thumb_centerX = msi_thumb_zc.Offset.X + (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualWidth * 0.5);
            double msi_thumb_centerY = msi_thumb_zc.Offset.Y + (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualHeight * 0.5);

            double msi_thumb_rect_centerX_dist = msi_thumb_rect_centerX - msi_thumb_centerX + msi_thumb_zc.Offset.X;
            double msi_thumb_rect_centerY_dist = msi_thumb_rect_centerY - msi_thumb_centerY + msi_thumb_zc.Offset.Y;

            artModeWin.msi_thumb_rect.RenderTransform = new TranslateTransform(msi_thumb_rect_centerX_dist, msi_thumb_rect_centerY_dist);
        }

        /// <summary>
        /// (initial setup) - viewbox of msi_tour --> msi_tour_thumb_rect: synchronizes highlighted region of artwork thumbnail navigator with MSI viewbox (the part of the artwork that the user is looking at)
        /// </summary>
        private void msi_tour_ViewboxUpdate()
        {
            artModeWin.resetAll();
            Rect viewbox = artModeWin.msi_tour.GetZoomableCanvas.ActualViewbox;


            /* SIZE OF OVERLAY */
            artModeWin.msi_thumb_rect.Width = (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualWidth * viewbox.Width) / artModeWin.msi_tour.GetImageActualWidth;
            artModeWin.msi_thumb_rect.Height = (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualHeight * viewbox.Height) / artModeWin.msi_tour.GetImageActualHeight;

            /* POSITION OF OVERLAY */
            double msi_thumb_rect_centerX = (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualWidth * viewbox.GetCenter().X) / artModeWin.msi_tour.GetImageActualWidth;
            double msi_thumb_rect_centerY = (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualHeight * viewbox.GetCenter().Y) / artModeWin.msi_tour.GetImageActualHeight;

            ZoomableCanvas msi_thumb_zc = artModeWin.msi_tour_thumb.GetZoomableCanvas;

            double msi_thumb_centerX = msi_thumb_zc.Offset.X + (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualWidth * 0.5);
            double msi_thumb_centerY = msi_thumb_zc.Offset.Y + (artModeWin.msi_tour_thumb.GetZoomableCanvas.Scale * artModeWin.msi_tour_thumb.GetImageActualHeight * 0.5);

            double msi_thumb_rect_centerX_dist = msi_thumb_rect_centerX - msi_thumb_centerX + msi_thumb_zc.Offset.X;
            double msi_thumb_rect_centerY_dist = msi_thumb_rect_centerY - msi_thumb_centerY + msi_thumb_zc.Offset.Y;

            artModeWin.msi_thumb_rect.RenderTransform = new TranslateTransform(msi_thumb_rect_centerX_dist, msi_thumb_rect_centerY_dist);
        }

        #endregion

        #region drawing utils

        public void drawPaths_Click(object sender, RoutedEventArgs e)
        {
            if (tourAuthoringOn)
            {
                undoableActionPerformed();
                MakeNewPathCanvas();
            }
        }

        public void drawHighlight_Click(object sender, RoutedEventArgs e)
        {
            if (tourAuthoringOn)
            {
                undoableActionPerformed();
                MakeNewHighlightCanvas();
            }
        }

        public void switchDrawMode(SurfaceInkEditingMode mode)
        {
            if (currentPathCanvas != null)
                currentPathCanvas.EditingMode = mode;
            if (currentHighlightCanvas != null)
                currentHighlightCanvas.EditingMode = mode;
        }

        public void MakeNewHighlightCanvas()
        {
            currentHighlightCanvas = new SurfaceInkCanvas();
            inkCanvases.Add(currentHighlightCanvas);
            currentHighlightCanvas.Width = 1920;
            currentHighlightCanvas.Height = 1080;
            currentHighlightCanvas.DefaultDrawingAttributes.Width = 50;
            currentHighlightCanvas.DefaultDrawingAttributes.Height = 50;
            currentHighlightCanvas.UsesTouchShape = false;
            currentHighlightCanvas.Background = Brushes.Transparent;
            currentHighlightCanvas.Opacity = 0.7;
            Canvas.SetZIndex(currentHighlightCanvas, 50);
            artModeWin.ImageArea.Children.Add(currentHighlightCanvas);
            TourEvent fadeIn = new FadeInHighlightEvent(currentHighlightCanvas, 1, 0.7);
            fadeIn.type = TourEvent.Type.fadeInHighlight;
            TourEvent fadeOut = new FadeOutHighlightEvent(currentHighlightCanvas, 1, 0.7);
            fadeOut.type = TourEvent.Type.fadeOutHighlight;

            TourParallelTL tourtl = new TourParallelTL();
            tourtl.type = TourTLType.highlight;
            tourtl.inkCanvas = currentHighlightCanvas;
            tourtl.displayName = "Mask";
            tourtl.file = getNextFile("Highlight");
            currentHighlightCanvasFile = tourtl.file;


            BiDictionary<double, TourEvent> tlbidict = new BiDictionary<double, TourEvent>();
            double start = authorTimerCountSpan.TotalSeconds;
            if (start < 1) start = 1;
            authorTimerCountSpan = TimeSpan.FromSeconds(start);
            tlbidict.Add(start - 1, fadeIn);
            tlbidict.Add(start + 1, fadeOut);
            tourBiDictionary.Add(tourtl, tlbidict);
            if (!tourAuthoringOn)
                currentHighlightCanvas.IsHitTestVisible = false;
            tourAuthoringUI.refreshUI();
        }


        public void MakeNewPathCanvas()
        {
            currentPathCanvas = new SurfaceInkCanvas();
            currentPathCanvas.DefaultDrawingAttributes.FitToCurve = true;
            inkCanvases.Add(currentPathCanvas);
            currentPathCanvas.Width = 1920;
            currentPathCanvas.Height = 1080;
            currentPathCanvas.UsesTouchShape = true;
            currentPathCanvas.Background = Brushes.Transparent;
            Canvas.SetZIndex(currentPathCanvas, 50);
            artModeWin.ImageArea.Children.Add(currentPathCanvas);
            TourEvent fadeIn = new FadeInPathEvent(currentPathCanvas, 1);
            fadeIn.type = TourEvent.Type.fadeInPath;
            TourEvent fadeOut = new FadeOutPathEvent(currentPathCanvas, 1);
            fadeOut.type = TourEvent.Type.fadeOutPath;

            TourParallelTL tourtl = new TourParallelTL();
            tourtl.type = TourTLType.path;
            tourtl.inkCanvas = currentPathCanvas;
            tourtl.displayName = "Drawing";
            tourtl.file = getNextFile("Path");
            currentPathCanvasFile = tourtl.file;
            BiDictionary<double, TourEvent> tlbidict = new BiDictionary<double, TourEvent>();
            double start = authorTimerCountSpan.TotalSeconds;
            if (start < 1) start = 1;
            authorTimerCountSpan = TimeSpan.FromSeconds(start);
            tlbidict.Add(start - 1, fadeIn);
            tlbidict.Add(start + 1, fadeOut);
            tourBiDictionary.Add(tourtl, tlbidict);
            if (!tourAuthoringOn)
                currentPathCanvas.IsHitTestVisible = false;
            tourAuthoringUI.refreshUI();
        }

        public void changeCanvas(bool isPath, SurfaceInkCanvas canvas, string file)
        {
            if (isPath)
            {
                canvas.DefaultDrawingAttributes.Width = 3;
                canvas.DefaultDrawingAttributes.Height = 3;
                canvas.DefaultDrawingAttributes.Color = System.Windows.Media.Color.FromRgb(0,0,0);
                canvas.UsesTouchShape = true;
                currentPathCanvas = canvas;
                currentPathCanvasFile = file;
                return;
            }
            canvas.DefaultDrawingAttributes.Width = 50;
            canvas.DefaultDrawingAttributes.Height = 50;
            canvas.DefaultDrawingAttributes.Color = System.Windows.Media.Color.FromRgb(0, 0, 0);
            currentHighlightCanvas = canvas;
            currentHighlightCanvasFile = file;
        }

        public string getNextFile(string str)
        {
            String path = "Data/Tour/Images";
            path = System.IO.Path.Combine(path, artModeWin.currentArtworkFileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, str);
            int i = 0;
            while (File.Exists(path + i + ".ink"))
            {
                i++;
            }
            return path + i + ".ink";
        }

        public void saveInkCanvas(SurfaceInkCanvas canvas, string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            FileStream fs = new FileStream(path, FileMode.CreateNew);
            canvas.Strokes.Save(fs);
            fs.Close();
        }

        public void SaveInkCanvases()
        {
            foreach (Timeline timeline in tourBiDictionary.firstKeys)
            {
                TourTL tourtl = (TourTL)timeline;
                if (tourtl.type == TourTLType.highlight || tourtl.type == TourTLType.path)
                {
                    SurfaceInkCanvas can = ((TourParallelTL)tourtl).inkCanvas;
                    saveInkCanvas(can, tourtl.file);
                }
            }
        }

        public void loadInkCanvas(SurfaceInkCanvas canvas, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            System.Windows.Ink.StrokeCollection collection = new System.Windows.Ink.StrokeCollection(fs);
            canvas.Strokes.Add(collection);
        }

        public void StopDrawingPath()
        {
            if (currentPathCanvas == null)
                return;
            currentPathCanvas.EditingMode = SurfaceInkEditingMode.None;
            currentPathCanvas.IsHitTestVisible = false;
            saveInkCanvas(currentPathCanvas, currentPathCanvasFile);
            currentPathCanvas = null;
            currentPathCanvasFile = null;
        }

        public void StopDrawingHighlight()
        {
            if (currentHighlightCanvas == null)
                return;
            currentHighlightCanvas.EditingMode = SurfaceInkEditingMode.None;
            currentHighlightCanvas.IsHitTestVisible = false;
            saveInkCanvas(currentHighlightCanvas, currentHighlightCanvasFile);
            currentHighlightCanvas = null;
            currentHighlightCanvasFile = null;
        }

        public void ChangeOpacity(double opacity)
        {
            if (currentHighlightCanvas != null)
            {
                foreach (Timeline tl in tourBiDictionary.firstKeys)
                {
                    TourTL tourtl = (TourTL)tl;
                    if (tourtl.type == TourTLType.highlight && ((TourParallelTL)tourtl).inkCanvas == currentHighlightCanvas)
                    {
                        BiDictionary<double, TourEvent> dict = tourBiDictionary[tl][0];
                        foreach (TourEvent te in dict.firstValues)
                        {
                            switch (te.type)
                            {
                                case TourEvent.Type.fadeInHighlight:
                                    FadeInHighlightEvent fih = (FadeInHighlightEvent)te;
                                    fih.opacity = opacity;
                                    break;
                                case TourEvent.Type.fadeOutHighlight:
                                    FadeOutHighlightEvent foh = (FadeOutHighlightEvent)te;
                                    foh.opacity = opacity;
                                    break;
                                case TourEvent.Type.zoomHighlight:
                                    ZoomHighlightEvent zhe = (ZoomHighlightEvent)te;
                                    zhe.opacity = opacity;
                                    break;
                            }
                        }
                    }
                }
            }
            tourAuthoringUI.refreshUI();
        }
        #endregion

        #region tour authoring & playback control buttons

        public Label getSaveSuccessfulLabel()
        {
            return tourAuthoringUI.successfulSaveLabel;
        }

        public void grabSound(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Sound Files|*.mp3";
            if (ofd.ShowDialog() != true)
            {
                return;

            }
            undoableActionPerformed();
            String audio_file1 = ofd.FileName;
            String audio_file = System.IO.Path.GetFileName(audio_file1);
            try
            {
                System.IO.File.Delete(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file);
            }
            catch (Exception exc)
            {

            }
            try
            {
                System.IO.File.Copy(audio_file1, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file);
            }
            catch (Exception exc)
            {
            }
            TourMediaTL tourAudio_TL = new TourMediaTL(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file, UriKind.Absolute));
            tourAudio_TL.type = TourTLType.audio;
            tourAudio_TL.displayName = audio_file;
            tourAudio_TL.file = audio_file;

            BiDictionary<double, TourEvent> tourAudio_TL_dict = new BiDictionary<double, TourEvent>(); // dummy TL_dict -- tourAudio_timeline obviously doesn't store any TourEvents
            tourBiDictionary.Add(tourAudio_TL, tourAudio_TL_dict);
            tourAudio_element = new MediaElement();
            tourAudio_element.Volume = 0.99;

            tourAudio_element.LoadedBehavior = MediaState.Manual;
            tourAudio_element.UnloadedBehavior = MediaState.Manual;

            Storyboard.SetTarget(tourAudio_TL, tourAudio_element);
            tourStoryboard.SlipBehavior = SlipBehavior.Slip;

            // took me quite a while to figure out that WPF really can't determine the duration of an MP3 until it's actually loaded (i.e. playing), and then it took me a little longer to finally find and accept this open-source library...argh
            TagLib.File audio_file_tags = TagLib.File.Create(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file);
            tourAudio_TL.Duration = audio_file_tags.Properties.Duration;
            if (tourStoryboard.totalDuration < (tourAudio_TL.Duration.TimeSpan.TotalSeconds + 1))
                resetTourLength(tourAudio_TL.Duration.TimeSpan.TotalSeconds + 1);
            tourAuthoringUI.refreshUI();
            tourAuthoringUI.reloadUI();

        }

        public void AddNewMetaDataTimeline(String imageFilePath, String fileName)
        {
            DockableItem dockItem = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, imageFilePath);

            dockItem.PreviewMouseWheel += new MouseWheelEventHandler(mediaMouseWheel);
            dockItem.removeDockability();
            dockItem.PreviewMouseMove += new MouseEventHandler(mediaTouchDown);
            dockItem.PreviewTouchDown += new EventHandler<TouchEventArgs>(mediaTouchDown);
            dockItem.PreviewMouseDown += new MouseButtonEventHandler(mediaTouchDown);
            dockItem.PreviewTouchMove += new EventHandler<TouchEventArgs>(mediaTouchMoved);
            dockItem.PreviewMouseMove += new MouseEventHandler(mediaTouchMoved);
            dockItem.PreviewTouchUp += new EventHandler<TouchEventArgs>(mediaTouchUp);
            dockItem.PreviewMouseUp += new MouseButtonEventHandler(mediaTouchUp);

            dockableItemsLoaded.Add(dockItem);
            dockItem.Visibility = Visibility.Hidden;

            TourParallelTL dockItem_TL = new TourParallelTL();
            dockItem_TL.type = TourTLType.media;

            dockItem_TL.displayName = fileName;
            dockItem_TL.file = System.IO.Path.GetFileName(imageFilePath);

            itemToTLDict.Add(dockItem, dockItem_TL);

            BiDictionary<double, TourEvent> dockItem_TL_dict = new BiDictionary<double, TourEvent>();

            tourBiDictionary.Add(dockItem_TL, dockItem_TL_dict);

            // Add fade in
            double toScreenPointX = 500;
            double toScreenPointY = 500;
            double scale = 1.0;
            double FadeInduration = 1.0;

            TourEvent fadeInMediaEvent = new FadeInMediaEvent(dockItem, toScreenPointX, toScreenPointY, scale, FadeInduration);

            double FadeInbeginTime = authorTimerCountSpan.TotalSeconds - 1;
            if (FadeInbeginTime < 0) FadeInbeginTime = 0;
            if (authorTimerCountSpan.TotalSeconds == 0) FadeInbeginTime += 1;
            authorTimerCountSpan = TimeSpan.FromSeconds(FadeInbeginTime+1);
            dockItem_TL_dict.Add(FadeInbeginTime, fadeInMediaEvent);

            // Add fade out
            double FadeOutduration = 1.0;

            TourEvent fadeOutMediaEvent = new FadeOutMediaEvent(dockItem, FadeOutduration);

            double FadeOutbeginTime = FadeInbeginTime + 2;
            dockItem_TL_dict.Add(FadeOutbeginTime, fadeOutMediaEvent);

            TourAuthoringUI.timelineInfo tli = tourAuthoringUI.addTimeline(dockItem_TL, dockItem_TL_dict, fileName, tourAuthoringUI.getNextPos());
            tourAuthoringUI.addTourEvent(tli, fadeInMediaEvent, tli.lengthSV, FadeInbeginTime, FadeInduration);
            tourAuthoringUI.addTourEvent(tli, fadeOutMediaEvent, tli.lengthSV, FadeOutbeginTime, FadeOutduration);
            tourAuthoringUI.timelineCount += 1;
            tourAuthoringUI.updateTLSize();

            StopAndReloadTourAuthoringUIFromDict(authorTimerCountSpan.TotalSeconds);

            tourAuthoringUI.refreshUI();


        }

        bool paused = true;

        public void TourControlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!tourStoryboard.GetIsPaused(artModeWin))
            {
                tourStoryboard.Pause(artModeWin);
                paused = true;
            }
            else
            {
                try
                {
                    tourAuthoringUI.refreshUI();
                }
                catch (Exception exc)
                { }
                tourStoryboard.Resume(artModeWin);
                paused = false;
            }
        }

           // Enters 'Explore' mode which is essentially artwork mode with a 'resume tour' function
        // and assets from the tour pre-set on the view.
        public void TourExploreButton_Click(object sender, RoutedEventArgs e)
        {

            
            // First, pause the tour.
            if (!tourStoryboard.GetIsPaused(artModeWin))
            {
                tourStoryboard.Pause(artModeWin);
                //tourStoryboard.Stop(artModeWin);
                paused = true;
            }

            // Create the Dictionaries that we use for managing assets in explore mode.
            this.initializeExploreAssets();


            if (tourPlaybackOn)
            {
                // Get the render transform of the main tour image.
                Transform t = null;
                foreach (MultiScaleImage msiItem in msiItemsLoaded)
                {
                    msiItem.Visibility = Visibility.Hidden; // hide artwork MSI used in tour (in retrospect, not sure if this is necessary anymore since I added a new MSI instance called msi_tour)                               
                    t = msiItem.GetZoomableCanvas.RenderTransform;
                }
                // Kind of hacky: extract an offset from the image's render transform, and apply that offset to the main image.
                // Also extract scale from the render transform as well (from matrix).
                artModeWin.ArtModeLayout();
                artModeWin.msi.GetZoomableCanvas.Scale = t.Value.M11;
                Point p = new Point(-t.Value.OffsetX, -t.Value.OffsetY);
                artModeWin.msi.GetZoomableCanvas.Offset = p;

                // swap artwork navigator event handlers
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.RemoveValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.AddValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                artModeWin.msi_tour_thumb.Visibility = Visibility.Hidden;

                // Populate the dictionary of DockableItem positions so that they can be restored.
                /*TourDockablePositions = new Dictionary<DockableItem, TourDockablePosition>();
                foreach (DockableItem item in dockableItemsLoaded)
                {
                    TourDockablePosition pos = new TourDockablePosition();
                    pos.height = item.Height;
                    pos.width = item.Width;
                    pos.center_point = item.Center;
                    pos.orientation = item.Orientation;
                    pos.opacity = item.Opacity;
                    TourDockablePositions.Add(item, pos);
                    // Also, change the item to be opaque:
                    // TODO: This doesn't work, possibly because the Opacity animation hasn't finished,
                    // so changing the FillBehavior on it isn't sufficient.  Fix!
                    // item.Opacity = 1.0;
                }*/

                artModeWin.MSIScatterView.Visibility = Visibility.Hidden;

                // swap tour control buttons with artwork inter-mode navigation ones
                artModeWin.toggleLeftSide();
                //artModeWin.languageButton.Visibility = Visibility.Visible;
                artModeWin.exitButton.Visibility = Visibility.Visible;
                artModeWin.tourAuthoringButton.Visibility = Visibility.Collapsed;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Collapsed;
                artModeWin.resetArtworkButton.Visibility = Visibility.Collapsed;
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;              
                artModeWin.tourExploreButton.Visibility = Visibility.Collapsed;
                artModeWin.tourControlButton.Visibility = Visibility.Collapsed;
                artModeWin.tourStopButton.Visibility = Visibility.Collapsed;
                artModeWin.hideMetaList();
                artModeWin.m_hotspotCollection.reAddHotspotIcons();
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;

                artModeWin.ImageArea.IsHitTestVisible = true;
                artModeWin.MainScatterView.IsHitTestVisible = true;
                tourPlaybackOn = false;
                artModeWin.msi_tour.EnableEventHandlers();
                // Don't kill the currently playing tour.
                //currentlyPlayingTour = null;

                // Turn off hit tests for the ink canvas so that you can't continue to draw on tour annotations when exploring.
                foreach (SurfaceInkCanvas ic in inkCanvases)
                {
                    ic.IsHitTestVisible = false;
                    ic.Visibility = Visibility.Hidden;
                    
                    Console.Out.WriteLine("canvas" + ic.Visibility);
                    
                }
                
                //Hide the tour progress bar
                artModeWin.tourSeekBarMarker.Visibility = Visibility.Hidden;
                artModeWin.tourSeekBar.Visibility = Visibility.Hidden;
                // Switch the tours panel with the tour resume panel.
                artModeWin.labelTours.Visibility = Visibility.Collapsed;
                artModeWin.labelResumeTour.Visibility = Visibility.Visible;
                artModeWin.TourScroll.Visibility = Visibility.Collapsed;
                artModeWin.sBResumeTour1.Visibility = Visibility.Visible;
                artModeWin.MSIScatterView.Visibility = Visibility.Visible;
                Canvas.SetTop(artModeWin.labelResumeTour, Canvas.GetTop(artModeWin.Tours));
                Canvas.SetTop(artModeWin.sBResumeTour1, Canvas.GetTop(artModeWin.TourScroll));
                _isExploreMode = true;
          
            }
        }

        // Create the Dictionaries that we use for managing assets in explore mode.
        private void initializeExploreAssets()
        {
            this.exploreAssetsOnCanvas = new Dictionary<String, DockableItem>();
            this.exploreAssetsInDock = new Dictionary<String, DockableItem>();
            this.exploreDisposableAssets = new Dictionary<String, DockableItem>();
            // Fill exploreAssetsInDock with the assets in the dock.
            foreach (WorkspaceElement item in artModeWin.Bar.Items)
            {
                exploreAssetsInDock.Add(item.item.scatteruri, item.item);
            }
            // Fill exploreAssetsOnCanvas with the assets on the canvas.
            foreach (DockableItem item in artModeWin.MainScatterView.Items)
            {
                if (!exploreAssetsInDock.ContainsKey(item.scatteruri))
                    exploreAssetsOnCanvas.Add(item.scatteruri, item);
            }
            // For each asset to be explored:
            foreach (DockableItem item in dockableItemsLoaded)
            {
                // If it isn't actually visible, skip.
                if (item.Visibility != Visibility.Visible || item.Opacity == 0) continue;
                // if in exploreAssetsOnCanvas, position to tour asset's position.
                if (exploreAssetsOnCanvas.ContainsKey(item.scatteruri))
                {
                    alignAssets(exploreAssetsOnCanvas[item.scatteruri], item);
                }
                else if (exploreAssetsInDock.ContainsKey(item.scatteruri))
                {
                    // if in exploreAssetsInDock, undock and move to position.
                    DockableItem asset = exploreAssetsInDock[item.scatteruri];
                    alignAssets(asset.wke.quickReleaseItem(), item);
                    // Also, remove from exploreAssetsInDock and put in exploreAssetsOnCanvas
                    exploreAssetsOnCanvas.Add(asset.scatteruri, asset);
                    exploreAssetsInDock.Remove(asset.scatteruri);
                }
                else
                {// Otherwise, make a new one, add to disposable, and position it.
                    Helpers _helpers = new Helpers();  // Are you freaking kidding me, why isn't everything in this class just static?
                    // Make the DockableItem the normal way:
                    if (_helpers.IsImageFile(item.scatteruri))
                    {
                        DockableItem asset = new DockableItem(artModeWin.getMainScatterView(), artModeWin, artModeWin.getBar(), item.scatteruri);
                        artModeWin._openedAssets.Add(item.scatteruri, asset);
                        alignAssets(asset, item);
                        exploreDisposableAssets.Add(asset.scatteruri, asset);
                    }
                    else if (_helpers.IsVideoFile(item.scatteruri))
                    {
                        // TODO: Video assets?
                        /*artModeWin._openedAssets.Add(item.scatteruri, new DockableItem(artModeWin.getMainScatterView(), artModeWin, artModeWin.getBar(), item.scatteruri, this, new LADSVideoBubble(item.scatteruri, 500, 500), new VideoItem()));*/
                        //video-specific constructor
                    }
                }

            }
        }

        // Remove the assets that were only needed for exploring.
        private void teardownExploreAssets()
        {
            foreach (DockableItem item in exploreDisposableAssets.Values)
            {
                artModeWin._openedAssets.Remove(item.scatteruri);
                artModeWin.getMainScatterView().Items.Remove(item);
            }
            exploreDisposableAssets.Clear();
            exploreAssetsInDock = null;
            exploreAssetsOnCanvas = null;
            exploreDisposableAssets = null;
        }

        // Resume the currently playing tour.
        // Called when the 'Resume' button  is clicked from inside explore mode.
        public void resumeExploringTour()
        {
            foreach (MultiScaleImage msiItem in msiItemsLoaded)
            {
                msiItem.Visibility = Visibility.Visible;
            }
            // Swap event handlers (reverse of what is done when the tour was paused).
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
            dpd.RemoveValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
            dpd.AddValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
            artModeWin.msi_tour_thumb.Visibility = Visibility.Visible;

            // Switch in the normal tours panel again.
            //artModeWin.ResumeTour.Visibility = Visibility.Collapsed;
            //artModeWin.Tours.Visibility = Visibility.Visible;
            artModeWin.labelTours.Visibility = Visibility.Visible;
            artModeWin.labelResumeTour.Visibility = Visibility.Collapsed;
            artModeWin.TourScroll.Visibility = Visibility.Visible;
            artModeWin.sBResumeTour1.Visibility = Visibility.Collapsed;

            artModeWin.TourLayout();
            artModeWin.toggleLeftSide();

            // Resume playback and normal tour controls.
            tourStoryboard.Resume(artModeWin);
            tourPlaybackOn = true;
            paused = false;
            artModeWin.tourExploreButton.Visibility = Visibility.Visible;
            artModeWin.tourControlButton.Visibility = Visibility.Visible;
            artModeWin.tourStopButton.Visibility = Visibility.Visible;

            //artModeWin.languageButton.Visibility = Visibility.Visible;
            artModeWin.exitButton.Visibility = Visibility.Visible;
            artModeWin.HotspotOverlay.Visibility = Visibility.Hidden;
            //???artModeWin.showMetaList();
            artModeWin.m_hotspotCollection.removeHotspotIcons();
            artModeWin.HotspotOverlay.Visibility = Visibility.Hidden;

            // Disable hit tests so that you can no longer interact with tour media.
            artModeWin.ImageArea.IsHitTestVisible = false;
            artModeWin.MainScatterView.IsHitTestVisible = false;
            //???artModeWin.msi_tour.EnableEventHandlers();

            // Reenable hit tests on the ink canvas so that events can propogate properly.
            foreach (SurfaceInkCanvas ic in inkCanvases)
            {
                ic.IsHitTestVisible = true;
                ic.Visibility = Visibility.Visible;
            }
            artModeWin.tourSeekBarMarker.Visibility = Visibility.Visible;
            artModeWin.tourSeekBar.Visibility = Visibility.Visible;
            teardownExploreAssets();
            artModeWin.MSIScatterView.Visibility = Visibility.Visible;
         
            _isExploreMode = false;
        }

        public void TourStopButton_Click(object sender, RoutedEventArgs e)
        {

            if (tourPlaybackOn)
            {
               
                // remove associated media used in tour (in retrospect, perhaps a separate "MSITourScatterView" layer should have been created
                foreach (DockableItem item in dockableItemsLoaded)
                {
                    item.Visibility = Visibility.Hidden;
                    artModeWin.MSIScatterView.Items.Remove(item);
                }
                foreach (MultiScaleImage msiItem in msiItemsLoaded)
                {
                    msiItem.Visibility = Visibility.Hidden; // hide artwork MSI used in tour (in retrospect, not sure if this is necessary anymore since I added a new MSI instance called msi_tour
                }

                // swap artwork navigator event handlers
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.RemoveValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.AddValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                artModeWin.msi_tour_thumb.Visibility = Visibility.Hidden;

                tourStoryboard.CurrentTimeInvalidated -= new EventHandler(TourStoryboardPlayback_CurrentTimeInvalidated);
                tourStoryboard.Completed -= new EventHandler(TourStoryboardPlayback_Completed);
                tourStoryboard.Stop(artModeWin);

                artModeWin.tourSeekBar.Visibility = Visibility.Collapsed;

                artModeWin.msi.Visibility = Visibility.Visible;
                artModeWin.msi_thumb.Visibility = Visibility.Visible;


                artModeWin.ImageArea.RenderTransform = null;
                artModeWin.ArtModeLayout();

                // swap tour control buttons with artwork inter-mode navigation ones
                artModeWin.toggleLeftSide();
                artModeWin.tourAuthoringButton.Visibility = Visibility.Visible;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Visible;
                artModeWin.resetArtworkButton.Visibility = Visibility.Visible;
                artModeWin.exitButton.Visibility = Visibility.Visible;
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;
                artModeWin.tourControlButton.Visibility = Visibility.Collapsed;
                artModeWin.tourExploreButton.Visibility = Visibility.Collapsed;

                artModeWin.tourStopButton.Visibility = Visibility.Collapsed;
                artModeWin.hideMetaList();
                artModeWin.m_hotspotCollection.reAddHotspotIcons();
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;

                artModeWin.ImageArea.IsHitTestVisible = true;
                artModeWin.MainScatterView.IsHitTestVisible = true;
                tourPlaybackOn = false;
                artModeWin.msi_tour.EnableEventHandlers();

                foreach (SurfaceInkCanvas ic in inkCanvases)
                {
                    
                    ic.IsHitTestVisible = false;
                    ic.Visibility = Visibility.Visible;
                }
                if (artModeWin.bottomPanelVisible)
                {
                    artModeWin.collapseButtonDown.Visibility = Visibility.Visible;
                }
                else
                {
                    artModeWin.collapseButtonUp.Visibility = Visibility.Visible;
                }
                if (artModeWin.leftPanelVisible)
                {
                    artModeWin.collapseButtonLeft.Visibility = Visibility.Visible;
                }
                else
                {
                    artModeWin.collapseButtonRight.Visibility = Visibility.Visible;
                }
            }
        }

        public void registerMSI(MultiScaleImage m, Timeline t)
        {
            if (!msiToTLDict.ContainsKey(m))
                msiToTLDict.Add(m, t);
        }

        public void registerDockableItem(DockableItem d, Timeline t)
        {
            if (!itemToTLDict.ContainsKey(d))
                itemToTLDict.Add(d, t);
        }

        public void TourAuthoringDoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (artModeWin.leftPanelVisible)
            {
                artModeWin.collapseButtonLeft.Visibility = Visibility.Visible;
            }
            else
            {
                artModeWin.collapseButtonRight.Visibility = Visibility.Visible;
            }
            if (artModeWin.bottomPanelVisible)
            {
                artModeWin.collapseButtonDown.Visibility = Visibility.Visible;
            }
            else
            {
                artModeWin.collapseButtonUp.Visibility = Visibility.Visible;
            }

            e.Handled = true;
            if (tourAuthoringOn)
            {
                // remove associated media used in tour (in retrospect, perhaps a separate "MSITourScatterView" layer should have been created
                foreach (DockableItem item in dockableItemsLoaded)
                {
                    item.Visibility = Visibility.Hidden;
                    artModeWin.MSIScatterView.Items.Remove(item);
                }
                foreach (MultiScaleImage msiItem in msiItemsLoaded)
                {
                    msiItem.Visibility = Visibility.Hidden; // hide artwork MSI used in tour (in retrospect, not sure if this is necessary anymore since I added a new MSI instance called msi_tour
                }

                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.RemoveValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.AddValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                artModeWin.msi_tour_thumb.Visibility = Visibility.Hidden;

                tourStoryboard.CurrentTimeInvalidated -= new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_CurrentTimeInvalidated);
                tourStoryboard.Completed -= new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_Completed);

                artModeWin.msi.Visibility = Visibility.Visible;
                artModeWin.msi_thumb.Visibility = Visibility.Visible;

                tourAuthoringUI.removeAuthTools();

                // swap tour control buttons with artwork inter-mode navigation ones
                artModeWin.tourAuthoringButton.Visibility = Visibility.Visible;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Visible;
                artModeWin.resetArtworkButton.Visibility = Visibility.Visible;
                //artModeWin.languageButton.Visibility = Visibility.Visible;
                artModeWin.exitButton.Visibility = Visibility.Visible;
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;
                artModeWin.tourExploreButton.Visibility = Visibility.Collapsed;
                artModeWin.tourControlButton.Visibility = Visibility.Collapsed;
                artModeWin.hideMetaList();
                artModeWin.metaData.Visibility = Visibility.Collapsed;
                artModeWin.m_hotspotCollection.reAddHotspotIcons();
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;
                artModeWin.MainScatterView.Visibility = Visibility.Visible;
                // swap tour authoring UI with workspace panel
                artModeWin.tourAuthoringUICanvas.Visibility = Visibility.Collapsed;
                artModeWin.BottomPanel.Visibility = Visibility.Visible;

                artModeWin.AuthLeftPanel.Visibility = Visibility.Hidden;
                artModeWin.LeftPanel.Visibility = Visibility.Visible;
                artModeWin.ArtModeLayout();
                foreach (SurfaceInkCanvas c in inkCanvases)
                {
                    artModeWin.ImageArea.Children.Remove(c);
                }
                inkCanvases.Clear();
                tourStoryboard.Stop(artModeWin);
                loadTourButtons();
                tourAuthoringOn = false;
            }
        }

    

        

        public void TourAuthoringDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (tourAuthoringOn)
            {
                if (MessageBox.Show("Are you sure you want to delete this tour?",
                  "Delete the Tour", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (File.Exists("Data/Tour/XML/" + artModeWin.currentArtworkFileName + "." + "xml"))
                    {
                        File.Delete("Data/Tour/XML/" + artModeWin.currentArtworkFileName + "." + "xml");
                    }

                    this.TourAuthoringDoneButton_Click(sender, e); //bad idea?
                }
                else
                {
                    return;
                }
            }
        }

        public void undoableActionPerformed()
        {
            undoStack.Push(copyTourDict(tourBiDictionary));
            redoStack.Clear();
        }

        public BiDictionary<Timeline, BiDictionary<double, TourEvent>> copyTourDict(BiDictionary<Timeline, BiDictionary<double, TourEvent>> toCopy)
        {

            BiDictionary<Timeline, BiDictionary<double, TourEvent>> toUndo = new BiDictionary<Timeline, BiDictionary<double, TourEvent>>();
            foreach (Timeline tl in toCopy.firstKeys)
            {
                IList<BiDictionary<double, TourEvent>> list = toCopy[tl];
                TourTL tourTL = (TourTL)tl;
                Timeline newTL = tourTL.copy() as Timeline;
                BiDictionary<double, TourEvent> newDict = new BiDictionary<double, TourEvent>();
                foreach (double d in list[0].firstKeys)
                {
                    newDict.Add(d, (list[0][d][0]).copy());
                }
                toUndo.Add(newTL, newDict);
            }
            return toUndo;
        }

        public void undo()
        {
            if (undoStack.Count != 0)
            {
                redoStack.Push(copyTourDict(tourBiDictionary));
                tourBiDictionary = undoStack.Pop();
                tourAuthoringUI.refreshUI();
            }

        }
        public void redo()
        {
            if (redoStack.Count != 0)
            {
                undoStack.Push(copyTourDict(tourBiDictionary));
                tourBiDictionary = redoStack.Pop();
                tourAuthoringUI.refreshUI();
            }
        }

        #endregion

        #region tour playback seek bar event handlers

        private void TourSeekBarMarker_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            tourStoryboard.CurrentTimeInvalidated -= TourStoryboardPlayback_CurrentTimeInvalidated;
            ((Rectangle)sender).CaptureTouch(e.TouchDevice);
            startDragPoint = e.TouchDevice.GetCenterPosition(artModeWin);
        }

        private void TourSeekBarMarker_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            tourStoryboard.CurrentTimeInvalidated -= TourStoryboardPlayback_CurrentTimeInvalidated;
            ((Rectangle)sender).CaptureMouse();
            startDragPoint = e.MouseDevice.GetCenterPosition(artModeWin);
            mouseIsDown = true;
        }

        private void TourSeekBarMarker_PreviewTouchMove(object sender, TouchEventArgs e)
        {

            Point current = e.TouchDevice.GetCenterPosition(artModeWin);
            Double dragDistance = current.X - startDragPoint.X;

            double tourSeekBarProgressTargetWidth = artModeWin.tourSeekBarProgress.Width + dragDistance;
            if (tourSeekBarProgressTargetWidth < 0)
            {
                tourSeekBarProgressTargetWidth = 0;
            }
            else if (tourSeekBarProgressTargetWidth > artModeWin.tourSeekBarSlider.Width)
            {
                tourSeekBarProgressTargetWidth = artModeWin.tourSeekBarSlider.Width;
            }

            tourTimerCount = (int)((tourSeekBarProgressTargetWidth / artModeWin.tourSeekBarSlider.Width) * tourStoryboard.Duration.TimeSpan.TotalSeconds); // will this duration thingy work?
            tourTimerCountSpan = TimeSpan.FromSeconds(tourTimerCount);
            authorTimerCountSpan = tourTimerCountSpan;
            tourTimerCountSpanString = string.Format("{0:D2}:{1:D2}", tourTimerCountSpan.Minutes, tourTimerCountSpan.Seconds);
            artModeWin.tourSeekBarTimerCount.Content = tourTimerCountSpanString;

            artModeWin.tourSeekBarMarker.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth - 20);
            artModeWin.tourSeekBarProgress.Width = tourSeekBarProgressTargetWidth;

            startDragPoint = current;
        }

        private void TourSeekBarMarker_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseIsDown)
            {
                Point current = e.MouseDevice.GetCenterPosition(artModeWin);
                Double dragDistance = current.X - startDragPoint.X;

                double tourSeekBarProgressTargetWidth = artModeWin.tourSeekBarProgress.Width + dragDistance;
                if (tourSeekBarProgressTargetWidth < 0)
                {
                    tourSeekBarProgressTargetWidth = 0;
                }
                else if (tourSeekBarProgressTargetWidth > artModeWin.tourSeekBarSlider.Width)
                {
                    tourSeekBarProgressTargetWidth = artModeWin.tourSeekBarSlider.Width;
                }

                tourTimerCount = (int)((tourSeekBarProgressTargetWidth / artModeWin.tourSeekBarSlider.Width) * tourStoryboard.Duration.TimeSpan.TotalSeconds); // will this duration thingy work?
                tourTimerCountSpan = TimeSpan.FromSeconds(tourTimerCount);
                authorTimerCountSpan = tourTimerCountSpan;
                tourTimerCountSpanString = string.Format("{0:D2}:{1:D2}", tourTimerCountSpan.Minutes, tourTimerCountSpan.Seconds);
                artModeWin.tourSeekBarTimerCount.Content = tourTimerCountSpanString;

                artModeWin.tourSeekBarMarker.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth - 20);
                artModeWin.tourSeekBarProgress.Width = tourSeekBarProgressTargetWidth;

                startDragPoint = current;
            }
        }

        private void TourSeekBarMarker_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            tourStoryboard.Seek(artModeWin, tourTimerCountSpan, TimeSeekOrigin.BeginTime);
            tourStoryboard.CurrentTimeInvalidated += TourStoryboardPlayback_CurrentTimeInvalidated;
        }

        private void TourSeekBarMarker_PreviewMouseUp(object sender, MouseEventArgs e)
        {
            if (mouseIsDown)
            {
                tourStoryboard.Seek(artModeWin, tourTimerCountSpan, TimeSeekOrigin.BeginTime);
                tourStoryboard.CurrentTimeInvalidated += TourStoryboardPlayback_CurrentTimeInvalidated;
                mouseIsDown = false;
                Mouse.Capture(null);
            }
        }

        #endregion

        #region tour authoring items event handlers

        void msiTouchDown(object sender, EventArgs e)
        {
            if (!tourAuthoringOn)
                return;

            MultiScaleImage msi = sender as MultiScaleImage;
            ZoomableCanvas zcan = msi.GetZoomableCanvas;
            Point offset = zcan.Offset;
            double scale = zcan.Scale;
            zcan.ApplyAnimationClock(ZoomableCanvas.OffsetProperty, null);
            zcan.ApplyAnimationClock(ZoomableCanvas.ScaleProperty, null);
            zcan.Offset = new Point(offset.X, offset.Y);
            zcan.Scale = scale;
        }

        void msiTouchMoved(object sender, EventArgs e)
        {

        }

        void msiTouchUp(object sender, EventArgs e)
        {
            if (!tourAuthoringOn)
                return;
            undoableActionPerformed();
            double scrubtime = authorTimerCountSpan.TotalSeconds;
            ZoomableCanvas zcan = (sender as MultiScaleImage).GetZoomableCanvas;
            Timeline tl;
            if (msiToTLDict.TryGetValue(sender as MultiScaleImage, out tl))
            {
                BiDictionary<double, TourEvent> itemDict;
                IList<BiDictionary<double, TourEvent>> list = tourBiDictionary[tl];
                if (list.Count != 0)
                {
                    itemDict = list[0];
                    double startEventTime = System.Double.MaxValue;
                    bool needToAddEvent = true;
                    foreach (TourEvent tevent in itemDict.firstValues)
                    {
                        startEventTime = itemDict[tevent][0];
                        // TODO: Fill in the switch which changes to each type of event

                        if ((authorTimerCountSpan.TotalSeconds > startEventTime && authorTimerCountSpan.TotalSeconds <= (startEventTime + tevent.duration)))
                        {
                            needToAddEvent = false;
                            scrubtime = startEventTime + tevent.duration;
                            switch (tevent.type)
                            {
                                case TourEvent.Type.fadeInMSI:
                                    break;
                                case TourEvent.Type.fadeOutMSI:
                                    break;
                                case TourEvent.Type.zoomMSI:
                                    ZoomMSIEvent zoomMediaEvent = (ZoomMSIEvent)tevent;
                                    zoomMediaEvent.zoomToMSIPointX = zcan.Offset.X;
                                    zoomMediaEvent.zoomToMSIPointY = zcan.Offset.Y;
                                    zoomMediaEvent.absoluteScale = zcan.Scale;
                                    break;
                                default:
                                    break;
                            }
                            tourAuthoringUI.refreshUI();
                        }
                    }
                    if (needToAddEvent)
                    {
                        ZoomMSIEvent toAdd = new ZoomMSIEvent(sender as MultiScaleImage, zcan.Scale, zcan.Offset.X, zcan.Offset.Y, 1);
                        double time = authorTimerCountSpan.TotalSeconds - 1;
                        if (time < 0) time = 0;
                        itemDict.Add(time, toAdd);
                    }
                }
            }
            tourAuthoringUI.refreshUI();
            this.StopAndReloadTourAuthoringUIFromDict(scrubtime);
        }

        public void msiMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!tourAuthoringOn)
                return;

            undoableActionPerformed();
            if (!tourAuthoringOn)
                return;

            MultiScaleImage msi = sender as MultiScaleImage;
            ZoomableCanvas zcan = (sender as MultiScaleImage).GetZoomableCanvas;
            Point offset = zcan.Offset;
            double scale = zcan.Scale;
            zcan.ApplyAnimationClock(ZoomableCanvas.OffsetProperty, null);
            zcan.ApplyAnimationClock(ZoomableCanvas.ScaleProperty, null);
            zcan.Offset = new Point(offset.X, offset.Y);
            zcan.Scale = scale;

            (sender as MultiScaleImage).ScaleFromWheel(e);
            double scrubtime = authorTimerCountSpan.TotalSeconds;
            
            Timeline tl;

            if (msiToTLDict.TryGetValue(sender as MultiScaleImage, out tl))
            {
                BiDictionary<double, TourEvent> itemDict;
                IList<BiDictionary<double, TourEvent>> list = tourBiDictionary[tl];
                if (list.Count != 0)
                {
                    itemDict = list[0];
                    double startEventTime = System.Double.MaxValue;
                    bool needToAddEvent = true;
                    foreach (TourEvent tevent in itemDict.firstValues)
                    {
                        // When the object gets moved, set the scrub to where the object first appears.
                        startEventTime = itemDict[tevent][0];
                        // Dealing with each type of event
                        // TODO: Fill in the switch which changes to each type of event

                        if ((authorTimerCountSpan.TotalSeconds > startEventTime && authorTimerCountSpan.TotalSeconds <= (startEventTime + tevent.duration)))
                        {
                            needToAddEvent = false;
                            scrubtime = startEventTime + tevent.duration;
                            switch (tevent.type)
                            {
                                case TourEvent.Type.fadeInMSI:
                                    break;
                                case TourEvent.Type.fadeOutMSI:
                                    break;
                                case TourEvent.Type.zoomMSI:
                                    ZoomMSIEvent zoomMediaEvent = (ZoomMSIEvent)tevent;
                                    zoomMediaEvent.zoomToMSIPointX = zcan.Offset.X;
                                    zoomMediaEvent.zoomToMSIPointY = zcan.Offset.Y;
                                    zoomMediaEvent.absoluteScale = zcan.Scale;
                                    break;
                                default:
                                    break;
                            }
                            tourAuthoringUI.refreshUI();
                        }
                    }
                    if (needToAddEvent)
                    {
                        ZoomMSIEvent toAdd = new ZoomMSIEvent(sender as MultiScaleImage, zcan.Scale, zcan.Offset.X, zcan.Offset.Y, 1);
                        double time = authorTimerCountSpan.TotalSeconds - 1;
                        if (time < 0) time = 0;
                        itemDict.Add(time, toAdd);
                    }
                }
            }
            tourAuthoringUI.refreshUI();
            this.StopAndReloadTourAuthoringUIFromDict(scrubtime);
        }


        /*
         * removes animations from item's center location to allow for moving of the piece. 
         * 
         */
        void mediaTouchDown(object sender, EventArgs e)
        {
            if (!tourAuthoringOn)
                return;

            DockableItem dockItem = sender as DockableItem;
            Point center = dockItem.ActualCenter;
            double height = dockItem.ActualHeight;
            double width = dockItem.ActualWidth;
            dockItem.ApplyAnimationClock(DockableItem.CenterProperty, null);
            dockItem.ApplyAnimationClock(DockableItem.HeightProperty, null);
            dockItem.ApplyAnimationClock(DockableItem.WidthProperty, null);
            dockItem.Center = center;
            dockItem.Width = width;
            dockItem.Height = height;

        }

        /*  
         * TODO: possibly nothing?
         */
        void mediaTouchMoved(object sender, EventArgs e)
        {

        }

        /*
         * TODO: change current tour event and reattach animation
         * 
         * Does nothing if not in authoring mode
         * If it is, then it checks to find the tour events that it's attached to, and, if it's DURING a tour event
         */
        void mediaTouchUp(object sender, EventArgs e)
        {
            if (!tourAuthoringOn)
                return;
            undoableActionPerformed();
            double scrubtime = authorTimerCountSpan.TotalSeconds;

            DockableItem dockItem = sender as DockableItem;
            Timeline tl;
            if (itemToTLDict.TryGetValue(dockItem, out tl))
            {
                BiDictionary<double, TourEvent> itemDict;
                IList<BiDictionary<double, TourEvent>> list = tourBiDictionary[tl];
                if (list.Count != 0)
                {
                    itemDict = list[0];
                    double startEventTime = System.Double.MaxValue;
                    bool needToAddEvent = true;
                    foreach (TourEvent tevent in itemDict.firstValues)
                    {
                        startEventTime = itemDict[tevent][0];
                        if (tevent.type == TourEvent.Type.zoomMedia)
                        {
                            ZoomMediaEvent ze = (ZoomMediaEvent)tevent;
                        }

                        if ((authorTimerCountSpan.TotalSeconds >= startEventTime && authorTimerCountSpan.TotalSeconds <= (startEventTime + tevent.duration)))
                        {
                            needToAddEvent = false;
                            scrubtime = startEventTime + tevent.duration;
                            switch (tevent.type)
                            {
                                case TourEvent.Type.fadeInMedia:
                                    FadeInMediaEvent fadeInMediaEvent = (FadeInMediaEvent)tevent;
                                    fadeInMediaEvent.fadeInMediaToScreenPointX = dockItem.ActualCenter.X;
                                    fadeInMediaEvent.fadeInMediaToScreenPointY = dockItem.ActualCenter.Y;
                                    fadeInMediaEvent.absoluteScale = dockItem.image.ActualWidth / dockItem.image.Source.Width;
                                    break;
                                case TourEvent.Type.fadeOutMedia:
                                    break;
                                case TourEvent.Type.zoomMedia:
                                    ZoomMediaEvent zoomMediaEvent = (ZoomMediaEvent)tevent;
                                    zoomMediaEvent.zoomMediaToScreenPointX = dockItem.ActualCenter.X;
                                    zoomMediaEvent.zoomMediaToScreenPointY = dockItem.ActualCenter.Y;
                                    zoomMediaEvent.absoluteScale = dockItem.image.ActualWidth / dockItem.image.Source.Width;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    if (needToAddEvent)
                    {
                        double time = authorTimerCountSpan.TotalSeconds - 1;
                        if (time < 0)
                        {
                            time = 0;
                        }
                        ZoomMediaEvent toAdd = new ZoomMediaEvent(dockItem, dockItem.image.ActualWidth / dockItem.image.Source.Width, dockItem.ActualCenter.X, dockItem.ActualCenter.Y, 1);
                        itemDict.Add(time, toAdd);
                    }
                }
            }
            tourAuthoringUI.refreshUI();
            this.StopAndReloadTourAuthoringUIFromDict(scrubtime - .01);
        }

        public void mediaMouseWheel(object sender, MouseWheelEventArgs e)
        {
            DockableItem dockItem = sender as DockableItem;

            double newWidth;
            newWidth = ((double)e.Delta) / 5.0 + dockItem.ActualWidth;

            if (newWidth < 50) return;

            if (!tourAuthoringOn)
                return;

            undoableActionPerformed();
            double scrubtime = authorTimerCountSpan.TotalSeconds;
            Timeline tl;
            if (itemToTLDict.TryGetValue(dockItem, out tl))
            {
                BiDictionary<double, TourEvent> itemDict;
                IList<BiDictionary<double, TourEvent>> list = tourBiDictionary[tl];
                if (list.Count != 0)
                {
                    itemDict = list[0];
                    double startEventTime = System.Double.MaxValue;
                    bool needToAddEvent = true;
                    foreach (TourEvent tevent in itemDict.firstValues)
                    {
                        startEventTime = itemDict[tevent][0];
                        if (tevent.type == TourEvent.Type.zoomMedia)
                        {
                            ZoomMediaEvent ze = (ZoomMediaEvent)tevent;
                        }

                        if ((authorTimerCountSpan.TotalSeconds >= startEventTime && authorTimerCountSpan.TotalSeconds <= (startEventTime + tevent.duration)))
                        {
                            needToAddEvent = false;
                            scrubtime = startEventTime + tevent.duration;
                            switch (tevent.type)
                            {
                                case TourEvent.Type.fadeInMedia:
                                    FadeInMediaEvent fadeInMediaEvent = (FadeInMediaEvent)tevent;
                                    fadeInMediaEvent.fadeInMediaToScreenPointX = dockItem.ActualCenter.X;
                                    fadeInMediaEvent.fadeInMediaToScreenPointY = dockItem.ActualCenter.Y;
                                    fadeInMediaEvent.absoluteScale = newWidth / dockItem.image.Source.Width;
                                    break;
                                case TourEvent.Type.fadeOutMedia:
                                    break;
                                case TourEvent.Type.zoomMedia:
                                    ZoomMediaEvent zoomMediaEvent = (ZoomMediaEvent)tevent;
                                    zoomMediaEvent.zoomMediaToScreenPointX = dockItem.ActualCenter.X;
                                    zoomMediaEvent.zoomMediaToScreenPointY = dockItem.ActualCenter.Y;
                                    zoomMediaEvent.absoluteScale = newWidth / dockItem.image.Source.Width;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    if (needToAddEvent)
                    {
                        double time = authorTimerCountSpan.TotalSeconds - 1;
                        if (time < 0)
                        {
                            time = 0;
                        }
                        ZoomMediaEvent toAdd = new ZoomMediaEvent(dockItem, newWidth / dockItem.image.Source.Width, dockItem.ActualCenter.X, dockItem.ActualCenter.Y, 1);
                        itemDict.Add(time, toAdd);
                    }
                }
            }
            tourAuthoringUI.refreshUI();
            this.StopAndReloadTourAuthoringUIFromDict(scrubtime - .01);

        }

        #endregion

        #region tour playback storyboard event handlers

        private void TourStoryboardPlayback_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            try
            {
                // update seek bar time values
                if ((TimeSpan)tourStoryboard.GetCurrentTime(artModeWin) != null)
                {
                    tourCurrentTime = (TimeSpan)tourStoryboard.GetCurrentTime(artModeWin);
                    authorTimerCountSpan = tourCurrentTime;
                    tourCurrentTimeString = string.Format("{0:D2}:{1:D2}", tourCurrentTime.Minutes, tourCurrentTime.Seconds);
                    artModeWin.tourSeekBarTimerCount.Content = tourCurrentTimeString;

                }
                if (tourStoryboard.Duration.HasTimeSpan)
                {
                    tourDuration = tourStoryboard.Duration.TimeSpan;
                    tourDurationString = string.Format("{0:D2}:{1:D2}", tourDuration.Minutes, tourDuration.Seconds);
                    artModeWin.tourSeekBarLength.Content = " / " + tourDurationString;

                    double seekBarLocation = ((double)tourCurrentTime.TotalSeconds / (double)tourDuration.TotalSeconds) * (double)artModeWin.tourSeekBarSlider.Width;
                    if (seekBarLocation <= artModeWin.tourSeekBarSlider.Width)
                    {
                        artModeWin.tourSeekBarMarker.SetValue(Canvas.LeftProperty, seekBarLocation - 20);
                        artModeWin.tourSeekBarProgress.Width = seekBarLocation;
                    }
                }
            }
            catch (Exception exc)
            {

            }
        }

        private void TourStoryboardPlayback_Completed(object sender, EventArgs e)
        {
            // END TOUR
            if (tourPlaybackOn)
            {
                foreach (DockableItem item in dockableItemsLoaded)
                {
                    item.Visibility = Visibility.Hidden;
                    artModeWin.MSIScatterView.Items.Remove(item);
                }
                foreach (MultiScaleImage msiItem in msiItemsLoaded)
                {
                    msiItem.Visibility = Visibility.Hidden;
                }

                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.RemoveValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.AddValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                artModeWin.msi_tour_thumb.Visibility = Visibility.Hidden;

                tourStoryboard.CurrentTimeInvalidated -= new EventHandler(TourStoryboardPlayback_CurrentTimeInvalidated);
                tourStoryboard.Stop(artModeWin);

                artModeWin.tourSeekBar.Visibility = Visibility.Collapsed;

                artModeWin.msi.Visibility = Visibility.Visible;
                artModeWin.msi_thumb.Visibility = Visibility.Visible;

                artModeWin.msi_ViewboxUpdate();

                artModeWin.toggleLeftSide();
                artModeWin.tourAuthoringButton.Visibility = Visibility.Visible;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Visible;
                artModeWin.resetArtworkButton.Visibility = Visibility.Visible;
                artModeWin.exitButton.Visibility = Visibility.Visible;
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;
                artModeWin.tourControlButton.Visibility = Visibility.Collapsed;
                artModeWin.tourExploreButton.Visibility = Visibility.Collapsed;
                artModeWin.tourStopButton.Visibility = Visibility.Collapsed;
                artModeWin.hideMetaList();
                tourPlaybackOn = false;
            }
        }

        #endregion

        #region tour loading - helper methods


        /// <summary>
        /// Add new zoom tour event
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="media"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="scale"></param>
        private void addZoomTourEvent(DockableItem media, double scale, double x, double y, double start, double duration)
        {
            ZoomMediaEvent toAdd = new ZoomMediaEvent(media, scale, x, y, duration);
            Timeline tl;
            if (itemToTLDict.TryGetValue(media, out tl))
            {
                BiDictionary<double, TourEvent> itemDict;
                IList<BiDictionary<double, TourEvent>> itemDictList;
                if (tourBiDictionary.TryGetValue(tl, out itemDictList))
                {
                    itemDict = itemDictList[0];
                    itemDict.Add(start, toAdd);
                }
            }
        }


        /// <summary>
        /// Adding of animations for each TourEvent during loading of tourDict --> tourStoryboard
        /// </summary>
        /// <param name="tourParallelTL"></param>
        /// <param name="tourEvent"></param>
        /// <param name="timerCount"></param>
        public void addAnim(TourParallelTL tourParallelTL, TourEvent tourEvent, double timerCount)
        {
            switch (tourEvent.type)
            {
                case TourEvent.Type.fadeInMedia:
                    FadeInMediaEvent fadeInMediaEvent = (FadeInMediaEvent)tourEvent;

                    // used to use MSI points, but now using screen points -- see artwork mode documentation in Google Doc
                    Point mediaPoint = new Point();
                    mediaPoint.X = fadeInMediaEvent.fadeInMediaToScreenPointX;
                    mediaPoint.Y = fadeInMediaEvent.fadeInMediaToScreenPointY;

                    double initialScale = fadeInMediaEvent.absoluteScale;

                    DockableItem fadeInMediaItem = fadeInMediaEvent.media;
                    fadeInMediaItem.ApplyAnimationClock(DockableItem.CenterProperty, null);
                    fadeInMediaItem.Center = mediaPoint;
                    fadeInMediaItem.Width = fadeInMediaItem.image.Source.Width * initialScale;
                    fadeInMediaItem.Height = fadeInMediaItem.image.Source.Height * initialScale;
                    fadeInMediaItem.Orientation = 0;

                    DoubleAnimation fadeInMediaWidth = new DoubleAnimation(fadeInMediaItem.Width, fadeInMediaItem.Width, new Duration(TimeSpan.FromSeconds(0.0)));
                    Storyboard.SetTarget(fadeInMediaWidth, fadeInMediaItem);
                    Storyboard.SetTargetProperty(fadeInMediaWidth, new PropertyPath(DockableItem.WidthProperty));
                    fadeInMediaWidth.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInMediaWidth);

                    DoubleAnimation fadeInMediaHeight = new DoubleAnimation(fadeInMediaItem.Height, fadeInMediaItem.Height, new Duration(TimeSpan.FromSeconds(0.0)));
                    Storyboard.SetTarget(fadeInMediaHeight, fadeInMediaItem);
                    Storyboard.SetTargetProperty(fadeInMediaHeight, new PropertyPath(DockableItem.HeightProperty));
                    fadeInMediaHeight.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInMediaHeight);

                    PointAnimation fadeInMediaCenter = new PointAnimation(fadeInMediaItem.Center, fadeInMediaItem.Center, new Duration(TimeSpan.FromSeconds(0.0)));
                    Storyboard.SetTarget(fadeInMediaCenter, fadeInMediaItem);
                    Storyboard.SetTargetProperty(fadeInMediaCenter, new PropertyPath(DockableItem.CenterProperty));
                    fadeInMediaCenter.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInMediaCenter);

                    ObjectAnimationUsingKeyFrames fadeInMediaAnim_vis = new ObjectAnimationUsingKeyFrames();
                    fadeInMediaAnim_vis.Duration = new TimeSpan(0, 0, 0);
                    DiscreteObjectKeyFrame fadeInMediaAnim_vis_kf1 = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan(0, 0, 0));
                    fadeInMediaAnim_vis.KeyFrames.Add(fadeInMediaAnim_vis_kf1);
                    Storyboard.SetTarget(fadeInMediaAnim_vis, fadeInMediaItem);
                    Storyboard.SetTargetProperty(fadeInMediaAnim_vis, new PropertyPath(DockableItem.VisibilityProperty));
                    fadeInMediaAnim_vis.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInMediaAnim_vis);

                    DoubleAnimation fadeInMediaAnim = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(fadeInMediaEvent.duration)));
                    Storyboard.SetTarget(fadeInMediaAnim, fadeInMediaItem);
                    Storyboard.SetTargetProperty(fadeInMediaAnim, new PropertyPath(DockableItem.OpacityProperty));
                    fadeInMediaAnim.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInMediaAnim);

                    break;



                case TourEvent.Type.fadeOutMedia:
                    FadeOutMediaEvent fadeOutMediaEvent = (FadeOutMediaEvent)tourEvent;

                    DockableItem fadeOutMediaItem = fadeOutMediaEvent.media;
                    fadeOutMediaItem.ApplyAnimationClock(DockableItem.CenterProperty, null);
                    DoubleAnimation fadeOutMediaAnim = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(fadeOutMediaEvent.duration)));
                    Storyboard.SetTarget(fadeOutMediaAnim, fadeOutMediaItem);
                    Storyboard.SetTargetProperty(fadeOutMediaAnim, new PropertyPath(DockableItem.OpacityProperty));
                    fadeOutMediaAnim.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeOutMediaAnim);

                    ObjectAnimationUsingKeyFrames fadeOutMediaAnim_vis = new ObjectAnimationUsingKeyFrames();
                    fadeOutMediaAnim_vis.Duration = new TimeSpan(0, 0, 0);
                    DiscreteObjectKeyFrame fadeOutMediaAnim_vis_kf1 = new DiscreteObjectKeyFrame(Visibility.Hidden, new TimeSpan(0, 0, 0));
                    fadeOutMediaAnim_vis.KeyFrames.Add(fadeOutMediaAnim_vis_kf1);
                    Storyboard.SetTarget(fadeOutMediaAnim_vis, fadeOutMediaItem);
                    Storyboard.SetTargetProperty(fadeOutMediaAnim_vis, new PropertyPath(DockableItem.VisibilityProperty));
                    fadeOutMediaAnim_vis.BeginTime = TimeSpan.FromSeconds(timerCount + fadeOutMediaEvent.duration);
                    tourParallelTL.Children.Add(fadeOutMediaAnim_vis);

                    break;
                case TourEvent.Type.zoomMedia:
                    ZoomMediaEvent zoomMediaEvent = (ZoomMediaEvent)tourEvent;

                    DockableItem zoomMediaItem = zoomMediaEvent.media;
                    zoomMediaItem.ApplyAnimationClock(DockableItem.CenterProperty, null);
                    double targetMediaScale = zoomMediaEvent.absoluteScale;

                    DoubleAnimation zoomWidth = new DoubleAnimation(zoomMediaItem.image.Source.Width * targetMediaScale, new Duration(TimeSpan.FromSeconds(zoomMediaEvent.duration)));
                    Storyboard.SetTarget(zoomWidth, zoomMediaItem);
                    Storyboard.SetTargetProperty(zoomWidth, new PropertyPath(DockableItem.WidthProperty));
                    zoomWidth.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(zoomWidth);

                    DoubleAnimation zoomHeight = new DoubleAnimation(zoomMediaItem.image.Source.Height * targetMediaScale, new Duration(TimeSpan.FromSeconds(zoomMediaEvent.duration)));
                    Storyboard.SetTarget(zoomHeight, zoomMediaItem);
                    Storyboard.SetTargetProperty(zoomHeight, new PropertyPath(DockableItem.HeightProperty));
                    zoomHeight.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(zoomHeight);

                    // used to use MSI points, but now using screen points -- see artwork mode documentation in Google Doc
                    Point zoomEndPoint = new Point();
                    zoomEndPoint.X = zoomMediaEvent.zoomMediaToScreenPointX;
                    zoomEndPoint.Y = zoomMediaEvent.zoomMediaToScreenPointY;

                    PointAnimation zoomMediaPan = new PointAnimation(zoomEndPoint, new Duration(TimeSpan.FromSeconds(zoomMediaEvent.duration)), FillBehavior.HoldEnd);
                    Storyboard.SetTarget(zoomMediaPan, zoomMediaItem);
                    Storyboard.SetTargetProperty(zoomMediaPan, new PropertyPath(DockableItem.CenterProperty));
                    zoomMediaPan.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(zoomMediaPan);

                    break;
                case TourEvent.Type.zoomMSI:
                    ZoomMSIEvent zoomMSIEvent = (ZoomMSIEvent)tourEvent;

                    double targetMSIScale = zoomMSIEvent.absoluteScale;

                    Point zoomToMSIPoint = new Point(zoomMSIEvent.zoomToMSIPointX, zoomMSIEvent.zoomToMSIPointY);

                    // new
                    ZoomableCanvas zoomMSI_canvas = zoomMSIEvent.msi.GetZoomableCanvas;
                    targetMSIScale = zoomMSIEvent.msi.ClampTargetScale(targetMSIScale);

                    Point targetOffset = new Point(zoomToMSIPoint.X, zoomToMSIPoint.Y);

                    PointAnimation zoomMSI_pan = new PointAnimation(targetOffset, new Duration(TimeSpan.FromSeconds(zoomMSIEvent.duration)));
                    Storyboard.SetTarget(zoomMSI_pan, zoomMSI_canvas);
                    Storyboard.SetTargetProperty(zoomMSI_pan, new PropertyPath(ZoomableCanvas.OffsetProperty));
                    zoomMSI_pan.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(zoomMSI_pan);

                    DoubleAnimation zoomMSI_scale = new DoubleAnimation(targetMSIScale, new Duration(TimeSpan.FromSeconds(zoomMSIEvent.duration)));
                    zoomMSI_scale.CurrentStateInvalidated += new EventHandler(ZoomMSI_scale_CurrentStateInvalidated);
                    Storyboard.SetTarget(zoomMSI_scale, zoomMSI_canvas);
                    Storyboard.SetTargetProperty(zoomMSI_scale, new PropertyPath(ZoomableCanvas.ScaleProperty));
                    zoomMSI_scale.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(zoomMSI_scale);

                    break;
                case TourEvent.Type.fadeInPath:
                    FadeInPathEvent fadeInPathEvent = (FadeInPathEvent)tourEvent;

                    // used to use MSI points, but now using screen points -- see artwork mode documentation in Google Doc

                    SurfaceInkCanvas fadeInPathItem = fadeInPathEvent.inkCanvas;
                    fadeInPathItem.Opacity = 0.0;


                    ObjectAnimationUsingKeyFrames fadeInPathAnim_vis = new ObjectAnimationUsingKeyFrames();
                    fadeInPathAnim_vis.Duration = new TimeSpan(0, 0, 0);
                    DiscreteObjectKeyFrame fadeInPathAnim_vis_kf1 = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan(0, 0, 0));
                    fadeInPathAnim_vis.KeyFrames.Add(fadeInPathAnim_vis_kf1);
                    Storyboard.SetTarget(fadeInPathAnim_vis, fadeInPathItem);
                    Storyboard.SetTargetProperty(fadeInPathAnim_vis, new PropertyPath(DockableItem.VisibilityProperty));
                    fadeInPathAnim_vis.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInPathAnim_vis);

                    DoubleAnimation fadeInPathAnim = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(fadeInPathEvent.duration)));
                    Storyboard.SetTarget(fadeInPathAnim, fadeInPathItem);
                    Storyboard.SetTargetProperty(fadeInPathAnim, new PropertyPath(DockableItem.OpacityProperty));
                    fadeInPathAnim.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInPathAnim);

                    break;
                case TourEvent.Type.fadeOutPath:
                    FadeOutPathEvent fadeOutPathEvent = (FadeOutPathEvent)tourEvent;

                    SurfaceInkCanvas fadeOutPathItem = fadeOutPathEvent.inkCanvas;
                    DoubleAnimation fadeOutPathAnim = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(fadeOutPathEvent.duration)));
                    Storyboard.SetTarget(fadeOutPathAnim, fadeOutPathItem);
                    Storyboard.SetTargetProperty(fadeOutPathAnim, new PropertyPath(DockableItem.OpacityProperty));
                    fadeOutPathAnim.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeOutPathAnim);

                    ObjectAnimationUsingKeyFrames fadeOutPathAnim_vis = new ObjectAnimationUsingKeyFrames();
                    fadeOutPathAnim_vis.Duration = new TimeSpan(0, 0, 0);
                    DiscreteObjectKeyFrame fadeOutPathAnim_vis_kf1 = new DiscreteObjectKeyFrame(Visibility.Hidden, new TimeSpan(0, 0, 0));
                    fadeOutPathAnim_vis.KeyFrames.Add(fadeOutPathAnim_vis_kf1);
                    Storyboard.SetTarget(fadeOutPathAnim_vis, fadeOutPathItem);
                    Storyboard.SetTargetProperty(fadeOutPathAnim_vis, new PropertyPath(DockableItem.VisibilityProperty));
                    fadeOutPathAnim_vis.BeginTime = TimeSpan.FromSeconds(timerCount + fadeOutPathEvent.duration);
                    tourParallelTL.Children.Add(fadeOutPathAnim_vis);

                    break;

                case TourEvent.Type.fadeInHighlight:

                    FadeInHighlightEvent fadeInHighlightEvent = (FadeInHighlightEvent)tourEvent;

                    // used to use MSI points, but now using screen points -- see artwork mode documentation in Google Doc

                    SurfaceInkCanvas fadeInHighlightItem = fadeInHighlightEvent.inkCanvas;
                    fadeInHighlightItem.Opacity = 0.0;

                    ObjectAnimationUsingKeyFrames fadeInHighlightAnim_vis = new ObjectAnimationUsingKeyFrames();
                    fadeInHighlightAnim_vis.Duration = new TimeSpan(0, 0, 0);
                    DiscreteObjectKeyFrame fadeInHighlightAnim_vis_kf1 = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan(0, 0, 0));
                    fadeInHighlightAnim_vis.KeyFrames.Add(fadeInHighlightAnim_vis_kf1);
                    Storyboard.SetTarget(fadeInHighlightAnim_vis, fadeInHighlightItem);
                    Storyboard.SetTargetProperty(fadeInHighlightAnim_vis, new PropertyPath(DockableItem.VisibilityProperty));
                    fadeInHighlightAnim_vis.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInHighlightAnim_vis);

                    DoubleAnimation fadeInHighlightAnim = new DoubleAnimation(0.0, fadeInHighlightEvent.opacity, new Duration(TimeSpan.FromSeconds(fadeInHighlightEvent.duration)));
                    Storyboard.SetTarget(fadeInHighlightAnim, fadeInHighlightItem);
                    Storyboard.SetTargetProperty(fadeInHighlightAnim, new PropertyPath(DockableItem.OpacityProperty));
                    fadeInHighlightAnim.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInHighlightAnim);

                    break;
                case TourEvent.Type.fadeOutHighlight:
                    FadeOutHighlightEvent fadeOutHighlightEvent = (FadeOutHighlightEvent)tourEvent;

                    SurfaceInkCanvas fadeOutHighlightItem = fadeOutHighlightEvent.inkCanvas;
                    DoubleAnimation fadeOutHighlightAnim = new DoubleAnimation(fadeOutHighlightEvent.opacity, 0.0, new Duration(TimeSpan.FromSeconds(fadeOutHighlightEvent.duration)));
                    Storyboard.SetTarget(fadeOutHighlightAnim, fadeOutHighlightItem);
                    Storyboard.SetTargetProperty(fadeOutHighlightAnim, new PropertyPath(DockableItem.OpacityProperty));
                    fadeOutHighlightAnim.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeOutHighlightAnim);

                    ObjectAnimationUsingKeyFrames fadeOutHighlightAnim_vis = new ObjectAnimationUsingKeyFrames();
                    fadeOutHighlightAnim_vis.Duration = new TimeSpan(0, 0, 0);
                    DiscreteObjectKeyFrame fadeOutHighlightAnim_vis_kf1 = new DiscreteObjectKeyFrame(Visibility.Hidden, new TimeSpan(0, 0, 0));
                    fadeOutHighlightAnim_vis.KeyFrames.Add(fadeOutHighlightAnim_vis_kf1);
                    Storyboard.SetTarget(fadeOutHighlightAnim_vis, fadeOutHighlightItem);
                    Storyboard.SetTargetProperty(fadeOutHighlightAnim_vis, new PropertyPath(DockableItem.VisibilityProperty));
                    fadeOutHighlightAnim_vis.BeginTime = TimeSpan.FromSeconds(timerCount + fadeOutHighlightEvent.duration);
                    tourParallelTL.Children.Add(fadeOutHighlightAnim_vis);

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// loads tiles for end zoom level of ZoomMSIEvents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomMSI_scale_CurrentStateInvalidated(object sender, EventArgs e)
        {
            foreach (MultiScaleImage msiItem in msiItemsLoaded)
            {
                msiItem.LoadCurrentZoomLevelTiles();
            }
        }

        #endregion

        #region tour testing buttons (in left sidebar of artwork mode)

        public void loadTourButtons()
        {
            artModeWin.TourScroll.Children.Clear();
            //IEnumerable<string> tours = Directory.EnumerateFiles(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\XML");
            //foreach (string filepath in tours)
            //{
            string filepath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\XML\\" + artModeWin.currentArtworkFileName+".xml";
                String name = "";
                XmlDocument doc = new XmlDocument();
            SurfaceButton button = new SurfaceButton();
            button.Width = artModeWin.Tours.Width;
            artModeWin.TourScroll.Children.Add(button);
            string filename = System.IO.Path.GetFileNameWithoutExtension(filepath);

            //button.Content = name;
            button.Tag = filename;
            if(File.Exists(filepath))
            {
                doc.Load(filepath);
                if (doc.HasChildNodes)
                {
                    foreach (XmlNode tourNode in doc.ChildNodes)
                    {
                        if (tourNode.Name == "TourStoryboard")
                        {
                            name = tourNode.Attributes.GetNamedItem("displayName").InnerText;
                        }
                    }
                }//

              
                
           
                button.Content = "Play Guided Tour";
                button.PreviewMouseDown += TourButton_Click;
                button.PreviewTouchDown += new EventHandler<TouchEventArgs>(TourButton_Click);
                
                object o = button.Parent;
                //artModeWin.TourScroll.SelectionChanged += new SelectionChangedEventHandler(TourScroll_SelectionChanged);
                
            }
            else
            {
                button.Content = "No availabel tours";
            }
        }
            
           


        void TourScroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           // artModeWin.TourScroll.UnselectAll();
        }
        private void artareasizechanged(object sender, EventArgs e)
        {
            int i = 0;
            i++;
            int j;
            int k;
        }

        public Boolean TourPlaybackOn
        {
            get { return tourPlaybackOn; }
            set { tourPlaybackOn = value; }
        }

        private void TourButton_Click(object sender, EventArgs e)
        {
            artModeWin.collapseButtonDown.Visibility = Visibility.Hidden;
            artModeWin.collapseButtonUp.Visibility = Visibility.Hidden;
            artModeWin.collapseButtonLeft.Visibility = Visibility.Hidden;
            artModeWin.collapseButtonRight.Visibility = Visibility.Hidden;
            string filename = (string)(sender as SurfaceButton).Tag; //this makes it try to find the file with the name on the button
            //currentlyPlayingTour = filename;
            this.LoadDictFromXML(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\XML\\" + filename + ".xml");
            this.LoadTourPlaybackFromDict();
            if (!tourPlaybackOn)
            {
                artModeWin.msi.Visibility = Visibility.Hidden;
                artModeWin.msi_thumb.Visibility = Visibility.Hidden;
                //artModeWin.sceneAudio.Stop();

                artModeWin.msi_tour.DisableEventHandlers();
                artModeWin.msi_tour.ResetArtwork();
                artModeWin.msi_tour.Visibility = Visibility.Visible;
                artModeWin.TourLayout();

                artModeWin.msi_tour_thumb.Visibility = Visibility.Visible;
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.AddValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.RemoveValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                this.msi_tour_ViewboxUpdate();

                artModeWin.toggleLeftSide();
                artModeWin.tourAuthoringButton.Visibility = Visibility.Collapsed;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Collapsed;
                artModeWin.resetArtworkButton.Visibility = Visibility.Collapsed;
                //artModeWin.languageButton.Visibility = Visibility.Visible;
                artModeWin.exitButton.Visibility = Visibility.Visible;
                artModeWin.HotspotOverlay.Visibility = Visibility.Collapsed;
                artModeWin.tourExploreButton.Visibility = Visibility.Visible;
                artModeWin.tourControlButton.Visibility = Visibility.Visible;
                artModeWin.tourStopButton.Visibility = Visibility.Visible;
                artModeWin.tourSeekBar.Visibility = Visibility.Visible;
                artModeWin.m_hotspotCollection.removeHotspotIcons();

                tourStoryboard.CurrentTimeInvalidated += new EventHandler(TourStoryboardPlayback_CurrentTimeInvalidated);
                tourStoryboard.Completed += new EventHandler(TourStoryboardPlayback_Completed);
                tourStoryboard.Begin(artModeWin, true);
                tourPlaybackOn = true;

                artModeWin.ImageArea.IsHitTestVisible = false;
                artModeWin.MainScatterView.IsHitTestVisible = false;
                //if (tourSwapDict[filename] == "English" || tourSwapDict[filename] == "Italian")
                //{
                //    artModeWin.tourInOneLang(tourSwapDict[filename]);
                //}
            }

            if (!artModeWin.bottomPanelVisible)
            {
                artModeWin.BottomPanel.Visibility = Visibility.Visible;
                artModeWin.ShowBottomPanel();
            }
        }

        public void loadAuthoringGUI()
        {
            if (!tourAuthoringOn)
            {
                artModeWin.msi.Visibility = Visibility.Hidden;
                artModeWin.msi_thumb.Visibility = Visibility.Hidden;

                artModeWin.MainScatterView.Visibility = Visibility.Hidden;
                artModeWin.msi_tour.ResetArtwork();
                artModeWin.msi_tour.Visibility = Visibility.Visible;

                artModeWin.msi_tour_thumb.Visibility = Visibility.Visible;
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.AddValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.RemoveValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                this.msi_tour_ViewboxUpdate();

                artModeWin.LeftPanel.Visibility = Visibility.Hidden;
                artModeWin.tourAuthoringButton.Visibility = Visibility.Collapsed;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Collapsed;
                artModeWin.resetArtworkButton.Visibility = Visibility.Collapsed;
                artModeWin.exitButton.Visibility = Visibility.Collapsed;
                artModeWin.HotspotOverlay.Visibility = Visibility.Collapsed;
                artModeWin.m_hotspotCollection.removeHotspotIcons();


                tourStoryboard.CurrentTimeInvalidated += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_CurrentTimeInvalidated);
                tourStoryboard.Completed += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_Completed);
                tourStoryboard.Begin(artModeWin, true);
                tourStoryboard.Pause(artModeWin);

                artModeWin.BottomPanel.Visibility = Visibility.Collapsed;
                artModeWin.tourAuthoringUICanvas.Visibility = Visibility.Visible;

                artModeWin.AuthLeftPanel.Visibility = Visibility.Visible;

                tourAuthoringOn = true;
            }
        }



        public void LoadTour(String XMLfilePath)
        {
            this.LoadDictFromXML(XMLfilePath);
            this.LoadTourPlaybackFromDict();
            this.LoadTourAuthoringUIFromDict();

            if (!tourAuthoringOn)
            {
                artModeWin.msi.Visibility = Visibility.Hidden;
                artModeWin.msi_thumb.Visibility = Visibility.Hidden;

                artModeWin.msi_tour.ResetArtwork();
                artModeWin.msi_tour.Visibility = Visibility.Visible;

                artModeWin.msi_tour_thumb.Visibility = Visibility.Visible;
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.AddValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.RemoveValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                this.msi_tour_ViewboxUpdate();

                artModeWin.toggleLeftSide();
                artModeWin.tourAuthoringButton.Visibility = Visibility.Collapsed;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Collapsed;
                artModeWin.resetArtworkButton.Visibility = Visibility.Collapsed;
                artModeWin.exitButton.Visibility = Visibility.Collapsed;
                artModeWin.HotspotOverlay.Visibility = Visibility.Collapsed;
                tourStoryboard.CurrentTimeInvalidated += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_CurrentTimeInvalidated);
                tourStoryboard.Completed += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_Completed);
                tourStoryboard.Begin(artModeWin, true);
                tourStoryboard.Pause(artModeWin);

                artModeWin.BottomPanel.Visibility = Visibility.Collapsed;
                artModeWin.tourAuthoringUICanvas.Visibility = Visibility.Visible;

                tourAuthoringOn = true;
            }
            else
            {
                artModeWin.BottomPanel.Visibility = Visibility.Visible;
            }
        }

        public Boolean TourAuthoringOn
        {
            get { return tourAuthoringOn; }
            set {tourAuthoringOn = value; }
        }

        public void CreateNewBlankTour(String XMLfilePath, String artworkFileName, String displayString, String description)
        {
            String fileContentString =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +

                "<TourStoryboard duration=\"40\" displayName=\"" + displayString + "\" description=\"" + description + "\">\r\n" +
                "<TourParallelTL type=\"artwork\" displayName=\"Main Artwork\" file=\"" + artworkFileName + "\">\r\n" +
                "<TourEvent beginTime=\"-1\" type=\"ZoomMSIEvent\" scale=\"0.2\" toMSIPointX=\"0\" toMSIPointY=\"0\" duration=\"1\"></TourEvent>\r\n" +
                "</TourParallelTL>\r\n" +
                "</TourStoryboard>";

            System.IO.FileStream fileStream = System.IO.File.OpenWrite(XMLfilePath);
            Byte[] output =
                new UTF8Encoding(true).GetBytes(fileContentString);
            fileStream.Write(output, 0, output.Length);
            fileStream.Flush();
            fileStream.Close();
        }

        #endregion

        #region XML <--> tourDict

        public void LoadDictFromString(String xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            if (doc.HasChildNodes)
            {
                foreach (XmlNode tourNode in doc.ChildNodes)
                {
                    if (tourNode.Name == "TourStoryboard")
                    {
                        tourStoryboard = new TourStoryboard();
                        tourStoryboard.displayName = tourNode.Attributes.GetNamedItem("displayName").InnerText;
                        tourStoryboard.description = tourNode.Attributes.GetNamedItem("description").InnerText;
                        //// Time experiment
                        tourStoryboard.totalDuration = double.Parse(tourNode.Attributes.GetNamedItem("duration").InnerText);

                        dockableItemsLoaded = new List<DockableItem>();
                        msiItemsLoaded = new List<MultiScaleImage>();
                        tourBiDictionary = new BiDictionary<Timeline, BiDictionary<double, TourEvent>>();
                        undoStack = new Stack<BiDictionary<Timeline, BiDictionary<double, TourEvent>>>();
                        redoStack = new Stack<BiDictionary<Timeline, BiDictionary<double, TourEvent>>>();
                        itemToTLDict = new Dictionary<DockableItem, Timeline>();
                        msiToTLDict = new Dictionary<MultiScaleImage, Timeline>();

                        foreach (XmlNode TLNode in tourNode.ChildNodes)
                        {
                            if (TLNode.Name == "TourParallelTL")
                            {
                                if (TLNode.Attributes.GetNamedItem("type").InnerText == "artwork")
                                {
                                    String msi_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    String msi_tour_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\DeepZoom\\" + msi_file + "\\dz.xml";
                                    artModeWin.msi_tour.SetImageSource(msi_tour_path);
                                    artModeWin.msi_tour_thumb.SetImageSource(msi_tour_path);

                                    msiItemsLoaded.Add(artModeWin.msi_tour);
                                    artModeWin.msi_tour.PreviewTouchDown += new EventHandler<TouchEventArgs>(msiTouchDown);
                                    artModeWin.msi_tour.PreviewMouseDown += new MouseButtonEventHandler(msiTouchDown);
                                    artModeWin.msi_tour.PreviewTouchMove += new EventHandler<TouchEventArgs>(msiTouchMoved);
                                    artModeWin.msi_tour.PreviewMouseMove += new MouseEventHandler(msiTouchMoved);
                                    artModeWin.msi_tour.PreviewTouchUp += new EventHandler<TouchEventArgs>(msiTouchUp);
                                    artModeWin.msi_tour.PreviewMouseUp += new MouseButtonEventHandler(msiTouchUp);
                                    artModeWin.msi_tour.PreviewMouseWheel += new MouseWheelEventHandler(msiMouseWheel);
                                    artModeWin.msi_tour.disableInertia();
                                    TourParallelTL msi_tour_TL = new TourParallelTL();

                                    msiToTLDict.Add(artModeWin.msi_tour, msi_tour_TL);


                                    msi_tour_TL.type = TourTLType.artwork;
                                    msi_tour_TL.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    msi_tour_TL.file = TLNode.Attributes.GetNamedItem("file").InnerText;

                                    BiDictionary<double, TourEvent> msi_tour_TL_dict = new BiDictionary<double, TourEvent>();
                                    tourBiDictionary.Add(msi_tour_TL, msi_tour_TL_dict);

                                    foreach (XmlNode TourEventNode in TLNode.ChildNodes)
                                    {
                                        if (TourEventNode.Name == "TourEvent")
                                        {
                                            if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "ZoomMSIEvent")
                                            {
                                                double scale = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("scale").InnerText);
                                                double toMSIPointX = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toMSIPointX").InnerText);
                                                double toMSIPointY = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toMSIPointY").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);
                                                TourEvent zoomMSIEvent = new ZoomMSIEvent(artModeWin.msi_tour, scale, toMSIPointX, toMSIPointY, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                msi_tour_TL_dict.Add(beginTime, zoomMSIEvent);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "media")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    DockableItem dockItem = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Metadata\\" + media_file);
                                    dockItem.PreviewMouseMove += new MouseEventHandler(mediaTouchDown);
                                    dockItem.PreviewTouchDown += new EventHandler<TouchEventArgs>(mediaTouchDown);
                                    dockItem.PreviewMouseDown += new MouseButtonEventHandler(mediaTouchDown);
                                    dockItem.PreviewTouchMove += new EventHandler<TouchEventArgs>(mediaTouchMoved);
                                    dockItem.PreviewMouseMove += new MouseEventHandler(mediaTouchMoved);
                                    dockItem.PreviewTouchUp += new EventHandler<TouchEventArgs>(mediaTouchUp);
                                    dockItem.PreviewMouseUp += new MouseButtonEventHandler(mediaTouchUp);

                                    dockableItemsLoaded.Add(dockItem);
                                    dockItem.Visibility = Visibility.Hidden;

                                    TourParallelTL dockItem_TL = new TourParallelTL();
                                    dockItem_TL.type = TourTLType.media;

                                    dockItem_TL.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    dockItem_TL.file = TLNode.Attributes.GetNamedItem("file").InnerText;

                                    itemToTLDict.Add(dockItem, dockItem_TL);

                                    BiDictionary<double, TourEvent> dockItem_TL_dict = new BiDictionary<double, TourEvent>();
                                    tourBiDictionary.Add(dockItem_TL, dockItem_TL_dict);

                                    foreach (XmlNode TourEventNode in TLNode.ChildNodes)
                                    {
                                        if (TourEventNode.Name == "TourEvent")
                                        {
                                            if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeInMediaEvent")
                                            {
                                                double toScreenPointX = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toScreenPointX").InnerText);
                                                double toScreenPointY = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toScreenPointY").InnerText);
                                                double scale = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("scale").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeInMediaEvent = new FadeInMediaEvent(dockItem, toScreenPointX, toScreenPointY, scale, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                dockItem_TL_dict.Add(beginTime, fadeInMediaEvent);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "ZoomMediaEvent")
                                            {
                                                double scale = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("scale").InnerText);
                                                double toScreenPointX = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toScreenPointX").InnerText);
                                                double toScreenPointY = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toScreenPointY").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent zoomMediaEvent = new ZoomMediaEvent(dockItem, scale, toScreenPointX, toScreenPointY, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                dockItem_TL_dict.Add(beginTime, zoomMediaEvent);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutMediaEvent")
                                            {
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutMediaEvent = new FadeOutMediaEvent(dockItem, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                dockItem_TL_dict.Add(beginTime, fadeOutMediaEvent);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "highlight")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    SurfaceInkCanvas sic = new SurfaceInkCanvas();
                                    inkCanvases.Add(sic);
                                    sic.Width = 1920;
                                    sic.Height = 1080;
                                    sic.DefaultDrawingAttributes.Width = 50;
                                    sic.DefaultDrawingAttributes.Height = 50;
                                    sic.UsesTouchShape = false;
                                    sic.Background = Brushes.Transparent;
                                    sic.Opacity = 0.7;
                                    sic.Visibility = Visibility.Collapsed;
                                    Canvas.SetZIndex(sic, 50);
                                    loadInkCanvas(sic, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + media_file);
                                    artModeWin.ImageArea.Children.Add(sic);

                                    TourParallelTL tourtl = new TourParallelTL();
                                    tourtl.type = TourTLType.highlight;
                                    tourtl.inkCanvas = sic;
                                    tourtl.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    tourtl.file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    BiDictionary<double, TourEvent> tldict = new BiDictionary<double, TourEvent>();
                                    tourBiDictionary.Add(tourtl, tldict);

                                    foreach (XmlNode TourEventNode in TLNode.ChildNodes)
                                    {
                                        if (TourEventNode.Name == "TourEvent")
                                        {
                                            if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeInHighlightEvent")
                                            {
                                                double opacity = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("opacity").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeInHighlightEvent = new FadeInHighlightEvent(sic, duration, opacity);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeInHighlightEvent);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutHighlightEvent")
                                            {
                                                double opacity = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("opacity").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutHighlightEvent = new FadeOutHighlightEvent(sic, duration, opacity);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeOutHighlightEvent);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "path")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    SurfaceInkCanvas sic = new SurfaceInkCanvas();
                                    inkCanvases.Add(sic);
                                    sic.Width = 1920;
                                    sic.Height = 1080;
                                    sic.UsesTouchShape = true;
                                    sic.DefaultDrawingAttributes.FitToCurve = true;
                                    sic.Background = Brushes.Transparent;
                                    sic.Opacity = 0;
                                    sic.Visibility = Visibility.Collapsed;
                                    Canvas.SetZIndex(sic, 50);
                                    loadInkCanvas(sic, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + media_file);
                                    artModeWin.ImageArea.Children.Add(sic);

                                    TourParallelTL tourtl = new TourParallelTL();
                                    tourtl.type = TourTLType.path;
                                    tourtl.inkCanvas = sic;
                                    tourtl.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    tourtl.file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    BiDictionary<double, TourEvent> tldict = new BiDictionary<double, TourEvent>();
                                    tourBiDictionary.Add(tourtl, tldict);

                                    foreach (XmlNode TourEventNode in TLNode.ChildNodes)
                                    {
                                        if (TourEventNode.Name == "TourEvent")
                                        {
                                            if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeInPathEvent")
                                            {
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeInPathEvent = new FadeInPathEvent(sic, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeInPathEvent);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutPathEvent")
                                            {
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutPathEvent = new FadeOutPathEvent(sic, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeOutPathEvent);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (TLNode.Name == "TourMediaTL")
                            {
                                String audio_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                TourMediaTL tourAudio_TL = new TourMediaTL(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file, UriKind.Absolute));
                                tourAudio_TL.type = TourTLType.audio;
                                tourAudio_TL.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                tourAudio_TL.file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                if (TLNode.Attributes.GetNamedItem("beginTime") != null)
                                    tourAudio_TL.BeginTime = TimeSpan.FromSeconds(Convert.ToDouble(TLNode.Attributes.GetNamedItem("beginTime").InnerText));

                                if (TLNode.Attributes.GetNamedItem("duration") != null)
                                    tourAudio_TL.Duration = TimeSpan.FromSeconds(Convert.ToDouble(TLNode.Attributes.GetNamedItem("duration").InnerText));
                                else
                                {
                                    TagLib.File audio_file_tags = TagLib.File.Create(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file);
                                    tourAudio_TL.Duration = audio_file_tags.Properties.Duration;
                                }

                                BiDictionary<double, TourEvent> tourAudio_TL_dict = new BiDictionary<double, TourEvent>(); // dummy TL_dict -- tourAudio_timeline obviously doesn't store any TourEvents
                                tourBiDictionary.Add(tourAudio_TL, tourAudio_TL_dict);

                                tourAudio_element = new MediaElement();
                                tourAudio_element.Volume = 0.99;

                                tourAudio_element.LoadedBehavior = MediaState.Manual;
                                tourAudio_element.UnloadedBehavior = MediaState.Manual;

                                Storyboard.SetTarget(tourAudio_TL, tourAudio_element);
                                tourStoryboard.SlipBehavior = SlipBehavior.Slip;

                                // took me quite a while to figure out that WPF really can't determine the duration of an MP3 until it's actually loaded (i.e. playing), and then it took me a little longer to finally find and accept this open-source library...argh

                            }
                        }
                    }
                }
            }
        }


        public void LoadDictFromXML(String xmlFileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFileName);

            if (doc.HasChildNodes)
            {
                foreach (XmlNode tourNode in doc.ChildNodes)
                {
                    if (tourNode.Name == "TourStoryboard")
                    {
                        tourStoryboard = new TourStoryboard();
                        tourStoryboard.displayName = tourNode.Attributes.GetNamedItem("displayName").InnerText;
                        tourStoryboard.description = tourNode.Attributes.GetNamedItem("description").InnerText;
                        //// Time experiment
                        tourStoryboard.totalDuration = double.Parse(tourNode.Attributes.GetNamedItem("duration").InnerText);

                        dockableItemsLoaded = new List<DockableItem>();
                        msiItemsLoaded = new List<MultiScaleImage>();
                        tourBiDictionary = new BiDictionary<Timeline, BiDictionary<double, TourEvent>>();
                        undoStack = new Stack<BiDictionary<Timeline, BiDictionary<double, TourEvent>>>();
                        redoStack = new Stack<BiDictionary<Timeline, BiDictionary<double, TourEvent>>>();
                        itemToTLDict = new Dictionary<DockableItem, Timeline>();
                        msiToTLDict = new Dictionary<MultiScaleImage, Timeline>();

                        foreach (XmlNode TLNode in tourNode.ChildNodes)
                        {
                            if (TLNode.Name == "TourParallelTL")
                            {
                                if (TLNode.Attributes.GetNamedItem("type").InnerText == "artwork")
                                {
                                    String msi_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    String msi_tour_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\DeepZoom\\" + msi_file + "\\dz.xml";
                                    artModeWin.msi_tour.SetImageSource(msi_tour_path);
                                    artModeWin.msi_tour_thumb.SetImageSource(msi_tour_path);

                                    msiItemsLoaded.Add(artModeWin.msi_tour);
                                    artModeWin.msi_tour.PreviewTouchDown += new EventHandler<TouchEventArgs>(msiTouchDown);
                                    artModeWin.msi_tour.PreviewMouseDown += new MouseButtonEventHandler(msiTouchDown);
                                    artModeWin.msi_tour.PreviewTouchMove += new EventHandler<TouchEventArgs>(msiTouchMoved);
                                    artModeWin.msi_tour.PreviewMouseMove += new MouseEventHandler(msiTouchMoved);
                                    artModeWin.msi_tour.PreviewTouchUp += new EventHandler<TouchEventArgs>(msiTouchUp);
                                    artModeWin.msi_tour.PreviewMouseUp += new MouseButtonEventHandler(msiTouchUp);
                                    artModeWin.msi_tour.PreviewMouseWheel += new MouseWheelEventHandler(msiMouseWheel);
                                    artModeWin.msi_tour.disableInertia();
                                    TourParallelTL msi_tour_TL = new TourParallelTL();

                                    msiToTLDict.Add(artModeWin.msi_tour, msi_tour_TL);


                                    msi_tour_TL.type = TourTLType.artwork;
                                    msi_tour_TL.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    msi_tour_TL.file = TLNode.Attributes.GetNamedItem("file").InnerText;

                                    BiDictionary<double, TourEvent> msi_tour_TL_dict = new BiDictionary<double, TourEvent>();
                                    tourBiDictionary.Add(msi_tour_TL, msi_tour_TL_dict);

                                    foreach (XmlNode TourEventNode in TLNode.ChildNodes)
                                    {
                                        if (TourEventNode.Name == "TourEvent")
                                        {
                                            if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "ZoomMSIEvent")
                                            {
                                                double scale = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("scale").InnerText);
                                                double toMSIPointX = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toMSIPointX").InnerText);
                                                double toMSIPointY = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toMSIPointY").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);
                                                TourEvent zoomMSIEvent = new ZoomMSIEvent(artModeWin.msi_tour, scale, toMSIPointX, toMSIPointY, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                msi_tour_TL_dict.Add(beginTime, zoomMSIEvent);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "media")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    DockableItem dockItem = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Metadata\\" + media_file);

                                    dockItem.PreviewMouseMove += new MouseEventHandler(mediaTouchDown);
                                    dockItem.PreviewTouchDown += new EventHandler<TouchEventArgs>(mediaTouchDown);
                                    dockItem.PreviewMouseDown += new MouseButtonEventHandler(mediaTouchDown);
                                    dockItem.PreviewTouchMove += new EventHandler<TouchEventArgs>(mediaTouchMoved);
                                    dockItem.PreviewMouseMove += new MouseEventHandler(mediaTouchMoved);
                                    dockItem.PreviewTouchUp += new EventHandler<TouchEventArgs>(mediaTouchUp);
                                    dockItem.PreviewMouseUp += new MouseButtonEventHandler(mediaTouchUp);

                                    dockableItemsLoaded.Add(dockItem);
                                    dockItem.Visibility = Visibility.Hidden;

                                    TourParallelTL dockItem_TL = new TourParallelTL();
                                    dockItem_TL.type = TourTLType.media;

                                    dockItem_TL.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    dockItem_TL.file = TLNode.Attributes.GetNamedItem("file").InnerText;

                                    itemToTLDict.Add(dockItem, dockItem_TL);

                                    BiDictionary<double, TourEvent> dockItem_TL_dict = new BiDictionary<double, TourEvent>();
                                    tourBiDictionary.Add(dockItem_TL, dockItem_TL_dict);

                                    foreach (XmlNode TourEventNode in TLNode.ChildNodes)
                                    {
                                        if (TourEventNode.Name == "TourEvent")
                                        {
                                            if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeInMediaEvent")
                                            {
                                                double toScreenPointX = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toScreenPointX").InnerText);
                                                double toScreenPointY = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toScreenPointY").InnerText);
                                                double scale = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("scale").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeInMediaEvent = new FadeInMediaEvent(dockItem, toScreenPointX, toScreenPointY, scale, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                dockItem_TL_dict.Add(beginTime, fadeInMediaEvent);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "ZoomMediaEvent")
                                            {
                                                double scale = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("scale").InnerText);
                                                double toScreenPointX = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toScreenPointX").InnerText);
                                                double toScreenPointY = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("toScreenPointY").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent zoomMediaEvent = new ZoomMediaEvent(dockItem, scale, toScreenPointX, toScreenPointY, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                dockItem_TL_dict.Add(beginTime, zoomMediaEvent);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutMediaEvent")
                                            {
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutMediaEvent = new FadeOutMediaEvent(dockItem, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                dockItem_TL_dict.Add(beginTime, fadeOutMediaEvent);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "highlight")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    SurfaceInkCanvas sic = new SurfaceInkCanvas();
                                    inkCanvases.Add(sic);
                                    sic.Width = 1920;
                                    sic.Height = 1080;
                                    sic.DefaultDrawingAttributes.Width = 50;
                                    sic.DefaultDrawingAttributes.Height = 50;
                                    sic.UsesTouchShape = false;
                                    sic.Background = Brushes.Transparent;
                                    sic.Opacity = 0.7;
                                    sic.Visibility = Visibility.Collapsed;
                                    Canvas.SetZIndex(sic, 50);
                                    loadInkCanvas(sic, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + media_file);
                                    artModeWin.ImageArea.Children.Add(sic);

                                    TourParallelTL tourtl = new TourParallelTL();
                                    tourtl.type = TourTLType.highlight;
                                    tourtl.inkCanvas = sic;
                                    tourtl.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    tourtl.file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    BiDictionary<double, TourEvent> tldict = new BiDictionary<double, TourEvent>();
                                    tourBiDictionary.Add(tourtl, tldict);

                                    foreach (XmlNode TourEventNode in TLNode.ChildNodes)
                                    {
                                        if (TourEventNode.Name == "TourEvent")
                                        {
                                            if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeInHighlightEvent")
                                            {
                                                double opacity = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("opacity").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeInHighlightEvent = new FadeInHighlightEvent(sic, duration, opacity);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeInHighlightEvent);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutHighlightEvent")
                                            {
                                                double opacity = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("opacity").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutHighlightEvent = new FadeOutHighlightEvent(sic, duration, opacity);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeOutHighlightEvent);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "path")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    SurfaceInkCanvas sic = new SurfaceInkCanvas();
                                    inkCanvases.Add(sic);
                                    sic.Width = 1920;
                                    sic.Height = 1080;
                                    sic.UsesTouchShape = true;
                                    sic.DefaultDrawingAttributes.FitToCurve = true;
                                    sic.Background = Brushes.Transparent;
                                    sic.Opacity = 0;
                                    sic.Visibility = Visibility.Collapsed;
                                    Canvas.SetZIndex(sic, 50);
                                    loadInkCanvas(sic, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + media_file);
                                    artModeWin.ImageArea.Children.Add(sic);

                                    TourParallelTL tourtl = new TourParallelTL();
                                    tourtl.type = TourTLType.path;
                                    tourtl.inkCanvas = sic;
                                    tourtl.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    tourtl.file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    BiDictionary<double, TourEvent> tldict = new BiDictionary<double, TourEvent>();
                                    tourBiDictionary.Add(tourtl, tldict);

                                    foreach (XmlNode TourEventNode in TLNode.ChildNodes)
                                    {
                                        if (TourEventNode.Name == "TourEvent")
                                        {
                                            if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeInPathEvent")
                                            {
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeInPathEvent = new FadeInPathEvent(sic, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeInPathEvent);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutPathEvent")
                                            {
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutPathEvent = new FadeOutPathEvent(sic, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeOutPathEvent);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (TLNode.Name == "TourMediaTL")
                            {
                                String audio_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                TourMediaTL tourAudio_TL = new TourMediaTL(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file, UriKind.Absolute));
                                tourAudio_TL.type = TourTLType.audio;
                                tourAudio_TL.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                tourAudio_TL.file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                if (TLNode.Attributes.GetNamedItem("beginTime") != null)
                                    tourAudio_TL.BeginTime = TimeSpan.FromSeconds(Convert.ToDouble(TLNode.Attributes.GetNamedItem("beginTime").InnerText));

                                if (TLNode.Attributes.GetNamedItem("duration") != null)
                                    tourAudio_TL.Duration = TimeSpan.FromSeconds(Convert.ToDouble(TLNode.Attributes.GetNamedItem("duration").InnerText));
                                else
                                {
                                    TagLib.File audio_file_tags = TagLib.File.Create(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file);
                                    tourAudio_TL.Duration = audio_file_tags.Properties.Duration;
                                }

                                BiDictionary<double, TourEvent> tourAudio_TL_dict = new BiDictionary<double, TourEvent>(); // dummy TL_dict -- tourAudio_timeline obviously doesn't store any TourEvents
                                tourBiDictionary.Add(tourAudio_TL, tourAudio_TL_dict);

                                tourAudio_element = new MediaElement();
                                tourAudio_element.Volume = 0.99;

                                tourAudio_element.LoadedBehavior = MediaState.Manual;
                                tourAudio_element.UnloadedBehavior = MediaState.Manual;

                                Storyboard.SetTarget(tourAudio_TL, tourAudio_element);
                                tourStoryboard.SlipBehavior = SlipBehavior.Slip;

                                // took me quite a while to figure out that WPF really can't determine the duration of an MP3 until it's actually loaded (i.e. playing), and then it took me a little longer to finally find and accept this open-source library...argh

                            }
                        }
                    }
                }
            }
        }

        // Given two (presumably identical) assets, scale, rotate, and translate one (first) so that it is
        // in the same position as the other (second).
        private void alignAssets(DockableItem to_position, DockableItem in_position)
        {
            to_position.Height = in_position.Height;
            to_position.Width = in_position.Width;
            to_position.Center = in_position.Center;
            to_position.Orientation = in_position.Orientation;
        }

        public void SaveDictToXML(String xmlFileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlWhitespace xml_declaration_newLine = doc.CreateWhitespace("\r\n");
            doc.InsertAfter(xml_declaration_newLine, docNode);

            XmlNode tourNode = doc.CreateElement("TourStoryboard");
            XmlAttribute tourNode_displayName = doc.CreateAttribute("displayName");
            XmlAttribute tourNode_description = doc.CreateAttribute("description");

            //// duratoin experiment
            XmlAttribute tourNode_duration = doc.CreateAttribute("duration");
            tourNode_duration.Value = tourStoryboard.totalDuration.ToString();
            tourNode.Attributes.Append(tourNode_duration);

            tourNode_displayName.Value = tourStoryboard.displayName;
            tourNode_description.Value = tourStoryboard.description;
            tourNode.Attributes.Append(tourNode_displayName);
            tourNode.Attributes.Append(tourNode_description);
            doc.AppendChild(tourNode);

            foreach (Timeline tl in tourBiDictionary.firstKeys)
            {
                TourTL tourTL = (TourTL)tl;
                BiDictionary<double, TourEvent> tourTL_dict = tourBiDictionary[tl][0];

                if ((tourTL.type == TourTLType.artwork) || (tourTL.type == TourTLType.media) || tourTL.type == TourTLType.highlight || tourTL.type == TourTLType.path)
                {
                    XmlNode TLNode = doc.CreateElement("TourParallelTL");
                    XmlAttribute TLNode_type = doc.CreateAttribute("type");
                    XmlAttribute TLNode_displayName = doc.CreateAttribute("displayName");
                    XmlAttribute TLNode_file = doc.CreateAttribute("file");
                    TLNode_type.Value = tourTL.type.ToString();
                    TLNode_displayName.Value = tourTL.displayName;
                    TLNode_file.Value = tourTL.file;
                    TLNode.Attributes.Append(TLNode_type);
                    TLNode.Attributes.Append(TLNode_displayName);
                    TLNode.Attributes.Append(TLNode_file);
                    tourNode.AppendChild(TLNode);

                    foreach (double beginTime in tourTL_dict.firstKeys)
                    {
                        TourEvent tourEvent = tourTL_dict[beginTime][0];

                        if (tourEvent.type == TourEvent.Type.zoomMSI)
                        {
                            ZoomMSIEvent zoomMSIEvent = (ZoomMSIEvent)tourEvent;

                            XmlNode TourEventNode = doc.CreateElement("TourEvent");
                            XmlAttribute TourEventNode_beginTime = doc.CreateAttribute("beginTime");
                            XmlAttribute TourEventNode_type = doc.CreateAttribute("type");
                            XmlAttribute TourEventNode_scale = doc.CreateAttribute("scale");
                            XmlAttribute TourEventNode_toMSIPointX = doc.CreateAttribute("toMSIPointX");
                            XmlAttribute TourEventNode_toMSIPointY = doc.CreateAttribute("toMSIPointY");
                            XmlAttribute TourEventNode_duration = doc.CreateAttribute("duration");
                            TourEventNode_beginTime.Value = beginTime.ToString();
                            TourEventNode_type.Value = "ZoomMSIEvent";
                            TourEventNode_scale.Value = zoomMSIEvent.absoluteScale.ToString();
                            TourEventNode_toMSIPointX.Value = zoomMSIEvent.zoomToMSIPointX.ToString();
                            TourEventNode_toMSIPointY.Value = zoomMSIEvent.zoomToMSIPointY.ToString();
                            TourEventNode_duration.Value = zoomMSIEvent.duration.ToString();
                            TourEventNode.Attributes.Append(TourEventNode_beginTime);
                            TourEventNode.Attributes.Append(TourEventNode_type);
                            TourEventNode.Attributes.Append(TourEventNode_scale);
                            TourEventNode.Attributes.Append(TourEventNode_toMSIPointX);
                            TourEventNode.Attributes.Append(TourEventNode_toMSIPointY);
                            TourEventNode.Attributes.Append(TourEventNode_duration);
                            TLNode.AppendChild(TourEventNode);
                        }

                        else if (tourEvent.type == TourEvent.Type.fadeInMedia)
                        {
                            FadeInMediaEvent fadeInMediaEvent = (FadeInMediaEvent)tourEvent;

                            XmlNode TourEventNode = doc.CreateElement("TourEvent");
                            XmlAttribute TourEventNode_beginTime = doc.CreateAttribute("beginTime");
                            XmlAttribute TourEventNode_type = doc.CreateAttribute("type");
                            XmlAttribute TourEventNode_toScreenPointX = doc.CreateAttribute("toScreenPointX");
                            XmlAttribute TourEventNode_toScreenPointY = doc.CreateAttribute("toScreenPointY");
                            XmlAttribute TourEventNode_scale = doc.CreateAttribute("scale");
                            XmlAttribute TourEventNode_duration = doc.CreateAttribute("duration");
                            TourEventNode_beginTime.Value = beginTime.ToString();
                            TourEventNode_type.Value = "FadeInMediaEvent";
                            TourEventNode_toScreenPointX.Value = fadeInMediaEvent.fadeInMediaToScreenPointX.ToString();
                            TourEventNode_toScreenPointY.Value = fadeInMediaEvent.fadeInMediaToScreenPointY.ToString();
                            TourEventNode_scale.Value = fadeInMediaEvent.absoluteScale.ToString();
                            TourEventNode_duration.Value = fadeInMediaEvent.duration.ToString();
                            TourEventNode.Attributes.Append(TourEventNode_beginTime);
                            TourEventNode.Attributes.Append(TourEventNode_type);
                            TourEventNode.Attributes.Append(TourEventNode_toScreenPointX);
                            TourEventNode.Attributes.Append(TourEventNode_toScreenPointY);
                            TourEventNode.Attributes.Append(TourEventNode_scale);
                            TourEventNode.Attributes.Append(TourEventNode_duration);
                            TLNode.AppendChild(TourEventNode);
                        }

                        else if (tourEvent.type == TourEvent.Type.fadeOutMedia)
                        {
                            FadeOutMediaEvent fadeOutMediaEvent = (FadeOutMediaEvent)tourEvent;

                            XmlNode TourEventNode = doc.CreateElement("TourEvent");
                            XmlAttribute TourEventNode_beginTime = doc.CreateAttribute("beginTime");
                            XmlAttribute TourEventNode_type = doc.CreateAttribute("type");
                            XmlAttribute TourEventNode_duration = doc.CreateAttribute("duration");
                            TourEventNode_beginTime.Value = beginTime.ToString();
                            TourEventNode_type.Value = "FadeOutMediaEvent";
                            TourEventNode_duration.Value = fadeOutMediaEvent.duration.ToString();
                            TourEventNode.Attributes.Append(TourEventNode_beginTime);
                            TourEventNode.Attributes.Append(TourEventNode_type);
                            TourEventNode.Attributes.Append(TourEventNode_duration);
                            TLNode.AppendChild(TourEventNode);
                        }

                        else if (tourEvent.type == TourEvent.Type.zoomMedia)
                        {
                            ZoomMediaEvent zoomMediaEvent = (ZoomMediaEvent)tourEvent;

                            XmlNode TourEventNode = doc.CreateElement("TourEvent");
                            XmlAttribute TourEventNode_beginTime = doc.CreateAttribute("beginTime");
                            XmlAttribute TourEventNode_type = doc.CreateAttribute("type");
                            XmlAttribute TourEventNode_scale = doc.CreateAttribute("scale");
                            XmlAttribute TourEventNode_toScreenPointX = doc.CreateAttribute("toScreenPointX");
                            XmlAttribute TourEventNode_toScreenPointY = doc.CreateAttribute("toScreenPointY");
                            XmlAttribute TourEventNode_duration = doc.CreateAttribute("duration");
                            TourEventNode_beginTime.Value = beginTime.ToString();
                            TourEventNode_type.Value = "ZoomMediaEvent";
                            TourEventNode_scale.Value = zoomMediaEvent.absoluteScale.ToString();
                            TourEventNode_toScreenPointX.Value = zoomMediaEvent.zoomMediaToScreenPointX.ToString();
                            TourEventNode_toScreenPointY.Value = zoomMediaEvent.zoomMediaToScreenPointY.ToString();
                            TourEventNode_duration.Value = zoomMediaEvent.duration.ToString();
                            TourEventNode.Attributes.Append(TourEventNode_beginTime);
                            TourEventNode.Attributes.Append(TourEventNode_type);
                            TourEventNode.Attributes.Append(TourEventNode_scale);
                            TourEventNode.Attributes.Append(TourEventNode_toScreenPointX);
                            TourEventNode.Attributes.Append(TourEventNode_toScreenPointY);
                            TourEventNode.Attributes.Append(TourEventNode_duration);
                            TLNode.AppendChild(TourEventNode);
                        }

                        else if (tourEvent.type == TourEvent.Type.fadeInHighlight)
                        {
                            FadeInHighlightEvent fadeInHighlightEvent = (FadeInHighlightEvent)tourEvent;

                            XmlNode TourEventNode = doc.CreateElement("TourEvent");
                            XmlAttribute TourEventNode_beginTime = doc.CreateAttribute("beginTime");
                            XmlAttribute TourEventNode_type = doc.CreateAttribute("type");
                            XmlAttribute TourEventNode_duration = doc.CreateAttribute("duration");
                            XmlAttribute TourEventNode_opacity = doc.CreateAttribute("opacity");
                            TourEventNode_beginTime.Value = beginTime.ToString();
                            TourEventNode_type.Value = "FadeInHighlightEvent";
                            TourEventNode_duration.Value = fadeInHighlightEvent.duration.ToString();
                            TourEventNode_opacity.Value = fadeInHighlightEvent.opacity.ToString();
                            TourEventNode.Attributes.Append(TourEventNode_beginTime);
                            TourEventNode.Attributes.Append(TourEventNode_type);
                            TourEventNode.Attributes.Append(TourEventNode_duration);
                            TourEventNode.Attributes.Append(TourEventNode_opacity);
                            TLNode.AppendChild(TourEventNode);
                        }

                        else if (tourEvent.type == TourEvent.Type.fadeOutHighlight)
                        {
                            FadeOutHighlightEvent fadeOutHighlightEvent = (FadeOutHighlightEvent)tourEvent;

                            XmlNode TourEventNode = doc.CreateElement("TourEvent");
                            XmlAttribute TourEventNode_beginTime = doc.CreateAttribute("beginTime");
                            XmlAttribute TourEventNode_type = doc.CreateAttribute("type");
                            XmlAttribute TourEventNode_duration = doc.CreateAttribute("duration");
                            XmlAttribute TourEventNode_opacity = doc.CreateAttribute("opacity");
                            TourEventNode_beginTime.Value = beginTime.ToString();
                            TourEventNode_type.Value = "FadeOutHighlightEvent";
                            TourEventNode_duration.Value = fadeOutHighlightEvent.duration.ToString();
                            TourEventNode_opacity.Value = fadeOutHighlightEvent.opacity.ToString();
                            TourEventNode.Attributes.Append(TourEventNode_beginTime);
                            TourEventNode.Attributes.Append(TourEventNode_type);
                            TourEventNode.Attributes.Append(TourEventNode_duration);
                            TourEventNode.Attributes.Append(TourEventNode_opacity);
                            TLNode.AppendChild(TourEventNode);
                        }

                        else if (tourEvent.type == TourEvent.Type.fadeInPath)
                        {
                            FadeInPathEvent fadeInPathEvent = (FadeInPathEvent)tourEvent;

                            XmlNode TourEventNode = doc.CreateElement("TourEvent");
                            XmlAttribute TourEventNode_beginTime = doc.CreateAttribute("beginTime");
                            XmlAttribute TourEventNode_type = doc.CreateAttribute("type");
                            XmlAttribute TourEventNode_duration = doc.CreateAttribute("duration");
                            TourEventNode_beginTime.Value = beginTime.ToString();
                            TourEventNode_type.Value = "FadeInPathEvent";
                            TourEventNode_duration.Value = fadeInPathEvent.duration.ToString();
                            TourEventNode.Attributes.Append(TourEventNode_beginTime);
                            TourEventNode.Attributes.Append(TourEventNode_type);
                            TourEventNode.Attributes.Append(TourEventNode_duration);
                            TLNode.AppendChild(TourEventNode);
                        }

                        else if (tourEvent.type == TourEvent.Type.fadeOutPath)
                        {
                            FadeOutPathEvent fadeOutPathEvent = (FadeOutPathEvent)tourEvent;

                            XmlNode TourEventNode = doc.CreateElement("TourEvent");
                            XmlAttribute TourEventNode_beginTime = doc.CreateAttribute("beginTime");
                            XmlAttribute TourEventNode_type = doc.CreateAttribute("type");
                            XmlAttribute TourEventNode_duration = doc.CreateAttribute("duration");
                            TourEventNode_beginTime.Value = beginTime.ToString();
                            TourEventNode_type.Value = "FadeOutPathEvent";
                            TourEventNode_duration.Value = fadeOutPathEvent.duration.ToString();
                            TourEventNode.Attributes.Append(TourEventNode_beginTime);
                            TourEventNode.Attributes.Append(TourEventNode_type);
                            TourEventNode.Attributes.Append(TourEventNode_duration);
                            TLNode.AppendChild(TourEventNode);
                        }
                    }
                }

                else if (tourTL.type == TourTLType.audio)
                {
                    XmlNode TLNode = doc.CreateElement("TourMediaTL");
                    XmlAttribute TLNode_type = doc.CreateAttribute("type");
                    XmlAttribute TLNode_displayName = doc.CreateAttribute("displayName");
                    XmlAttribute TLNode_file = doc.CreateAttribute("file");
                    XmlAttribute TLNode_beginTime = doc.CreateAttribute("beginTime");
                    XmlAttribute TLNode_duration = doc.CreateAttribute("duration");
                    TLNode_type.Value = tourTL.type.ToString();
                    TLNode_displayName.Value = tourTL.displayName;
                    TLNode_file.Value = tourTL.file;
                    TLNode_beginTime.Value = ((TourMediaTL)tourTL).BeginTime.Value.TotalSeconds.ToString();
                    TLNode_duration.Value = ((TourMediaTL)tourTL).Duration.TimeSpan.TotalSeconds.ToString();
                    TLNode.Attributes.Append(TLNode_type);
                    TLNode.Attributes.Append(TLNode_displayName);
                    TLNode.Attributes.Append(TLNode_file);
                    TLNode.Attributes.Append(TLNode_beginTime);
                    TLNode.Attributes.Append(TLNode_duration);
                    tourNode.AppendChild(TLNode);
                }
            }

            doc.Save(xmlFileName);
        }

        #endregion

        #region (STILL UNDER DEVELOPMENT) tourDict <--> storyboard for playback during playback/authoring modes

        public bool resetTourLength(double newDuration)
        {
            tourAuthoringUI.reloadUI();
            double tourStoryboard_endTime = 0.0;

            foreach (Timeline tourTL in tourBiDictionary.firstKeys)
            {
                BiDictionary<double, TourEvent> tourTL_dict = tourBiDictionary[tourTL][0];

                double tourTL_endTime = 0.0;

                foreach (double beginTime in tourTL_dict.firstKeys) // MediaTimeline will ignore this
                {
                    TourEvent tourEvent = tourTL_dict[beginTime][0];

                    double tourEvent_endTime = beginTime + tourEvent.duration;

                    if (tourEvent_endTime > tourTL_endTime)
                    {
                        tourTL_endTime = tourEvent_endTime;
                    }
                }

                if (((TourTL)tourTL).type == TourTLType.audio)
                {
                    TourMediaTL mediaTimeline = tourTL as TourMediaTL;
                    tourTL_endTime = mediaTimeline.BeginTime.Value.TotalSeconds + mediaTimeline.Duration.TimeSpan.TotalSeconds;
                }


                if (tourTL_endTime > tourStoryboard_endTime)
                {
                    tourStoryboard_endTime = tourTL_endTime;

                }

            }
            if (newDuration >= tourStoryboard_endTime)
            {
                tourStoryboard.totalDuration = newDuration;
                tourStoryboard.Duration = TimeSpan.FromSeconds(tourStoryboard.totalDuration);
                tourAuthoringUI.reloadUI();
                return true;
            }
            return false;



        }

        public void LoadTourPlaybackFromDict()
        {
            tourStoryboard.Children.Clear(); // clear storyboard - necessary when editing

            double tourStoryboard_endTime = 0.0;

            foreach (Timeline tourTL in tourBiDictionary.firstKeys)
            {
                BiDictionary<double, TourEvent> tourTL_dict = tourBiDictionary[tourTL][0];

                if ((((TourTL)tourTL).type == TourTLType.artwork) || (((TourTL)tourTL).type == TourTLType.media) || (((TourTL)tourTL).type == TourTLType.path) || (((TourTL)tourTL).type == TourTLType.highlight))
                {
                    ((TourParallelTL)tourTL).Children.Clear(); // clear timelines - necessary when editing
                }

                double tourTL_endTime = 0.0;

                List<double> keys = new List<double>(tourTL_dict.firstKeys);
                keys.Sort();

                foreach (double beginTime in keys)
                {
                    TourEvent tourEvent = tourTL_dict[beginTime][0];
                    double tourEvent_endTime = beginTime + tourEvent.duration;

                    if (tourEvent_endTime > tourTL_endTime)
                    {
                        tourTL_endTime = tourEvent_endTime;
                    }

                    this.addAnim((TourParallelTL)tourTL, tourEvent, beginTime);
                }

                if (!tourTL.Duration.HasTimeSpan) // only MediaTimeline will fail this
                {
                    tourTL.Duration = TimeSpan.FromSeconds(tourStoryboard.totalDuration);

                }

                if (tourTL.Duration.TimeSpan.TotalSeconds > tourStoryboard_endTime)
                {
                    tourStoryboard_endTime = tourTL.Duration.TimeSpan.TotalSeconds;
                }

                tourStoryboard.Children.Add(tourTL);
            }

            tourStoryboard.Duration = TimeSpan.FromSeconds(tourStoryboard.totalDuration);
        }

        public void LoadTourAuthoringUIFromDict()
        {
            tourAuthoringUI = new TourAuthoringUI(artModeWin, this);

            tourAuthoringUI.timelineCount = tourBiDictionary.firstKeys.Count; //clear the old yellow moveable thing

            tourAuthoringUI.ClearAuthoringUI();

            tourAuthoringUI.timelineCount = tourBiDictionary.firstKeys.Count;

            tourAuthoringUI.timelineLength = tourStoryboard.Duration.TimeSpan.TotalSeconds;

            tourAuthoringUI.canvasWrapper.Children.Clear();
            tourAuthoringUI.initialize();
            tourAuthoringUI.initAuthTools();

            int i = 0;
            foreach (Timeline tourTL in tourBiDictionary.firstKeys)
            {
                BiDictionary<double, TourEvent> tourTL_dict = tourBiDictionary[tourTL][0];

                TourAuthoringUI.timelineInfo timelineInfo = tourAuthoringUI.addTimeline(tourTL, tourTL_dict, ((TourTL)tourTL).displayName, i * tourAuthoringUI.timelineHeight);

                foreach (double beginTime in tourTL_dict.firstKeys) // MediaTimeline will ignore this
                {
                    TourEvent tourEvent = tourTL_dict[beginTime][0];
                    tourAuthoringUI.addTourEvent(timelineInfo, tourEvent, timelineInfo.lengthSV, beginTime, tourEvent.duration);
                }

                if (((TourTL)tourTL).type == TourTLType.audio) // for MediaTimeline
                {
                    TourMediaTL mediaTL = tourTL as TourMediaTL;
                    double mediaBeginTime = 0;
                    if (mediaTL.BeginTime.HasValue)
                    {
                        mediaBeginTime = mediaTL.BeginTime.Value.TotalSeconds;
                    }

                    tourAuthoringUI.addAudioEvent(timelineInfo, null, timelineInfo.lengthSV, mediaBeginTime, tourTL.Duration.TimeSpan.TotalSeconds); // will this work?
                }

                i++;
            }
        }



        public void refreshAuthoringUI(bool completeReload)
        {
            msiToTLDict.Clear();
            itemToTLDict.Clear();
            tourAuthoringUI.timelineCount = tourBiDictionary.firstKeys.Count; //clear the old yellow moveable thing

            Point center = tourAuthoringUI.leftRightSVI.ActualCenter;
            if (!completeReload)
                tourAuthoringUI.ClearTimelines();
            else
                tourAuthoringUI.ClearAuthoringUI();

            tourAuthoringUI.timelineCount = tourBiDictionary.firstKeys.Count;
            tourAuthoringUI.timelineLength = tourStoryboard.Duration.TimeSpan.TotalSeconds;

            if (!completeReload)
                tourAuthoringUI.reinitalize();
            else
                tourAuthoringUI.initialize();
            int i = 0;
            foreach (Timeline tourTL in tourBiDictionary.firstKeys)
            {
                BiDictionary<double, TourEvent> tourTL_dict = tourBiDictionary[tourTL][0];

                TourAuthoringUI.timelineInfo timelineInfo = tourAuthoringUI.addTimeline(tourTL, tourTL_dict, ((TourTL)tourTL).displayName, i * tourAuthoringUI.timelineHeight);
                foreach (double beginTime in tourTL_dict.firstKeys)
                {
                    TourEvent tourEvent = tourTL_dict[beginTime][0];
                    tourAuthoringUI.addTourEvent(timelineInfo, tourEvent, timelineInfo.lengthSV, beginTime, tourEvent.duration);
                }

                if (((TourTL)tourTL).type == TourTLType.audio)
                {
                    TourMediaTL mediaTL = tourTL as TourMediaTL;
                    double mediaBeginTime = 0;
                    if (mediaTL.BeginTime.HasValue)
                    {
                        mediaBeginTime = mediaTL.BeginTime.Value.TotalSeconds;
                    }

                    tourAuthoringUI.addAudioEvent(timelineInfo, null, timelineInfo.lengthSV, mediaBeginTime, tourTL.Duration.TimeSpan.TotalSeconds); // will this work?

                    if (Storyboard.GetTarget(mediaTL) == null)
                    {
                        
                        tourAudio_element = new MediaElement();
                        tourAudio_element.Volume = 0.99;

                        tourAudio_element.LoadedBehavior = MediaState.Manual;
                        tourAudio_element.UnloadedBehavior = MediaState.Manual;

                        Storyboard.SetTarget(mediaTL, tourAudio_element);
                        tourStoryboard.SlipBehavior = SlipBehavior.Slip;
                    }
                }

                i++;
            }
            tourAuthoringUI.EnableDrawingIfNeeded();
        }

        public void StopAndReloadTourAuthoringUIFromDict(double beginTime)
        {

            tourStoryboard.CurrentTimeInvalidated -= new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_CurrentTimeInvalidated);
            tourStoryboard.Completed -= new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_Completed);
            tourStoryboard.Stop(artModeWin);

            this.LoadTourPlaybackFromDict();

            tourStoryboard.CurrentTimeInvalidated += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_CurrentTimeInvalidated);
            tourStoryboard.Completed += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_Completed);
            tourStoryboard.Begin(artModeWin, true);
            tourStoryboard.Pause(artModeWin);


            tourStoryboard.CurrentTimeInvalidated -= new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_CurrentTimeInvalidated);
            tourStoryboard.Seek(artModeWin, TimeSpan.FromSeconds(beginTime), TimeSeekOrigin.BeginTime);
            tourStoryboard.CurrentTimeInvalidated += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_CurrentTimeInvalidated);
        }

        #endregion
    }
}
