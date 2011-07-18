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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Threading;
using LADSArtworkMode.TourEvents;

namespace LADSArtworkMode
{
    class TourAuthoringUI
    {
        #region global variables

        ArtworkModeWindow artModeWin;
        TourSystem tourSystem;

        public Canvas canvasWrapper; // layer #1 - parent canvas

        List<ScatterViewItem> currentTouched = new List<ScatterViewItem>();

        double w; // width of parent canvas (in this case, canvasWrapper)
        double timelineAreaHeight; // height of all timelines added together (regardless of the need to scroll down)
        double textWidth = 0; // first column of the timeline area that contains the names of the timeline items
        double timelineRulerTickInterval = 0;
        double timelineWidth = 0; // width of a timeline (must accomodate widest timeline that is needed)
        public double timelineLength = 0; // duration in seconds
        public double timelineHeight = 0; // height of a single timline
        double centerX = 0; // x-coordinate of the center of the screen (parent canvas/window)
        double centerX_LRScatterView = 0; // x-coordinate of the center of a timeline
        double centerX_LRScatterView_diff; // difference between where centerX_LRScatterView is with respect to where it would be if it is aligned with the left side of the window
        double centerY = 0; // y-coordinate of the center of the screen (parent canvas/window)
        double windowW = 0; // width of parent canvas/window
        public int timelineCount = 0; // number of timelines
        double curWidth = 0;
        double oldWidth = 0;
        double timeLineBarHeight = 0;

        SurfaceButton tourControlButton;

        Rectangle movableScrubHandleBackground; // part of canvasWrapper
        Rectangle movableScrubHandle; // part of canvasWrapper
        Rectangle movableScrubHandleExt; // part of canvasWrapper
        bool movableScrubHandle_userDragged;
        Rectangle movableScrub; // part of leftRightCanvas
        Rectangle timelineAreaTopLeft; // part of canvasWrapper
        ScatterView timelineRulerSV;  // layer #2
        ScatterViewItem timelineRulerSVI; // layer #2.1
        bool timelineRulerSVI_userDragged;
        Canvas timelineRulerCanvas; // layer #2.1.1

        Point startDragPoint;
        double tourSeekBarProgressWidth = 0;

        private TimeSpan tourCurrentTime, tourDuration;
        private String tourCurrentTimeString, tourDurationString;
        private double tourTimerCount;
        private TimeSpan tourTimerCountSpan;
        private String tourTimerCountSpanString;

        private Label tourSeekBarTimerCount;
        private Label tourSeekBarLength;

        ScatterView mainSV; // layer #2
        ScatterViewItem mainSVI; // layer #2.1
        Canvas mainCanvas; // layer #2.1.1

        ScatterView leftRightSV; // layer #3
        public ScatterViewItem leftRightSVI; // layer #3.1
        public Canvas leftRightCanvas; // layer #3.1.1

        Canvas titleCanvas; // layer #4.1

        bool scatterViewLR_userDragged;
        bool backgroundMoved;

        Rectangle currentHighlightedTL = null;
        public timelineInfo highlightData;
        List<timelineInfo> timeLineList;

        DispatcherTimer highlightTimer = null; // was used for highlighting, but I'm not sure if it's necessary -- it may be useful to add a delay

        public ScatterViewItem highlightedTourEvent;

        private SurfaceInkCanvas inkCanvas, highlightInkCanvas;

        public bool highlightActive = false;


        public struct timelineInfo // struct for a timeline
        {
            public Timeline timeline; // the timeline that will be added to tourStoryboard
            public BiDictionary<double, TourEvent> tourTL_dict;

            public String title; // corresponds to "displayName" in XML file for the tour

            public double pos; // y-position within leftRightCanvas
            public TextBox titlebox;
            public ScatterView lengthSV; // the physical timeline ScatterView
        }

        public struct tourEventInfo // struct for a TourEvent bar
        {
            public timelineInfo timelineInfoStruct;
            public double beginTime;
            public TourEvent tourEvent;
            public Double originalLoc;

            public Rectangle r; // the physical TourEvent bar
            public Double centerX;
            public Double centerY;
        }

        #endregion

        public TourAuthoringUI(ArtworkModeWindow artModeWinParam, TourSystem tourSystemParam)
        {
            artModeWin = artModeWinParam;
            tourSystem = tourSystemParam;

            // artModeWin.tourSeekBarTimeDisplayBackground.TouchDown +=new EventHandler<TouchEventArgs>(tourSeekBarTimeDisplayBackground_TouchDown);
            canvasWrapper = artModeWin.tourAuthoringUICanvas;
            opacitySlider = new SurfaceSlider();

            w = artModeWin.Width;
            timelineHeight = 40; // 40 px is a good guideline to follow for touchability -- borrowed from Microsoft's touch guidelines

            windowW = w;
            centerX = w / 2;

            timelineLength = 0;
            timelineCount = 0;

            tourSeekBarTimerCount = new Label();
            tourSeekBarLength = new Label();
            backgroundMoved = false;
        }

        #region init methods

        public void reinitalize()
        {
            timelineAreaHeight = (timelineHeight * timelineCount) + 16;
            if (timelineAreaHeight < (canvasWrapper.Height - 60))
            {
                timelineAreaHeight = canvasWrapper.Height - 60; // minimum height to fill UI space on bottom of screen
            }

            timeLineList = new List<timelineInfo>();
            centerY = timelineAreaHeight / 2;

            // mainSV, mainSVI, mainCanvas
            //mainSV = new ScatterView();
            mainSV.Height = timelineAreaHeight;
            //mainSV.Width = w;

            //if (w < 192)
            //    textWidth = w / 10;
            //else
            //    textWidth = 192;

            //timelineRulerTickInterval = (w - textWidth) / 15.0;
            //timelineWidth = timelineRulerTickInterval * timelineLength;

            //mainSVI = new ScatterViewItem();
            mainSVI.Height = timelineAreaHeight;
            //mainSVI.Width = w;
            //mainSVI.CanRotate = false;
            //mainSVI.CanScale = false;
            //mainSVI.Center = new Point(w / 2, timelineAreaHeight / 2);
            //mainSVI.Orientation = 0;
            //mainSVI.PreviewTouchUp += new EventHandler<TouchEventArgs>(mainCanvas_PreviewTouchUp);
            //mainScatterViewItem.PreviewTouchDown += new EventHandler<TouchEventArgs>(mainScatterViewItem_PreviewTouchDown); // needed if timer is used
            //mainScatterViewItem.PreviewTouchUp += new EventHandler<TouchEventArgs>(mainScatterViewItem_PreviewTouchUp); // needed if timer is used

            //DependencyPropertyDescriptor dpd1 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            //dpd1.AddValueChanged(mainSVI, MainSVICenterChanged); // for scrolling list of timelines (panning up and down)

            //mainCanvas = new Canvas();
            mainCanvas.Height = timelineAreaHeight;
            //mainCanvas.Width = w;

            //mainSVI.Content = mainCanvas;

            // leftRightSV, leftRightSVI, leftRightCanvas
            //leftRightSV = new ScatterView();
            leftRightSV.Width = timelineWidth;
            leftRightSV.Height = timelineAreaHeight;

            //leftRightSVI = new ScatterViewItem();
            leftRightSVI.Height = timelineAreaHeight;
            leftRightSVI.Width = timelineWidth;
            //centerX_LRScatterView = leftRightSVI.Width / 2;
            //centerX_LRScatterView_diff = textWidth;
            leftRightSVI.Center = new Point(leftRightSVI.Center.X, centerY);
            //leftRightSVI.Orientation = 0;
            //leftRightSVI.CanRotate = false;
            //leftRightSVI.CanScale = false;
            //DependencyPropertyDescriptor dpd2 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            //dpd2.AddValueChanged(leftRightSVI, leftRightSVICenterChanged); // for panning timelines left and right
            scatterViewLR_userDragged = true;

            //leftRightSV.Items.Add(leftRightSVI);

            //leftRightCanvas = new Canvas();
            leftRightCanvas.Height = timelineAreaHeight;
            leftRightCanvas.Width = timelineWidth;
            //leftRightCanvas.Background = (Brush)(new BrushConverter().ConvertFrom("#79aa89"));
            //leftRightSVI.Content = leftRightCanvas;

            //titleCanvas = new Canvas();
            titleCanvas.Height = timelineAreaHeight;
            //titleCanvas.Width = textWidth;

            Rectangle r = new Rectangle();
            r.Height = titleCanvas.Height;
            r.Width = titleCanvas.Width;
            r.Fill = (Brush)(new BrushConverter().ConvertFrom("#093024"));
            titleCanvas.Children.Add(r);

            //mainCanvas.Children.Add(leftRightSV);
            //mainCanvas.Children.Add(titleCanvas);

            // movable scrub bar and its handle
            movableScrub = new Rectangle();
            movableScrub.Height = timelineAreaHeight;
            movableScrub.Width = 3;
            movableScrub.Fill = Brushes.Yellow;

            leftRightCanvas.Children.Add(movableScrub);
            //Console.Out.WriteLine("number of chidren" + leftRightCanvas.Children.Count);
            Canvas.SetLeft(movableScrub, 0);
            Canvas.SetZIndex(movableScrub, 12);
            Canvas.SetTop(movableScrub, 0);

            //movableScrubHandleBackground = new Rectangle();
            //movableScrubHandleBackground.Width = w;
            //movableScrubHandleBackground.Height = 40;
            //movableScrubHandleBackground.Fill = Brushes.DarkRed;
            //movableScrubHandleBackground.TouchDown += new EventHandler<TouchEventArgs>(movableScrubHandleBackground_TouchDown);
            //movableScrubHandleBackground.TouchUp += new EventHandler<TouchEventArgs>(movableScrubHandleBackground_TouchUp);

            //canvasWrapper.Children.Add(movableScrubHandleBackground);
            //Canvas.SetLeft(movableScrubHandleBackground, 0);
            //Canvas.SetTop(movableScrubHandleBackground, 0);
            //Canvas.SetZIndex(movableScrubHandleBackground, 15);

            //movableScrubHandle = new Rectangle();
            //movableScrubHandle.Width = 40;
            //movableScrubHandle.Height = 40;
            //movableScrubHandle.Fill = Brushes.Yellow;

            //canvasWrapper.Children.Add(movableScrubHandle);
            //Canvas.SetLeft(movableScrubHandle, textWidth - 18.5);
            //Canvas.SetTop(movableScrubHandle, 0);
            //Canvas.SetZIndex(movableScrubHandle, 20);
            //DependencyPropertyDescriptor dpd3 = DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(Canvas));
            //dpd3.AddValueChanged(movableScrubHandle, movableScrubHandle_CanvasLeftChanged);

            //startDragPoint = new Point();
            //movableScrubHandle.PreviewTouchDown += new EventHandler<TouchEventArgs>(movableScrubHandle_PreviewTouchDown);
            //movableScrubHandle.PreviewTouchMove += new EventHandler<TouchEventArgs>(movableScrubHandle_PreviewTouchMove);
            //movableScrubHandle.PreviewTouchUp += new EventHandler<TouchEventArgs>(movableScrubHandle_PreviewTouchUp);
            //movableScrubHandle_userDragged = true;

            //movableScrubHandleExt = new Rectangle();
            //movableScrubHandleExt.Width = 3;
            //movableScrubHandleExt.Height = 20;
            //movableScrubHandleExt.Fill = Brushes.Yellow;

            //canvasWrapper.Children.Add(movableScrubHandleExt);
            //Canvas.SetLeft(movableScrubHandleExt, textWidth);
            //Canvas.SetTop(movableScrubHandleExt, 40);
            //Canvas.SetZIndex(movableScrubHandleExt, 12);
            /*
            SurfaceButton tourControlButton = new SurfaceButton();
            tourControlButton.Content = "Start";
            tourControlButton.Height = 30;
            tourControlButton.MinHeight = 30;
            tourControlButton.Width = 80;
            tourControlButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            tourControlButton.Click += tourSystem.TourControlButton_Click;
            canvasWrapper.Children.Add(tourControlButton);
            Canvas.SetLeft(tourControlButton, textWidth - movableScrubHandle.Width / 2 - 5 - tourControlButton.Width);
            Canvas.SetTop(tourControlButton, (movableScrubHandleBackground.Height - tourControlButton.Height) / 2);
            Canvas.SetZIndex(tourControlButton, 15);

            SurfaceButton tourLengthButton = new SurfaceButton();
            tourLengthButton.Content = "Time";
            tourLengthButton.Height = 30;
            tourLengthButton.MinHeight = 30;
            tourLengthButton.Width = 70;
            tourLengthButton.Click += resetTimeButton_Click;
            tourLengthButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            //tourLengthButton.Click += tourSystem.TourControlButton_Click;
            canvasWrapper.Children.Add(tourLengthButton);
            Canvas.SetLeft(tourLengthButton, 5);
            Canvas.SetTop(tourLengthButton, (movableScrubHandleBackground.Height - tourLengthButton.Height) / 2);
            Canvas.SetZIndex(tourLengthButton, 15);
            
            // timelineRulerSV, timelineRulerSVI, timelineRulerCanvas
            timelineAreaTopLeft = new Rectangle();
            timelineAreaTopLeft.Width = textWidth;
            timelineAreaTopLeft.Height = 20;
            timelineAreaTopLeft.Fill = (Brush)(new BrushConverter().ConvertFrom("#5a675f"));

            canvasWrapper.Children.Add(timelineAreaTopLeft);
            Canvas.SetLeft(timelineAreaTopLeft, 0);
            Canvas.SetTop(timelineAreaTopLeft, 40);
            Canvas.SetZIndex(timelineAreaTopLeft, 15);

            canvasWrapper.Children.Add(tourSeekBarTimerCount);
            canvasWrapper.Children.Add(tourSeekBarLength);
            Canvas.SetLeft(tourSeekBarTimerCount, 0);
            Canvas.SetTop(tourSeekBarTimerCount, 35);
            Canvas.SetZIndex(tourSeekBarTimerCount, 15);
            tourSeekBarTimerCount.Foreground = Brushes.Yellow;
            tourSeekBarTimerCount.Content = "00:00";
            Canvas.SetLeft(tourSeekBarLength, 42);
            Canvas.SetTop(tourSeekBarLength, 35);
            Canvas.SetZIndex(tourSeekBarLength, 15);
            tourSeekBarLength.Foreground = Brushes.White;
            tourSeekBarLength.Content = "/ 00:00";
            */

            /*timelineRulerSV = new ScatterView();
            timelineRulerSV.Width = timelineWidth;
            timelineRulerSV.Height = 20.0;

            timelineRulerSVI = new ScatterViewItem();
            timelineRulerSVI.Width = timelineWidth;
            timelineRulerSVI.MinHeight = 0.0; // Apparently, a ScatterViewItem's MinHeight is 80.  Microsoft should document stuff like this.
            timelineRulerSVI.Height = 20.0;
            timelineRulerSVI.Center = new Point(centerX_LRScatterView, timelineRulerSVI.Height / 2);
            timelineRulerSVI.Orientation = 0;
            timelineRulerSVI.CanRotate = false;
            timelineRulerSVI.CanScale = false;
            timelineRulerSVI.Background = new SolidColorBrush(Colors.Transparent);*/
            // the commented block below is supposed to remove the default SVI shadow behind the timelineRulerSVI
            /*RoutedEventHandler loadedEventHandler = null;
            loadedEventHandler = new RoutedEventHandler(delegate
            {
                timelineRulerSVI.Loaded -= loadedEventHandler;
                Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
                ssc = timelineRulerSVI.Template.FindName("shadow", timelineRulerSVI) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
                ssc.Visibility = Visibility.Hidden;
            });
            timelineRulerSVI.Loaded += loadedEventHandler;*/
            // not sure why the commented block above isn't working, but the shadow doesn't seem to be really noticeable anymore

            /*timelineRulerSV.Items.Add(timelineRulerSVI);
            DependencyPropertyDescriptor dpd4 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd2.AddValueChanged(timelineRulerSVI, timelineRulerSVICenterChanged); // for dragging the timeline ruler itself
            timelineRulerSVI_userDragged = true;

            timelineRulerCanvas = new Canvas();
            timelineRulerCanvas.Width = timelineWidth;
            timelineRulerCanvas.Height = 20;
            timelineRulerCanvas.Background = Brushes.Black;
            timelineRulerSVI.Content = timelineRulerCanvas;

            this.addTimelineRulerTickMarks();

            canvasWrapper.Children.Add(timelineRulerSV);
            Canvas.SetLeft(timelineRulerSV, textWidth);
            Canvas.SetTop(timelineRulerSV, 40);
            Canvas.SetZIndex(timelineRulerSV, 10);

            // putting it all together
            mainSV.Items.Add(mainSVI);
            canvasWrapper.Children.Add(mainSV);
            Canvas.SetTop(mainSV, 60);*/

        }

        public void initialize()
        {
            // setting some properties
            timelineAreaHeight = (timelineHeight * timelineCount);
            if (timelineAreaHeight < (canvasWrapper.Height - 60))
            {
                timelineAreaHeight = canvasWrapper.Height - 60; // minimum height to fill UI space on bottom of screen
            }

            timeLineList = new List<timelineInfo>();
            centerY = timelineAreaHeight / 2;

            // mainSV, mainSVI, mainCanvas
            mainSV = new ScatterView();
            mainSV.Height = timelineAreaHeight;
            mainSV.Width = w;

            if (w < 192)
                textWidth = w / 10;
            else
                textWidth = 192;

            timelineRulerTickInterval = (w - textWidth) / 15.0;
            timelineWidth = timelineRulerTickInterval * timelineLength;

            mainSVI = new ScatterViewItem();
            mainSVI.Height = timelineAreaHeight;
            mainSVI.Width = w;
            mainSVI.CanRotate = false;
            mainSVI.CanScale = false;
            mainSVI.Center = new Point(w / 2, timelineAreaHeight / 2);
            mainSVI.Orientation = 0;
            mainSVI.PreviewTouchUp += new EventHandler<TouchEventArgs>(mainCanvas_PreviewTouchUp);
            mainSVI.PreviewMouseUp += new MouseButtonEventHandler(mainCanvas_PreviewMouseUp);
            //mainScatterViewItem.PreviewTouchDown += new EventHandler<TouchEventArgs>(mainScatterViewItem_PreviewTouchDown); // needed if timer is used
            //mainScatterViewItem.PreviewTouchUp += new EventHandler<TouchEventArgs>(mainScatterViewItem_PreviewTouchUp); // needed if timer is used

            DependencyPropertyDescriptor dpd1 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd1.AddValueChanged(mainSVI, MainSVICenterChanged); // for scrolling list of timelines (panning up and down)

            mainCanvas = new Canvas();
            mainCanvas.Height = timelineAreaHeight;
            mainCanvas.Width = w;

            mainSVI.Content = mainCanvas;

            // leftRightSV, leftRightSVI, leftRightCanvas
            leftRightSV = new ScatterView();
            leftRightSV.Width = timelineWidth;
            leftRightSV.Height = timelineAreaHeight;

            leftRightSVI = new ScatterViewItem();
            leftRightSVI.Height = timelineAreaHeight;
            leftRightSVI.Width = timelineWidth;
            centerX_LRScatterView = leftRightSVI.Width / 2;
            centerX_LRScatterView_diff = textWidth;
            leftRightSVI.Center = new Point(centerX_LRScatterView + textWidth, centerY);
            leftRightSVI.Orientation = 0;
            leftRightSVI.CanRotate = false;
            leftRightSVI.CanScale = false;
            DependencyPropertyDescriptor dpd2 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd2.AddValueChanged(leftRightSVI, leftRightSVICenterChanged); // for panning timelines left and right
            scatterViewLR_userDragged = true;

            leftRightSV.Items.Add(leftRightSVI);

            leftRightCanvas = new Canvas();
            leftRightCanvas.Height = timelineAreaHeight;
            leftRightCanvas.Width = timelineWidth;
            leftRightCanvas.Background = (Brush)(new BrushConverter().ConvertFrom("#79aa89"));
            leftRightSVI.Content = leftRightCanvas;

            titleCanvas = new Canvas();
            titleCanvas.Height = timelineAreaHeight;
            titleCanvas.Width = textWidth;

            Rectangle r = new Rectangle();
            r.Height = titleCanvas.Height;
            r.Width = titleCanvas.Width;
            r.Fill = (Brush)(new BrushConverter().ConvertFrom("#093024"));
            titleCanvas.Children.Add(r);

            mainCanvas.Children.Add(leftRightSV);
            mainCanvas.Children.Add(titleCanvas);

            // movable scrub bar and its handle
            movableScrub = new Rectangle();
            movableScrub.Height = timelineAreaHeight;
            movableScrub.Width = 3;
            movableScrub.Fill = Brushes.Yellow;
            movableScrub.Visibility = Visibility.Hidden;

            leftRightCanvas.Children.Add(movableScrub);
            Console.Out.WriteLine("number of chidren" + leftRightCanvas.Children.Count);
            Canvas.SetLeft(movableScrub, 0);
            Canvas.SetZIndex(movableScrub, 12);
            Canvas.SetTop(movableScrub, 0);

            movableScrubHandleBackground = new Rectangle();
            movableScrubHandleBackground.Width = w;
            movableScrubHandleBackground.Height = 40;
            movableScrubHandleBackground.Fill = Brushes.DarkRed;
            movableScrubHandleBackground.TouchDown += new EventHandler<TouchEventArgs>(movableScrubHandleBackground_TouchDown);
            movableScrubHandleBackground.MouseDown +=new MouseButtonEventHandler(movableScrubHandleBackground_MouseDown);
            movableScrubHandleBackground.TouchUp += new EventHandler<TouchEventArgs>(movableScrubHandleBackground_TouchUp);
            movableScrubHandleBackground.MouseUp +=new MouseButtonEventHandler(movableScrubHandleBackground_TouchUp);

            canvasWrapper.Children.Add(movableScrubHandleBackground);
            Canvas.SetLeft(movableScrubHandleBackground, 0);
            Canvas.SetTop(movableScrubHandleBackground, 0);
            Canvas.SetZIndex(movableScrubHandleBackground, 15);

            movableScrubHandle = new Rectangle();
            movableScrubHandle.Width = 40;
            movableScrubHandle.Height = 40;
            movableScrubHandle.Fill = Brushes.Yellow;

            canvasWrapper.Children.Add(movableScrubHandle);
            Canvas.SetLeft(movableScrubHandle, textWidth - 18.5);
            Canvas.SetTop(movableScrubHandle, 0);
            Canvas.SetZIndex(movableScrubHandle, 20);
            DependencyPropertyDescriptor dpd3 = DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(Canvas));
            dpd3.AddValueChanged(movableScrubHandle, movableScrubHandle_CanvasLeftChanged);

            startDragPoint = new Point();
            movableScrubHandle.PreviewTouchDown += new EventHandler<TouchEventArgs>(movableScrubHandle_PreviewTouchDown);
            movableScrubHandle.PreviewMouseDown += new MouseButtonEventHandler(movableScrubHandle_PreviewMouseDown);
            movableScrubHandle.PreviewTouchMove += new EventHandler<TouchEventArgs>(movableScrubHandle_PreviewTouchMove);
            movableScrubHandle.PreviewMouseMove += new MouseEventHandler(movableScrubHandle_PreviewMouseMove);
            movableScrubHandle.PreviewTouchUp += new EventHandler<TouchEventArgs>(movableScrubHandle_PreviewTouchUp);
            movableScrubHandle.PreviewMouseUp += new MouseButtonEventHandler(movableScrubHandle_PreviewMouseUp);
            movableScrubHandle_userDragged = true;

            movableScrubHandleExt = new Rectangle();
            movableScrubHandleExt.Width = 3;
            movableScrubHandleExt.Height = 20;
            movableScrubHandleExt.Fill = Brushes.Yellow;
           // movableScrubHandleExt.Visibility = Visibility.Hidden;

            canvasWrapper.Children.Add(movableScrubHandleExt);
            Canvas.SetLeft(movableScrubHandleExt, textWidth);
            Canvas.SetTop(movableScrubHandleExt, 40);
            Canvas.SetZIndex(movableScrubHandleExt, 12);

            tourControlButton = new SurfaceButton();
            //tourControlButton.Content = "Play/Pause";
            tourControlButton.Height = 30;
            tourControlButton.MinHeight = 30;
            tourControlButton.Width = 70;
            tourControlButton.Padding = new Thickness(7, 1, 7, 0);


            //adding triangle for pause/play button
            Grid g = new Grid();
            g.Height = 30;
            g.Width = 100;
            g.HorizontalAlignment = HorizontalAlignment.Left;
            Polygon p = new Polygon();
            PointCollection ppoints = new PointCollection();
            ppoints.Add(new System.Windows.Point(4, 5));
            ppoints.Add(new System.Windows.Point(4, 26));
            ppoints.Add(new System.Windows.Point(23, 14));
            p.Points = ppoints;
            p.Fill = Brushes.Green;
            //p.Opacity = 1;
            p.Visibility = Visibility.Visible;
            p.Margin = new Thickness(0, 0, 0, 0);
            p.Stroke = Brushes.Black;
            p.StrokeThickness = 1;
            p.HorizontalAlignment = HorizontalAlignment.Left;
            p.VerticalAlignment = VerticalAlignment.Center;
            p.Height = 36;
            p.Width = 30;

            Polygon pause = new Polygon();
            PointCollection pausepoints = new PointCollection();
            pausepoints.Add(new System.Windows.Point(29, -1));
            pausepoints.Add(new System.Windows.Point(29, 22));
            pausepoints.Add(new System.Windows.Point(37, 22));
            pausepoints.Add(new System.Windows.Point(37, -1));
            pause.Points = pausepoints;
            pause.Fill = Brushes.Blue;
            //p.Opacity = 1;
            pause.Visibility = Visibility.Visible;
            pause.Margin = new Thickness(0, 0, 0, 0);
            pause.Stroke = Brushes.Black;
            pause.StrokeThickness = 1;
            pause.HorizontalAlignment = HorizontalAlignment.Left;
            pause.VerticalAlignment = VerticalAlignment.Center;
            //pause.Height = 36;
            //pause.Width = 30;

            Polygon pause2 = new Polygon();
            PointCollection pausepoints2 = new PointCollection();
            pausepoints2.Add(new System.Windows.Point(43, -1));
            pausepoints2.Add(new System.Windows.Point(43, 22));
            pausepoints2.Add(new System.Windows.Point(51, 22));
            pausepoints2.Add(new System.Windows.Point(51, -1));
            pause2.Points = pausepoints2;
            pause2.Fill = Brushes.Blue;
            //p.Opacity = 1;
            pause2.Visibility = Visibility.Visible;
            pause2.Margin = new Thickness(0, 0, 0, 0);
            pause2.Stroke = Brushes.Black;
            pause2.StrokeThickness = 1;
            pause2.HorizontalAlignment = HorizontalAlignment.Left;
            pause2.VerticalAlignment = VerticalAlignment.Center;

            g.Children.Add(p);
            g.Children.Add(pause);
            g.Children.Add(pause2);
            g.Visibility = Visibility.Visible;
            tourControlButton.Content = g;

            tourControlButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            tourControlButton.Click += tourSystem.TourControlButton_Click;
            canvasWrapper.Children.Add(tourControlButton);
            Canvas.SetLeft(tourControlButton, textWidth - movableScrubHandle.Width / 2 - 5 - tourControlButton.Width); //+20
            Canvas.SetTop(tourControlButton, (movableScrubHandleBackground.Height - tourControlButton.Height) / 2);
            Canvas.SetZIndex(tourControlButton, 30);

            SurfaceButton tourLengthButton = new SurfaceButton();
            tourLengthButton.Content = "Time";
            tourLengthButton.Height = 30;
            tourLengthButton.MinHeight = 30;
            tourLengthButton.Width = 70;
            tourLengthButton.Click += resetTimeButton_Click;
            tourLengthButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            //tourLengthButton.Click += tourSystem.TourControlButton_Click;
            canvasWrapper.Children.Add(tourLengthButton);
            Canvas.SetLeft(tourLengthButton, 5);
            Canvas.SetTop(tourLengthButton, (movableScrubHandleBackground.Height - tourLengthButton.Height) / 2);
            Canvas.SetZIndex(tourLengthButton, 30);

            // timelineRulerSV, timelineRulerSVI, timelineRulerCanvas
            timelineAreaTopLeft = new Rectangle();
            timelineAreaTopLeft.Width = textWidth;
            timelineAreaTopLeft.Height = 20;
            timelineAreaTopLeft.Fill = (Brush)(new BrushConverter().ConvertFrom("#5a675f"));

            canvasWrapper.Children.Add(timelineAreaTopLeft);
            Canvas.SetLeft(timelineAreaTopLeft, 0);
            Canvas.SetTop(timelineAreaTopLeft, 40);
            Canvas.SetZIndex(timelineAreaTopLeft, 15);

            canvasWrapper.Children.Add(tourSeekBarTimerCount);
            canvasWrapper.Children.Add(tourSeekBarLength);
            Canvas.SetLeft(tourSeekBarTimerCount, 0);
            Canvas.SetTop(tourSeekBarTimerCount, 35);
            Canvas.SetZIndex(tourSeekBarTimerCount, 15);
            tourSeekBarTimerCount.Foreground = Brushes.Yellow;
            tourSeekBarTimerCount.Content = "00:00";
            Canvas.SetLeft(tourSeekBarLength, 42);
            Canvas.SetTop(tourSeekBarLength, 35);
            Canvas.SetZIndex(tourSeekBarLength, 15);
            tourSeekBarLength.Foreground = Brushes.White;
            tourSeekBarLength.Content = "/ 00:00";

            timelineRulerSV = new ScatterView();
            timelineRulerSV.Width = timelineWidth;
            timelineRulerSV.Height = 20.0;

            timelineRulerSVI = new ScatterViewItem();
            timelineRulerSVI.Width = timelineWidth;
            timelineRulerSVI.MinHeight = 0.0; // Apparently, a ScatterViewItem's MinHeight is 80.  Microsoft should document stuff like this.
            timelineRulerSVI.Height = 20.0;
            timelineRulerSVI.Center = new Point(centerX_LRScatterView, timelineRulerSVI.Height / 2);
            timelineRulerSVI.Orientation = 0;
            timelineRulerSVI.CanRotate = false;
            timelineRulerSVI.CanScale = false;
            timelineRulerSVI.Background = new SolidColorBrush(Colors.Transparent);
            // the commented block below is supposed to remove the default SVI shadow behind the timelineRulerSVI
            /*RoutedEventHandler loadedEventHandler = null;
            loadedEventHandler = new RoutedEventHandler(delegate
            {
                timelineRulerSVI.Loaded -= loadedEventHandler;
                Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
                ssc = timelineRulerSVI.Template.FindName("shadow", timelineRulerSVI) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
                ssc.Visibility = Visibility.Hidden;
            });
            timelineRulerSVI.Loaded += loadedEventHandler;*/
            // not sure why the commented block above isn't working, but the shadow doesn't seem to be really noticeable anymore
            timelineRulerSV.Items.Add(timelineRulerSVI);
            DependencyPropertyDescriptor dpd4 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd2.AddValueChanged(timelineRulerSVI, timelineRulerSVICenterChanged); // for dragging the timeline ruler itself
            timelineRulerSVI_userDragged = true;

            timelineRulerCanvas = new Canvas();
            timelineRulerCanvas.Width = timelineWidth;
            timelineRulerCanvas.Height = 20;
            timelineRulerCanvas.Background = Brushes.Black;
            timelineRulerSVI.Content = timelineRulerCanvas;

            this.addTimelineRulerTickMarks();

            canvasWrapper.Children.Add(timelineRulerSV);
            Canvas.SetLeft(timelineRulerSV, textWidth);
            Canvas.SetTop(timelineRulerSV, 40);
            Canvas.SetZIndex(timelineRulerSV, 10);

            // putting it all together
            mainSV.Items.Add(mainSVI);
            canvasWrapper.Children.Add(mainSV);
            Canvas.SetTop(mainSV, 60);




        }

        public SurfaceButton newMediaButton = new SurfaceButton();
        public SurfaceButton newAudioButton = new SurfaceButton();
        public SurfaceButton newDrawingButton = new SurfaceButton();
        public SurfaceButton newHighlightButton = new SurfaceButton();
        public SurfaceButton doneButton = new SurfaceButton();
        public SurfaceButton saveButton = new SurfaceButton();
        public SurfaceButton deleteButton = new SurfaceButton();
        public SurfaceButton undoButton = new SurfaceButton();
        public SurfaceButton redoButton = new SurfaceButton();
        public SurfaceButton tourQuitButton = new SurfaceButton();
        public SurfaceButton removeComponentButton = new SurfaceButton();
        public SurfaceButton removeEventButton = new SurfaceButton();
        public SurfaceButton eraseButton = new SurfaceButton();
        public Label opacityLabel = new Label();
        public Label successfulSaveLabel = new Label();
        public SurfaceSlider opacitySlider = new SurfaceSlider();
        public SurfaceSlider timelineSlider = new SurfaceSlider();
        public Label timeLineLabel = new Label();
        public SurfaceButton renameTimelineButton = new SurfaceButton();

        public void removeAuthTools()
        {
            try
            {
                (successfulSaveLabel.Parent as Panel).Children.Remove(successfulSaveLabel);
                (newMediaButton.Parent as Panel).Children.Remove(newMediaButton);
                (newAudioButton.Parent as Panel).Children.Remove(newAudioButton);
                (newDrawingButton.Parent as Panel).Children.Remove(newDrawingButton);
                (newHighlightButton.Parent as Panel).Children.Remove(newHighlightButton);
                //(doneButton.Parent as Panel).Children.Remove(doneButton);
                doneButton.PreviewMouseUp -= tourSystem.TourAuthoringDoneButton_Click;
                doneButton.Visibility = Visibility.Collapsed;
                //(saveButton.Parent as Panel).Children.Remove(saveButton);
                saveButton.Visibility = Visibility.Collapsed;
                saveButton.PreviewMouseUp -= artModeWin.TourAuthoringSaveButton_Click;
                //(deleteButton.Parent as Panel).Children.Remove(deleteButton);
                tourQuitButton.Visibility = Visibility.Collapsed;
                tourQuitButton.PreviewMouseUp -= tourSystem.ExitButton_Click;
                deleteButton.Visibility = Visibility.Collapsed;
                deleteButton.PreviewMouseUp -= tourSystem.TourAuthoringDeleteButton_Click;
                (undoButton.Parent as Panel).Children.Remove(undoButton);
                (redoButton.Parent as Panel).Children.Remove(redoButton);
                (removeComponentButton.Parent as Panel).Children.Remove(removeComponentButton);
                (removeEventButton.Parent as Panel).Children.Remove(removeEventButton);
                (eraseButton.Parent as Panel).Children.Remove(eraseButton);
                (opacityLabel.Parent as Panel).Children.Remove(opacityLabel);
                (opacitySlider.Parent as Panel).Children.Remove(opacitySlider);
                (timelineSlider.Parent as Panel).Children.Remove(timelineSlider);
                (timeLineLabel.Parent as Panel).Children.Remove(timeLineLabel);
                (renameTimelineButton.Parent as Panel).Children.Remove(renameTimelineButton);
            }
            catch (Exception e)
            {

            }
        }

        public void initAuthTools()
        {
            double toolBoxHeight = artModeWin.Main.ActualHeight * .8;
            double boxPartition = .06;
            double buttonHeight = .7 * toolBoxHeight * boxPartition;
            double buttonWidth = artModeWin.Main.ActualWidth * .15;





            Label addNewLabel = new Label();
            Label editLabel = new Label();
            successfulSaveLabel = new Label();
            newMediaButton = new SurfaceButton();
            newAudioButton = new SurfaceButton();
            newDrawingButton = new SurfaceButton();
            newHighlightButton = new SurfaceButton();
            //doneButton = new SurfaceButton();
            doneButton = artModeWin.tourAuthoringDoneButton;
            saveButton = artModeWin.tourAuthoringSaveButton;
            undoButton = new SurfaceButton();
            redoButton = new SurfaceButton();
            //deleteButton = new SurfaceButton();
            deleteButton = artModeWin.tourAuthoringDeleteButton;
            tourQuitButton = artModeWin.quitButton;
            removeComponentButton = new SurfaceButton();
            removeEventButton = new SurfaceButton();
            eraseButton = new SurfaceButton();
            opacityLabel = new Label();
            opacitySlider = new SurfaceSlider();
            timelineSlider = new SurfaceSlider();
            timeLineLabel = new Label();
            renameTimelineButton = new SurfaceButton();

            setButtonEnabled(removeComponentButton, false);
            setButtonEnabled(removeEventButton, false);
            setButtonEnabled(eraseButton, false);
            setButtonEnabled(renameTimelineButton, false);
            opacitySlider.IsEnabled = false;


            addNewLabel.Content = "Add New Component...";
            editLabel.Content = "Edit...";
            successfulSaveLabel.Content = "Save Successful";
            newMediaButton.Content = "Asset";
            newAudioButton.Content = "Audio";
            newDrawingButton.Content = "Drawing";
            newHighlightButton.Content = "Mask";
            doneButton.Content = "Done";
            undoButton.Content = "Undo";
            redoButton.Content = "Redo";
            saveButton.Content = "Save";
            deleteButton.Content = "Delete";
            tourQuitButton.Content = "QUIT";
            removeComponentButton.Content = "Remove Component";
            removeEventButton.Content = "Remove Event";
            opacityLabel.Content = "Mask Opacity";
            eraseButton.Content = "Erase";
            timeLineLabel.Content = "Scrub through tour";
            renameTimelineButton.Content = "Rename Component";

            doneButton.Visibility = Visibility.Visible;
            saveButton.Visibility = Visibility.Visible;
            deleteButton.Visibility = Visibility.Visible;
            tourQuitButton.Visibility = Visibility.Visible;
            artModeWin.Main.Children.Add(successfulSaveLabel);
            //tourQuitButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            //Canvas.SetZIndex(doneButton, 100);
            //Canvas.SetZIndex(saveButton, 100);
            //Canvas.SetZIndex(deleteButton, 100);
            //Canvas.SetZIndex(successfulSaveLabel, 100);

            artModeWin.AuthTools.Children.Add(addNewLabel);
            artModeWin.AuthTools.Children.Add(newMediaButton);
            artModeWin.AuthTools.Children.Add(newAudioButton);
            artModeWin.AuthTools.Children.Add(newDrawingButton);
            artModeWin.AuthTools.Children.Add(newHighlightButton);
            artModeWin.AuthTools.Children.Add(removeComponentButton);
            artModeWin.AuthTools.Children.Add(removeEventButton);
            artModeWin.AuthTools.Children.Add(undoButton);
            artModeWin.AuthTools.Children.Add(redoButton);
            artModeWin.AuthTools.Children.Add(editLabel);
            artModeWin.AuthTools.Children.Add(eraseButton);
            artModeWin.AuthTools.Children.Add(opacityLabel);
            artModeWin.AuthTools.Children.Add(opacitySlider);
            artModeWin.AuthTools.Children.Add(renameTimelineButton);
            artModeWin.AuthTools.Children.Add(timelineSlider);
            artModeWin.AuthTools.Children.Add(timeLineLabel);



            Canvas.SetTop(tourQuitButton, 0);
            Canvas.SetTop(saveButton, 0);
            Canvas.SetTop(successfulSaveLabel, saveButton.Height+10);
            Canvas.SetTop(doneButton, 0);
            Canvas.SetTop(deleteButton, 0);
            Canvas.SetTop(undoButton, toolBoxHeight * boxPartition);
            Canvas.SetTop(redoButton, toolBoxHeight * boxPartition);
            Canvas.SetTop(addNewLabel, 2.3 * toolBoxHeight * boxPartition);
            Canvas.SetTop(newMediaButton, 3.0 * toolBoxHeight * boxPartition);
            Canvas.SetTop(newAudioButton, 4.0 * toolBoxHeight * boxPartition);
            Canvas.SetTop(newDrawingButton, 5.0 * toolBoxHeight * boxPartition);
            Canvas.SetTop(newHighlightButton, 6.0 * toolBoxHeight * boxPartition);
            Canvas.SetTop(editLabel, 7.3 * toolBoxHeight * boxPartition);
            Canvas.SetTop(removeEventButton, 8.0 * toolBoxHeight * boxPartition);
            Canvas.SetTop(removeComponentButton, 9.0 * toolBoxHeight * boxPartition);
            Canvas.SetTop(renameTimelineButton, 10.0 * toolBoxHeight * boxPartition);
            Canvas.SetTop(eraseButton, 11.5 * toolBoxHeight * boxPartition);
            Canvas.SetTop(opacityLabel, 12.9 * toolBoxHeight * boxPartition);
            Canvas.SetTop(opacitySlider, 13.5 * toolBoxHeight * boxPartition);
            Canvas.SetTop(timeLineLabel, 14.9 * toolBoxHeight * boxPartition);
            Canvas.SetTop(timelineSlider, 15.5 * toolBoxHeight * boxPartition);

            Canvas.SetRight(tourQuitButton, 20);
            Canvas.SetRight(successfulSaveLabel, 45);
            Canvas.SetRight(doneButton, 75);
            Canvas.SetRight(saveButton, 140);
            Canvas.SetRight(deleteButton, 200);
            Canvas.SetLeft(newMediaButton, 20);
            Canvas.SetLeft(newAudioButton, 20);
            Canvas.SetLeft(newDrawingButton, 20);
            Canvas.SetLeft(newHighlightButton, 20);
            Canvas.SetLeft(removeComponentButton, 20);
            Canvas.SetLeft(removeEventButton, 20);
            Canvas.SetLeft(undoButton, 20);
            Canvas.SetLeft(redoButton, 85);
            Canvas.SetLeft(opacityLabel, 20);
            Canvas.SetLeft(opacitySlider, 20);
            Canvas.SetLeft(eraseButton, 20);
            Canvas.SetLeft(timeLineLabel, 20);
            Canvas.SetLeft(timelineSlider, 20);
            Canvas.SetLeft(renameTimelineButton, 20);
            successfulSaveLabel.Opacity = 0.0;

            opacitySlider.Height = 23;
            opacitySlider.Width = 200;
            opacitySlider.Minimum = 0;
            opacitySlider.Maximum = 1;
            opacitySlider.IsSnapToTickEnabled = true;
            opacitySlider.Value = .6;
            opacitySlider.ValueChanged += sliderOpacity_ValueChanged;
            opacitySlider.TickFrequency = .1;

            timelineSlider.Height = 23;
            timelineSlider.Width = 200;
            timelineSlider.Minimum = 0;
            timelineSlider.Maximum = 1;
            timelineSlider.IsSnapToTickEnabled = false;
            timelineSlider.Value = .6;
            Canvas.SetZIndex(timelineSlider, 500);
            timelineSlider.ValueChanged += timeLineSlider_ValueChanged;
            timelineSlider.ManipulationCompleted += timeLineSlider_Completed;
            timelineSlider.TickFrequency = .1;



            newMediaButton.Width = buttonWidth;
            newAudioButton.Width = buttonWidth;
            newDrawingButton.Width = buttonWidth;
            newHighlightButton.Width = buttonWidth;
            removeComponentButton.Width = buttonWidth;
            removeEventButton.Width = buttonWidth;
            eraseButton.Width = buttonWidth;
            renameTimelineButton.Width = buttonWidth;

            newMediaButton.Height = buttonHeight;
            newAudioButton.Height = buttonHeight;
            newDrawingButton.Height = buttonHeight;
            newHighlightButton.Height = buttonHeight;
            removeComponentButton.Height = buttonHeight;
            removeEventButton.Height = buttonHeight;
            eraseButton.Height = buttonHeight;
            renameTimelineButton.Height = buttonHeight;

            //newMediaButton.MinHeight = buttonHeight;
            //newAudioButton.MinHeight = buttonHeight;
            //newDrawingButton.MinHeight = buttonHeight;
            //newHighlightButton.MinHeight = buttonHeight;
            //removeTimelineButton.MinHeight = buttonHeight;
            //removeEventButton.MinHeight = buttonHeight;
            //eraseButton.MinHeight = buttonHeight;
            //renameTimelineButton.MinHeight = buttonHeight;

            tourQuitButton.Click += artModeWin.ExitButton_Click;
            saveButton.Click += artModeWin.TourAuthoringSaveButton_Click;
            doneButton.Click += tourSystem.TourAuthoringDoneButton_Click;
            deleteButton.Click += tourSystem.TourAuthoringDeleteButton_Click;
            undoButton.Click += undoButton_Click;
            redoButton.Click += redoButton_Click;
            //removeEventButton.PreviewTouchUp += removeHighlightedAnimation;
            removeEventButton.Click += removeHighlightedAnimation;
            //removeComponentButton.PreviewTouchUp += removeHighlightedTimeline;
            removeComponentButton.Click += removeHighlightedTimeline;
            newMediaButton.Click += addMetadata_Clicked;
            newAudioButton.Click += tourSystem.grabSound;
            newDrawingButton.Click += tourSystem.drawPaths_Click;
            newHighlightButton.Click += tourSystem.drawHighlight_Click;
            eraseButton.Click += erase_Click;
            renameTimelineButton.Click += renameTimelineButton_Click;

            ////////////////////////////
            artModeWin.applyRenameTimelineButton.Click += applyRenameTimelineButton_Click;
            artModeWin.cancelRenameTimelineButton.Click += cancelRenameTimelineButton_Click;


        }

        void undoButton_Click(object sender, RoutedEventArgs e)
        {
            tourSystem.undo();
        }

        void redoButton_Click(object sender, RoutedEventArgs e)
        {
            tourSystem.redo();
        }

        public void renameTimelineButton_Click(object sender, RoutedEventArgs e)
        {
            if (highlightActive)
            {
                artModeWin.renameTimelineBox.Visibility = Visibility.Visible;
                artModeWin.RenameBorder.Visibility = Visibility.Visible;

            }
        }

        public void cancelRenameTimelineButton_Click(object sender, RoutedEventArgs e)
        {
            artModeWin.renameTimelineBox.Visibility = Visibility.Collapsed;
            artModeWin.RenameBorder.Visibility = Visibility.Collapsed;

        }
        public void applyRenameTimelineButton_Click(object sender, RoutedEventArgs e)
        {
            if (highlightActive)
            {
                tourSystem.undoableActionPerformed();
                (highlightData.timeline as TourTL).displayName = artModeWin.renameTimelineTextBox.Text;
                //highlightData.title = artModeWin.renameTimelineTextBox.Text;
                //highlightData.titlebox.Text = artModeWin.renameTimelineTextBox.Text;
                this.refreshUI();
                artModeWin.renameTimelineBox.Visibility = Visibility.Collapsed;
                artModeWin.RenameBorder.Visibility = Visibility.Collapsed;
            }

            //}

        }

        public void erase_Click(object sender, RoutedEventArgs e)
        {
            if (eraseButton.Content == "Erase")
            {
                tourSystem.switchDrawMode(SurfaceInkEditingMode.EraseByPoint);
                eraseButton.Content = "Draw";
            }
            else
            {
                eraseButton.Content = "Erase";
                tourSystem.switchDrawMode(SurfaceInkEditingMode.Ink);
            }
        }

        private void addMetadata_Clicked(object sender, RoutedEventArgs e)
        {
            artModeWin.showMetaList();
        }


        public void sliderOpacity_ValueChanged(object sender, RoutedEventArgs e)
        {
            tourSystem.ChangeOpacity(((SurfaceSlider)sender).Value);
        }

        public void sliderOpacity_Completed(object sender, RoutedEventArgs e)
        {
            tourSystem.undoableActionPerformed();
        }

        public void timeLineSlider_Completed(object sender, RoutedEventArgs e)
        {

        }

        public void timeLineSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            //Console.Out.WriteLine("timeLine changed!");
            //tourSystem.tourStoryboard.CurrentTimeInvalidated -= TourStoryboardAuthoring_CurrentTimeInvalidated;
            //Console.Out.WriteLine("before move time");
            Point current = new Point();
            current.X = timelineSlider.Value * timelineWidth;
            current.Y = centerY;
            //Console.Out.WriteLine("current timeLineWidth" + timelineWidth);
            Console.Out.WriteLine("x" + current.X);
            Double dragDistance = current.X;

            double tourSeekBarProgressTargetWidth = dragDistance;
            //Console.Out.WriteLine("tourSeekBarProgressTargetWidth"+ tourSeekBarProgressTargetWidth);
            if (tourSeekBarProgressTargetWidth < 0)
            {
                tourSeekBarProgressTargetWidth = 0;
            }
            else if (tourSeekBarProgressTargetWidth > leftRightCanvas.Width)
            {
                tourSeekBarProgressTargetWidth = leftRightCanvas.Width;
            }
            //movableScrub.SetValue(Canvas.LeftProperty, (double)movableScrubHandle.GetValue(Canvas.LeftProperty) - centerX_LRScatterView_diff + 18.5);
            tourTimerCount = ((tourSeekBarProgressTargetWidth / timelineRulerCanvas.Width) * tourSystem.tourStoryboard.Duration.TimeSpan.TotalSeconds);
            tourTimerCountSpan = TimeSpan.FromSeconds(tourTimerCount);
            tourSystem.authorTimerCountSpan = tourTimerCountSpan;
            tourTimerCountSpanString = string.Format("{0:D2}:{1:D2}", tourTimerCountSpan.Minutes, tourTimerCountSpan.Seconds);
            tourSeekBarTimerCount.Content = tourTimerCountSpanString;

            movableScrubHandle.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth - 18.5 + centerX_LRScatterView_diff);
            movableScrubHandleExt.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth + centerX_LRScatterView_diff);

            tourSeekBarProgressWidth = tourSeekBarProgressTargetWidth;

            //startDragPoint should be the point where the yellow handler is
            startDragPoint = current;
            Console.Out.WriteLine("x" + startDragPoint.X);
            //Console.Out.WriteLine("after move time");
            //tourSystem.tourStoryboard.CurrentTimeInvalidated -= TourStoryboardAuthoring_CurrentTimeInvalidated;

            Point newLeftRightSVICenter = new Point();
            newLeftRightSVICenter.Y = centerY;

            //This part of leftRightCenter change should be fixed
            int ratio = (int)(current.X) / (int)(w - textWidth);
            Console.Out.WriteLine("ratio" + ratio);
            if (ratio == 0)
            {
                Console.Out.WriteLine("smaller");
                leftRightSVI.Center = new Point(centerX_LRScatterView + textWidth, centerY);

            }
            else if (ratio == 1)
            {
                newLeftRightSVICenter.X = w - (current.X - ratio * (w - textWidth));
                leftRightSVI.Center = newLeftRightSVICenter;
            }
            //still not perfect here, but almost right
            else
            {
                newLeftRightSVICenter.X = w - (current.X - (w - textWidth));
                leftRightSVI.Center = newLeftRightSVICenter;

            }
            Console.Out.WriteLine("centerSVI" + newLeftRightSVICenter.X);
            //leftRightSVI.Center = newLeftRightSVICenter ;



        }


        public void ClearAuthoringUI()
        {
            canvasWrapper.Children.Clear();
        }

        public void ClearTimelines()
        {
            leftRightCanvas.Children.Clear();
            titleCanvas.Children.Clear();
        }

        void addTimelineRulerTickMarks()
        {
            int i;
            for (i = 0; i < timelineLength; i++)
            {
                this.drawTickMark(i, i * timelineRulerTickInterval);
            }
        }

        private void drawTickMark(int second, double xPos)
        {
            Line l = new Line();

            l.X1 = xPos;
            l.Y1 = 0;
            l.X2 = xPos;
            l.Y2 = timelineRulerCanvas.Height;

            // set Line's width and color
            l.StrokeThickness = 3;
            l.Stroke = Brushes.White;

            // add line
            timelineRulerCanvas.Children.Add(l);

            TextBlock tickLabel = new TextBlock();
            TimeSpan tickTimeSpan = TimeSpan.FromSeconds(second);
            tickLabel.Text = string.Format("{0:D1}:{1:D2}", tickTimeSpan.Minutes, tickTimeSpan.Seconds);
            tickLabel.FontSize = 16;
            tickLabel.Foreground = Brushes.White;
            timelineRulerCanvas.Children.Add(tickLabel);
            Canvas.SetLeft(tickLabel, xPos + 5);
            Canvas.SetTop(tickLabel, 0);
        }







        #endregion

        #region tour authoring storyboard event handlers

        public void resetTimeButton_Click(object sender, EventArgs e)
        {
            artModeWin.TourLengthBox.Visibility = Visibility.Visible;
            artModeWin.TimeBorder.Visibility = Visibility.Visible;

        }

        public void TourStoryboardAuthoring_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            // update seek bar time values
            try
            {
                if ((TimeSpan)tourSystem.tourStoryboard.GetCurrentTime(artModeWin) != null)
                {
                    tourCurrentTime = (TimeSpan)tourSystem.tourStoryboard.GetCurrentTime(artModeWin);
                    //Console.Out.WriteLine("current time" + tourCurrentTime); ;
                    tourCurrentTimeString = string.Format("{0:D2}:{1:D2}", tourCurrentTime.Minutes, tourCurrentTime.Seconds);
                    tourSeekBarTimerCount.Content = tourCurrentTimeString;
                    tourSystem.authorTimerCountSpan = tourCurrentTime;
                }
                if (tourSystem.tourStoryboard.Duration.HasTimeSpan)
                {
                    tourDuration = tourSystem.tourStoryboard.Duration.TimeSpan;
                    tourDurationString = string.Format("{0:D2}:{1:D2}", tourDuration.Minutes, tourDuration.Seconds);
                    tourSeekBarLength.Content = " / " + tourDurationString;

                    double seekBarLocation = ((double)tourCurrentTime.TotalSeconds / (double)tourDuration.TotalSeconds) * (double)timelineRulerCanvas.Width;
                    if (seekBarLocation <= timelineRulerCanvas.Width)
                    {
                        movableScrubHandle.SetValue(Canvas.LeftProperty, seekBarLocation - 18.5 + centerX_LRScatterView_diff);
                        movableScrubHandleExt.SetValue(Canvas.LeftProperty, seekBarLocation + centerX_LRScatterView_diff);

                        tourSeekBarProgressWidth = seekBarLocation;
                    }
                }
            }
            catch (Exception exc)
            {
            }
        }

        public void TourStoryboardAuthoring_Completed(object sender, EventArgs e)
        {
            Console.WriteLine("Done tour");
            tourSystem.tourStoryboard.Pause(artModeWin);

        }

        #endregion

        #region movable scrub bar/handle event handlers

        void movableScrubHandle_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            //Console.Out.WriteLine("preview touch down");
            tourSystem.tourStoryboard.CurrentTimeInvalidated -= TourStoryboardAuthoring_CurrentTimeInvalidated;

            Rectangle r = sender as Rectangle;
            r.CaptureTouch(e.TouchDevice);
            //startDragPoint = e.TouchDevice.GetCenterPosition(mainCanvas);
            startDragPoint = e.TouchDevice.GetCenterPosition(leftRightCanvas); // I think the innermost canvas (leftRightCanvas) is the best option.

            e.Handled = true; // when is this necessary?  I haven't really been using it elsewhere, but Ferdi put this here in the first version of this file.
        }

        void movableScrubHandle_PreviewMouseDown(object sender, MouseEventArgs e) {
            mouseDownOnScrub = true;
            //Console.Out.WriteLine("preview touch down");
            tourSystem.tourStoryboard.CurrentTimeInvalidated -= TourStoryboardAuthoring_CurrentTimeInvalidated;

            Rectangle r = sender as Rectangle;
            r.CaptureMouse();
            //r.CaptureMouse(e.MouseDevice);
            //startDragPoint = e.TouchDevice.GetCenterPosition(mainCanvas);
            startDragPoint = e.MouseDevice.GetCenterPosition(leftRightCanvas); // I think the innermost canvas (leftRightCanvas) is the best option.

            e.Handled = true; // when is this necessary?  I haven't really been using it elsewhere, but Ferdi put this here in the first version of this file.
            refreshUI();
        }

        private bool mouseDownOnScrub = false;
        void movableScrubHandle_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (!backgroundMoved)
            {
                
                //Console.WriteLine("preview move");
                Point current = e.TouchDevice.GetCenterPosition(leftRightCanvas);

                Double dragDistance = current.X - startDragPoint.X;

                double tourSeekBarProgressTargetWidth = tourSeekBarProgressWidth + dragDistance;
                if (tourSeekBarProgressTargetWidth < 0)
                {
                    tourSeekBarProgressTargetWidth = 0;
                }
                else if (tourSeekBarProgressTargetWidth > leftRightCanvas.Width)
                {
                    tourSeekBarProgressTargetWidth = leftRightCanvas.Width;
                }

                tourTimerCount = ((tourSeekBarProgressTargetWidth / timelineRulerCanvas.Width) * tourSystem.tourStoryboard.Duration.TimeSpan.TotalSeconds);
                tourTimerCountSpan = TimeSpan.FromSeconds(tourTimerCount);
                tourTimerCountSpanString = string.Format("{0:D2}:{1:D2}", tourTimerCountSpan.Minutes, tourTimerCountSpan.Seconds);
                tourSeekBarTimerCount.Content = tourTimerCountSpanString;

                // debugging info
                /*Console.WriteLine("current.X = " + current.X + "startDragPoint.X = " + startDragPoint.X);
                Console.WriteLine("dragDistance = " + dragDistance);
                Console.WriteLine("tourSeekBarProgressWidth = " + tourSeekBarProgressWidth);
                Console.WriteLine("tourSeekBarPdrogressTargetWidth = " + tourSeekBarProgressTargetWidth);
                Console.WriteLine("centerX_LRScatterView_diff = " + centerX_LRScatterView_diff);
                Console.WriteLine("\n");*/

                movableScrubHandle.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth - 18.5 + centerX_LRScatterView_diff);
                movableScrubHandleExt.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth + centerX_LRScatterView_diff);

                tourSeekBarProgressWidth = tourSeekBarProgressTargetWidth;

                startDragPoint.X = tourSeekBarProgressTargetWidth;

                e.Handled = true; // when is this necessary?  I haven't really been using it elsewhere, but Ferdi put this here in the first version of this file.
            }
            refreshUI();
        }

        void movableScrubHandle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!backgroundMoved && mouseDownOnScrub)
            {
                //Console.WriteLine("preview move");
                Point current = e.MouseDevice.GetCenterPosition(leftRightCanvas);

                Double dragDistance = current.X - startDragPoint.X;

                double tourSeekBarProgressTargetWidth = tourSeekBarProgressWidth + dragDistance;
                if (tourSeekBarProgressTargetWidth < 0)
                {
                    tourSeekBarProgressTargetWidth = 0;
                }
                else if (tourSeekBarProgressTargetWidth > leftRightCanvas.Width)
                {
                    tourSeekBarProgressTargetWidth = leftRightCanvas.Width;
                }

                tourTimerCount = ((tourSeekBarProgressTargetWidth / timelineRulerCanvas.Width) * tourSystem.tourStoryboard.Duration.TimeSpan.TotalSeconds);
                tourTimerCountSpan = TimeSpan.FromSeconds(tourTimerCount);
                tourTimerCountSpanString = string.Format("{0:D2}:{1:D2}", tourTimerCountSpan.Minutes, tourTimerCountSpan.Seconds);
                tourSeekBarTimerCount.Content = tourTimerCountSpanString;

                // debugging info
                /*Console.WriteLine("current.X = " + current.X + "startDragPoint.X = " + startDragPoint.X);
                Console.WriteLine("dragDistance = " + dragDistance);
                Console.WriteLine("tourSeekBarProgressWidth = " + tourSeekBarProgressWidth);
                Console.WriteLine("tourSeekBarPdrogressTargetWidth = " + tourSeekBarProgressTargetWidth);
                Console.WriteLine("centerX_LRScatterView_diff = " + centerX_LRScatterView_diff);
                Console.WriteLine("\n");*/

                movableScrubHandle.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth - 18.5 + centerX_LRScatterView_diff);
                movableScrubHandleExt.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth + centerX_LRScatterView_diff);

                tourSeekBarProgressWidth = tourSeekBarProgressTargetWidth;

                startDragPoint.X = tourSeekBarProgressTargetWidth;

                e.Handled = true; // when is this necessary?  I haven't really been using it elsewhere, but Ferdi put this here in the first version of this file.
            }

        }

        void movableScrubHandle_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            
            //Console.WriteLine("preview touch up");
            Rectangle r = sender as Rectangle;
            r.ReleaseTouchCapture(e.TouchDevice);

            tourSystem.tourStoryboard.Seek(artModeWin, tourTimerCountSpan, TimeSeekOrigin.BeginTime);
            tourSystem.tourStoryboard.CurrentTimeInvalidated += TourStoryboardAuthoring_CurrentTimeInvalidated;
            tourSystem.authorTimerCountSpan = tourTimerCountSpan;
            refreshUI();
        }

        void movableScrubHandle_PreviewMouseUp(object sender, MouseEventArgs e)
        {
            mouseDownOnScrub = false;
            //Console.WriteLine("preview touch up");
            Rectangle r = sender as Rectangle;
            r.ReleaseMouseCapture();
            r.ReleaseAllCaptures();
            //r.ReleaseTouchCapture(e.TouchDevice);

            tourSystem.tourStoryboard.Seek(artModeWin, tourTimerCountSpan, TimeSeekOrigin.BeginTime);
            tourSystem.tourStoryboard.CurrentTimeInvalidated += TourStoryboardAuthoring_CurrentTimeInvalidated;
            tourSystem.authorTimerCountSpan = tourTimerCountSpan;
            refreshUI();
        }

        void movableScrubHandle_CanvasLeftChanged(object sender, EventArgs e)
        {
            // Console.WriteLine("leftRightChanged");
            if (movableScrubHandle_userDragged)
            {
                movableScrub.SetValue(Canvas.LeftProperty, (double)movableScrubHandle.GetValue(Canvas.LeftProperty) - centerX_LRScatterView_diff + 18.5);
                //movableScrub.GetValue(Canvas.LeftProperty);
                //need to set the startDragPoiNT


            }

            movableScrubHandle_userDragged = true;

        }

        #endregion

        #region bidirectional event handlers for panning leftRightSVI and timelineRulerSVI left and right

        private void leftRightSVICenterChanged(Object sender, EventArgs e) // also bidirectional with movableScrubHandle event handler
        {
            Console.WriteLine("leftRightSVI changed");
            if (scatterViewLR_userDragged)
            {
                timelineRulerSVI_userDragged = false; // prevents event handler loop

                ScatterViewItem currentScatter = sender as ScatterViewItem;

                if (currentScatter.Center.X < (w - (currentScatter.Width / 2)))
                {
                    currentScatter.Center = new Point(w - (currentScatter.Width / 2), centerY);
                    timelineRulerSVI.Center = new Point(w - (currentScatter.Width / 2) - textWidth, timelineRulerSVI.Height / 2);
                }
                else if (currentScatter.Center.X > centerX_LRScatterView + textWidth)
                {
                    currentScatter.Center = new Point(centerX_LRScatterView + textWidth, centerY);
                    timelineRulerSVI.Center = new Point(centerX_LRScatterView, timelineRulerSVI.Height / 2);
                }
                else
                {
                    currentScatter.Center = new Point(currentScatter.Center.X, centerY);
                    timelineRulerSVI.Center = new Point(currentScatter.Center.X - textWidth, timelineRulerSVI.Height / 2);
                }

                double newCurrentScatterX = currentScatter.Center.X;
                centerX_LRScatterView_diff = newCurrentScatterX - centerX_LRScatterView;

                movableScrubHandle_userDragged = false; // prevents event handler loop
                movableScrubHandle.SetValue(Canvas.LeftProperty, (double)movableScrub.GetValue(Canvas.LeftProperty) + centerX_LRScatterView_diff - 18.5);
                movableScrubHandleExt.SetValue(Canvas.LeftProperty, (double)movableScrub.GetValue(Canvas.LeftProperty) + centerX_LRScatterView_diff);
            }

            scatterViewLR_userDragged = true;
        }

        private void timelineRulerSVICenterChanged(Object sender, EventArgs e) // also bidirectional with movableScrubHandle event handler
        {
            Console.WriteLine("timeline CHANGED! ");
            if (timelineRulerSVI_userDragged)
            {
                scatterViewLR_userDragged = false; // prevents event handler loop

                ScatterViewItem currentSVI = sender as ScatterViewItem;

                if (currentSVI.Center.X < (w - (currentSVI.Width / 2)) - textWidth)
                {
                    currentSVI.Center = new Point(w - (currentSVI.Width / 2) - textWidth, currentSVI.Height / 2);
                    leftRightSVI.Center = new Point(w - (currentSVI.Width / 2), centerY);
                }
                else if (currentSVI.Center.X > centerX_LRScatterView)
                {
                    currentSVI.Center = new Point(centerX_LRScatterView, currentSVI.Height / 2);
                    leftRightSVI.Center = new Point(centerX_LRScatterView + textWidth, centerY);
                }
                else
                {
                    currentSVI.Center = new Point(currentSVI.Center.X, currentSVI.Height / 2);
                    leftRightSVI.Center = new Point(currentSVI.Center.X + textWidth, centerY);
                }

                double newleftRightScatterItemX = leftRightSVI.Center.X;
                centerX_LRScatterView_diff = newleftRightScatterItemX - centerX_LRScatterView;

                movableScrubHandle_userDragged = false; // prevents event handler loop
                movableScrubHandle.SetValue(Canvas.LeftProperty, (double)movableScrub.GetValue(Canvas.LeftProperty) + centerX_LRScatterView_diff - 18.5);
                movableScrubHandleExt.SetValue(Canvas.LeftProperty, (double)movableScrub.GetValue(Canvas.LeftProperty) + centerX_LRScatterView_diff);
            }

            timelineRulerSVI_userDragged = true;
        }

        #endregion

        #region methods and event handlers for highlighting timelines


        private void highlightTL(double x)
        {

            removeTLHighlight();
            currentHighlightedTL = new Rectangle();
            currentHighlightedTL.Width = windowW;
            currentHighlightedTL.Height = timelineHeight;
            currentHighlightedTL.Stroke = Brushes.Red;
            currentHighlightedTL.RenderTransform = new TranslateTransform(0, x - 1.5);

            mainCanvas.Children.Add(currentHighlightedTL);
            highlightActive = true;
        }

        private void removeTLHighlight()
        {
            if (currentHighlightedTL != null)
            {
                highlightActive = false;
                highlightData.lengthSV.IsHitTestVisible = false;
                ((Canvas)currentHighlightedTL.Parent).Children.Remove(currentHighlightedTL);
                currentHighlightedTL = null;
            }
        }

        /*void mainCanvas_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (highlightTimer != null)
            {
                highlightTimer.Stop();
                highlightTimer = null;
            }

            highlightTimer = new DispatcherTimer();
            highlightTimer.Tick += new EventHandler(highlightTimer_Tick);
            highlightTimer.Interval = TimeSpan.FromMilliseconds(1000); // jcchin
            int count = 0;
            highlightTimer.Tag = count;
            highlightTimer.Start();
        }*/

        /*void highlightTimer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer t = sender as DispatcherTimer;
            int count = (int)t.Tag;
            count++;
            t.Tag = count;
        }*/
        void mainCanvas_PreviewMouseUp(object sender, MouseEventArgs e) {
            bool found = false;

            /*if (highlightTimer != null)
            {
                int count = (int)highlightTimer.Tag;

                if (count < 2000)
                {
                    foreach (timeLines i in timeLineList)
                    {
                        if (e.TouchDevice.GetPosition(mainCanvas).Y > i.pos && e.TouchDevice.GetPosition(mainCanvas).Y < i.pos + timelineHeight)
                        {
                            found = true;
                            highlight(i.pos);
                            highlightData = i;
                            i.lengthScatter.IsHitTestVisible = true;
                            break;
                        }
                    }

                    if (!found)
                        removeHighlight();
                }
                count = 0;
                highlightTimer.Tag = count;
                highlightTimer.Stop();
                highlightTimer = null;
            }*/
            bool mainSelected = false;
            foreach (timelineInfo i in timeLineList)
            {
                tourSystem.StopDrawingHighlight();
                tourSystem.StopDrawingPath();
                e.GetType();
                
                if (e.MouseDevice.GetPosition(mainCanvas).Y > i.pos && e.MouseDevice.GetPosition(mainCanvas).Y < i.pos + timelineHeight)
                {

                    found = true;
                    highlightTL(i.pos);
                    highlightData = i;
                    TourTL curTL = (TourTL)highlightData.timeline;
                    if (curTL.type == TourTLType.artwork) mainSelected = true;
                    if (curTL.type != TourTLType.highlight && curTL.type != TourTLType.path)
                    {
                        setButtonEnabled(eraseButton, false);
                        opacitySlider.IsEnabled = false;
                        opacitySlider.Opacity = 0.4;
                    }
                    else
                    {
                        if (curTL.type == TourTLType.highlight)
                        {
                            opacitySlider.IsEnabled = true;
                            opacitySlider.Opacity = 1;
                        }
                        else
                        {
                            opacitySlider.IsEnabled = false;
                            opacitySlider.Opacity = 0.4;
                        }
                        setButtonEnabled(eraseButton, true);
                    }
                    /*successfulSaveLabel = new Label();
             newMediaButton = new SurfaceButton();
             newAudioButton = new SurfaceButton();
             newDrawingButton = new SurfaceButton();
             newHighlightButton = new SurfaceButton();
             doneButton = new SurfaceButton();
             saveButton = new SurfaceButton();
             removeComponentButton = new SurfaceButton();
             removeEventButton = new SurfaceButton();
             eraseButton = new SurfaceButton();
             opacityLabel = new Label();
             opacitySlider = new SurfaceSlider();
             timelineSlider = new SurfaceSlider();
             timeLineLabel = new Label();
             renameTimelineButton = new SurfaceButton();*/


                    i.lengthSV.IsHitTestVisible = true;
                    EnableDrawingIfNeeded();
                    eraseButton.Content = "Erase";
                    break;
                }
            }

            if (!found)
            {
                removeTLHighlight();
                setButtonEnabled(removeComponentButton, false);
                setButtonEnabled(renameTimelineButton, false);
            }
            else
            {
                setButtonEnabled(removeComponentButton, true);
                setButtonEnabled(renameTimelineButton, true);
                if (mainSelected)
                {
                    setButtonEnabled(removeComponentButton, false);
                }
            }
            EnableDrawingIfNeeded();

        }


        void mainCanvas_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            bool found = false;

            /*if (highlightTimer != null)
            {
                int count = (int)highlightTimer.Tag;

                if (count < 2000)
                {
                    foreach (timeLines i in timeLineList)
                    {
                        if (e.TouchDevice.GetPosition(mainCanvas).Y > i.pos && e.TouchDevice.GetPosition(mainCanvas).Y < i.pos + timelineHeight)
                        {
                            found = true;
                            highlight(i.pos);
                            highlightData = i;
                            i.lengthScatter.IsHitTestVisible = true;
                            break;
                        }
                    }

                    if (!found)
                        removeHighlight();
                }
                count = 0;
                highlightTimer.Tag = count;
                highlightTimer.Stop();
                highlightTimer = null;
            }*/
            bool mainSelected = false;
            foreach (timelineInfo i in timeLineList)
            {
                tourSystem.StopDrawingHighlight();
                tourSystem.StopDrawingPath();
                e.GetType();
                if (e.TouchDevice.GetPosition(mainCanvas).Y > i.pos && e.TouchDevice.GetPosition(mainCanvas).Y < i.pos + timelineHeight)
                {

                    found = true;
                    highlightTL(i.pos);
                    highlightData = i;
                    TourTL curTL = (TourTL)highlightData.timeline;
                    if (curTL.type == TourTLType.artwork) mainSelected = true;
                    if (curTL.type != TourTLType.highlight && curTL.type != TourTLType.path)
                    {
                        setButtonEnabled(eraseButton, false);
                        opacitySlider.IsEnabled = false;
                        opacitySlider.Opacity = 0.4;
                    }
                    else
                    {
                        if (curTL.type == TourTLType.highlight)
                        {
                            opacitySlider.IsEnabled = true;
                            opacitySlider.Opacity = 1;
                        }
                        else
                        {
                            opacitySlider.IsEnabled = false;
                            opacitySlider.Opacity = 0.4;
                        }
                        setButtonEnabled(eraseButton, true);
                    }
                    /*successfulSaveLabel = new Label();
             newMediaButton = new SurfaceButton();
             newAudioButton = new SurfaceButton();
             newDrawingButton = new SurfaceButton();
             newHighlightButton = new SurfaceButton();
             doneButton = new SurfaceButton();
             saveButton = new SurfaceButton();
             removeComponentButton = new SurfaceButton();
             removeEventButton = new SurfaceButton();
             eraseButton = new SurfaceButton();
             opacityLabel = new Label();
             opacitySlider = new SurfaceSlider();
             timelineSlider = new SurfaceSlider();
             timeLineLabel = new Label();
             renameTimelineButton = new SurfaceButton();*/


                    i.lengthSV.IsHitTestVisible = true;
                    EnableDrawingIfNeeded();
                    eraseButton.Content = "Erase";
                    break;
                }
            }

            if (!found)
            {
                removeTLHighlight();
                setButtonEnabled(removeComponentButton, false);
                setButtonEnabled(renameTimelineButton, false);
            }
            else
            {
                setButtonEnabled(removeComponentButton, true);
                setButtonEnabled(renameTimelineButton, true);
                if (mainSelected)
                {
                    setButtonEnabled(removeComponentButton, false);
                }
            }
            EnableDrawingIfNeeded();
        }

        public void setButtonEnabled(SurfaceButton button, bool enabled)
        {
            if (enabled)
            {
                //button.Background = (Brush)(new BrushConverter().ConvertFrom("#4D000000"));
                button.IsEnabled = true;
            }
            else
            {
                button.IsEnabled = false;
                //button.Background = Brushes.Gray;
            }
        }

        #endregion

        #region methods for adding/removing timelines and TourEvents to timelines

        public double getNextPos()
        {
            return leftRightCanvas.ActualHeight - 3;
        }

        public void addTimelineAndEventPostInit(Timeline timeline, BiDictionary<double, TourEvent> tourTL_dict, String title, TourEvent te, double beginTime, double duration)
        {
            TourAuthoringUI.timelineInfo tli = addTimeline(timeline, tourTL_dict, title, getNextPos());
            addTourEvent(tli, te, tli.lengthSV, beginTime, duration);
            timelineCount += 1;
            updateTLSize();
        }

        public void removeHighlightedTimeline(Object sender, EventArgs e)
        {
            if (((TourTL)highlightData.timeline) != null)
            {
                tourSystem.undoableActionPerformed();
                if (((TourTL)highlightData.timeline).type == TourTLType.artwork)
                    return;
                //tourSystem.tourDict.Remove(highlightData.timeline);
                //tourSystem.tourDictRev.Remove(highlightData.timeline);
                tourSystem.tourBiDictionary.RemoveByFirst(highlightData.timeline);
                
                TourTL timeline = (TourTL)highlightData.timeline;
                if (timeline.type == TourTLType.highlight || timeline.type == TourTLType.path)
                {
                    SurfaceInkCanvas sic = ((TourParallelTL)timeline).inkCanvas;
                    sic.Visibility = Visibility.Collapsed;
                    tourSystem.inkCanvases.Remove(sic);
                }
                removeTLHighlight();
                this.refreshUI();
            }
            setButtonEnabled(removeEventButton, false);
            setButtonEnabled(removeComponentButton, false);
            setButtonEnabled(eraseButton, false);
            setButtonEnabled(renameTimelineButton, false);
            opacitySlider.IsEnabled = false;
            
        }

        public bool isFadeType(TourEvent.Type type)
        {
            if (type == TourEvent.Type.fadeInHighlight ||
                type == TourEvent.Type.fadeOutHighlight ||
                type == TourEvent.Type.fadeInMedia ||
                type == TourEvent.Type.fadeOutMedia ||
                type == TourEvent.Type.fadeInMSI ||
                type == TourEvent.Type.fadeOutMSI ||
                type == TourEvent.Type.fadeInPath ||
                type == TourEvent.Type.fadeOutPath)
                return true;
            return false;
        }
        public void refreshUI()
        {
            //double x = leftRightSVI.Center.X;
            //double oldWidth = leftRightSVI.Width;
            double scrub = tourSystem.authorTimerCountSpan.TotalSeconds;
            double left = Canvas.GetLeft(movableScrubHandle);
            tourSystem.refreshAuthoringUI(false);
            Console.WriteLine("SCRUB: " + scrub);
            tourSystem.StopAndReloadTourAuthoringUIFromDict(scrub);
            Canvas.SetLeft(movableScrubHandle, left);
            leftRightSVICenterChanged(leftRightSVI, new EventArgs());
        }
        public void reloadUI()
        {
            double scrub = tourSystem.authorTimerCountSpan.TotalSeconds;
            tourSystem.refreshAuthoringUI(true);
            tourSystem.StopAndReloadTourAuthoringUIFromDict(scrub);
        }

        public void removeHighlightedAnimation(Object sender, EventArgs e)
        {
            if (highlightedTourEvent != null)
            {
                tourSystem.undoableActionPerformed();
                tourEventInfo eventInfo = (tourEventInfo)highlightedTourEvent.Tag;
                if (isFadeType(eventInfo.tourEvent.type))
                    return;
                timelineInfo eventTimelineInfo = eventInfo.timelineInfoStruct;
                IList<BiDictionary<double,TourEvent>> listdict = tourSystem.tourBiDictionary.GetByFirst(eventTimelineInfo.timeline);
                if (listdict.Count!=0)
                    listdict[0].RemoveByFirst(eventInfo.beginTime);
                else
                eventTimelineInfo.lengthSV.Items.Remove(highlightedTourEvent);
            }
            //eventTim
            setButtonEnabled(removeEventButton, false);
            tourSystem.StopAndReloadTourAuthoringUIFromDict(tourSystem.authorTimerCountSpan.TotalSeconds);
            refreshUI();

        }


        public timelineInfo addTimeline(Timeline timeline, BiDictionary<double, TourEvent> tourTL_dict, String title, double pos)
        {
            timelineInfo current = new timelineInfo();
            //current.tourEventList = new List<ScatterViewItem>(); // not really useful
            current.pos = pos + 3;

            current.timeline = timeline;
            current.tourTL_dict = tourTL_dict;

            current.title = title;
            current.titlebox = new TextBox();
            current.titlebox.Text = title;
            current.titlebox.Height = timelineHeight - 3;
            current.titlebox.Width = textWidth;
            current.titlebox.BorderThickness = new Thickness(0);
            current.titlebox.FontSize = 20;
            current.titlebox.Tag = current;
            current.titlebox.Background = (Brush)(new BrushConverter().ConvertFrom("#093024"));
            current.titlebox.Foreground = Brushes.White;
            current.titlebox.IsHitTestVisible=false;
            titleCanvas.Children.Add(current.titlebox);
            Canvas.SetTop(current.titlebox, pos + 3);

            current.lengthSV = new ScatterView();
            current.lengthSV.Width = timelineWidth; // jcchin
            current.lengthSV.Height = timelineHeight - 3;
            current.lengthSV.Background = Brushes.White;
            current.lengthSV.IsHitTestVisible = false;

            leftRightCanvas.Children.Add(current.lengthSV);


            Canvas.SetTop(current.lengthSV, pos + 3);

            // addTourEvent(current.lengthScatter, 0, 1, current); // sample testing line - not needed anymore

            timeLineList.Add(current);

            return current;
        }
        public void updateTLSize()
        {
            timelineAreaHeight = (timelineHeight * timelineCount);
            if (timelineAreaHeight < (canvasWrapper.Height - 60))
            {
                timelineAreaHeight = canvasWrapper.Height - 60; // minimum height to fill UI space on bottom of screen
            }
            centerY = timelineAreaHeight / 2;
            mainSV.Height = timelineAreaHeight;

            mainSVI.Height = timelineAreaHeight;
            mainCanvas.Height = timelineAreaHeight;
            titleCanvas.Height = timelineAreaHeight;
            leftRightSVI.Height = timelineAreaHeight;
            leftRightCanvas.Height = timelineAreaHeight;


        }

        public void insertTourEvent(TourEvent te, Timeline timeline, double beginTime)
        {
            foreach (timelineInfo ti in timeLineList)
            {
                if (ti.timeline.Equals(timeline))
                {
                    addTourEvent(ti, te, ti.lengthSV, beginTime, te.duration);
                }
            }
        }


        public void addAudioEvent(timelineInfo timelineInfoStruct, TourEvent tourEvent, ScatterView timelineSV, double beginTime, double duration)
        {
            //if (beginTime < 0) beginTime = 0;
            ScatterViewItem currentSVI = new ScatterViewItem();
            currentSVI.MinWidth = 10; // don't want it to disappear, but still need it to be touchable (even if resolution is as low as 1024 x 768)
            currentSVI.MinHeight = 10;
            currentSVI.Width = duration * (timelineWidth / timelineLength);
            currentSVI.Height = timelineHeight - 7;
            currentSVI.Background = new SolidColorBrush(Colors.Transparent);
            currentSVI.Orientation = 0;
            currentSVI.CanRotate = false;
            //currentSVI.CanScale = false;
            currentSVI.Deceleration = double.NaN; // disables inertia
            currentSVI.Center = new Point((beginTime * (timelineWidth / timelineLength)) + (currentSVI.Width / 2), (timelineHeight / 2) - 2);
            currentSVI.Opacity = .7;
            currentSVI.ContainerManipulationCompleted += new ContainerManipulationCompletedEventHandler(tourAudioEventSVI_ContainerManipulationCompleted);
            currentSVI.PreviewTouchUp += new EventHandler<TouchEventArgs>(tourEventSVI_PreviewTouchUp);
            currentSVI.PreviewMouseUp += new MouseButtonEventHandler(tourEventSVI_PreviewTouchUp);

            currentSVI.PreviewTouchUp += new EventHandler<TouchEventArgs>(tourAudioEventSVI_PreviewTouchUp);
            currentSVI.PreviewMouseUp += new MouseButtonEventHandler(tourAudioEventSVI_PreviewTouchUp);
            currentSVI.SizeChanged += new SizeChangedEventHandler(tourEventSVI_SizeChanged);
            currentSVI.PreviewMouseWheel += new MouseWheelEventHandler(currentAudioSVI_PreviewMouseWheel);
            DependencyPropertyDescriptor dpd1 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd1.AddValueChanged(currentSVI, tourAudioEventCenterChanged);

            tourEventInfo currentAnimInfo = new tourEventInfo();
            currentAnimInfo.timelineInfoStruct = timelineInfoStruct;
            currentAnimInfo.beginTime = beginTime;
            currentAnimInfo.tourEvent = tourEvent;
            currentAnimInfo.centerY = (timelineHeight / 2) - 2;
            currentAnimInfo.centerX = (beginTime * (timelineWidth / timelineLength)) + (currentSVI.Width / 2);
            currentAnimInfo.originalLoc = beginTime * (timelineWidth / timelineLength);
            //currentSVI.MaxHeight = currentSVI.Height;
            Rectangle r = new Rectangle();
            r.Width = currentSVI.Width;
            r.Height = currentSVI.Height;
            
            
            
            r.Fill = (Brush)(new BrushConverter().ConvertFrom("#245c4f"));
            
            currentSVI.Content = r;

            currentAnimInfo.r = r;
            currentSVI.Tag = currentAnimInfo;


            timelineSV.Items.Add(currentSVI);
        }


        public void addTourEvent(timelineInfo timelineInfoStruct, TourEvent tourEvent, ScatterView timelineSV, double beginTime, double duration)
        {
            
            //if (beginTime < 0) beginTime = 0;
            ScatterViewItem currentSVI = new ScatterViewItem();
            currentSVI.MinWidth = 10; // don't want it to disappear, but still need it to be touchable (even if resolution is as low as 1024 x 768)
            currentSVI.MinHeight = 10;
            currentSVI.Width = duration * (timelineWidth / timelineLength);
            currentSVI.Height = timelineHeight - 7;
            currentSVI.Background = new SolidColorBrush(Colors.Transparent);
            currentSVI.Orientation = 0;
            currentSVI.CanRotate = false;
            currentSVI.Deceleration = double.NaN; // disables inertia
            currentSVI.Center = new Point((beginTime * (timelineWidth / timelineLength)) + (currentSVI.Width / 2), (timelineHeight / 2) - 2);
            currentSVI.Opacity = .7;
            currentSVI.ContainerManipulationCompleted += new ContainerManipulationCompletedEventHandler(tourEventSVI_ContainerManipulationCompleted);
            currentSVI.PreviewTouchUp += new EventHandler<TouchEventArgs>(tourEventSVI_PreviewTouchUp);
            currentSVI.PreviewMouseUp += new MouseButtonEventHandler(tourEventSVI_PreviewTouchUp);
            currentSVI.PreviewMouseDown += tourEventSVI_PreviewMouseDown;

            DependencyPropertyDescriptor dpd1 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd1.AddValueChanged(currentSVI, tourEventCenterChanged);

            tourEventInfo currentAnimInfo = new tourEventInfo();
            currentAnimInfo.timelineInfoStruct = timelineInfoStruct;
            currentAnimInfo.beginTime = beginTime;
            currentAnimInfo.tourEvent = tourEvent;
            currentAnimInfo.centerY = (timelineHeight / 2) - 2;
            currentAnimInfo.centerX = (beginTime * (timelineWidth / timelineLength)) + (currentSVI.Width / 2);
            currentAnimInfo.originalLoc = beginTime * (timelineWidth / timelineLength);
            //currentSVI.MaxHeight = currentSVI.Height;
            Rectangle r = new Rectangle();
            r.Width = currentSVI.Width;
            r.Height = currentSVI.Height;
            Timeline timeline = timelineInfoStruct.timeline;
            if (tourEvent != null)
            {
                LinearGradientBrush fadeInBrush = new LinearGradientBrush();
                fadeInBrush.StartPoint = new Point(0, 0);
                fadeInBrush.EndPoint = new Point(1, 0);
                fadeInBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
                fadeInBrush.GradientStops.Add(new GradientStop((Color)System.Windows.Media.ColorConverter.ConvertFromString("#245c4f"), 0.7));
                fadeInBrush.GradientStops.Add(new GradientStop((Color)System.Windows.Media.ColorConverter.ConvertFromString("#245c4f"), 1.0));
                LinearGradientBrush fadeOutBrush = new LinearGradientBrush();
                fadeOutBrush.StartPoint = new Point(0, 0);
                fadeOutBrush.EndPoint = new Point(1, 0);
                fadeOutBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
                fadeOutBrush.GradientStops.Add(new GradientStop((Color)System.Windows.Media.ColorConverter.ConvertFromString("#245c4f"), 0.0));
                fadeOutBrush.GradientStops.Add(new GradientStop((Color)System.Windows.Media.ColorConverter.ConvertFromString("#245c4f"), 0.3));
                switch (tourEvent.type)
                {

                    case TourEvent.Type.fadeInMedia:
                        tourSystem.registerDockableItem((tourEvent as FadeInMediaEvent).media, timeline);
                        r.Fill = fadeInBrush;
                        break;
                    case TourEvent.Type.fadeOutMedia:
                        tourSystem.registerDockableItem((tourEvent as FadeOutMediaEvent).media, timeline);
                        r.Fill = fadeOutBrush;
                        break;
                    case TourEvent.Type.zoomMedia:
                        tourSystem.registerDockableItem((tourEvent as ZoomMediaEvent).media, timeline);
                        r.Fill = (Brush)(new BrushConverter().ConvertFrom("#245c4f"));
                        break;
                    case TourEvent.Type.fadeInPath:
                        r.Fill = fadeInBrush;
                        break;
                    case TourEvent.Type.fadeOutPath:
                        r.Fill = fadeOutBrush;
                        break;
                    case TourEvent.Type.fadeInHighlight:
                        r.Fill = fadeInBrush;
                        break;
                    case TourEvent.Type.fadeOutHighlight:
                        r.Fill = fadeOutBrush;
                        break;
                    case TourEvent.Type.zoomMSI:
                        tourSystem.registerMSI((tourEvent as ZoomMSIEvent).msi, timeline);
                        r.Fill = (Brush)(new BrushConverter().ConvertFrom("#245c4f"));
                        break;
                    default:
                        r.Fill = (Brush)(new BrushConverter().ConvertFrom("#245c4f"));
                        break;

                }
            }
            else
            {
                r.Fill = (Brush)(new BrushConverter().ConvertFrom("#245c4f"));
            }
            currentSVI.Content = r;

            currentAnimInfo.r = r;
            currentSVI.Tag = currentAnimInfo;
            if (tourEvent == null)
                currentSVI.IsManipulationEnabled = false;
            currentSVI.PreviewMouseWheel +=new MouseWheelEventHandler(currentSVI_PreviewMouseWheel);
            currentSVI.SizeChanged += new SizeChangedEventHandler(tourEventSVI_SizeChanged);
            //timeline.tourEventList.Add(currentSVI); // not really useful

            timelineSV.Items.Add(currentSVI);
        }

        #endregion

        #region mainSVI event handler for panning list of timelines up and down

        private void MainSVICenterChanged(Object sender, EventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            if (currentScatter.Center.Y > (currentScatter.Height / 2))
            {
                currentScatter.Center = new Point(centerX, (currentScatter.Height / 2));
            }
            else if (currentScatter.Center.Y < ((canvasWrapper.Height - 60) - (currentScatter.Height / 2)))
            {
                currentScatter.Center = new Point(centerX, ((canvasWrapper.Height - 60) - (currentScatter.Height / 2)));
            }
            else
            {
                currentScatter.Center = new Point(centerX, currentScatter.Center.Y);
            }
        }

        #endregion

        #region tourEventSVI event handlers for highlighting, modifying TourEvent beginTime/duration, panning TourEvent bar left and right, and changing width of TourEvent bar

        private void tourEventSVI_PreviewMouseDown(Object sender, EventArgs e)
        {
            if (!currentTouched.Contains(sender as ScatterViewItem))
            {
                currentTouched.Add(sender as ScatterViewItem);
                Console.WriteLine("Added a thing!");
            }
        }

        private void tourEventSVI_PreviewTouchUp(Object sender, EventArgs e)
        {
            Console.WriteLine("1: tourEventSVI_PreviewTouchUp");
            if (highlightedTourEvent != null)
            {
                tourEventInfo previousAnimInfo = (tourEventInfo)highlightedTourEvent.Tag;
                TourEvent tourEvent = previousAnimInfo.tourEvent;
                if (tourEvent != null)
                {
                    LinearGradientBrush fadeInBrush = new LinearGradientBrush();
                    fadeInBrush.StartPoint = new Point(0, 0);
                    fadeInBrush.EndPoint = new Point(1, 0);
                    fadeInBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
                    fadeInBrush.GradientStops.Add(new GradientStop((Color)System.Windows.Media.ColorConverter.ConvertFromString("#245c4f"), 0.7));
                    fadeInBrush.GradientStops.Add(new GradientStop((Color)System.Windows.Media.ColorConverter.ConvertFromString("#245c4f"), 1.0));
                    LinearGradientBrush fadeOutBrush = new LinearGradientBrush();
                    fadeOutBrush.StartPoint = new Point(0, 0);
                    fadeOutBrush.EndPoint = new Point(1, 0);
                    fadeOutBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
                    fadeOutBrush.GradientStops.Add(new GradientStop((Color)System.Windows.Media.ColorConverter.ConvertFromString("#245c4f"), 0.0));
                    fadeOutBrush.GradientStops.Add(new GradientStop((Color)System.Windows.Media.ColorConverter.ConvertFromString("#245c4f"), 0.3));
                    switch (tourEvent.type)
                    {
                        case TourEvent.Type.fadeInMedia:
                            previousAnimInfo.r.Fill = fadeInBrush;
                            break;
                        case TourEvent.Type.fadeOutMedia:
                            previousAnimInfo.r.Fill = fadeOutBrush;
                            break;
                        case TourEvent.Type.fadeInPath:
                            previousAnimInfo.r.Fill = fadeInBrush;
                            break;
                        case TourEvent.Type.fadeOutPath:
                            previousAnimInfo.r.Fill = fadeOutBrush;
                            break;
                        case TourEvent.Type.fadeInHighlight:
                            previousAnimInfo.r.Fill = fadeInBrush;
                            break;
                        case TourEvent.Type.fadeOutHighlight:
                            previousAnimInfo.r.Fill = fadeOutBrush;
                            break;

                        default:
                            previousAnimInfo.r.Fill = (Brush)(new BrushConverter().ConvertFrom("#245c4f"));
                            break;

                    }
                }
                else
                    previousAnimInfo.r.Fill = (Brush)(new BrushConverter().ConvertFrom("#245c4f"));
            }

            

            ScatterViewItem tourEventSVI = sender as ScatterViewItem;
            highlightedTourEvent = tourEventSVI;
            tourEventInfo current = (tourEventInfo)highlightedTourEvent.Tag;
            current.r.Fill = (Brush)(new BrushConverter().ConvertFrom("#79aa89"));
            if (current.tourEvent == null)
                return;
            if (isFadeType(current.tourEvent.type))
            {
                setButtonEnabled(removeEventButton, false);
            }
            else
            {
                setButtonEnabled(removeEventButton, true);
            }
            //refreshUI();
            EnableDrawingIfNeeded();
        }

        private void tourAudioEventSVI_PreviewTouchUp(Object sender, EventArgs e)
        {
            ScatterViewItem tourEventSVI = sender as ScatterViewItem;

            double begintime = (tourEventSVI.Center.X - (tourEventSVI.Width / 2)) * (timelineLength / timelineWidth);
            if (((TourTL)highlightData.timeline) != null)
            {
                if (((TourTL)highlightData.timeline).type == TourTLType.audio)
                {



                    TourMediaTL timeline = (TourMediaTL)highlightData.timeline;
                    timeline.BeginTime = TimeSpan.FromSeconds(begintime);
                    timeline.Duration = TimeSpan.FromSeconds(tourEventSVI.Width * (timelineLength / timelineWidth));
                }

                //this.refreshUI();
                tourSystem.StopAndReloadTourAuthoringUIFromDict(begintime);

            }
            


        }

        public void EnableDrawingIfNeeded()
        {
            foreach (SurfaceInkCanvas sic in tourSystem.inkCanvases)
            {
                sic.IsHitTestVisible = false;
                sic.Visibility = Visibility.Collapsed;
            }
            if (tourSystem.tourAuthoringOn)
            {
            try
            {
                TourTL tourtl = (TourTL)highlightData.timeline;
                if (tourtl.type == TourTLType.highlight)
                {
                    SurfaceInkCanvas can = ((TourParallelTL)tourtl).inkCanvas;
                    tourSystem.changeCanvas(false, can, tourtl.file);
                    can.EditingMode = SurfaceInkEditingMode.Ink;
                    can.IsHitTestVisible = true;
                    //can.Visibility = Visibility.Visible;
                }
                if (tourtl.type == TourTLType.path)
                {
                    SurfaceInkCanvas can = ((TourParallelTL)tourtl).inkCanvas;
                    tourSystem.changeCanvas(true, can, tourtl.file);
                    can.EditingMode = SurfaceInkEditingMode.Ink;
                    can.IsHitTestVisible = true;
                    //can.Visibility = Visibility.Visible;
                }
            }
            catch { }
            }
            
        }

        private void tourEventSVI_ContainerManipulationCompleted(Object sender, EventArgs e)
        {
            Console.WriteLine("2: tourEventSVI_ContainerManipulationCompleted");
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            tourEventInfo current = (tourEventInfo)currentScatter.Tag;
            if (current.tourEvent == null)
                return;
            // MODIFY TourEvent - beginTime & duration
            current.timelineInfoStruct.tourTL_dict.RemoveByFirst(current.beginTime);
            //Dictionary<TourEvent, double> itemDictRev = tourSystem.tourDictRev[current.timelineInfoStruct.timeline];
            //itemDictRev.Remove(current.tourEvent);

            //Console.WriteLine("************************************************************");

            //Console.WriteLine("Old begin: " + current.beginTime);
            double newBeginTime = (currentScatter.Center.X - (currentScatter.Width / 2)) * (timelineLength / timelineWidth);
            Console.WriteLine("New Begin Time = " + newBeginTime);


            current.beginTime = newBeginTime;
            // Console.WriteLine("New Begin: " + current.beginTime);

            //Console.WriteLine("Original Location: " + current.originalLoc);
            //Console.WriteLine("Width: " + currentScatter.Width);
            //Console.WriteLine("Center: " + currentScatter.Center.X);
            //Console.WriteLine("-----------------------------------------------------------");

            current.originalLoc = newBeginTime * (timelineWidth / timelineLength);
            currentScatter.Tag = current;

            current.tourEvent.duration = currentScatter.Width * (timelineLength / timelineWidth);
            current.timelineInfoStruct.tourTL_dict.Add(newBeginTime, current.tourEvent); // add new beginTime
            //itemDictRev.Add(current.tourEvent, newBeginTime);
            //// Testing code!!! findhere
            current.timelineInfoStruct.timeline.Duration = tourSystem.tourStoryboard.Duration;

            ////
            if (newBeginTime < 0) newBeginTime = 0;
            //this.refreshUI();
            tourSystem.StopAndReloadTourAuthoringUIFromDict(newBeginTime); // stop and reload tour authoring UI from tourDict
            
        }

        private void tourAudioEventSVI_ContainerManipulationCompleted(Object sender, EventArgs e)
        {
            Console.WriteLine("2: tourEventSVI_ContainerManipulationCompleted");
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            tourEventInfo current = (tourEventInfo)currentScatter.Tag;
            //if (current.tourEvent == null)
            //    return;
            // MODIFY TourEvent - beginTime & duration
            current.timelineInfoStruct.tourTL_dict.RemoveByFirst(current.beginTime);
            //Dictionary<TourEvent, double> itemDictRev = tourSystem.tourDictRev[current.timelineInfoStruct.timeline];
            //itemDictRev.Remove(current.tourEvent);

            //Console.WriteLine("************************************************************");

            //Console.WriteLine("Old begin: " + current.beginTime);
            double newBeginTime = (currentScatter.Center.X - (currentScatter.Width / 2)) * (timelineLength / timelineWidth);
            //Console.WriteLine("New Begin Time = " + newBeginTime);

            ///double newBeginTime = current.beginTime;
            current.beginTime = newBeginTime;
            // Console.WriteLine("New Begin: " + current.beginTime);

            //Console.WriteLine("Original Location: " + current.originalLoc);
            //Console.WriteLine("Width: " + currentScatter.Width);
            //Console.WriteLine("Center: " + currentScatter.Center.X);
            //Console.WriteLine("-----------------------------------------------------------");

            current.originalLoc = newBeginTime * (timelineWidth / timelineLength);
            currentScatter.Tag = current;

            //current.tourEvent.duration = currentScatter.Width * (timelineLength / timelineWidth);
            //current.timelineInfoStruct.tourTL_dict.Add(newBeginTime, current.tourEvent); // add new beginTime
            //itemDictRev.Add(current.tourEvent, newBeginTime);
            //// Testing code!!! findhere
            //current.timelineInfoStruct.timeline.Duration = tourSystem.tourStoryboard.Duration;

            ////
            if (newBeginTime < 0) newBeginTime = 0;
            //this.refreshUI();
            tourSystem.StopAndReloadTourAuthoringUIFromDict(newBeginTime); // stop and reload tour authoring UI from tourDict

        }



        private void tourEventCenterChanged(Object sender, EventArgs e)
        {
            Console.WriteLine("3: tourEventCenterChanged");
            ScatterViewItem currentScatter = sender as ScatterViewItem;

            if (currentTouched.Contains(sender as ScatterViewItem))
            {
                currentTouched.Remove(sender as ScatterViewItem);
                tourSystem.undoableActionPerformed();
            }

            tourEventInfo current = (tourEventInfo)currentScatter.Tag;


            if (currentScatter.Center.X < currentScatter.Width / 2)
            {
                //Console.WriteLine("AA");
                currentScatter.Center = new Point(currentScatter.Width / 2, current.centerY);
            }
            else if (currentScatter.Center.X > (leftRightSV.Width - (currentScatter.Width / 2)))
            {
                //Console.WriteLine("BB");
                currentScatter.Center = new Point(leftRightSV.Width - (currentScatter.Width / 2), current.centerY);
            }
            else
            {

                currentScatter.Center = new Point(currentScatter.Center.X, current.centerY);

            }
            //Console.WriteLine("originalLoc: " + current.originalLoc.ToString());
            current.centerX = currentScatter.Center.X;
            current.centerY = currentScatter.Center.Y;
            //current.originalLoc = currentScatter.Center.X - current.r.Width / 2.0;
            currentScatter.Tag = current;
        }

        private void tourAudioEventCenterChanged(Object sender, EventArgs e)
        {
            Console.WriteLine("3: tourEventCenterChanged");
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            tourEventInfo current = (tourEventInfo)currentScatter.Tag;


            if (currentScatter.Center.X < currentScatter.Width / 2)
            {
                //Console.WriteLine("AA");
                currentScatter.Center = new Point(currentScatter.Width / 2, current.centerY);
            }
            else if (currentScatter.Center.X > (leftRightSV.Width - (currentScatter.Width / 2)))
            {
                //Console.WriteLine("BB");
                currentScatter.Center = new Point(leftRightSV.Width - (currentScatter.Width / 2), current.centerY);
            }
            else
            {

                currentScatter.Center = new Point(currentScatter.Center.X, current.centerY);

            }
            /*
            //Console.WriteLine("originalLoc: " + current.originalLoc.ToString());
            current.centerX = currentScatter.Center.X;
            current.centerY = currentScatter.Center.Y;*/
            //current.originalLoc = currentScatter.Center.X - current.r.Width / 2.0;
            currentScatter.Tag = current;
            //this.tourAudioEventSVI_PreviewTouchUp(sender, e);
        }

        private void currentSVI_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScatterViewItem tourEventSVI = sender as ScatterViewItem;
            double newWidth = 0;
            
            
            if (highlightedTourEvent != null)

                newWidth = ((double)e.Delta) + tourEventSVI.ActualWidth;
                  

            if (newWidth < 10) return;

            ScatterViewItem currentScatter = sender as ScatterViewItem;
            tourEventInfo current = (tourEventInfo)currentScatter.Tag;
            if (current.tourEvent == null)
                return;
            if (e.Delta > 0)
            {
                if (current.beginTime + current.tourEvent.duration > tourDuration.TotalSeconds - 0.5) return;
            }

            tourEventSVI.Width = newWidth;

            // MODIFY TourEvent - beginTime & duration
            current.timelineInfoStruct.tourTL_dict.RemoveByFirst(current.beginTime);

            double newBeginTime = current.beginTime;
            current.beginTime = newBeginTime;

            current.originalLoc = newBeginTime * (timelineWidth / timelineLength);
            currentScatter.Tag = current;

            current.tourEvent.duration = (currentScatter.Width -((double)e.Delta)/2.0)  * (timelineLength / timelineWidth);
            current.timelineInfoStruct.tourTL_dict.Add(newBeginTime, current.tourEvent); // add new beginTime

            current.timelineInfoStruct.timeline.Duration = tourSystem.tourStoryboard.Duration;

            if (newBeginTime < 0) newBeginTime = 0;
 
            tourSystem.StopAndReloadTourAuthoringUIFromDict(newBeginTime); // stop and reload tour authoring UI from tourDict

            
        }

        private void currentAudioSVI_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            ScatterViewItem currentScatter = sender as ScatterViewItem;
            //currentScatter.SizeChanged -= new SizeChangedEventHandler(tourEventSVI_SizeChanged);
            double newWidth = 0;
            double delta = ((double)e.Delta) / 3.0;

            double oldWidth = currentScatter.Width;
            if (currentScatter != null)

                newWidth = delta + currentScatter.Width;


            if (newWidth < 20) return;

            

            //double begintime = (currentScatter.Center.X - (currentScatter.Width / 2)) * (timelineLength / timelineWidth);
           

            //ScatterViewItem currentScatter = sender as ScatterViewItem;
            tourEventInfo current = (tourEventInfo)currentScatter.Tag;
            if (e.Delta > 0)
            {
                if (current.beginTime + current.tourEvent.duration > tourDuration.TotalSeconds - 0.5) return;
            }
            current.timelineInfoStruct.tourTL_dict.RemoveByFirst(current.beginTime);

            //double newBeginTime = (currentScatter.Center.X - (currentScatter.Width / 2)) * (timelineLength / timelineWidth);
            double newBeginTime = current.beginTime;
            current.beginTime = newBeginTime;


            current.originalLoc = newBeginTime * (timelineWidth / timelineLength);
            currentScatter.Tag = current;

            ////
            if (newBeginTime < 0) newBeginTime = 0;
            //this.refreshUI();

            if (((TourTL)highlightData.timeline) != null)
            {
                if (((TourTL)highlightData.timeline).type == TourTLType.audio)
                {



                    TourMediaTL timeline = (TourMediaTL)highlightData.timeline;
                    timeline.BeginTime = TimeSpan.FromSeconds(newBeginTime);
                    timeline.Duration = TimeSpan.FromSeconds((newWidth - (delta/2.0)) * (timelineLength / timelineWidth));
                }


                //this.reloadUI();
                tourSystem.StopAndReloadTourAuthoringUIFromDict(newBeginTime);
            }

            currentScatter.Width = newWidth;

            //currentScatter.SizeChanged += new SizeChangedEventHandler(tourEventSVI_SizeChanged);
            //tourSystem.StopAndReloadTourAuthoringUIFromDict(newBeginTime);


           // this.tourAudioEventSVI_PreviewTouchUp(sender, e);
        }


        private void tourEventSVI_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //MessageBox.Show("4");
            Console.WriteLine("4: tourEventSVI_SizeChanged");

            ScatterViewItem currentScatter = sender as ScatterViewItem;
            tourEventInfo current = (tourEventInfo)currentScatter.Tag;
            if (currentTouched.Contains(sender as ScatterViewItem))
            {
                currentTouched.Remove(sender as ScatterViewItem);
                tourSystem.undoableActionPerformed();
            }
            currentScatter.Height = timelineHeight; // locks height

            double oldBegin = current.centerX - current.r.Width / 2.0;
            //currentScatter.Width = (currentScatter.Center.X - oldBegin) * 2;

            //Console.WriteLine("oldCenterX: " + current.centerX + " - " + currentScatter.Center.X);
            //Console.WriteLine("oldWidth: " + current.r.Width + " - " + currentScatter.Width);
            //Console.WriteLine("oldBegin:" + oldBegin);
            //Console.WriteLine("new center: " + currentScatter.Center.X);
            //Console.WriteLine("width: " + (currentScatter.Center.X - oldBegin) * 2);

            double left = currentScatter.Center.X - currentScatter.Width / 2.0;
            double delta = current.originalLoc - left;
            currentScatter.Center = new Point(currentScatter.Center.X + delta / 2.0, currentScatter.Center.Y);
            if (!((currentScatter.Width - delta)<=0))

            currentScatter.Width = currentScatter.Width - delta;
            current.r.Width = currentScatter.Width; // can only scale widtd

            oldWidth = e.PreviousSize.Width;
            curWidth = e.NewSize.Width;
        }



        #endregion

        public void movableScrubHandleBackground_TouchDown(object Sender, TouchEventArgs e)
        {
            //Console.Out.WriteLine("Touch down");
            //This step is crucial as it sets the correct time of the yellow thing
            tourSystem.tourStoryboard.CurrentTimeInvalidated -= TourStoryboardAuthoring_CurrentTimeInvalidated;
            startDragPoint.X = tourSeekBarProgressWidth;
            //Console.Out.WriteLine("before move time");
            Point current = e.TouchDevice.GetCenterPosition(leftRightCanvas);

            Double dragDistance = current.X - startDragPoint.X;

            double tourSeekBarProgressTargetWidth = tourSeekBarProgressWidth + dragDistance;
            if (tourSeekBarProgressTargetWidth < 0)
            {
                tourSeekBarProgressTargetWidth = 0;
            }
            else if (tourSeekBarProgressTargetWidth > leftRightCanvas.Width)
            {
                tourSeekBarProgressTargetWidth = leftRightCanvas.Width;
            }
            backgroundMoved = true;
            //movableScrub.SetValue(Canvas.LeftProperty, (double)movableScrubHandle.GetValue(Canvas.LeftProperty) - centerX_LRScatterView_diff + 18.5);
            tourTimerCount = ((tourSeekBarProgressTargetWidth / timelineRulerCanvas.Width) * tourSystem.tourStoryboard.Duration.TimeSpan.TotalSeconds);
            tourTimerCountSpan = TimeSpan.FromSeconds(tourTimerCount);
            tourTimerCountSpanString = string.Format("{0:D2}:{1:D2}", tourTimerCountSpan.Minutes, tourTimerCountSpan.Seconds);
            tourSeekBarTimerCount.Content = tourTimerCountSpanString;

            //Console.Out.WriteLine("right most" + tourSeekBarProgressTargetWidth + centerX_LRScatterView_diff);
            movableScrubHandle.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth - 18.5 + centerX_LRScatterView_diff);
            movableScrubHandleExt.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth + centerX_LRScatterView_diff);

            tourSeekBarProgressWidth = tourSeekBarProgressTargetWidth;

            //startDragPoint should be the point where the yellow handler is
            startDragPoint.X = tourSeekBarProgressWidth;
            Console.Out.WriteLine("x" + startDragPoint.X);
            //Console.Out.WriteLine("after move time");
            tourSystem.tourStoryboard.CurrentTimeInvalidated -= TourStoryboardAuthoring_CurrentTimeInvalidated;
            refreshUI();

        }

        public void movableScrubHandleBackground_MouseDown(object Sender, MouseEventArgs e)
        {
            
            //Console.Out.WriteLine("Touch down");
            //This step is crucial as it sets the correct time of the yellow thing
            tourSystem.tourStoryboard.CurrentTimeInvalidated -= TourStoryboardAuthoring_CurrentTimeInvalidated;
            startDragPoint.X = tourSeekBarProgressWidth;
            //Console.Out.WriteLine("before move time");
            Point current = e.MouseDevice.GetCenterPosition(leftRightCanvas);

            Double dragDistance = current.X - startDragPoint.X;

            double tourSeekBarProgressTargetWidth = tourSeekBarProgressWidth + dragDistance;
            if (tourSeekBarProgressTargetWidth < 0)
            {
                tourSeekBarProgressTargetWidth = 0;
            }
            else if (tourSeekBarProgressTargetWidth > leftRightCanvas.Width)
            {
                tourSeekBarProgressTargetWidth = leftRightCanvas.Width;
            }
            backgroundMoved = true;
            //movableScrub.SetValue(Canvas.LeftProperty, (double)movableScrubHandle.GetValue(Canvas.LeftProperty) - centerX_LRScatterView_diff + 18.5);
            tourTimerCount = ((tourSeekBarProgressTargetWidth / timelineRulerCanvas.Width) * tourSystem.tourStoryboard.Duration.TimeSpan.TotalSeconds);
            tourTimerCountSpan = TimeSpan.FromSeconds(tourTimerCount);
            tourTimerCountSpanString = string.Format("{0:D2}:{1:D2}", tourTimerCountSpan.Minutes, tourTimerCountSpan.Seconds);
            tourSeekBarTimerCount.Content = tourTimerCountSpanString;

            //Console.Out.WriteLine("right most" + tourSeekBarProgressTargetWidth + centerX_LRScatterView_diff);
            movableScrubHandle.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth - 18.5 + centerX_LRScatterView_diff);
            movableScrubHandleExt.SetValue(Canvas.LeftProperty, tourSeekBarProgressTargetWidth + centerX_LRScatterView_diff);

            tourSeekBarProgressWidth = tourSeekBarProgressTargetWidth;

            //startDragPoint should be the point where the yellow handler is
            startDragPoint.X = tourSeekBarProgressWidth;
            Console.Out.WriteLine("x" + startDragPoint.X);
            //Console.Out.WriteLine("after move time");
            tourSystem.tourStoryboard.CurrentTimeInvalidated -= TourStoryboardAuthoring_CurrentTimeInvalidated;
            refreshUI();
            
        }

        private void movableScrubHandleBackground_TouchUp(object sender, EventArgs e)
        {
            //Console.WriteLine("22");
            //Console.Out.WriteLine("MouseUp");
            tourSystem.tourStoryboard.Seek(artModeWin, tourTimerCountSpan, TimeSeekOrigin.BeginTime);
            tourSystem.tourStoryboard.CurrentTimeInvalidated += TourStoryboardAuthoring_CurrentTimeInvalidated;
            tourSystem.authorTimerCountSpan = tourTimerCountSpan;
            backgroundMoved = false; //enable the preview move now
        }

        
    }
}

