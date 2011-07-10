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
    /// TourSystem - tour authoring & playback system (see http://cs.brown.edu/research/pubs/theses/masters/2011/chin.pdf for what I was envisioning for this)
    /// </summary>
    class TourSystem
    {
        private ArtworkModeWindow artModeWin;

        public bool tourPlaybackOn;
        public bool tourAuthoringOn;
        private bool drawingON;

        public Dictionary<Timeline, Dictionary<double, TourEvent>> tourDict;
        public BiDictionary<Timeline, BiDictionary<double, TourEvent>> tourBiDictionary;
        public Dictionary<Timeline, Dictionary<TourEvent, double>> tourDictRev;
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

            /* HOTSPOTS - disabled during tour playback */
            /*double HotspotOverlay_centerX = (artModeWin.msi_tour.GetZoomableCanvas.Scale * artModeWin.msi_tour.GetImageActualWidth * 0.5) - artModeWin.msi_tour.GetZoomableCanvas.Offset.X;
            double HotspotOverlay_centerY = (artModeWin.msi_tour.GetZoomableCanvas.Scale * artModeWin.msi_tour.GetImageActualHeight * 0.5) - artModeWin.msi_tour.GetZoomableCanvas.Offset.Y;

            double msi_clip_centerX = artModeWin.msi_tour.GetZoomableCanvas.ActualWidth * 0.5;
            double msi_clip_centerY = artModeWin.msi_tour.GetZoomableCanvas.ActualHeight * 0.5;

            double HotspotOverlay_centerX_dist = HotspotOverlay_centerX - msi_clip_centerX;
            double HotspotOverlay_centerY_dist = HotspotOverlay_centerY - msi_clip_centerY;

            artModeWin.m_hotspotCollection.updateHotspotLocations(artModeWin.HotspotOverlay, artModeWin.MSIScatterView, artModeWin.msi_tour);*/
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

            /* HOTSPOTS - disabled during tour playback */
            /*double HotspotOverlay_centerX = (artModeWin.msi_tour.GetZoomableCanvas.Scale * artModeWin.msi_tour.GetImageActualWidth * 0.5) - artModeWin.msi_tour.GetZoomableCanvas.Offset.X;
            double HotspotOverlay_centerY = (artModeWin.msi_tour.GetZoomableCanvas.Scale * artModeWin.msi_tour.GetImageActualHeight * 0.5) - artModeWin.msi_tour.GetZoomableCanvas.Offset.Y;

            double msi_clip_centerX = artModeWin.msi_tour.GetZoomableCanvas.ActualWidth * 0.5;
            double msi_clip_centerY = artModeWin.msi_tour.GetZoomableCanvas.ActualHeight * 0.5;

            double HotspotOverlay_centerX_dist = HotspotOverlay_centerX - msi_clip_centerX;
            double HotspotOverlay_centerY_dist = HotspotOverlay_centerY - msi_clip_centerY;

            artModeWin.m_hotspotCollection.updateHotspotLocations(artModeWin.HotspotOverlay, artModeWin.MSIScatterView, artModeWin.msi_tour);*/
        }

        #endregion

        #region drawing utils

        public void drawPaths_Click(object sender, RoutedEventArgs e)
        {
            if (tourAuthoringOn)
            {
                MakeNewPathCanvas();
            }
        }
        /*
        public void drawVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                (sender as SurfaceInkCanvas).IsHitTestVisible = false;
            }
            else
            {
                if ((sender as SurfaceInkCanvas).Opacity == 0)
                {

                    (sender as SurfaceInkCanvas).IsHitTestVisible = false;
                }
                else (sender as SurfaceInkCanvas).IsHitTestVisible = true;
            }
        }*/

        public void drawHighlight_Click(object sender, RoutedEventArgs e)
        {
            if (tourAuthoringOn)
            {
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
            //currentHighlightCanvas.IsVisibleChanged += new DependencyPropertyChangedEventHandler(drawVisibilityChanged);
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

            //TourEvent startOut = new FadeOutHighlightEvent(currentHighlightCanvas, 1, 0.7);
            //startOut.type = TourEvent.Type.fadeOutHighlight;

            TourParallelTL tourtl = new TourParallelTL();
            tourtl.type = TourTLType.highlight;
            tourtl.inkCanvas = currentHighlightCanvas;
            tourtl.displayName = "Highlight";
            tourtl.file = getNextFile("Highlight");
            currentHighlightCanvasFile = tourtl.file;
            Dictionary<double, TourEvent> tldict = new Dictionary<double, TourEvent>();
            double start = authorTimerCountSpan.TotalSeconds;
            if (start == 0) start = 1;
            tldict.Add(start - 1, fadeIn);
            tldict.Add(start + 1, fadeOut);
            //tldict.Add(-2, startOut);
            tourDict.Add(tourtl, tldict);
            Dictionary<TourEvent, double> tldictrev = new Dictionary<TourEvent, double>();
            tldictrev.Add(fadeIn, start - 1);
            tldictrev.Add(fadeOut, start + 1);
            //tldictrev.Add(startOut,-2);

            tourDictRev.Add(tourtl, tldictrev);
            //tourAuthoringUI.addTimelineAndEventPostInit(tourtl, tldict, "Highlight", fadeIn, authorTimerCountSpan.TotalSeconds - 1, 1);
            //tourAuthoringUI.insertTourEvent(fadeOut, tourtl, authorTimerCountSpan.TotalSeconds);
            tourAuthoringUI.refreshUI();
        }


        public void MakeNewPathCanvas()
        {
            currentPathCanvas = new SurfaceInkCanvas();
            currentPathCanvas.DefaultDrawingAttributes.FitToCurve = true;
            //currentPathCanvas.IsVisibleChanged += new DependencyPropertyChangedEventHandler(drawVisibilityChanged);
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

            //TourEvent startOut = new FadeOutPathEvent(currentPathCanvas, 1);
            //startOut.type = TourEvent.Type.fadeOutPath;
            TourParallelTL tourtl = new TourParallelTL();
            tourtl.type = TourTLType.path;
            tourtl.inkCanvas = currentPathCanvas;
            tourtl.displayName = "Path";
            tourtl.file = getNextFile("Path");
            currentPathCanvasFile = tourtl.file;
            Dictionary<double, TourEvent> tldict = new Dictionary<double, TourEvent>();
            double start = authorTimerCountSpan.TotalSeconds;
            if (start == 0) start = 1;
            tldict.Add(start - 1, fadeIn);
            tldict.Add(start + 1, fadeOut);
            tourDict.Add(tourtl, tldict);
            Dictionary<TourEvent, double> tldictrev = new Dictionary<TourEvent, double>();
            tldictrev.Add(fadeIn, start - 1);
            tldictrev.Add(fadeOut, start + 1);
            tourDictRev.Add(tourtl, tldictrev);
            //tourAuthoringUI.addTimelineAndEventPostInit(tourtl, tldict, "Path", fadeIn, authorTimerCountSpan.TotalSeconds - 1, 1);
            //tourAuthoringUI.insertTourEvent(fadeOut, tourtl, authorTimerCountSpan.TotalSeconds);
            //StopAndReloadTourAuthoringUIFromDict(authorTimerCountSpan.TotalSeconds);
            tourAuthoringUI.refreshUI();
        }

        public void changeCanvas(bool isPath, SurfaceInkCanvas canvas, string file)
        {
            if (isPath)
            {
                currentPathCanvas = canvas;
                currentPathCanvasFile = file;
                return;
            }
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
            foreach (Timeline timeline in tourDict.Keys)
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
                //currentHighlightCanvas.Opacity = opacity;
                foreach (Timeline tl in tourDict.Keys)
                {
                    TourTL tourtl = (TourTL)tl;
                    if (tourtl.type == TourTLType.highlight && ((TourParallelTL)tourtl).inkCanvas == currentHighlightCanvas)
                    {
                        Dictionary<double, TourEvent> dict = tourDict[tl];
                        foreach (TourEvent te in dict.Values)
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

            Dictionary<double, TourEvent> tourAudio_TL_dict = new Dictionary<double, TourEvent>(); // dummy TL_dict -- tourAudio_timeline obviously doesn't store any TourEvents
            tourDict.Add(tourAudio_TL, tourAudio_TL_dict);

            tourAudio_element = new MediaElement();
            tourAudio_element.Volume = 0.99;

            tourAudio_element.LoadedBehavior = MediaState.Manual;
            tourAudio_element.UnloadedBehavior = MediaState.Manual;

            Storyboard.SetTarget(tourAudio_TL, tourAudio_element);
            tourStoryboard.SlipBehavior = SlipBehavior.Slip;

            // took me quite a while to figure out that WPF really can't determine the duration of an MP3 until it's actually loaded (i.e. playing), and then it took me a little longer to finally find and accept this open-source library...argh
            TagLib.File audio_file_tags = TagLib.File.Create(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file);
            tourAudio_TL.Duration = audio_file_tags.Properties.Duration;
            resetTourLength(tourAudio_TL.Duration.TimeSpan.TotalSeconds + 1);
            tourAuthoringUI.refreshUI();
            tourAuthoringUI.reloadUI();

        }

        public void AddNewMetaDataTimeline(String imageFilePath, String fileName)
        {
            DockableItem dockItem = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, imageFilePath);

            dockItem.PreviewTouchDown += new EventHandler<TouchEventArgs>(mediaTouchDown);
            dockItem.PreviewMouseDown += new MouseButtonEventHandler(mediaTouchDown);
            dockItem.PreviewTouchMove += new EventHandler<TouchEventArgs>(mediaTouchMoved);
            dockItem.PreviewMouseMove +=new MouseEventHandler(mediaTouchMoved);
            dockItem.PreviewTouchUp += new EventHandler<TouchEventArgs>(mediaTouchUp);
            dockItem.PreviewMouseUp += new MouseButtonEventHandler(mediaTouchUp);

            dockableItemsLoaded.Add(dockItem);
            dockItem.Visibility = Visibility.Hidden;

            TourParallelTL dockItem_TL = new TourParallelTL();
            dockItem_TL.type = TourTLType.media;

            dockItem_TL.displayName = fileName;
            dockItem_TL.file = fileName;

            itemToTLDict.Add(dockItem, dockItem_TL);

            Dictionary<double, TourEvent> dockItem_TL_dict = new Dictionary<double, TourEvent>();
            Dictionary<TourEvent, double> dockItem_TL_dict_rev = new Dictionary<TourEvent, double>();
            tourDict.Add(dockItem_TL, dockItem_TL_dict);
            tourDictRev.Add(dockItem_TL, dockItem_TL_dict_rev);


            // Add fade in
            double toScreenPointX = 500;
            double toScreenPointY = 500;
            double scale = 1.0;
            double FadeInduration = 1.0;

            TourEvent fadeInMediaEvent = new FadeInMediaEvent(dockItem, toScreenPointX, toScreenPointY, scale, FadeInduration);

            double FadeInbeginTime = authorTimerCountSpan.TotalSeconds - 1;
            if (FadeInbeginTime < 0) FadeInbeginTime = 0;
            if (authorTimerCountSpan.TotalSeconds == 0) FadeInbeginTime += 1;
            dockItem_TL_dict.Add(FadeInbeginTime, fadeInMediaEvent);
            dockItem_TL_dict_rev.Add(fadeInMediaEvent, FadeInbeginTime);

            // Add fade out
            double FadeOutduration = 1.0;

            TourEvent fadeOutMediaEvent = new FadeOutMediaEvent(dockItem, FadeOutduration);

            double FadeOutbeginTime = FadeInbeginTime + 2;
            dockItem_TL_dict.Add(FadeOutbeginTime, fadeOutMediaEvent);
            dockItem_TL_dict_rev.Add(fadeOutMediaEvent, FadeOutbeginTime);

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
            if (paused == false)
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

                artModeWin.msi_ViewboxUpdate(); // force msi viewbox to refresh itself

                // swap tour control buttons with artwork inter-mode navigation ones
                artModeWin.toggleLeftSide();
                artModeWin.tourAuthoringButton.Visibility = Visibility.Visible;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Visible;
                //artModeWin.activateKW.Visibility = Visibility.Visible;
                artModeWin.resetArtworkButton.Visibility = Visibility.Visible;
                artModeWin.exitButton.Visibility = Visibility.Visible;
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;
                //artModeWin.tourResumeButton.Visibility = Visibility.Collapsed;
                //artModeWin.tourPauseButton.Visibility = Visibility.Collapsed;
                artModeWin.tourControlButton.Visibility = Visibility.Collapsed;
                artModeWin.tourStopButton.Visibility = Visibility.Collapsed;
                artModeWin.hideMetaList();

                tourPlaybackOn = false;
            }
        }


        public void TourAuthoringDoneButton_Click(object sender, RoutedEventArgs e)
        {
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
                
                artModeWin.msi_ViewboxUpdate(); // force msi viewbox to refresh itself
                tourAuthoringUI.removeAuthTools();
                
                // swap tour control buttons with artwork inter-mode navigation ones
                artModeWin.tourAuthoringButton.Visibility = Visibility.Visible;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Visible;
                //artModeWin.activateKW.Visibility = Visibility.Visible;
                artModeWin.resetArtworkButton.Visibility = Visibility.Visible;
                artModeWin.exitButton.Visibility = Visibility.Visible;
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;
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
                    Console.WriteLine("Data/Tour/XML/" + artModeWin.currentArtworkFileName + "." + "xml");
                    if (File.Exists("Data/Tour/XML/" + artModeWin.currentArtworkFileName + "." + "xml"))
                    {
                        File.Delete("Data/Tour/XML/" + artModeWin.currentArtworkFileName + "." + "xml");
                        Console.WriteLine("Deleted");
                    }

                    this.TourAuthoringDoneButton_Click(sender, e); //bad idea?
                }
                else
                {
                    return;
                }
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

        private void TourSeekBarMarker_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            tourStoryboard.Seek(artModeWin, tourTimerCountSpan, TimeSeekOrigin.BeginTime);
            tourStoryboard.CurrentTimeInvalidated += TourStoryboardPlayback_CurrentTimeInvalidated;
        }

        private void TourSeekBarMarker_PreviewMouseUp(object sender, MouseEventArgs e)
        {
            tourStoryboard.Seek(artModeWin, tourTimerCountSpan, TimeSeekOrigin.BeginTime);
            tourStoryboard.CurrentTimeInvalidated += TourStoryboardPlayback_CurrentTimeInvalidated;
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
            ZoomableCanvas zcan = (sender as MultiScaleImage).GetZoomableCanvas;
            Timeline tl;
            if (msiToTLDict.TryGetValue(sender as MultiScaleImage, out tl))
            {
                Dictionary<double, TourEvent> itemDict;
                Dictionary<TourEvent, double> itemDictRev;
                if (tourDict.TryGetValue(tl, out itemDict))
                {
                    itemDictRev = tourDictRev[tl];
                    double startEventTime = System.Double.MaxValue;
                    bool needToAddEvent = true;
                    foreach (TourEvent tevent in itemDict.Values)
                    {
                        // When the object gets moved, set the scrub to where the object first appears.
                        //startEventTime = Math.Min(startEventTime, itemDictRev[tevent]);
                        startEventTime = itemDictRev[tevent];
                        // Dealing with each type of event
                        // TODO: Fill in the switch which changes to each type of event

                        if ((authorTimerCountSpan.TotalSeconds > startEventTime && authorTimerCountSpan.TotalSeconds <= (startEventTime + tevent.duration)))
                        {
                            needToAddEvent = false;
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
                        itemDictRev.Add(toAdd, time);
                    }
                }
            }
            tourAuthoringUI.refreshUI();
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

            DockableItem dockItem = sender as DockableItem;
            Timeline tl;
            if (itemToTLDict.TryGetValue(dockItem, out tl))
            {
                Dictionary<double, TourEvent> itemDict;
                Dictionary<TourEvent, double> itemDictRev;
                if (tourDict.TryGetValue(tl, out itemDict))
                {
                    itemDictRev = tourDictRev[tl];
                    double startEventTime = System.Double.MaxValue;
                    bool needToAddEvent = true;
                    foreach (TourEvent tevent in itemDict.Values)
                    {
                        startEventTime = itemDictRev[tevent];
                        //if (startEventTime < 0) startEventTime = 0;
                        if (tevent.type == TourEvent.Type.zoomMedia)
                        {
                            ZoomMediaEvent ze = (ZoomMediaEvent)tevent;
                            Console.WriteLine(ze.zoomMediaToScreenPointX + "," + ze.zoomMediaToScreenPointY + " -> " + dockItem.ActualCenter.X + "," + dockItem.ActualCenter.Y);
                        }

                        if ((authorTimerCountSpan.TotalSeconds >= startEventTime && authorTimerCountSpan.TotalSeconds <= (startEventTime + tevent.duration)))
                        {
                            needToAddEvent = false;
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
                        /*double time = authorTimerCountSpan.TotalSeconds - 1;
                        if (time < 0) time = 0;
                        if (authorTimerCountSpan.TotalSeconds < 1)
                        {
                            time = 0;
                            //time = authorTimerCountSpan.TotalSeconds;
                        }*/
                        double time = 0;
                        if (authorTimerCountSpan.TotalSeconds - 1 > 0)
                        {
                            time = 0;
                        }
                        ZoomMediaEvent toAdd = new ZoomMediaEvent(dockItem, dockItem.image.ActualWidth / dockItem.image.Source.Width, dockItem.ActualCenter.X, dockItem.ActualCenter.Y, 1);
                        itemDict.Add(time, toAdd);
                        itemDictRev.Add(toAdd, time);
                    }
                }
            }
            tourAuthoringUI.refreshUI();
            tourAuthoringUI.refreshUI();
        }

        #endregion

        #region tour playback storyboard event handlers

        private void TourStoryboardPlayback_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("Current width" + artModeWin.ImageArea.ActualWidth);
                Console.WriteLine("Current height" + artModeWin.ImageArea.ActualHeight);
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
                //artModeWin.activateKW.Visibility = Visibility.Visible;
                artModeWin.resetArtworkButton.Visibility = Visibility.Visible;
                artModeWin.exitButton.Visibility = Visibility.Visible;
                artModeWin.HotspotOverlay.Visibility = Visibility.Visible;
                //artModeWin.tourResumeButton.Visibility = Visibility.Collapsed;
                //artModeWin.tourPauseButton.Visibility = Visibility.Collapsed;
                artModeWin.tourControlButton.Visibility = Visibility.Collapsed;
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
                Dictionary<double, TourEvent> itemDict;
                Dictionary<TourEvent, double> itemDictRev;
                if (tourDict.TryGetValue(tl, out itemDict))
                {
                    itemDictRev = tourDictRev[tl];
                    itemDict.Add(start, toAdd);
                    itemDictRev.Add(toAdd, start);



                    /*
                    int i = 0;
                    foreach (KeyValuePair<Timeline, Dictionary<double, TourEvent>> tourDict_KV in tourDict)
                    {
                        Timeline tourTL = tourDict_KV.Key;
                        Dictionary<double, TourEvent> tourTL_dict = tourDict_KV.Value;

                        TourAuthoringUI.timelineInfo timelineInfo = tourAuthoringUI.addTimeline(tourTL, tourTL_dict, ((TourTL)tourTL).displayName, i * tourAuthoringUI.timelineHeight);

                        foreach (KeyValuePair<double, TourEvent> tourTL_dict_KV in tourTL_dict) // MediaTimeline will ignore this
                        {
                            double beginTime = tourTL_dict_KV.Key;
                            TourEvent tourEvent = tourTL_dict_KV.Value;

                            tourAuthoringUI.addTourEvent(timelineInfo, tourEvent, timelineInfo.lengthSV, beginTime, tourEvent.duration);
                        }

                        if (((TourTL)tourTL).type == TourTLType.audio) // for MediaTimeline
                        {
                            tourAuthoringUI.addTourEvent(timelineInfo, null, timelineInfo.lengthSV, 0, tourTL.Duration.TimeSpan.TotalSeconds); // will this work?
                        }

                        i++;
                    }*/
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
                /*case TourEvent.Type.initMedia:
                    InitMediaEvent initMediaEvent = (InitMediaEvent)tourEvent;

                    //Point mediaPoint = new Point(initMediaEvent.initMediaToMSIPointX, initMediaEvent.initMediaToMSIPointY);

                    Point mediaPoint = new Point();
                    //mediaPoint.X = (artModeWin.msi_tour.GetZoomableCanvas.Scale * initMediaEvent.initMediaToMSIPointX) - artModeWin.msi_tour.GetZoomableCanvas.Offset.X;
                    //mediaPoint.Y = (artModeWin.msi_tour.GetZoomableCanvas.Scale * initMediaEvent.initMediaToMSIPointY) - artModeWin.msi_tour.GetZoomableCanvas.Offset.Y;
                    mediaPoint.X = initMediaEvent.initMediaToScreenPointX;
                    mediaPoint.Y = initMediaEvent.initMediaToScreenPointY;

                    double initialScale = initMediaEvent.absoluteScale;

                    DockableItem initMediaItem = initMediaEvent.media;
                    initMediaItem.Center = mediaPoint;
                    initMediaItem.Width = initMediaItem.image.Source.Width * initialScale;
                    initMediaItem.Height = initMediaItem.image.Source.Height * initialScale;
                    initMediaItem.Orientation = 0;

                    // NEW
                    DoubleAnimation initMediaWidth = new DoubleAnimation(initMediaItem.Width, initMediaItem.Width, new Duration(TimeSpan.FromSeconds(0.0)));
                    Storyboard.SetTarget(initMediaWidth, initMediaItem);
                    Storyboard.SetTargetProperty(initMediaWidth, new PropertyPath(DockableItem.WidthProperty));
                    initMediaWidth.BeginTime = TimeSpan.FromSeconds(timerCount);
                    TourParallelTL.Children.Add(initMediaWidth);

                    DoubleAnimation initMediaHeight = new DoubleAnimation(initMediaItem.Height, initMediaItem.Height, new Duration(TimeSpan.FromSeconds(0.0)));
                    Storyboard.SetTarget(initMediaHeight, initMediaItem);
                    Storyboard.SetTargetProperty(initMediaHeight, new PropertyPath(DockableItem.HeightProperty));
                    initMediaHeight.BeginTime = TimeSpan.FromSeconds(timerCount);
                    TourParallelTL.Children.Add(initMediaHeight);

                    PointAnimation initMediaCenter = new PointAnimation(initMediaItem.Center, initMediaItem.Center, new Duration(TimeSpan.FromSeconds(0.0)));
                    Storyboard.SetTarget(initMediaCenter, initMediaItem);
                    Storyboard.SetTargetProperty(initMediaCenter, new PropertyPath(DockableItem.CenterProperty));
                    initMediaCenter.BeginTime = TimeSpan.FromSeconds(timerCount);
                    TourParallelTL.Children.Add(initMediaCenter);

                    break;*/
                case TourEvent.Type.fadeInMedia:
                    FadeInMediaEvent fadeInMediaEvent = (FadeInMediaEvent)tourEvent;

                    // used to use MSI points, but now using screen points -- see artwork mode documentation in Google Doc
                    Point mediaPoint = new Point();
                    //mediaPoint.X = (artModeWin.msi_tour.GetZoomableCanvas.Scale * initMediaEvent.initMediaToMSIPointX) - artModeWin.msi_tour.GetZoomableCanvas.Offset.X;
                    //mediaPoint.Y = (artModeWin.msi_tour.GetZoomableCanvas.Scale * initMediaEvent.initMediaToMSIPointY) - artModeWin.msi_tour.GetZoomableCanvas.Offset.Y;
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
                    //zoomEndPoint.X = (artModeWin.msi_tour.GetZoomableCanvas.Scale * zoomMediaEvent.zoomMediaToMSIPointX) - artModeWin.msi_tour.GetZoomableCanvas.Offset.X;
                    //zoomEndPoint.Y = (artModeWin.msi_tour.GetZoomableCanvas.Scale * zoomMediaEvent.zoomMediaToMSIPointY) - artModeWin.msi_tour.GetZoomableCanvas.Offset.Y;
                    zoomEndPoint.X = zoomMediaEvent.zoomMediaToScreenPointX;
                    zoomEndPoint.Y = zoomMediaEvent.zoomMediaToScreenPointY;

                    PointAnimation zoomMediaPan = new PointAnimation(zoomEndPoint, new Duration(TimeSpan.FromSeconds(zoomMediaEvent.duration)), FillBehavior.HoldEnd);
                    Storyboard.SetTarget(zoomMediaPan, zoomMediaItem);
                    Storyboard.SetTargetProperty(zoomMediaPan, new PropertyPath(DockableItem.CenterProperty));
                    zoomMediaPan.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(zoomMediaPan);

                    break;
                case TourEvent.Type.fadeInMSI:
                    FadeInMSIEvent fadeInMSIEvent = (FadeInMSIEvent)tourEvent;

                    ObjectAnimationUsingKeyFrames fadeInMSIAnim_vis = new ObjectAnimationUsingKeyFrames();
                    fadeInMSIAnim_vis.Duration = new TimeSpan(0, 0, 0);
                    DiscreteObjectKeyFrame fadeInMSIAnim_vis_kf1 = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan(0, 0, 0));
                    fadeInMSIAnim_vis.KeyFrames.Add(fadeInMSIAnim_vis_kf1);
                    Storyboard.SetTarget(fadeInMSIAnim_vis, fadeInMSIEvent.msi);
                    Storyboard.SetTargetProperty(fadeInMSIAnim_vis, new PropertyPath(MultiScaleImage.VisibilityProperty));
                    fadeInMSIAnim_vis.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInMSIAnim_vis);

                    DoubleAnimation fadeInMSIAnim = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(fadeInMSIEvent.duration)));
                    Storyboard.SetTarget(fadeInMSIAnim, fadeInMSIEvent.msi);
                    Storyboard.SetTargetProperty(fadeInMSIAnim, new PropertyPath(MultiScaleImage.OpacityProperty));
                    fadeInMSIAnim.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeInMSIAnim);

                    break;
                case TourEvent.Type.fadeOutMSI:
                    FadeOutMSIEvent fadeOutMSIEvent = (FadeOutMSIEvent)tourEvent;

                    DoubleAnimation fadeOutMSIAnim = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(fadeOutMSIEvent.duration)));
                    Storyboard.SetTarget(fadeOutMSIAnim, fadeOutMSIEvent.msi);
                    Storyboard.SetTargetProperty(fadeOutMSIAnim, new PropertyPath(MultiScaleImage.OpacityProperty));
                    fadeOutMSIAnim.BeginTime = TimeSpan.FromSeconds(timerCount);
                    tourParallelTL.Children.Add(fadeOutMSIAnim);

                    ObjectAnimationUsingKeyFrames fadeOutMSIAnim_vis = new ObjectAnimationUsingKeyFrames();
                    fadeOutMSIAnim_vis.Duration = new TimeSpan(0, 0, 0);
                    DiscreteObjectKeyFrame fadeOutMSIAnim_vis_kf1 = new DiscreteObjectKeyFrame(Visibility.Hidden, new TimeSpan(0, 0, 0));
                    fadeOutMSIAnim_vis.KeyFrames.Add(fadeOutMSIAnim_vis_kf1);
                    Storyboard.SetTarget(fadeOutMSIAnim_vis, fadeOutMSIEvent.msi);
                    Storyboard.SetTargetProperty(fadeOutMSIAnim_vis, new PropertyPath(MultiScaleImage.VisibilityProperty));
                    fadeOutMSIAnim_vis.BeginTime = TimeSpan.FromSeconds(timerCount + fadeOutMSIEvent.duration);
                    tourParallelTL.Children.Add(fadeOutMSIAnim_vis);

                    break;
                case TourEvent.Type.zoomMSI:
                    ZoomMSIEvent zoomMSIEvent = (ZoomMSIEvent)tourEvent;

                    double targetMSIScale = zoomMSIEvent.absoluteScale;

                    Point zoomToMSIPoint = new Point(zoomMSIEvent.zoomToMSIPointX, zoomMSIEvent.zoomToMSIPointY);

                    // new
                    ZoomableCanvas zoomMSI_canvas = zoomMSIEvent.msi.GetZoomableCanvas;
                    targetMSIScale = zoomMSIEvent.msi.ClampTargetScale(targetMSIScale);

                    //Point targetOffset = new Point((zoomToMSIPoint.X * targetMSIScale) - (zoomMSI_canvas.ActualWidth * 0.5), (zoomToMSIPoint.Y * targetMSIScale) - (zoomMSI_canvas.ActualHeight * 0.5));
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

        #region HARD-CODED SAMPLE TOURS
        private void LoadPanel43Tour()
        {

            // STORYBOARD initialization
            tourStoryboard = new TourStoryboard();
            tourStoryboard.CurrentTimeInvalidated += new EventHandler(TourStoryboardPlayback_CurrentTimeInvalidated);
            tourStoryboard.Completed += new EventHandler(TourStoryboardPlayback_Completed);

            // TOUR definition
            tourPlaybackOn = false;

            dockableItemsLoaded = new List<DockableItem>();
            msiItemsLoaded = new List<MultiScaleImage>();

            // tour event initialization
            //tourDict = new Dictionary<int, List<TourEvent>>();
            tourDict = new Dictionary<Timeline, Dictionary<double, TourEvent>>();

            TourParallelTL msi_tour_TL = new TourParallelTL();
            tourStoryboard.Children.Add(msi_tour_TL);
            Dictionary<double, TourEvent> msi_tour_TL_dict = new Dictionary<double, TourEvent>();
            tourDict.Add(msi_tour_TL, msi_tour_TL_dict);

            // LIST - 0:01
            /*List<TourEvent> tourList0 = new List<TourEvent>();
            TourEvent initMSI0 = new InitMSIEvent(artModeWin.msi, artModeWin.msi.GetImageActualWidth / 2, artModeWin.msi.GetImageActualHeight / 2, 0.166153846153846);
            TourEvent fadeInMSI0 = new FadeInMSIEvent(artModeWin.msi, 1);
            tourList0.Add(initMSI0);
            tourList0.Add(fadeInMSI0);
            tourDict.Add(1, tourList0);*/

            // LIST - 0:31
            //List<TourEvent> tourList1 = new List<TourEvent>();
            TourEvent zoomMSI1 = new ZoomMSIEvent(artModeWin.msi_tour, 0.2, 4897, 3255, 22); // EVENT - zoom into art slowly over long period // * 0.8
            /*tourList1.Add(zoomMSI1);
            tourDict.Add(31, tourList1);*/
            msi_tour_TL_dict.Add(31, zoomMSI1);
            this.addAnim(msi_tour_TL, zoomMSI1, 31); // playback et al.

            // LIST - 0:40
            //List<TourEvent> tourList2 = new List<TourEvent>();
            DockableItem dockItem1 = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\ColDunne.jpg");
            dockableItemsLoaded.Add(dockItem1);
            dockItem1.Visibility = Visibility.Hidden;
            TourParallelTL dockItem1_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem1_TL);
            Dictionary<double, TourEvent> dockItem1_TL_dict = new Dictionary<double, TourEvent>();
            tourDict.Add(dockItem1_TL, dockItem1_TL_dict);
            //TourEvent initMedia1 = new InitMediaEvent(dockItem1, 1440, 432, 1);
            //TourEvent initMedia1 = new InitMediaEvent(dockItem1, 7513, 2623, 1); //  * 0.8
            //TourEvent fadeInMedia1 = new FadeInMediaEvent(dockItem1, 0.5); // EVENT - open picture of Colonel Dunne
            TourEvent fadeInMedia1 = new FadeInMediaEvent(dockItem1, 1440, 432, 1, 1); // EVENT - open picture of Colonel Dunne
            /*tourList2.Add(initMedia1);
            tourList2.Add(fadeInMedia1);
            tourDict.Add(40, tourList2);*/
            //dockItem1_TL_dict.Add(40, initMedia1);
            dockItem1_TL_dict.Add(40, fadeInMedia1);
            //this.addAnim(dockItem1_TL, initMedia1, 40); // playback
            this.addAnim(dockItem1_TL, fadeInMedia1, 40); // playback

            // LIST - 0:53
            //List<TourEvent> tourList3 = new List<TourEvent>();
            TourEvent fadeOutMedia1 = new FadeOutMediaEvent(dockItem1, 1); // EVENT - close picture of Colonel Dunne
            /*tourList3.Add(fadeOutMedia1);
            tourDict.Add(53, tourList3);*/
            dockItem1_TL_dict.Add(53, fadeOutMedia1);
            this.addAnim(dockItem1_TL, fadeOutMedia1, 53); // playback

            // LIST - 0:54 & 0:55
            //List<TourEvent> tourList4 = new List<TourEvent>();
            DockableItem dockItem2 = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\Ms_page105_96dpi.jpg");
            dockableItemsLoaded.Add(dockItem2);
            dockItem2.Visibility = Visibility.Hidden;
            TourParallelTL dockItem2_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem2_TL);
            Dictionary<double, TourEvent> dockItem2_TL_dict = new Dictionary<double, TourEvent>();
            tourDict.Add(dockItem2_TL, dockItem2_TL_dict);
            //TourEvent initMedia2 = new InitMediaEvent(dockItem2, 480, 432, 1);
            //TourEvent initMedia2 = new InitMediaEvent(dockItem2, 2467, 2685, 1); //  * 0.8
            //TourEvent fadeInMedia2 = new FadeInMediaEvent(dockItem2, 0.5); // EVENT - open manuscript
            TourEvent fadeInMedia2 = new FadeInMediaEvent(dockItem2, 480, 432, 1, 1); // EVENT - open manuscript
            TourEvent zoomMedia1 = new ZoomMediaEvent(dockItem2, 1.5, 480, 400, 4); // EVENT - zoom manuscript
            //TourEvent zoomMedia1 = new ZoomMediaEvent(dockItem2, 1.5, 2467, 2525, 5); // EVENT - zoom manuscript //  * 0.8
            /*tourList4.Add(initMedia2);
            tourList4.Add(fadeInMedia2);
            tourList4.Add(zoomMedia1);
            tourDict.Add(54, tourList4);*/
            //dockItem2_TL_dict.Add(54, initMedia2);
            dockItem2_TL_dict.Add(54, fadeInMedia2);
            dockItem2_TL_dict.Add(55, zoomMedia1);
            //this.addAnim(dockItem2_TL, initMedia2, 54);
            this.addAnim(dockItem2_TL, fadeInMedia2, 54);
            this.addAnim(dockItem2_TL, zoomMedia1, 55);

            // LIST - 1:00 (60)
            //List<TourEvent> tourList5 = new List<TourEvent>();
            DockableItem dockItem3 = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\PercyWindham.jpg");
            dockableItemsLoaded.Add(dockItem3);
            dockItem3.Visibility = Visibility.Hidden;
            TourParallelTL dockItem3_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem3_TL);
            Dictionary<double, TourEvent> dockItem3_TL_dict = new Dictionary<double, TourEvent>();
            tourDict.Add(dockItem3_TL, dockItem3_TL_dict);
            //TourEvent initMedia3 = new InitMediaEvent(dockItem3, 1440, 432, 1);
            //TourEvent initMedia3 = new InitMediaEvent(dockItem3, 7267, 2685, 1); // * 0.8
            //TourEvent fadeInMedia3 = new FadeInMediaEvent(dockItem3, 0.5); // EVENT - open picture of Percy Windham
            TourEvent fadeInMedia3 = new FadeInMediaEvent(dockItem3, 1440, 432, 1, 1); // EVENT - open picture of Percy Windham
            // TourEvent panMedia1 = new PanMediaEvent(dockItem2, 2467, 525, 28); // EVENT - pan manuscript
            TourEvent zoomMedia1a = new ZoomMediaEvent(dockItem2, 1.5, 480, 0, 28); // EVENT - pan manuscript
            //TourEvent zoomMedia1a = new ZoomMediaEvent(dockItem2, 1.5, 2467, 525, 28); // REPLACES panMedia1 // * 0.8
            /*tourList5.Add(initMedia3);
            tourList5.Add(fadeInMedia3);
            //tourList5.Add(panMedia1);
            tourList5.Add(zoomMedia1a);
            tourDict.Add(60, tourList5);*/
            //dockItem3_TL_dict.Add(60, initMedia3);
            dockItem3_TL_dict.Add(60, fadeInMedia3);
            dockItem2_TL_dict.Add(60, zoomMedia1a);
            //this.addAnim(dockItem3_TL, initMedia3, 60);
            this.addAnim(dockItem3_TL, fadeInMedia3, 60);
            this.addAnim(dockItem2_TL, zoomMedia1a, 60);

            // LIST - 1:10 (70)
            //List<TourEvent> tourList6 = new List<TourEvent>();
            TourEvent fadeOutMedia2 = new FadeOutMediaEvent(dockItem3, 1); // EVENT - close picture of Percy Windham
            /*tourList6.Add(fadeOutMedia2);
            tourDict.Add(70, tourList6);*/
            dockItem3_TL_dict.Add(70, fadeOutMedia2);
            this.addAnim(dockItem3_TL, fadeOutMedia2, 70);

            // LIST - 1:11 (71)
            //List<TourEvent> tourList7 = new List<TourEvent>();
            DockableItem dockItem4 = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\PeterCunningham.jpg");
            dockableItemsLoaded.Add(dockItem4);
            dockItem4.Visibility = Visibility.Hidden;
            TourParallelTL dockItem4_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem4_TL);
            Dictionary<double, TourEvent> dockItem4_TL_dict = new Dictionary<double, TourEvent>();
            tourDict.Add(dockItem4_TL, dockItem4_TL_dict);
            //TourEvent initMedia4 = new InitMediaEvent(dockItem4, 1440, 432, 1);
            //TourEvent initMedia4 = new InitMediaEvent(dockItem4, 7267, 2685, 1); // * 0.8
            //TourEvent fadeInMedia4 = new FadeInMediaEvent(dockItem4, 0.5); // EVENT - open picture of Peter Cunningham
            TourEvent fadeInMedia4 = new FadeInMediaEvent(dockItem4, 1440, 432, 1, 0.5); // EVENT - open picture of Peter Cunningham
            //TourEvent fadeInMedia4 = new FadeInMediaEvent(dockItem4, 1440, 432, 1, 0.5); // EVENT - open picture of Peter Cunningham
            /*tourList7.Add(initMedia4);
            tourList7.Add(fadeInMedia4);
            tourDict.Add(71, tourList7);*/
            //dockItem4_TL_dict.Add(71, initMedia4);
            dockItem4_TL_dict.Add(71, fadeInMedia4);
            //this.addAnim(dockItem4_TL, initMedia4, 71);
            this.addAnim(dockItem4_TL, fadeInMedia4, 71);

            // LIST - 1:28 (88)
            //List<TourEvent> tourList8 = new List<TourEvent>();
            TourEvent fadeOutMedia3 = new FadeOutMediaEvent(dockItem2, 1); // EVENT - close manuscript
            TourEvent fadeOutMedia4 = new FadeOutMediaEvent(dockItem4, 1); // EVENT - close picture of Peter Cunningham
            TourEvent zoomMSI2 = new ZoomMSIEvent(artModeWin.msi_tour, 0.135, 6890, 3947, 6); // EVENT //  * 0.8
            /*tourList8.Add(fadeOutMedia3);
            tourList8.Add(fadeOutMedia4);
            tourList8.Add(zoomMSI2);
            tourDict.Add(88, tourList8);*/
            dockItem2_TL_dict.Add(88, fadeOutMedia3);
            dockItem4_TL_dict.Add(88, fadeOutMedia4);
            msi_tour_TL_dict.Add(88, zoomMSI2);
            this.addAnim(dockItem2_TL, fadeOutMedia3, 88);
            this.addAnim(dockItem4_TL, fadeOutMedia4, 88);
            this.addAnim(msi_tour_TL, zoomMSI2, 88);

            // LIST - 1:34 (94)
            //List<TourEvent> tourList9 = new List<TourEvent>();
            DockableItem dockItem5 = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\ILN_Aug11_1860_96dpi.jpg");
            dockableItemsLoaded.Add(dockItem5);
            dockItem5.Visibility = Visibility.Hidden;
            TourParallelTL dockItem5_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem5_TL);
            Dictionary<double, TourEvent> dockItem5_TL_dict = new Dictionary<double, TourEvent>();
            tourDict.Add(dockItem5_TL, dockItem5_TL_dict);
            //TourEvent initMedia5 = new InitMediaEvent(dockItem5, 1573, 506, 0.5);
            //TourEvent initMedia5 = new InitMediaEvent(dockItem5, 11386, 3651, 0.5); // * 0.8
            //TourEvent fadeInMedia5 = new FadeInMediaEvent(dockItem5, 0.5); // EVENT
            TourEvent fadeInMedia5 = new FadeInMediaEvent(dockItem5, 1573, 506, 0.5, 1); // EVENT
            //TourEvent fadeInMedia5 = new FadeInMediaEvent(dockItem5, 1573, 506, 0.5, 0.5); // EVENT
            /*tourList9.Add(initMedia5);
            tourList9.Add(fadeInMedia5);
            tourDict.Add(94, tourList9);*/
            //dockItem5_TL_dict.Add(94, initMedia5);
            dockItem5_TL_dict.Add(94, fadeInMedia5);
            //this.addAnim(dockItem5_TL, initMedia5, 94);
            this.addAnim(dockItem5_TL, fadeInMedia5, 94);

            // LIST - 1:40 (100)
            //List<TourEvent> tourList10 = new List<TourEvent>();
            TourEvent zoomMSI3 = new ZoomMSIEvent(artModeWin.msi, 0.171, 7680, 3566, 5); // * 0.8
            TourEvent zoomMedia2 = new ZoomMediaEvent(dockItem5, 0.75, 1455, 64, 5);
            //TourEvent zoomMedia2 = new ZoomMediaEvent(dockItem5, 0.75, 10512, 377, 5); // * 0.8
            /*tourList10.Add(zoomMSI3);
            tourList10.Add(zoomMedia2);
            tourDict.Add(100, tourList10);*/
            msi_tour_TL_dict.Add(100, zoomMSI3);
            dockItem5_TL_dict.Add(100, zoomMedia2);
            this.addAnim(msi_tour_TL, zoomMSI3, 100);
            this.addAnim(dockItem5_TL, zoomMedia2, 100);

            // LIST - 1:46 (106)
            //List<TourEvent> tourList11 = new List<TourEvent>();
            TourEvent zoomMSI4 = new ZoomMSIEvent(artModeWin.msi_tour, 0.67, 7820, 5690, 5); //  * 0.8
            //TourEvent panMedia2 = new PanMediaEvent(dockItem5, 10586, -288, 5);
            TourEvent zoomMedia2a = new ZoomMediaEvent(dockItem5, 0.75, 1463, -113, 5);
            //TourEvent zoomMedia2a = new ZoomMediaEvent(dockItem5, 0.75, 10586, -288, 5); // REPLACES panMedia2 // * 0.8
            /*tourList11.Add(zoomMSI4);
            //tourList11.Add(panMedia2);
            tourList11.Add(zoomMedia2a);
            tourDict.Add(106, tourList11);*/
            msi_tour_TL_dict.Add(106, zoomMSI4);
            dockItem5_TL_dict.Add(106, zoomMedia2a);
            this.addAnim(msi_tour_TL, zoomMSI4, 106);
            this.addAnim(dockItem5_TL, zoomMedia2a, 106);

            // LIST - 1:57 (117)
            //List<TourEvent> tourList12 = new List<TourEvent>();
            TourEvent zoomMSI5 = new ZoomMSIEvent(artModeWin.msi_tour, 0.40, 2869, 5013, 3); // * 0.8
            TourEvent zoomMedia3 = new ZoomMediaEvent(dockItem5, 1.25, 1794, -437, 3);
            //TourEvent zoomMedia3 = new ZoomMediaEvent(dockItem5, 1.25, 9056, 4223, 3); // * 0.8
            /*tourList12.Add(zoomMSI5);
            tourList12.Add(zoomMedia3);
            tourDict.Add(117, tourList12);*/
            msi_tour_TL_dict.Add(117, zoomMSI5);
            dockItem5_TL_dict.Add(117, zoomMedia3);
            this.addAnim(msi_tour_TL, zoomMSI5, 117);
            this.addAnim(dockItem5_TL, zoomMedia3, 117);


            // LIST - 2:01 (121)
            //List<TourEvent> tourList13 = new List<TourEvent>();
            TourEvent zoomMSI6 = new ZoomMSIEvent(artModeWin.msi_tour, 0.85, 2926, 2739, 2); // * 0.8
            //TourEvent panMedia3 = new PanMediaEvent(dockItem5, 4944, 2773, 2);
            TourEvent zoomMedia3a = new ZoomMediaEvent(dockItem5, 1.25, 1796, -350, 2);
            //TourEvent zoomMedia3a = new ZoomMediaEvent(dockItem5, 1.25, 4944, 2773, 2); // REPLACES panMedia3 // * 0.8
            /*tourList13.Add(zoomMSI6);
            //tourList13.Add(panMedia3);
            tourList13.Add(zoomMedia3a);
            tourDict.Add(121, tourList13);*/
            msi_tour_TL_dict.Add(121, zoomMSI6);
            dockItem5_TL_dict.Add(121, zoomMedia3a);
            this.addAnim(msi_tour_TL, zoomMSI6, 121);
            this.addAnim(dockItem5_TL, zoomMedia3a, 121);

            // LIST - 2:04 (124)
            //List<TourEvent> tourList14 = new List<TourEvent>();
            TourEvent zoomMSI7 = new ZoomMSIEvent(artModeWin.msi_tour, 0.46, 8040, 4503, 2); // * 0.8
            TourEvent zoomMedia4 = new ZoomMediaEvent(dockItem5, 1.845, 1624, -627, 2);
            //TourEvent zoomMedia4 = new ZoomMediaEvent(dockItem5, 1.845, 3700, 1359, 2); // * 0.8
            /*tourList14.Add(zoomMSI7);
            tourList14.Add(zoomMedia4);
            tourDict.Add(124, tourList14);*/
            msi_tour_TL_dict.Add(124, zoomMSI7);
            dockItem5_TL_dict.Add(124, zoomMedia4);
            this.addAnim(msi_tour_TL, zoomMSI7, 124);
            this.addAnim(dockItem5_TL, zoomMedia4, 124);

            // LIST - 2:12 (132)
            //List<TourEvent> tourList15 = new List<TourEvent>();
            TourEvent zoomMSI8 = new ZoomMSIEvent(artModeWin.msi_tour, 0.72, 8499, 4612, 7); // * 0.8
            TourEvent zoomMedia5 = new ZoomMediaEvent(dockItem5, 1.21, 1451, -227, 7);
            //TourEvent zoomMedia5 = new ZoomMediaEvent(dockItem5, 1.21, 9094, 2823, 7); // * 0.8
            /*tourList15.Add(zoomMSI8);
            tourList15.Add(zoomMedia5);
            tourDict.Add(132, tourList15);*/
            msi_tour_TL_dict.Add(132, zoomMSI8);
            dockItem5_TL_dict.Add(132, zoomMedia5);
            this.addAnim(msi_tour_TL, zoomMSI8, 132);
            this.addAnim(dockItem5_TL, zoomMedia5, 132);

            // LIST - 2:31 (151)
            //List<TourEvent> tourList16 = new List<TourEvent>();
            TourEvent zoomMSI9 = new ZoomMSIEvent(artModeWin.msi_tour, 0.25, 9781, 2427, 2); // * 0.8
            TourEvent zoomMedia6 = new ZoomMediaEvent(dockItem5, 1.17, 1388, -132, 2);
            //TourEvent zoomMedia6 = new ZoomMediaEvent(dockItem5, 1.17, 9085, 3670, 2); // * 0.8
            /*tourList16.Add(zoomMSI9);
            tourList16.Add(zoomMedia6);
            tourDict.Add(151, tourList16);*/
            msi_tour_TL_dict.Add(151, zoomMSI9);
            dockItem5_TL_dict.Add(151, zoomMedia6);
            this.addAnim(msi_tour_TL, zoomMSI9, 151);
            this.addAnim(dockItem5_TL, zoomMedia6, 151);

            // LIST - 2:50 (170)
            //List<TourEvent> tourList17 = new List<TourEvent>();
            TourEvent zoomMSI10 = new ZoomMSIEvent(artModeWin.msi_tour, 0.135, 6890, 3947, 3); // * 0.8
            TourEvent zoomMedia7 = new ZoomMediaEvent(dockItem5, 0.5, 1573, 506, 3);
            //TourEvent zoomMedia7 = new ZoomMediaEvent(dockItem5, 0.5, 12209, 2267, 3); // * 0.8
            /*tourList17.Add(zoomMSI10);
            tourList17.Add(zoomMedia7);
            tourDict.Add(170, tourList17);*/
            msi_tour_TL_dict.Add(170, zoomMSI10);
            dockItem5_TL_dict.Add(170, zoomMedia7);
            this.addAnim(msi_tour_TL, zoomMSI10, 170);
            this.addAnim(dockItem5_TL, zoomMedia7, 170);

            // LIST - 2:57 (177)
            //List<TourEvent> tourList18 = new List<TourEvent>();
            TourEvent fadeOutMedia5 = new FadeOutMediaEvent(dockItem5, 1);
            /*tourList18.Add(fadeOutMedia5);
            tourDict.Add(177, tourList18);*/
            dockItem5_TL_dict.Add(177, fadeOutMedia5);
            this.addAnim(dockItem5_TL, fadeOutMedia5, 177);

            // LIST - 2:58 (178)
            /*List<TourEvent> tourList19 = new List<TourEvent>();
            TourEvent fadeOutMSI0 = new FadeOutMSIEvent(artModeWin.msi, 1);
            tourList19.Add(fadeOutMSI0);
            tourDict.Add(178, tourList19);*/

            TourMediaTL tourAudio_timeline = new TourMediaTL(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\GaribaldiScene43Tour_mastered.mp3", UriKind.Absolute));
            tourAudio_element = new MediaElement();
            tourAudio_element.Volume = 0.99;
            //_audioMediaElement.Name = “audioMediaElement”;
            //RegisterName(_audioMediaElement.Name, _audioMediaElement);

            tourAudio_element.LoadedBehavior = MediaState.Manual;
            tourAudio_element.UnloadedBehavior = MediaState.Manual;

            Storyboard.SetTarget(tourAudio_timeline, tourAudio_element);
            tourStoryboard.SlipBehavior = SlipBehavior.Slip;
            tourStoryboard.Children.Add(tourAudio_timeline);

            // for proof-of-concept only
            TagLib.File audio_file_tags = TagLib.File.Create(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\GaribaldiScene43Tour_mastered.mp3");
            tourStoryboard.Duration = audio_file_tags.Properties.Duration;

            //Console.WriteLine("Panel 43 (sample): tourStoryboard.Duration = " + tourStoryboard.Duration); // testing, debugging
        }

        private void LoadChineseScrollTour()
        {
            // *NOTE: Doesn't include tourDict proof-of-concept b/c fadeINMSI and fadeOutMSI not being used anymore -- was just for mockup.

            // STORYBOARD initialization
            tourStoryboard = new TourStoryboard();
            tourStoryboard.CurrentTimeInvalidated += new EventHandler(TourStoryboardPlayback_CurrentTimeInvalidated);
            tourStoryboard.Completed += new EventHandler(TourStoryboardPlayback_Completed);

            // TOUR definition
            tourPlaybackOn = false;

            dockableItemsLoaded = new List<DockableItem>();
            msiItemsLoaded = new List<MultiScaleImage>();

            // tour event initialization
            //tourDict = new Dictionary<int, List<TourEvent>>();
            TourParallelTL msi_tour_TL = new TourParallelTL();
            tourStoryboard.Children.Add(msi_tour_TL);

            // LIST - 0:01
            /*List<TourEvent> tourList0 = new List<TourEvent>();
            TourEvent initMSI0 = new InitMSIEvent(artModeWin.msi, artModeWin.msi.GetImageActualWidth / 2, artModeWin.msi.GetImageActualHeight / 2, 0.0865020724454857);
            TourEvent fadeInMSI0 = new FadeInMSIEvent(artModeWin.msi, 1);
            tourList0.Add(initMSI0);
            tourList0.Add(fadeInMSI0);
            tourDict.Add(1, tourList0);*/

            // LIST - 0:15
            //List<TourEvent> tourList1 = new List<TourEvent>();
            //TourEvent zoomMSI1 = new ZoomMSIEvent(artModeWin.msi, 0.58882, 11217, 764, 7);
            TourEvent zoomMSI1 = new ZoomMSIEvent(artModeWin.msi_tour, 0.58882, 11217, 764, 7); // * 0.8
            /*tourList1.Add(zoomMSI1);
            tourDict.Add(15, tourList1);*/
            this.addAnim(msi_tour_TL, zoomMSI1, 15);

            // LIST - 0:22
            //List<TourEvent> tourList2 = new List<TourEvent>();
            //TourEvent panMSI1 = new PanMSIEvent(artModeWin.msi, 20577, 758, 14);
            TourEvent zoomMSI1a = new ZoomMSIEvent(artModeWin.msi_tour, 0.58883, 20577, 758, 14); // REPLACES panMSI1 // 0.58882 * 0.8
            //tourList2.Add(panMSI1);
            /*tourList2.Add(zoomMSI1a);
            tourDict.Add(22, tourList2);*/
            this.addAnim(msi_tour_TL, zoomMSI1a, 22);

            // LIST - 0:36
            //List<TourEvent> tourList3 = new List<TourEvent>();

            // TESTING BEGIN
            String xmlPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\DeepZoom\\chinese_scroll_composition.bmp\\dz.xml";
            artModeWin.msi_tour_overlay.SetImageSource(xmlPath);
            artModeWin.msi_tour_overlay.DisableEventHandlers();

            artModeWin.msi_tour_overlay.Visibility = Visibility.Hidden; // testing

            msiItemsLoaded.Add(artModeWin.msi_tour_overlay);

            TourParallelTL msi_tour_overlay_TL = new TourParallelTL();
            tourStoryboard.Children.Add(msi_tour_overlay_TL);
            //TourEvent zoomMSI2a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.800991, 21004, 686, 5);
            //TourEvent zoomMSI2 = new ZoomMSIEvent(artModeWin.msi, 0.800991, 21004, 686, 5);
            TourEvent zoomMSI2a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.800991, 21004, 686, 4); // * 0.8
            TourEvent zoomMSI2 = new ZoomMSIEvent(artModeWin.msi_tour, 0.800991, 21004, 686, 4); // * 0.8
            /*tourList3.Add(zoomMSI2a);
            tourList3.Add(zoomMSI2);
            tourDict.Add(36, tourList3);*/
            this.addAnim(msi_tour_overlay_TL, zoomMSI2a, 36);
            this.addAnim(msi_tour_TL, zoomMSI2, 36);

            // NEW - 4-19-2011...actually, go forward 2s on 5-12-2011
            // LIST - 0:40 (40)
            //List<TourEvent> tourList3a = new List<TourEvent>();
            DockableItem dockItem3L = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\ZhangZeduan-BeautyofGreenMtnsAndRivers.jpg");
            dockableItemsLoaded.Add(dockItem3L);
            dockItem3L.Visibility = Visibility.Hidden;
            TourParallelTL dockItem3L_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem3L_TL);
            //TourEvent initMedia3L = new InitMediaEvent(dockItem3L, 20511, 542, 0.225661506825554);
            //TourEvent initMedia3L = new InitMediaEvent(dockItem3L, 480, 540, 0.225661506825554);
            //TourEvent fadeInMedia3L = new FadeInMediaEvent(dockItem3L, 1);
            TourEvent fadeInMedia3L = new FadeInMediaEvent(dockItem3L, 480, 540, 0.225661506825554, 1);
            DockableItem dockItem3R = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\ZhangZeduan-LoftyMtLu.jpg");
            dockableItemsLoaded.Add(dockItem3R);
            dockItem3R.Visibility = Visibility.Hidden;
            TourParallelTL dockItem3R_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem3R_TL);
            //TourEvent initMedia3R = new InitMediaEvent(dockItem3R, 21460, 534, 0.700128992726015);
            //TourEvent initMedia3R = new InitMediaEvent(dockItem3R, 1440, 540, 0.700128992726015);
            //TourEvent fadeInMedia3R = new FadeInMediaEvent(dockItem3R, 1);
            TourEvent fadeInMedia3R = new FadeInMediaEvent(dockItem3R, 1440, 540, 0.700128992726015, 1);
            /*tourList3a.Add(initMedia3L);
            tourList3a.Add(fadeInMedia3L);
            tourList3a.Add(initMedia3R);
            tourList3a.Add(fadeInMedia3R);
            tourDict.Add(40, tourList3a);*/
            //this.addAnim(dockItem3L_TL, initMedia3L, 40);
            this.addAnim(dockItem3L_TL, fadeInMedia3L, 40);
            //this.addAnim(dockItem3R_TL, initMedia3R, 40);
            this.addAnim(dockItem3R_TL, fadeInMedia3R, 40);

            // LIST - 0:53 (53)
            //List<TourEvent> tourList3b = new List<TourEvent>();
            TourEvent fadeOutMedia3L = new FadeOutMediaEvent(dockItem3L, 1);
            TourEvent fadeOutMedia3R = new FadeOutMediaEvent(dockItem3R, 1);
            /*tourList3b.Add(fadeOutMedia3L);
            tourList3b.Add(fadeOutMedia3R);
            tourDict.Add(53, tourList3b);*/
            this.addAnim(dockItem3L_TL, fadeOutMedia3L, 53);
            this.addAnim(dockItem3R_TL, fadeOutMedia3R, 53);

            // NEW - 4-19-2011
            // SHIFT EVERYTHING BELOW BY 13 SECONDS - 4-19-2011 [DONE]
            // LIST - 0:54
            //List<TourEvent> tourList4 = new List<TourEvent>();
            //TourEvent initMSI2 = new InitMSIEvent(artModeWin.msi_tour_overlay, 21004, 686, 0.800991); // have to zoom in before
            TourEvent fadeInMSI1 = new FadeInMSIEvent(artModeWin.msi_tour_overlay, 2);
            //TourEvent panMSI2a = new PanMSIEvent(artModeWin.msi_tour_overlay, 18781, 686, 10);
            TourEvent zoomMSI2c = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.800992, 18781, 686, 10); // REPLACES panMSI2a // 0.800991 * 0.8
            // TESTING END
            //TourEvent panMSI2 = new PanMSIEvent(artModeWin.msi, 18781, 686, 10);
            TourEvent zoomMSI2b = new ZoomMSIEvent(artModeWin.msi_tour, 0.800992, 18781, 686, 10); // REPLACES panMSI2 // 0.800991 * 0.8
            /*//tourList4.Add(initMSI2);
            tourList4.Add(fadeInMSI1);
            //tourList4.Add(panMSI2a);
            //tourList4.Add(panMSI2);
            tourList4.Add(zoomMSI2c);
            tourList4.Add(zoomMSI2b);
            tourDict.Add(54, tourList4);*/
            this.addAnim(msi_tour_overlay_TL, fadeInMSI1, 54);
            this.addAnim(msi_tour_overlay_TL, zoomMSI2c, 54);
            this.addAnim(msi_tour_TL, zoomMSI2b, 54);

            // LIST - 1:04 (64)
            //List<TourEvent> tourList5 = new List<TourEvent>();
            TourEvent fadeOutMSI1 = new FadeOutMSIEvent(artModeWin.msi_tour_overlay, 2);
            //TourEvent zoomMSI3a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 1.089, 16281, 645.96, 10);
            //TourEvent zoomMSI3 = new ZoomMSIEvent(artModeWin.msi, 1.089, 16281, 645.96, 10);
            TourEvent zoomMSI3a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 1.089, 16281, 645.96, 10); // * 0.8
            TourEvent zoomMSI3 = new ZoomMSIEvent(artModeWin.msi_tour, 1.089, 16281, 645.96, 10); // * 0.8
            /*tourList5.Add(fadeOutMSI1);
            tourList5.Add(zoomMSI3a);
            tourList5.Add(zoomMSI3);
            tourDict.Add(64, tourList5);*/
            this.addAnim(msi_tour_overlay_TL, fadeOutMSI1, 64);
            this.addAnim(msi_tour_overlay_TL, zoomMSI3a, 64);
            this.addAnim(msi_tour_TL, zoomMSI3, 64);

            // LIST - 1:14 (74)
            // TODO: isolation
            //List<TourEvent> tourList6 = new List<TourEvent>();
            DockableItem dockItem4 = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\Path4.png");
            dockableItemsLoaded.Add(dockItem4);
            dockItem4.Visibility = Visibility.Hidden;
            TourParallelTL dockItem4_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem4_TL);
            //TourEvent initMedia4 = new InitMediaEvent(dockItem4, 980, 432, 1);
            //TourEvent initMedia4 = new InitMediaEvent(dockItem4, 16299, 547, 1); // 0.8
            //TourEvent fadeInMedia4 = new FadeInMediaEvent(dockItem4, 1);
            TourEvent fadeInMedia4 = new FadeInMediaEvent(dockItem4, 980, 432, 1, 1);
            /*tourList6.Add(initMedia4);
            tourList6.Add(fadeInMedia4);
            tourDict.Add(74, tourList6);*/
            //this.addAnim(dockItem4_TL, initMedia4, 74);
            this.addAnim(dockItem4_TL, fadeInMedia4, 74);

            // LIST - 1:21 (81)
            //List<TourEvent> tourList6a = new List<TourEvent>();
            TourEvent fadeOutMedia4 = new FadeOutMediaEvent(dockItem4, 1);
            /*tourList6a.Add(fadeOutMedia4);
            tourDict.Add(81, tourList6a);*/
            this.addAnim(dockItem4_TL, fadeOutMedia4, 81);

            // ADDING TWO SECONDS TO EVERYTHING BEYOND THIS POINT - 4-19-2011
            // LIST - 1:23 (83)
            //List<TourEvent> tourList7 = new List<TourEvent>();
            //TourEvent zoomMSI4a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.7985, 15560, 689, 5);
            //TourEvent zoomMSI4 = new ZoomMSIEvent(artModeWin.msi, 0.7985, 15560, 689, 5);
            //TourEvent zoomMSI4a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.7985, 15560, 689, 7);
            //TourEvent zoomMSI4 = new ZoomMSIEvent(artModeWin.msi, 0.7985, 15560, 689, 7);
            TourEvent zoomMSI4a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.7985, 15560, 689, 7); // * 0.8
            TourEvent zoomMSI4 = new ZoomMSIEvent(artModeWin.msi_tour, 0.7985, 15560, 689, 7); // * 0.8
            /*tourList7.Add(zoomMSI4a);
            tourList7.Add(zoomMSI4);
            tourDict.Add(83, tourList7);*/
            this.addAnim(msi_tour_overlay_TL, zoomMSI4a, 83);
            this.addAnim(msi_tour_TL, zoomMSI4, 83);

            // LIST - 1:30 (90)
            //List<TourEvent> tourList8 = new List<TourEvent>();
            //TourEvent zoomMSI5a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 1.1995, 11107, 466, 28);
            //TourEvent zoomMSI5 = new ZoomMSIEvent(artModeWin.msi, 1.1995, 11107, 466, 28);
            TourEvent zoomMSI5a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 1.1995, 11107, 466, 28); // * 0.8
            TourEvent zoomMSI5 = new ZoomMSIEvent(artModeWin.msi_tour, 1.1995, 11107, 466, 28); // * 0.8
            /*tourList8.Add(zoomMSI5a);
            tourList8.Add(zoomMSI5);
            tourDict.Add(90, tourList8);*/
            this.addAnim(msi_tour_overlay_TL, zoomMSI5a, 90);
            this.addAnim(msi_tour_TL, zoomMSI5, 90);

            // LIST - 1:43 (103)
            //List<TourEvent> tourList8a = new List<TourEvent>();
            TourEvent fadeInMSI2 = new FadeInMSIEvent(artModeWin.msi_tour_overlay, 1);
            /*tourList8a.Add(fadeInMSI2);
            tourDict.Add(103, tourList8a);*/
            this.addAnim(msi_tour_overlay_TL, fadeInMSI2, 103);

            // LIST - 1:57 (117)
            //List<TourEvent> tourList8b = new List<TourEvent>();
            TourEvent fadeOutMSI2 = new FadeOutMSIEvent(artModeWin.msi_tour_overlay, 1);
            /*tourList8b.Add(fadeOutMSI2);
            tourDict.Add(117, tourList8b);*/
            this.addAnim(msi_tour_overlay_TL, fadeOutMSI2, 117);

            // LIST - 1:58 (118)
            // TODO: isolation
            //List<TourEvent> tourList9 = new List<TourEvent>();
            DockableItem dockItem7 = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\Path7.png");
            dockableItemsLoaded.Add(dockItem7);
            dockItem7.Visibility = Visibility.Hidden;
            TourParallelTL dockItem7_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem7_TL);
            //TourEvent initMedia7 = new InitMediaEvent(dockItem7, 980, 432, 1);
            //TourEvent initMedia7 = new InitMediaEvent(dockItem7, 11124, 376, 1); // 0.8
            //TourEvent fadeInMedia7 = new FadeInMediaEvent(dockItem7, 1);
            TourEvent fadeInMedia7 = new FadeInMediaEvent(dockItem7, 980, 432, 1, 1);
            /*tourList9.Add(initMedia7);
            tourList9.Add(fadeInMedia7);
            tourDict.Add(118, tourList9);*/
            //this.addAnim(dockItem7_TL, initMedia7, 118);
            this.addAnim(dockItem7_TL, fadeInMedia7, 118);

            // LIST - 2:04 (124)
            //List<TourEvent> tourList9a = new List<TourEvent>();
            TourEvent fadeOutMedia7 = new FadeOutMediaEvent(dockItem7, 1);
            /*tourList9a.Add(fadeOutMedia7);
            tourDict.Add(124, tourList9a);*/
            this.addAnim(dockItem7_TL, fadeOutMedia7, 124);

            // LIST - 2:06 (126)
            // TODO: isolation
            //List<TourEvent> tourList9b = new List<TourEvent>();
            DockableItem dockItem8 = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\Path8.png");
            dockableItemsLoaded.Add(dockItem8);
            dockItem8.Visibility = Visibility.Hidden;
            TourParallelTL dockItem8_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem8_TL);
            //TourEvent initMedia8 = new InitMediaEvent(dockItem8, 980, 432, 1);
            //TourEvent initMedia8 = new InitMediaEvent(dockItem8, 11124, 376, 1); // 0.8
            //TourEvent fadeInMedia8 = new FadeInMediaEvent(dockItem8, 1);
            TourEvent fadeInMedia8 = new FadeInMediaEvent(dockItem8, 980, 432, 1, 1);
            /*tourList9b.Add(initMedia8);
            tourList9b.Add(fadeInMedia8);
            tourDict.Add(126, tourList9b);*/
            //this.addAnim(dockItem8_TL, initMedia8, 126);
            this.addAnim(dockItem8_TL, fadeInMedia8, 126);

            // LIST - 2:15 (135)
            //List<TourEvent> tourList9c = new List<TourEvent>();
            TourEvent fadeOutMedia8 = new FadeOutMediaEvent(dockItem8, 1);
            /*tourList9c.Add(fadeOutMedia8);
            tourDict.Add(135, tourList9c);*/
            this.addAnim(dockItem8_TL, fadeOutMedia8, 135);

            // LIST - 2:16 (136)
            //List<TourEvent> tourList10 = new List<TourEvent>();
            //TourEvent zoomMSI6 = new ZoomMSIEvent(artModeWin.msi, 1.2, 5916, 637, 26);
            TourEvent zoomMSI6 = new ZoomMSIEvent(artModeWin.msi_tour, 1.2, 5916, 637, 26); // * 0.8
            /*tourList10.Add(zoomMSI6);
            tourDict.Add(136, tourList10);*/
            this.addAnim(msi_tour_TL, zoomMSI6, 136);

            // LIST - 2:44 (164)
            //List<TourEvent> tourList11 = new List<TourEvent>();
            //TourEvent zoomMSI7a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.6139, 6860, 763, 6);
            //TourEvent zoomMSI7 = new ZoomMSIEvent(artModeWin.msi, 0.6139, 6860, 763, 6);
            TourEvent zoomMSI7a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.6139, 6860, 763, 6); // * 0.8
            TourEvent zoomMSI7 = new ZoomMSIEvent(artModeWin.msi_tour, 0.6139, 6860, 763, 6); // * 0.8
            /*tourList11.Add(zoomMSI7a);
            tourList11.Add(zoomMSI7);
            tourDict.Add(164, tourList11);*/
            this.addAnim(msi_tour_overlay_TL, zoomMSI7a, 164);
            this.addAnim(msi_tour_TL, zoomMSI7, 164);

            // LIST - 2:51 (171)
            //List<TourEvent> tourList11a = new List<TourEvent>();
            TourEvent fadeInMSI3 = new FadeInMSIEvent(artModeWin.msi_tour_overlay, 1);
            /*tourList11a.Add(fadeInMSI3);
            tourDict.Add(170, tourList11a);*/
            this.addAnim(msi_tour_overlay_TL, fadeInMSI3, 170);

            // LIST - 2:52 (172)
            //List<TourEvent> tourList11b = new List<TourEvent>();
            TourEvent fadeOutMSI3 = new FadeOutMSIEvent(artModeWin.msi_tour_overlay, 7);
            /*tourList11b.Add(fadeOutMSI3);
            tourDict.Add(171, tourList11b);*/
            this.addAnim(msi_tour_overlay_TL, fadeOutMSI3, 171);

            // LIST - 2:59 (179)
            //List<TourEvent> tourList13 = new List<TourEvent>();
            //TourEvent panMSI3a = new PanMSIEvent(artModeWin.msi_tour_overlay, 2683, 744, 3.75);
            //TourEvent panMSI3 = new PanMSIEvent(artModeWin.msi, 2683, 744, 3.75);
            TourEvent zoomMSI3c = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.6140, 2683, 744, 4); // REPLACES panMSI3a // 0.6139 * 0.8
            TourEvent zoomMSI3b = new ZoomMSIEvent(artModeWin.msi_tour, 0.6140, 2683, 744, 4); // REPLACES panMSI3 // 0.6139 * 0.8
            //tourList13.Add(panMSI3a);
            //tourList13.Add(panMSI3);
            /*tourList13.Add(zoomMSI3c);
            tourList13.Add(zoomMSI3b);
            tourDict.Add(178, tourList13);*/
            this.addAnim(msi_tour_overlay_TL, zoomMSI3c, 178);
            this.addAnim(msi_tour_TL, zoomMSI3b, 178);

            // LIST - 3:05 (185)
            //List<TourEvent> tourList14 = new List<TourEvent>();
            TourEvent fadeInMSI4 = new FadeInMSIEvent(artModeWin.msi_tour_overlay, 1);
            TourEvent zoomMSI8a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.54, 8365, 776, 28);
            TourEvent zoomMSI8 = new ZoomMSIEvent(artModeWin.msi_tour, 0.54, 8365, 776, 28);
            //TourEvent zoomMSI8a = new ZoomMSIEvent(artModeWin.msi_tour_overlay, 0.372590190763185, 8365, 776, 28);
            //TourEvent zoomMSI8 = new ZoomMSIEvent(artModeWin.msi_tour, 0.372590190763185, 8365, 776, 28);
            /*tourList14.Add(fadeInMSI4);
            tourList14.Add(zoomMSI8a);
            tourList14.Add(zoomMSI8);
            tourDict.Add(185, tourList14);*/
            this.addAnim(msi_tour_overlay_TL, fadeInMSI4, 185);
            this.addAnim(msi_tour_overlay_TL, zoomMSI8a, 185);
            this.addAnim(msi_tour_TL, zoomMSI8, 185);

            // LIST - 3:20 (200)
            //List<TourEvent> tourList14a = new List<TourEvent>();
            TourEvent fadeOutMSI4 = new FadeOutMSIEvent(artModeWin.msi_tour_overlay, 1);
            /*tourList14a.Add(fadeOutMSI4);
            tourDict.Add(200, tourList14a);*/
            this.addAnim(msi_tour_overlay_TL, fadeOutMSI4, 200);

            // LIST - 3:33 (213)
            // TODO: magnifying glass effect
            //List<TourEvent> tourList15 = new List<TourEvent>();
            DockableItem dockItem13a = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\Zoom13a.png");
            dockableItemsLoaded.Add(dockItem13a);
            dockItem13a.Visibility = Visibility.Hidden;
            TourParallelTL dockItem13a_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem13a_TL);
            DockableItem dockItem13b = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\Zoom13b.png");
            dockableItemsLoaded.Add(dockItem13b);
            dockItem13b.Visibility = Visibility.Hidden;
            TourParallelTL dockItem13b_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem13b_TL);
            //TourEvent initMedia13a = new InitMediaEvent(dockItem13a, 253.204805376231, 274.813657099805, 0.54);
            //TourEvent initMedia13a = new InitMediaEvent(dockItem13a, 7056, 285, 0.372590190763185);
            //TourEvent fadeInMedia13a = new FadeInMediaEvent(dockItem13a, 1);
            TourEvent fadeInMedia13a = new FadeInMediaEvent(dockItem13a, 253.204805376231, 274.813657099805, 0.54, 1);
            //TourEvent initMedia13b = new InitMediaEvent(dockItem13b, 1741.04281448417, 288.770985229617, 0.54);
            //TourEvent initMedia13b = new InitMediaEvent(dockItem13b, 9811, 311, 0.372590190763185);
            //TourEvent fadeInMedia13b = new FadeInMediaEvent(dockItem13b, 1);
            TourEvent fadeInMedia13b = new FadeInMediaEvent(dockItem13b, 1741.04281448417, 288.770985229617, 0.54, 1);
            /*tourList15.Add(initMedia13a);,
            tourList15.Add(fadeInMedia13a);
            tourList15.Add(initMedia13b);
            tourList15.Add(fadeInMedia13b);
            tourDict.Add(213, tourList15);*/
            //this.addAnim(dockItem13a_TL, initMedia13a, 213);
            this.addAnim(dockItem13a_TL, fadeInMedia13a, 213);
            //this.addAnim(dockItem13b_TL, initMedia13b, 213);
            this.addAnim(dockItem13b_TL, fadeInMedia13b, 213);

            // LIST - 3:34 (214)
            //List<TourEvent> tourList15a = new List<TourEvent>();
            TourEvent zoomMedia13a = new ZoomMediaEvent(dockItem13a, 1, 505.624855996816, 429.598477844792, 1);
            //TourEvent zoomMedia13a = new ZoomMediaEvent(dockItem13a, 1, 7524, 572, 1);
            TourEvent zoomMedia13b = new ZoomMediaEvent(dockItem13b, 1, 1430.62467538835, 429.598477844792, 1);
            //TourEvent zoomMedia13b = new ZoomMediaEvent(dockItem13b, 1, 9237, 572, 1);
            /*tourList15a.Add(zoomMedia13a);
            tourList15a.Add(zoomMedia13b);
            tourDict.Add(214, tourList15a);*/
            this.addAnim(dockItem13a_TL, zoomMedia13a, 214);
            this.addAnim(dockItem13b_TL, zoomMedia13b, 214);

            // LIST - 3:39 (219)
            //List<TourEvent> tourList15b = new List<TourEvent>();
            TourEvent zoomMedia13a_out = new ZoomMediaEvent(dockItem13a, 0.54, 253.204805376231, 274.813657099805, 1);
            //TourEvent zoomMedia13a_out = new ZoomMediaEvent(dockItem13a, 0.372590190763185, 7056, 285, 1);
            TourEvent zoomMedia13b_out = new ZoomMediaEvent(dockItem13b, 0.54, 1741.04281448417, 288.770985229617, 1);
            //TourEvent zoomMedia13b_out = new ZoomMediaEvent(dockItem13b, 0.372590190763185, 9811, 311, 1);
            /*tourList15b.Add(zoomMedia13a_out);
            tourList15b.Add(zoomMedia13b_out);
            tourDict.Add(219, tourList15b);*/
            this.addAnim(dockItem13a_TL, zoomMedia13a_out, 219);
            this.addAnim(dockItem13b_TL, zoomMedia13b_out, 219);

            // LIST - 3:40 (220)
            //List<TourEvent> tourList15c = new List<TourEvent>();
            TourEvent fadeOutMedia13a = new FadeOutMediaEvent(dockItem13a, 1);
            TourEvent fadeOutMedia13b = new FadeOutMediaEvent(dockItem13b, 1);
            /*tourList15c.Add(fadeOutMedia13a);
            tourList15c.Add(fadeOutMedia13b);
            tourDict.Add(220, tourList15c);*/
            this.addAnim(dockItem13a_TL, fadeOutMedia13a, 220);
            this.addAnim(dockItem13b_TL, fadeOutMedia13b, 220);

            // LIST - 3:42 (222)
            //List<TourEvent> tourList16 = new List<TourEvent>();
            TourEvent zoomMSI9 = new ZoomMSIEvent(artModeWin.msi_tour, 0.421, 13804, 845, 7);
            //TourEvent zoomMSI9 = new ZoomMSIEvent(artModeWin.msi_tour, 0.23451357526061, 13804, 845, 7);
            /*tourList16.Add(zoomMSI9);
            tourDict.Add(222, tourList16);*/
            this.addAnim(msi_tour_TL, zoomMSI9, 222);

            // LIST - 3:49 (229)
            // TODO: magnifying glass effect
            //List<TourEvent> tourList17 = new List<TourEvent>();
            DockableItem dockItem14a = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\Zoom14a.png");
            dockableItemsLoaded.Add(dockItem14a);
            dockItem14a.Visibility = Visibility.Hidden;
            TourParallelTL dockItem14a_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem14a_TL);
            DockableItem dockItem14b = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Images\\Zoom14b.png");
            dockableItemsLoaded.Add(dockItem14b);
            dockItem14b.Visibility = Visibility.Hidden;
            TourParallelTL dockItem14b_TL = new TourParallelTL();
            tourStoryboard.Children.Add(dockItem14b_TL);
            //TourEvent initMedia14a = new InitMediaEvent(dockItem14a, 162.996369796512, 361.981905745962, 0.421);
            //TourEvent initMedia14a = new InitMediaEvent(dockItem14a, 11911, 422, 0.23451357526061);
            //TourEvent fadeInMedia14a = new FadeInMediaEvent(dockItem14a, 1);
            TourEvent fadeInMedia14a = new FadeInMediaEvent(dockItem14a, 162.996369796512, 361.981905745962, 0.421, 1);
            //TourEvent initMedia14b = new InitMediaEvent(dockItem14b, 1790.84402410069, 320.377869946527, 0.421);
            //TourEvent initMedia14b = new InitMediaEvent(dockItem14b, 15777, 323, 0.23451357526061);
            //TourEvent fadeInMedia14b = new FadeInMediaEvent(dockItem14b, 1);
            TourEvent fadeInMedia14b = new FadeInMediaEvent(dockItem14b, 1790.84402410069, 320.377869946527, 0.421, 1);
            /*tourList17.Add(initMedia14a);
            tourList17.Add(fadeInMedia14a);
            tourList17.Add(initMedia14b);
            tourList17.Add(fadeInMedia14b);
            tourDict.Add(229, tourList17);*/
            //this.addAnim(dockItem14a_TL, initMedia14a, 229);
            this.addAnim(dockItem14a_TL, fadeInMedia14a, 229);
            //this.addAnim(dockItem14b_TL, initMedia14b, 229);
            this.addAnim(dockItem14b_TL, fadeInMedia14b, 229);

            // LIST - 3:50 (230)
            //List<TourEvent> tourList17a = new List<TourEvent>();
            TourEvent zoomMedia14a = new ZoomMediaEvent(dockItem14a, 1, 505.624855996816, 429.598477844792, 1);
            //TourEvent zoomMedia14a = new ZoomMediaEvent(dockItem14a, 1, 12725, 583, 1);
            TourEvent zoomMedia14b = new ZoomMediaEvent(dockItem14b, 1, 1430.62467538835, 429.598477844792, 1);
            //TourEvent zoomMedia14b = new ZoomMediaEvent(dockItem14b, 1, 14922, 583, 1);
            /*tourList17a.Add(zoomMedia14a);
            tourList17a.Add(zoomMedia14b);
            tourDict.Add(230, tourList17a);*/
            this.addAnim(dockItem14a_TL, zoomMedia14a, 230);
            this.addAnim(dockItem14b_TL, zoomMedia14b, 230);

            // LIST - 3:58 (238)
            //List<TourEvent> tourList17b = new List<TourEvent>();
            TourEvent zoomMedia14a_out = new ZoomMediaEvent(dockItem14a, 0.421, 162.996369796512, 361.981905745962, 1);
            //TourEvent zoomMedia14a_out = new ZoomMediaEvent(dockItem14a, 0.23451357526061, 11911, 422, 1);
            TourEvent zoomMedia14b_out = new ZoomMediaEvent(dockItem14b, 0.421, 1790.84402410069, 320.377869946527, 1);
            //TourEvent zoomMedia14b_out = new ZoomMediaEvent(dockItem14b, 0.23451357526061, 15778, 323, 1);
            /*tourList17b.Add(zoomMedia14a_out);
            tourList17b.Add(zoomMedia14b_out);
            tourDict.Add(238, tourList17b);*/
            this.addAnim(dockItem14a_TL, zoomMedia14a_out, 238);
            this.addAnim(dockItem14b_TL, zoomMedia14b_out, 238);

            // LIST - 4:00 (240)
            // List<TourEvent> tourList17c = new List<TourEvent>();
            TourEvent fadeOutMedia14a = new FadeOutMediaEvent(dockItem14a, 1);
            TourEvent fadeOutMedia14b = new FadeOutMediaEvent(dockItem14b, 1);
            /*tourList17c.Add(fadeOutMedia14a);
            tourList17c.Add(fadeOutMedia14b);
            tourDict.Add(240, tourList17c);*/
            this.addAnim(dockItem14a_TL, fadeOutMedia14a, 240);
            this.addAnim(dockItem14b_TL, fadeOutMedia14b, 240);

            // LIST - 4:02 (242)
            //List<TourEvent> tourList18 = new List<TourEvent>();
            //TourEvent zoomMSI10 = new ZoomMSIEvent(artModeWin.msi, 1.2, 12552, 820, 16.5);
            TourEvent zoomMSI10 = new ZoomMSIEvent(artModeWin.msi_tour, 1.2, 12552, 820, 16.5); // * 0.8
            /*tourList18.Add(zoomMSI10);
            tourDict.Add(242, tourList18);*/
            this.addAnim(msi_tour_TL, zoomMSI10, 242);

            // LIST - 4:24 (264)
            //List<TourEvent> tourList19 = new List<TourEvent>();
            //TourEvent zoomMSI11 = new ZoomMSIEvent(artModeWin.msi, 0.811, 12552, 684, 7);
            TourEvent zoomMSI11 = new ZoomMSIEvent(artModeWin.msi_tour, 0.811, 12552, 684, 7); // * 0.8
            /*tourList19.Add(zoomMSI11);
            tourDict.Add(262, tourList19);*/
            this.addAnim(msi_tour_TL, zoomMSI11, 264);


            // LIST - 4:38 (278)
            //List<TourEvent> tourList20 = new List<TourEvent>();
            //TourEvent zoomMSI12 = new ZoomMSIEvent(artModeWin.msi, 0.0851, 10919, 1098, 8);
            TourEvent zoomMSI12 = new ZoomMSIEvent(artModeWin.msi_tour, 0.0851, 10919, 1098, 8); // * 0.8
            /*tourList20.Add(zoomMSI12);
            tourDict.Add(276, tourList20);*/
            this.addAnim(msi_tour_TL, zoomMSI12, 278);

            // LIST - 4:49 (289)
            /*List<TourEvent> tourList21 = new List<TourEvent>();
            TourEvent fadeOutMSI0 = new FadeOutMSIEvent(artModeWin.msi, 1);
            tourList21.Add(fadeOutMSI0);
            tourDict.Add(289, tourList21);*/

            TourMediaTL tourAudio_timeline = new TourMediaTL(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\ChineseScrollTourRev.mp3", UriKind.Absolute));
            tourAudio_element = new MediaElement();
            tourAudio_element.Volume = 0.99;
            //_audioMediaElement.Name = “audioMediaElement”;
            //RegisterName(_audioMediaElement.Name, _audioMediaElement);

            tourAudio_element.LoadedBehavior = MediaState.Manual;
            tourAudio_element.UnloadedBehavior = MediaState.Manual;

            Storyboard.SetTarget(tourAudio_timeline, tourAudio_element);
            tourStoryboard.SlipBehavior = SlipBehavior.Slip;
            tourStoryboard.Children.Add(tourAudio_timeline);

            // for proof-of-concept only
            TagLib.File audio_file_tags = TagLib.File.Create(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\ChineseScrollTourRev.mp3");
            tourStoryboard.Duration = audio_file_tags.Properties.Duration;
        }

        #endregion

        #region tour testing buttons (in left sidebar of artwork mode)

        public void loadTourButtons()
        {
            artModeWin.TourScroll.Items.Clear();
            IEnumerable<string> tours = Directory.EnumerateFiles(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\XML");
            foreach (string filepath in tours)
            {
                String name = "";
                XmlDocument doc = new XmlDocument();
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
                }

                string filename = System.IO.Path.GetFileNameWithoutExtension(filepath);
                SurfaceButton button = new SurfaceButton();
                button.Content = name;
                button.Tag = filename;
                button.Click += new RoutedEventHandler(TourButton_Click);
                artModeWin.TourScroll.Items.Add(button);
                object o = button.Parent;
                artModeWin.TourScroll.SelectionChanged += new SelectionChangedEventHandler(TourScroll_SelectionChanged);
            }

        }

        void TourScroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            artModeWin.TourScroll.UnselectAll();
            /*
            string filename = (string)(button).Content;
            this.LoadDictFromXML(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\XML\\" + filename + ".xml");
            this.LoadTourPlaybackFromDict();

            if (!tourPlaybackOn)
            {
                artModeWin.msi.Visibility = Visibility.Hidden;
                artModeWin.msi_thumb.Visibility = Visibility.Hidden;

                artModeWin.msi_tour.DisableEventHandlers();
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
                artModeWin.activateKW.Visibility = Visibility.Collapsed;
                artModeWin.resetArtworkButton.Visibility = Visibility.Collapsed;
                artModeWin.exitButton.Visibility = Visibility.Collapsed;
                artModeWin.HotspotOverlay.Visibility = Visibility.Collapsed;
                //artModeWin.tourResumeButton.Visibility = Visibility.Visible;
                //artModeWin.tourPauseButton.Visibility = Visibility.Visible;
                artModeWin.tourControlButton.Visibility = Visibility.Visible;
                artModeWin.tourStopButton.Visibility = Visibility.Visible;
                //   artModeWin.showMetaList();
                artModeWin.tourSeekBar.Visibility = Visibility.Visible;

                tourStoryboard.CurrentTimeInvalidated += new EventHandler(TourStoryboardPlayback_CurrentTimeInvalidated);
                tourStoryboard.Completed += new EventHandler(TourStoryboardPlayback_Completed);
                tourStoryboard.Begin(artModeWin, true);
                tourPlaybackOn = true;
            }*/
        }
        private void artareasizechanged(object sender, EventArgs e)
        {
            int i = 0;
            i++;
            int j;
            int k;
        }

        private void TourButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = (string)(sender as SurfaceButton).Tag; //this makes it try to find the file with the name on the button
            this.LoadDictFromXML(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\XML\\" + filename + ".xml");
            this.LoadTourPlaybackFromDict();

            if (!tourPlaybackOn)
            {
                artModeWin.msi.Visibility = Visibility.Hidden;
                artModeWin.msi_thumb.Visibility = Visibility.Hidden;

                artModeWin.msi_tour.DisableEventHandlers();
                artModeWin.msi_tour.ResetArtwork();
                artModeWin.msi_tour.Visibility = Visibility.Visible;
                artModeWin.InitTourLayout();

                artModeWin.msi_tour_thumb.Visibility = Visibility.Visible;
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.AddValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.RemoveValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                this.msi_tour_ViewboxUpdate();

                artModeWin.toggleLeftSide();
                artModeWin.tourAuthoringButton.Visibility = Visibility.Collapsed;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Collapsed;
                //artModeWin.activateKW.Visibility = Visibility.Collapsed;
                artModeWin.resetArtworkButton.Visibility = Visibility.Collapsed;
                artModeWin.exitButton.Visibility = Visibility.Collapsed;
                artModeWin.HotspotOverlay.Visibility = Visibility.Collapsed;
                //artModeWin.tourResumeButton.Visibility = Visibility.Visible;
                //artModeWin.tourPauseButton.Visibility = Visibility.Visible;
                artModeWin.tourControlButton.Visibility = Visibility.Visible;
                artModeWin.tourStopButton.Visibility = Visibility.Visible;
                //   artModeWin.showMetaList();
                artModeWin.tourSeekBar.Visibility = Visibility.Visible;

                tourStoryboard.CurrentTimeInvalidated += new EventHandler(TourStoryboardPlayback_CurrentTimeInvalidated);
                tourStoryboard.Completed += new EventHandler(TourStoryboardPlayback_Completed);
                tourStoryboard.Begin(artModeWin, true);
                tourPlaybackOn = true;
                Console.WriteLine("Current width" + artModeWin.ImageArea.ActualWidth);
                Console.WriteLine("Current height" + artModeWin.ImageArea.ActualHeight);
            }
        }

        public void loadAuthoringGUI()
        {
            if (!tourAuthoringOn)
            {
                artModeWin.msi.Visibility = Visibility.Hidden;
                artModeWin.msi_thumb.Visibility = Visibility.Hidden;

                //artModeWin.hotspot
                artModeWin.MainScatterView.Visibility = Visibility.Hidden;
                //artModeWin.msi_tour.DisableEventHandlers();
                artModeWin.msi_tour.ResetArtwork();
                artModeWin.msi_tour.Visibility = Visibility.Visible;

                artModeWin.msi_tour_thumb.Visibility = Visibility.Visible;
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.AddValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.RemoveValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                this.msi_tour_ViewboxUpdate();

                //artModeWin.toggleLeftSide();
                artModeWin.LeftPanel.Visibility = Visibility.Hidden;
                artModeWin.tourAuthoringButton.Visibility = Visibility.Collapsed;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Collapsed;
                //artModeWin.activateKW.Visibility = Visibility.Collapsed;
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


        public void Panel43TourSaveXMLButton_Click(object sender, RoutedEventArgs e)
        {
            this.SaveDictToXML(@"Data\Tour\XML\GaribaldiPanel43Tour_Saved.xml");
        }

        public void Panel43TourAuthoringButton_Click(object sender, RoutedEventArgs e)
        {
            this.LoadDictFromXML(@"Data\Tour\XML\GaribaldiPanel43Tour.xml");
            this.LoadTourPlaybackFromDict();
            this.LoadTourAuthoringUIFromDict();

            if (!tourAuthoringOn)
            {
                artModeWin.msi.Visibility = Visibility.Hidden;
                artModeWin.msi_thumb.Visibility = Visibility.Hidden;

                //artModeWin.msi_tour.DisableEventHandlers();
                artModeWin.msi_tour.ResetArtwork();
                artModeWin.msi_tour.Visibility = Visibility.Visible;

                artModeWin.msi_tour_thumb.Visibility = Visibility.Visible;
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomableCanvas.ActualViewboxProperty, typeof(ZoomableCanvas));
                dpd.AddValueChanged(artModeWin.msi_tour.GetZoomableCanvas, msi_tour_ViewboxChanged);
                dpd.RemoveValueChanged(artModeWin.msi.GetZoomableCanvas, artModeWin.msi_ViewboxChanged);
                this.msi_tour_ViewboxUpdate();

                //artModeWin.toggleLeftSide();
                artModeWin.LeftPanel.Visibility = Visibility.Hidden;
                artModeWin.tourAuthoringButton.Visibility = Visibility.Collapsed;
                artModeWin.switchToCatalogButton.Visibility = Visibility.Collapsed;
                //artModeWin.activateKW.Visibility = Visibility.Collapsed;
                artModeWin.resetArtworkButton.Visibility = Visibility.Collapsed;
                artModeWin.exitButton.Visibility = Visibility.Collapsed;
                artModeWin.HotspotOverlay.Visibility = Visibility.Collapsed;
                //artModeWin.tourResumeButton.Visibility = Visibility.Visible;
                //artModeWin.tourPauseButton.Visibility = Visibility.Visible;
                //artModeWin.tourAuthoringDoneButton.Visibility = Visibility.Visible;
                //artModeWin.drawPaths.Visibility = Visibility.Visible;
                //artModeWin.metaData.Visibility = Visibility.Visible;

                //artModeWin.tourAuthoringSaveButton.Visibility = Visibility.Visible;
                //artModeWin.addAudioButton.Visibility = Visibility.Visible;

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

                //artModeWin.msi_tour.DisableEventHandlers();
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
                //artModeWin.activateKW.Visibility = Visibility.Collapsed;
                artModeWin.resetArtworkButton.Visibility = Visibility.Collapsed;
                artModeWin.exitButton.Visibility = Visibility.Collapsed;
                artModeWin.HotspotOverlay.Visibility = Visibility.Collapsed;
                //artModeWin.tourResumeButton.Visibility = Visibility.Visible;
                //artModeWin.tourPauseButton.Visibility = Visibility.Visible;
                //artModeWin.tourAuthoringDoneButton.Visibility = Visibility.Visible;
                //artModeWin.drawPaths.Visibility = Visibility.Visible;
                //artModeWin.metaData.Visibility = Visibility.Visible;

                //artModeWin.tourAuthoringSaveButton.Visibility = Visibility.Visible;
                //artModeWin.addAudioButton.Visibility = Visibility.Visible;

                tourStoryboard.CurrentTimeInvalidated += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_CurrentTimeInvalidated);
                tourStoryboard.Completed += new EventHandler(tourAuthoringUI.TourStoryboardAuthoring_Completed);
                tourStoryboard.Begin(artModeWin, true);
                tourStoryboard.Pause(artModeWin);

                artModeWin.BottomPanel.Visibility = Visibility.Collapsed;
                artModeWin.tourAuthoringUICanvas.Visibility = Visibility.Visible;

                tourAuthoringOn = true;
            }
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
                        //////

                        dockableItemsLoaded = new List<DockableItem>();
                        msiItemsLoaded = new List<MultiScaleImage>();
                        tourDict = new Dictionary<Timeline, Dictionary<double, TourEvent>>();
                        tourDictRev = new Dictionary<Timeline, Dictionary<TourEvent, double>>();
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
                                    artModeWin.msi_tour.disableInertia();
                                    TourParallelTL msi_tour_TL = new TourParallelTL();

                                    msiToTLDict.Add(artModeWin.msi_tour, msi_tour_TL);


                                    msi_tour_TL.type = TourTLType.artwork;
                                    msi_tour_TL.displayName = TLNode.Attributes.GetNamedItem("displayName").InnerText;
                                    msi_tour_TL.file = TLNode.Attributes.GetNamedItem("file").InnerText;

                                    Dictionary<double, TourEvent> msi_tour_TL_dict = new Dictionary<double, TourEvent>();
                                    tourDict.Add(msi_tour_TL, msi_tour_TL_dict);
                                    Dictionary<TourEvent, double> msi_tour_TL_dict_rev = new Dictionary<TourEvent, double>();
                                    tourDictRev.Add(msi_tour_TL, msi_tour_TL_dict_rev);

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
                                                msi_tour_TL_dict_rev.Add(zoomMSIEvent, beginTime);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "media")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    DockableItem dockItem = new DockableItem(artModeWin.MSIScatterView, artModeWin, artModeWin.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Metadata\\" + media_file);

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

                                    Dictionary<double, TourEvent> dockItem_TL_dict = new Dictionary<double, TourEvent>();
                                    Dictionary<TourEvent, double> dockItem_TL_dict_rev = new Dictionary<TourEvent, double>();
                                    tourDict.Add(dockItem_TL, dockItem_TL_dict);
                                    tourDictRev.Add(dockItem_TL, dockItem_TL_dict_rev);

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
                                                dockItem_TL_dict_rev.Add(fadeInMediaEvent, beginTime);
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
                                                dockItem_TL_dict_rev.Add(zoomMediaEvent, beginTime);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutMediaEvent")
                                            {
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutMediaEvent = new FadeOutMediaEvent(dockItem, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                dockItem_TL_dict.Add(beginTime, fadeOutMediaEvent);
                                                dockItem_TL_dict_rev.Add(fadeOutMediaEvent, beginTime);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "highlight")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    SurfaceInkCanvas sic = new SurfaceInkCanvas();
                                    //sic.IsVisibleChanged+= new DependencyPropertyChangedEventHandler(drawVisibilityChanged);

                                    //currentHighlightCanvas = new SurfaceInkCanvas();
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
                                    Dictionary<double, TourEvent> tldict = new Dictionary<double, TourEvent>();
                                    tourDict.Add(tourtl, tldict);
                                    Dictionary<TourEvent, double> tldictrev = new Dictionary<TourEvent, double>();
                                    tourDictRev.Add(tourtl, tldictrev);

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
                                                tldictrev.Add(fadeInHighlightEvent, beginTime);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutHighlightEvent")
                                            {
                                                double opacity = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("opacity").InnerText);
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutHighlightEvent = new FadeOutHighlightEvent(sic, duration, opacity);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeOutHighlightEvent);
                                                tldictrev.Add(fadeOutHighlightEvent, beginTime);
                                            }
                                        }
                                    }
                                }

                                else if (TLNode.Attributes.GetNamedItem("type").InnerText == "path")
                                {
                                    String media_file = TLNode.Attributes.GetNamedItem("file").InnerText;
                                    SurfaceInkCanvas sic = new SurfaceInkCanvas();
                                    //sic.IsVisibleChanged += new DependencyPropertyChangedEventHandler(drawVisibilityChanged);

                                    //currentHighlightCanvas = new SurfaceInkCanvas();
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
                                    Dictionary<double, TourEvent> tldict = new Dictionary<double, TourEvent>();
                                    tourDict.Add(tourtl, tldict);
                                    Dictionary<TourEvent, double> tldictrev = new Dictionary<TourEvent, double>();
                                    tourDictRev.Add(tourtl, tldictrev);

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
                                                tldictrev.Add(fadeInPathEvent, beginTime);
                                            }

                                            else if (TourEventNode.Attributes.GetNamedItem("type").InnerText == "FadeOutPathEvent")
                                            {
                                                double duration = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("duration").InnerText);

                                                TourEvent fadeOutPathEvent = new FadeOutPathEvent(sic, duration);

                                                double beginTime = Convert.ToDouble(TourEventNode.Attributes.GetNamedItem("beginTime").InnerText);
                                                tldict.Add(beginTime, fadeOutPathEvent);
                                                tldictrev.Add(fadeOutPathEvent, beginTime);
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

                                Dictionary<double, TourEvent> tourAudio_TL_dict = new Dictionary<double, TourEvent>(); // dummy TL_dict -- tourAudio_timeline obviously doesn't store any TourEvents
                                tourDict.Add(tourAudio_TL, tourAudio_TL_dict);

                                tourAudio_element = new MediaElement();
                                tourAudio_element.Volume = 0.99;

                                tourAudio_element.LoadedBehavior = MediaState.Manual;
                                tourAudio_element.UnloadedBehavior = MediaState.Manual;

                                Storyboard.SetTarget(tourAudio_TL, tourAudio_element);
                                tourStoryboard.SlipBehavior = SlipBehavior.Slip;

                                // took me quite a while to figure out that WPF really can't determine the duration of an MP3 until it's actually loaded (i.e. playing), and then it took me a little longer to finally find and accept this open-source library...argh
                                TagLib.File audio_file_tags = TagLib.File.Create(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Tour\\Audio\\" + audio_file);
                                tourAudio_TL.Duration = audio_file_tags.Properties.Duration;
                            }
                        }
                    }
                }
            }
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

            foreach (KeyValuePair<Timeline, Dictionary<double, TourEvent>> tourDict_KV in tourDict)
            {
                TourTL tourTL = (TourTL)tourDict_KV.Key;
                Dictionary<double, TourEvent> tourTL_dict = tourDict_KV.Value;

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

                    foreach (KeyValuePair<double, TourEvent> tourTL_dict_KV in tourTL_dict)
                    {
                        double beginTime = tourTL_dict_KV.Key;
                        TourEvent tourEvent = tourTL_dict_KV.Value;

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
                    TLNode_type.Value = tourTL.type.ToString();
                    TLNode_displayName.Value = tourTL.displayName;
                    TLNode_file.Value = tourTL.file;
                    TLNode.Attributes.Append(TLNode_type);
                    TLNode.Attributes.Append(TLNode_displayName);
                    TLNode.Attributes.Append(TLNode_file);
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

            foreach (KeyValuePair<Timeline, Dictionary<double, TourEvent>> tourDict_KV in tourDict)
            {
                Timeline tourTL = tourDict_KV.Key;
                Dictionary<double, TourEvent> tourTL_dict = tourDict_KV.Value;

                double tourTL_endTime = 0.0;

                foreach (KeyValuePair<double, TourEvent> tourTL_dict_KV in tourTL_dict) // MediaTimeline will ignore this
                {
                    double beginTime = tourTL_dict_KV.Key;
                    TourEvent tourEvent = tourTL_dict_KV.Value;

                    double tourEvent_endTime = beginTime + tourEvent.duration;

                    if (tourEvent_endTime > tourTL_endTime)
                    {
                        tourTL_endTime = tourEvent_endTime;
                    }
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

            foreach (KeyValuePair<Timeline, Dictionary<double, TourEvent>> tourDict_KV in tourDict)
            {
                Timeline tourTL = tourDict_KV.Key;
                Dictionary<double, TourEvent> tourTL_dict = tourDict_KV.Value;

                if ((((TourTL)tourTL).type == TourTLType.artwork) || (((TourTL)tourTL).type == TourTLType.media) || (((TourTL)tourTL).type == TourTLType.path) || (((TourTL)tourTL).type == TourTLType.highlight))
                {
                    ((TourParallelTL)tourTL).Children.Clear(); // clear timelines - necessary when editing
                }

                double tourTL_endTime = 0.0;

                List<double> keys = new List<double>(tourTL_dict.Keys);
                keys.Sort();

                foreach (double beginTime in keys)
                {
                    TourEvent tourEvent = tourTL_dict[beginTime];
                    double tourEvent_endTime = beginTime + tourEvent.duration;

                    if (tourEvent_endTime > tourTL_endTime)
                    {
                        tourTL_endTime = tourEvent_endTime;
                    }

                    this.addAnim((TourParallelTL)tourTL, tourEvent, beginTime);
                }
                /*
                foreach (KeyValuePair<double, TourEvent> tourTL_dict_KV in tourTL_dict) // MediaTimeline will ignore this
                {
                    double beginTime = tourTL_dict_KV.Key;
                    TourEvent tourEvent = tourTL_dict_KV.Value;

                    double tourEvent_endTime = beginTime + tourEvent.duration;

                    if (tourEvent_endTime > tourTL_endTime)
                    {
                        tourTL_endTime = tourEvent_endTime;
                    }

                    this.addAnim((TourParallelTL)tourTL, tourEvent, beginTime);
                }*/

                if (!tourTL.Duration.HasTimeSpan) // only MediaTimeline will fail this
                {
                    //tourTL.Duration = TimeSpan.FromSeconds(tourTL_endTime);
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


            //Console.Out.WriteLine("reload here");
            tourAuthoringUI = new TourAuthoringUI(artModeWin, this);

            tourAuthoringUI.timelineCount = tourDict.Count; //clear the old yellow moveable thing


            //tourAuthoringUI = new TourAuthoringUI(artModeWin, this);
            tourAuthoringUI.ClearAuthoringUI();

            tourAuthoringUI.timelineCount = tourDict.Count;

            //if (tourStoryboard.Duration.TimeSpan.TotalSeconds < 60)
            //    tourStoryboard.Duration = TimeSpan.FromSeconds(60);
            tourAuthoringUI.timelineLength = tourStoryboard.Duration.TimeSpan.TotalSeconds;

            tourAuthoringUI.canvasWrapper.Children.Clear();
            tourAuthoringUI.initialize();
            tourAuthoringUI.initAuthTools();

            int i = 0;
            foreach (KeyValuePair<Timeline, Dictionary<double, TourEvent>> tourDict_KV in tourDict)
            {
                Timeline tourTL = tourDict_KV.Key;
                Dictionary<double, TourEvent> tourTL_dict = tourDict_KV.Value;

                TourAuthoringUI.timelineInfo timelineInfo = tourAuthoringUI.addTimeline(tourTL, tourTL_dict, ((TourTL)tourTL).displayName, i * tourAuthoringUI.timelineHeight);

                foreach (KeyValuePair<double, TourEvent> tourTL_dict_KV in tourTL_dict) // MediaTimeline will ignore this
                {
                    double beginTime = tourTL_dict_KV.Key;
                    TourEvent tourEvent = tourTL_dict_KV.Value;
                    tourAuthoringUI.addTourEvent(timelineInfo, tourEvent, timelineInfo.lengthSV, beginTime, tourEvent.duration);
                }

                if (((TourTL)tourTL).type == TourTLType.audio) // for MediaTimeline
                {
                    tourAuthoringUI.addTourEvent(timelineInfo, null, timelineInfo.lengthSV, 0, tourTL.Duration.TimeSpan.TotalSeconds); // will this work?
                }

                i++;
            }
        }



        public void refreshAuthoringUI(bool completeReload)
        {
            tourAuthoringUI.timelineCount = tourDict.Count; //clear the old yellow moveable thing

            Point center = tourAuthoringUI.leftRightSVI.ActualCenter;
            //tourAuthoringUI = new TourAuthoringUI(artModeWin, this);
            if (!completeReload)
                tourAuthoringUI.ClearTimelines();
            else
                tourAuthoringUI.ClearAuthoringUI();

            tourAuthoringUI.timelineCount = tourDict.Count;

            //if (tourStoryboard.Duration.TimeSpan.TotalSeconds < 60)
            //    tourStoryboard.Duration = TimeSpan.FromSeconds(60);
            tourAuthoringUI.timelineLength = tourStoryboard.Duration.TimeSpan.TotalSeconds;

            if (!completeReload)
                tourAuthoringUI.reinitalize();
            else
                tourAuthoringUI.initialize();
            int i = 0;
            foreach (KeyValuePair<Timeline, Dictionary<double, TourEvent>> tourDict_KV in tourDict)
            {
                Timeline tourTL = tourDict_KV.Key;
                Dictionary<double, TourEvent> tourTL_dict = tourDict_KV.Value;

                TourAuthoringUI.timelineInfo timelineInfo = tourAuthoringUI.addTimeline(tourTL, tourTL_dict, ((TourTL)tourTL).displayName, i * tourAuthoringUI.timelineHeight);

                foreach (KeyValuePair<double, TourEvent> tourTL_dict_KV in tourTL_dict) // MediaTimeline will ignore this
                {
                    double beginTime = tourTL_dict_KV.Key;
                    TourEvent tourEvent = tourTL_dict_KV.Value;
                    tourAuthoringUI.addTourEvent(timelineInfo, tourEvent, timelineInfo.lengthSV, beginTime, tourEvent.duration);
                }

                if (((TourTL)tourTL).type == TourTLType.audio) // for MediaTimeline
                {
                    tourAuthoringUI.addTourEvent(timelineInfo, null, timelineInfo.lengthSV, 0, tourTL.Duration.TimeSpan.TotalSeconds); // will this work?
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
            //refreshAuthoringUI();
            //if (tourAuthoringOn) this.LoadTourAuthoringUIFromDict();

            //artModeWin.msi_tour.UpdateLayout(); // trying to fix bug where first ZoomMSIEvent can't be previewed properly after its beginTime is modified, as the animation just continues from where it left off before it was moved 
            //artModeWin.msi_tour.ResetArtwork();

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
