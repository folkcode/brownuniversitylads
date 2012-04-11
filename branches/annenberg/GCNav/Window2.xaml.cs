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
            grid.ContentCanvas.Background = Brushes.White;

            AllocateNodes();
        }

        private void AllocateNodes()
        {

            Random r = new Random(Environment.TickCount);
            grid.VirtualChildren.Clear();
            int count = 100000;
            int total = count;
            double width = 1800;
            double prevEnd = 0;
            double prevY = 0;
            while (count > 0)
            {
                TextBlock t = new TextBlock();
                string text = (r.Next() * 10000).ToString();
                t.Text = text;
                t.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                Size a = t.DesiredSize;
                t = null;

                double current = (double)count / (double)total;
                if (prevEnd + a.Width + 10 > width)
                {
                    prevEnd = 0;
                    prevY += a.Height + 10;
                }
                Point pos = new Point(prevEnd, prevY);
                prevEnd+=a.Width+10;
                Size s = new Size(a.Width,a.Height);
                TestShape shape = new TestShape(new Rect(pos, s), text);
                grid.AddVirtualChild(shape);
                count--;
            }
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

        public TestShape(Rect bounds, string text)
        {
            _text = text;
            _bounds = bounds;
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
                _visual = t;
            }
            return _visual;
        }

        public void DisposeVisual()
        {
            _visual = null;
        }

        public Rect Bounds
        {
            get { return _bounds; }
        }

    }

}
