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
using System.Windows.Media.Animation;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;


namespace GCNav
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Timeline : UserControl
    {
        private int _startY, _endY;
        private double _pixelInterval = 200;
        private double _pixelsPY; //pixels per year
        private double _panPercent = 0;
        private Point startPoint = new Point();
        private ScatterViewItem _sv;
        private List<Event> _events;
        private bool mouseOnAndDown = false;

        public List<Event> getEvents()
        {
            return _events;
        }

        public Timeline()
        {
            InitializeComponent();

            //enable pan on timeline
            mainCanvas.PreviewMouseDown += mainCanvas_PreviewMouseDown;
            mainCanvas.PreviewMouseMove += mainCanvas_PreviewMouseMove;
            mainCanvas.PreviewMouseUp += mainCanvas_PreviewMouseUp;
            mainCanvas.PreviewTouchDown += new EventHandler<TouchEventArgs>(mainCanvas_PreviewTouchDown);
            mainCanvas.PreviewTouchMove += new EventHandler<TouchEventArgs>(mainCanvas_PreviewTouchMove);
        }

        public void setRef(ScatterViewItem sv)
        {
            _sv = sv;
        }

        public void setMouseOnAndDown(bool b)
        {
            mouseOnAndDown = b;
        }

        
        void mainCanvas_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            startPoint = e.Device.GetCenterPosition(this);
            e.Handled = false;
            mouseOnAndDown = true;
        }

        void mainCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseOnAndDown)
            {
                Point current = e.Device.GetCenterPosition(this);
                TranslateTransform t = new TranslateTransform();
                t.X = current.X - startPoint.X;
                t.Y = 0;

                startPoint = current;

                TransformGroup tg = new TransformGroup();
                tg.Children.Add(_tickmarksCanvas.RenderTransform);
                tg.Children.Add(t);
                _tickmarksCanvas.RenderTransform = tg;

                TransformGroup tg2 = new TransformGroup();
                tg2.Children.Add(_eventsCanvas.RenderTransform);
                tg2.Children.Add(t);
                _eventsCanvas.RenderTransform = tg2;

                //pan the main catalog at the same time
                _sv.Center = new Point(_sv.Center.X + t.X, _sv.Center.Y);
                e.Handled = false;
            }
        }

        void mainCanvas_PreviewMouseUp(object sender, MouseEventArgs e)
        {
            mouseOnAndDown = false;
        }

        /// <summary>
        /// The two event handlers together enables pan on timeline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mainCanvas_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            Point current = e.Device.GetCenterPosition(this);
            TranslateTransform t = new TranslateTransform();
            t.X = current.X - startPoint.X;
            t.Y = 0;

            startPoint = current;

            TransformGroup tg = new TransformGroup();
            tg.Children.Add(_tickmarksCanvas.RenderTransform);
            tg.Children.Add(t);
            _tickmarksCanvas.RenderTransform = tg;

            TransformGroup tg2 = new TransformGroup();
            tg2.Children.Add(_eventsCanvas.RenderTransform);
            tg2.Children.Add(t);
            _eventsCanvas.RenderTransform = tg2;

            //pan the main catalog at the same time
            _sv.Center = new Point(_sv.Center.X + t.X, _sv.Center.Y);
            e.Handled = false;
        }

        void mainCanvas_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            //((Canvas)sender).CaptureTouch(e.TouchDevice);
            startPoint = e.TouchDevice.GetCenterPosition(this);
            e.Handled = false;
        }

        /// <summary>
        /// set the size of the timeline, called when the window dimension changed
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void setSize(double width, double height) 
        {
            this.Width = width;
            this.Height = height;
            rectangle1.Width = this.Width;
            rectangle1.Height = this.Height;          
            Canvas.SetTop(_eventsCanvas, height / 2);
            foreach (Event e in _eventsCanvas.Children) {
                e.setHeight(height/2);
            }
        }

        /// <summary>
        /// update the events and tickmarks
        /// </summary>
        /// <param name="startY"></param>
        /// <param name="endY"></param>
        /// <param name="actualTimelineLength"></param>
        public void update(int startY, int endY, double actualTimelineLength){
            _tickmarksCanvas.Children.Clear();
            _eventsCanvas.Children.Clear();
            //make sure there is at least one year in range
            if (endY == startY)
            {
                endY = endY + 1;
            }
            _pixelsPY = actualTimelineLength / (endY - startY);
            _startY = startY;
            _endY = endY;
            this.updateTickMarks();

            EventImpl eventImpl = new EventImpl("Data/EventXML.xml", _pixelsPY, this.Height / 2);
            _events = eventImpl.getEvents();
            foreach (Event e in _events)
            {
                _eventsCanvas.Children.Add(e);
                e.setLocation((e.Start - _startY) * _pixelsPY, 0);
            }
        }


        /// <summary>
        /// redraw the tick marks
        /// </summary>
        private void updateTickMarks() 
        {
            _tickmarksCanvas.Children.Clear();

            //make sure there is at least 1 tick mark
            double temp = Math.Round(_pixelInterval / _pixelsPY / 10);
            temp = (temp == 0) ? 1 : temp;

            int intervalInYear = (int)(temp * 10);
            int i;
            //generates the tick marks for the part that has images
            for (i = _startY; i < _endY; i = i + intervalInYear)
            {
                //starts from the left edge of the window
                this.newMajorTickMark(i, (i - _startY) * _pixelsPY);
                this.newTickMark((i - _startY) * _pixelsPY + intervalInYear * _pixelsPY / 2);
            }

            //fill the rest of the window with tickmarks if the previous round didn't
            int j = i;
            if ((_endY - _startY) * _pixelsPY < this.Width)
            {
                for (j = i; (j - _startY) * _pixelsPY < this.Width; j = j + intervalInYear) 
                {
                    this.newMajorTickMark(j, (j - _startY) * _pixelsPY);
                    this.newTickMark((j - _startY) * _pixelsPY + intervalInYear * _pixelsPY / 2);
                }
            }

            //extends the tick marks to Width/2 off screen in both direction so that the timeline won't look weird while zooming out.
            for (int k = j; ((k - j)*_pixelsPY) < this.Width/2; k = k + intervalInYear)
            {
                this.newMajorTickMark(k, (k - _startY) * _pixelsPY);
                this.newTickMark((k - _startY) * _pixelsPY + intervalInYear * _pixelsPY / 2);
            }

            for (int k = _startY - intervalInYear; (k - _startY) * _pixelsPY >= -this.Width * 1 / 2; k = k - intervalInYear)
            {
                this.newMajorTickMark(k, (k - _startY) * _pixelsPY);
                this.newTickMark((k - _startY) * _pixelsPY + intervalInYear * _pixelsPY / 2);
            }

        }

        /// <summary>
        /// zoom the timeline by zoomPercent
        /// </summary>
        /// <param name="zoomPercent"></param>
        public void zoom(double zoomPercent) 
        {
            _pixelsPY = _pixelsPY * zoomPercent;
            if (_endY != _startY)
            { 
                this.updateTickMarks();
                //zoom the event canvas
                if (_events != null)
                {
                    foreach (Event e in _events)
                    {
                        e.zoom(zoomPercent);
                        e.setLocation((e.Start - _startY) * _pixelsPY, 0);
                    }
                }
            }
        }

        /// <summary>
        /// translate the whole time line to the given pan percentage
        /// </summary>
        /// <param name="panPercent"></param>
        public void pan(double panPercent) 
        {
            double fromX = -((_endY - _startY) * _panPercent) * _pixelsPY;
            double toX = -((_endY - _startY) * panPercent) * _pixelsPY;

            TranslateTransform pan = new TranslateTransform();
            _tickmarksCanvas.RenderTransform = pan;
            _eventsCanvas.RenderTransform = pan;
            DoubleAnimation animX = new DoubleAnimation(fromX, toX, TimeSpan.FromSeconds(0.2));
            pan.BeginAnimation(TranslateTransform.XProperty, animX);

            _panPercent = panPercent;
        }

        /// <summary>
        /// creates the longer tick mark with the given year label at the given x position
        /// </summary>
        /// <param name="year"></param>
        /// <param name="xPos"></param>
        private void newMajorTickMark(int year, double xPos) 
        {
            Line l = new Line();

            l.X1 = xPos;
            l.Y1 = 0;
            l.X2 = xPos;
            l.Y2 = rectangle1.Height/2;

            // Create a red Brush
            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Color.FromArgb(255,0,0,0);

            // Set Line's width and color
            l.StrokeThickness = 3;
            l.Stroke = blackBrush;

            // Add line to the Grid.
            _tickmarksCanvas.Children.Add(l);

            //this.addLine(xPos);

            TextBlock yearLabel = new TextBlock();
            yearLabel.Text = ""+year;
            yearLabel.FontSize = 20;
            yearLabel.Foreground = blackBrush;
            _tickmarksCanvas.Children.Add(yearLabel);
            Canvas.SetLeft(yearLabel, xPos+5);
            Canvas.SetTop(yearLabel, rectangle1.Height / 3 / 2);
        }

        /// <summary>
        /// creates the shorter tick mark at the given x position
        /// </summary>
        /// <param name="xPos"></param>
        private void newTickMark(double xPos) {
            Line l = new Line();
            l.X1 = xPos;
            l.Y1 = 0;
            l.X2 = xPos;
            l.Y2 = rectangle1.Height/2/2;

            // Create a red Brush
            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Colors.Black;

            // Set Line's width and color
            l.StrokeThickness = 1;
            l.Stroke = blackBrush;

            // Add line to the Grid.
            _tickmarksCanvas.Children.Add(l);
        }
    }
}
