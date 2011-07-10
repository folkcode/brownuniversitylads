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

namespace GCNav
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Event : UserControl, IComparable<Event>
    {
        private double _height, _width;
        private int _start, _end;
        private Rectangle _eventRec;
        private string _name, _location, _description;
        private TextBlock _eventText;
        private Color _recColor;
        private Navigator _parent;
        private Boolean _infoIsDisplayed;

        public string Event_Name { get { return _name; } set { _name = value; _eventText.Text = _name; _eventText.HorizontalAlignment = HorizontalAlignment.Center; } }
        public string Location { get { return _location; } set { _location = value; } }
        public string Description { get { return _description; } set { _description = value; } }
        public int Start { get { return _start; } set { _start = value; } }
        public int End { get { return _end; } set { _end = value; } }

        public Event(double width, double height)
        {
            InitializeComponent();
            _eventRec = new Rectangle();
            _height = height;
            _width = width;
            _recColor = Color.FromRgb(152, 245, 255);
            _eventText = new TextBlock();
            _eventText.TextWrapping = TextWrapping.NoWrap;
            _eventText.TextTrimming = TextTrimming.WordEllipsis;
            _eventText.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
            _eventText.Padding = new Thickness(4, 0, 4, 0);
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(152, 100, 255));
            brush.Opacity = .35;
            _name = "";
            _location = "";
            _description = "";
            _start = 0;
            _end = 0;
            _eventText.Text = _name;
            _eventText.TextAlignment = TextAlignment.Center;
            this.drawRectangle();
            this.setLocation(0, 0);
            _infoIsDisplayed = false;
        }

        /// <summary>
        /// draws the event rectangle and creates touch and mouse event handlers
        /// </summary>
        public void drawRectangle()
        {
            
            _eventRec.Width = _width;
            _eventRec.Height = _height;
            SolidColorBrush brush = new SolidColorBrush(_recColor);
            brush.Opacity = .35;
            _eventRec.Fill = brush;
            canvas.Children.Insert(0, _eventRec);
            canvas.Children.Add(_eventText);
            //_eventRec.PreviewTouchUp += new EventHandler<TouchEventArgs>(TouchUpHandler);
            //_eventRec.PreviewMouseUp += new MouseButtonEventHandler(TouchUpHandler);
        }

        /// <summary>
        /// computes the width of the event box based on the start and end dates
        /// </summary>
        /// <param name="yearWidth"></param>
        public void computeWidth(double yearWidth) //parameter is the distance between the tickmarks representing each year
        {
            int span = _end - _start;
            _width = span * yearWidth;
            _eventRec.Width = _width;
            _eventText.Width = _width;
        }

        /// <summary>
        /// sets the height of the event box
        /// </summary>
        /// <param name="height"></param>
        public void setHeight(double height)
        {
            _eventRec.Height = height;
        }

        /// <summary>
        /// zooms in or out on the event box
        /// </summary>
        /// <param name="zoompercent"></param>
        public void zoom(double zoompercent)
        {
            _eventRec.Width *= zoompercent;
            _eventText.Width *= zoompercent;
        }

        /// <summary>
        /// sets the location of the event box
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void setLocation(double x, double y)
        {
            Canvas.SetLeft(_eventRec, x);
            Canvas.SetTop(_eventRec, y);
            Canvas.SetLeft(_eventText, ((_eventRec.ActualWidth - _eventText.ActualWidth) / 2) + x);
            Canvas.SetTop(_eventText, (_height / 4) + y);
        }

        /// <summary>
        /// sets the color of the event box
        /// </summary>
        /// <param name="c"></param>
        public void setColor(Color c)
        {
            _recColor = c;
            SolidColorBrush brush = new SolidColorBrush(_recColor);
            brush.Opacity = .35;
            _eventRec.Fill = brush;
        }

        /// <summary>
        /// when the event box is clicked, tell the parent (navigator) this event has been selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TouchUpHandler(object sender, EventArgs e)
        {
            if (_parent!=null) {
                _parent.eventSelected(this);
            }
        }

        /// <summary>
        /// set the parent (navigator)
        /// </summary>
        /// <param name="nav"></param>
        public void setParent(Navigator nav)
        {
            _parent = nav;
        }

        /// <summary>
        /// tells the event whether or not its information is currently displayed
        /// </summary>
        /// <param name="b"></param>
        public void setInfoIsDisplayed(Boolean b)
        {
            _infoIsDisplayed = b; 
        }

        /// <summary>
        /// whether or not this event's information is currently displayed
        /// </summary>
        /// <returns></returns>
        public Boolean infoIsDisplayed()
        {
            return _infoIsDisplayed;
        }

        /// <summary>
        /// compares this event to another event based on the start date
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(Event obj)
        {
            return this.Start.CompareTo(obj.Start);
        }
    }
}
