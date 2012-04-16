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
using Microsoft.Sample.Controls;
using WpfApplication;
using System.Windows.Media.Animation;
using System.ComponentModel;
using Microsoft.Surface.Presentation.Controls;
using System.Xml;

namespace GCNav
{
    /// <summary>
    /// Interaction logic for NamesWall.xaml
    /// </summary>
    public partial class NamesWall : UserControl
    {
        public NamesWall()
        {
            InitializeComponent();
            Canvas target = Wall.ContentCanvas;

            Wall.ContentCanvas.Background = Brushes.Black;
            Wall.SmallScrollIncrement = new Size(10, 30);

            AllocateNodes();
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(MainSVI, mainSVI_CenterChanged);

            MainSVI.ApplyTemplate();
            Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
            ssc = MainSVI.Template.FindName("shadow", MainSVI) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;
        }

        private void AllocateNodes()
        {
            Wall.VirtualChildren.Clear();
            Wall.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            double width = Wall.DesiredSize.Width-20;
            Wall.ContentCanvas.Margin = new Thickness(10);
            Wall.ContentCanvas.Width = width;
            double xborder = 35;
            double yborder = 8;
            double prevX = xborder;
            double prevY = yborder;

            XmlNodeList xmlNodes = Helpers.LoadNamesFromXML();
            List<string> names = new List<string>();
            foreach (XmlNode node in xmlNodes)
            {
                //get rid of the weird names, necessary?
                string first = node.Attributes.GetNamedItem("PanelFirst").InnerText;
                if (first.Equals("1") || first.Equals("NULL") || first.Equals(" "))
                {
                    first = "";
                }
                string last = node.Attributes.GetNamedItem("PanelLast").InnerText;
                if (last.Equals("1") || last.Equals("NULL") || last.Equals(" "))
                {
                    last = "";
                }
 
                string name = (first.Equals("")) ? last : first + " " + last;
                name = (name.Equals("")) ? "UNKNOWN" : name;
                name = name.Trim();
                names.Add(name);
            }
            names.Sort();

            foreach (string name in names)
            {
                TextBlock t = new TextBlock();
                t.FontSize = 20;
                t.Text = name;
                t.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                Size a = t.DesiredSize;
                t = null;

                if (prevX + a.Width + xborder > width)
                {
                    prevX = xborder;
                    prevY += a.Height + yborder;
                }
                Point pos = new Point(prevX, prevY);
                prevX += a.Width + xborder;
                Size s = new Size(a.Width + 2 * xborder, a.Height + 2 * yborder);
                TestShape shape = new TestShape(new Rect(pos, s), name);
                Wall.AddVirtualChild(shape);
            }
        }

        private void mainSVI_CenterChanged(object sender, EventArgs e)
        {
            Point center = MainSVI.ActualCenter;
            if (center.X == 480 && center.Y == 540) return;
            double delta = 540 - center.Y;
            Wall.SetVerticalOffset(Wall.VerticalOffset + delta);
            MainSVI.Center = new Point(480, 540);
        }

        private void MainSVI_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            //Perform hit testing on all children
            foreach (IVirtualChild i in Wall.VirtualChildren)
            {
                UIElement visual = i.Visual;
                if (visual == null) continue;
                FrameworkElement visualElement = (FrameworkElement)visual;
                Point p = e.GetTouchPoint(visualElement).Position;
                if (p.X > 0 && p.X < visualElement.ActualWidth && p.Y>0 && p.Y < visualElement.ActualHeight)
                {
                    TestShapeUp((TestShape)i);
                }
            }

        }

        private void MainSVI_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Perform hit testing on all children
            foreach (IVirtualChild i in Wall.VirtualChildren)
            {
                UIElement visual = i.Visual;
                if (visual == null) continue;
                FrameworkElement visualElement = (FrameworkElement)visual;
                Point p = e.GetPosition(visualElement);
                if (p.X > 0 && p.X < visualElement.ActualWidth && p.Y > 0 && p.Y < visualElement.ActualHeight)
                {
                    TestShapeUp((TestShape)i);
                }
            }
        }

        private void TestShapeUp(TestShape shape)
        {

        }
    }

    class TestShape : IVirtualChild
    {
        Rect _bounds;
        public Brush Fill { get; set; }
        public Brush Stroke { get; set; }
        public string Label { get; set; }
        UIElement _visual;
        string _text;
        public event EventHandler BoundsChanged;
        Storyboard st = new Storyboard();
        DoubleAnimation da = new DoubleAnimation();

        public TestShape(Rect bounds, string text)
        {
            _text = text;
            _bounds = bounds;
            da.From = 0.0;
            da.To = 1.0;
            da.Duration = new Duration(TimeSpan.FromSeconds(0.25));
        }


        public UIElement Visual
        {
            get { return _visual; }
        }

        public UIElement CreateVisual(VirtualCanvas parent)
        {
            if (_visual == null)
            {
                TextBlock t = new TextBlock();
                t.FontSize = 20;
                t.Text = _text;
                t.Foreground = Brushes.Gray;
                t.PreviewTouchUp += new EventHandler<TouchEventArgs>(t_TouchUp);
                t.PreviewMouseUp += new MouseButtonEventHandler(t_PreviewMouseUp);
                //st.Children.Add(da);
                //Storyboard.SetTarget(da, t);
                //Storyboard.SetTargetProperty(da, new PropertyPath(TextBlock.OpacityProperty));
                //st.Begin();
                _visual = t;
            }
            return _visual;
        }

        void t_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            int a = 0;
        }

        void t_TouchUp(object sender, TouchEventArgs e)
        {
            int a = 0;
        }

        public void DisposeVisual()
        {
            //st.Stop();
            //st.Children.Clear();
            _visual = null;
        }

        public Rect Bounds
        {
            get { return _bounds; }
        }

    }
}
