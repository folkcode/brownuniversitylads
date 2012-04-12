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
using System.Windows.Shapes;
using Microsoft.Sample.Controls;
using System.Windows.Media.Animation;
using System.ComponentModel;
using Microsoft.Surface.Presentation.Controls;

namespace WpfApplication
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        VirtualCanvas grid;

        public Window2()
        {
            InitializeComponent();

            grid = Graph;
            Canvas target = grid.ContentCanvas;

            grid.Background = new SolidColorBrush(Color.FromRgb(0xd0, 0xd0, 0xd0));
            grid.ContentCanvas.Background = Brushes.Black;

            AllocateNodes();
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(MainSVI, mainSVI_CenterChanged);
        }

        private string RandomString(int size, Random random)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }


        private void AllocateNodes()
        {

            Random r = new Random(Environment.TickCount);
            grid.VirtualChildren.Clear();
            int count = 100000;
            int total = count;
            Graph.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            double width = Graph.DesiredSize.Width;
            Graph.ContentCanvas.Width = width;
            double xborder = 10;
            double yborder = 2;
            double prevX = xborder;
            double prevY = yborder;
            while (count > 0)
            {
                TextBlock t = new TextBlock();
                string text = RandomString(r.Next()%20,r);
                t.Text = text;
                t.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                Size a = t.DesiredSize;
                t = null;

                double current = (double)count / (double)total;
                if (prevX + a.Width + xborder > width)
                {
                    prevX = xborder;
                    prevY += a.Height + yborder;
                }
                Point pos = new Point(prevX, prevY);
                prevX += a.Width + xborder;
                Size s = new Size(a.Width + 2*xborder, a.Height+2*yborder);
                TestShape shape = new TestShape(new Rect(pos, s), text);
                grid.AddVirtualChild(shape);
                count--;
            }
            
        }

        private void mainSVI_CenterChanged(object sender, EventArgs e)
        {
            Point center = MainSVI.ActualCenter;
            if (center.X == 960 && center.Y == 540) return;
            double delta = 540- center.Y;
            Graph.SetVerticalOffset(Graph.VerticalOffset + delta);
            double a = 0;
            MainSVI.Center = new Point(960, 540);
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
                t.Text = _text;
                t.Foreground = Brushes.Gray;
                st.Children.Add(da);
                Storyboard.SetTarget(da, t);
                Storyboard.SetTargetProperty(da, new PropertyPath(TextBlock.OpacityProperty));
                st.Begin();
                _visual = t;
            }
            return _visual;
        }

        public void DisposeVisual()
        {
            st.Stop();
            st.Children.Clear();
            _visual = null;
        }

        public Rect Bounds
        {
            get { return _bounds; }
        }

    }

}
