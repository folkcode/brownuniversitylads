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
        private List<NameInfo> _names;
        private Dictionary<int, List<NameInfo>> _blockToNames;
        private static double MainSVI_CENTER_X;
        private static double MainSVI_CENTER_Y; 

        public NamesWall()
        {
            InitializeComponent();
            this.InitNames();
            
            Canvas target = Wall.ContentCanvas;
            Wall.ContentCanvas.Background = Brushes.Black;
            Wall.SmallScrollIncrement = new Size(10, 30);

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(MainSVI, mainSVI_CenterChanged);

            MainSVI.ApplyTemplate();
            Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
            ssc = MainSVI.Template.FindName("shadow", MainSVI) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;
        }

        private SurfaceWindow1 _mainWindow;
        public SurfaceWindow1 MainWindow { set { _mainWindow = value; } }

        public void StartAll()
        {
            AllocateNodes();
        }

        private void InitNames()
        {
            XmlNodeList xmlNodes = Helpers.LoadNamesFromXML();
            _names = new List<NameInfo>();
            _blockToNames = new Dictionary<int, List<NameInfo>>();
            foreach (XmlNode node in xmlNodes)
            {
                NameInfo nameInfo = new NameInfo();

                //parse block number
                string blockString = this.getNodeAttribute(node, "Block");
                try
                {
                    nameInfo.Block = Convert.ToInt32(blockString);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Block number not a number, but " + blockString);
                    Console.WriteLine(e.Message);
                    nameInfo.Block = 0;
                }
                //put into block map
                if (!_blockToNames.ContainsKey(nameInfo.Block))
                {
                    _blockToNames.Add(nameInfo.Block, new List<NameInfo>());
                }
                _blockToNames[nameInfo.Block].Add(nameInfo);

                //parse the string values
                nameInfo.OtherCities = this.getNodeAttribute(node, "OtherCities");
                nameInfo.City = this.getNodeAttribute(node, "City");
                nameInfo.State = this.getNodeAttribute(node, "State");
                nameInfo.Country = this.getNodeAttribute(node, "Country");
                nameInfo.ParseCityFull();
                nameInfo.DateReceived = this.getNodeAttribute(node, "DateReceived");
                nameInfo.Dob = this.getNodeAttribute(node, "dob");
                nameInfo.Dod = this.getNodeAttribute(node, "dod");
                nameInfo.MakerFirst = this.getNodeAttribute(node, "MakerFirst");
                nameInfo.MakerLast = this.getNodeAttribute(node, "MakerLast");
                nameInfo.PanelFirst = this.getNodeAttribute(node, "PanelFirst");
                nameInfo.PanelLast = this.getNodeAttribute(node, "PanelLast");
                nameInfo.ParsePanelName();

                nameInfo.PanelNumber = this.getNodeAttribute(node, "PanelNumber");
                //parse panel number: it's more than block number - number; so annoying!! 
                /*string panelNumberString = this.getNodeAttribute(node, "PanelNumber");
                string[] nums = panelNumberString.Split('-');
                if (nums.Length >= 2)
                {
                    panelNumberString = nums[1];
                    try
                    {
                        nameInfo.PanelNumber = Convert.ToInt16(panelNumberString);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Panel number not a number, but " + panelNumberString);
                        Console.WriteLine(e.Message);
                        nameInfo.PanelNumber = 0;
                    }
                }
                else
                {
                    Console.WriteLine("something is wrong, can't get the panel number out of " + panelNumberString);
                }*/
                
                _names.Add(nameInfo);
            }

            _names.Sort(delegate (NameInfo n1, NameInfo n2)
            {
                return n1.PanelName.CompareTo(n2.PanelName);
            });
        }

        private string getNodeAttribute(XmlNode node, string attribute)
        {
            return node.Attributes.GetNamedItem(attribute).InnerText;
        }

        private void AllocateNodes()
        {
            Wall.VirtualChildren.Clear();
            Wall.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            double width = Wall.Width-20;
            Wall.ContentCanvas.Margin = new Thickness(10);
            Wall.ContentCanvas.Width = width;
            double xborder = 35;
            double yborder = 8;
            double prevX = xborder;
            double prevY = yborder;

            foreach (NameInfo name in _names)
            {
                TextBlock t = new TextBlock();
                t.FontSize = 20;
                t.Text = name.PanelName;
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
            if (center.X == MainSVI_CENTER_X && center.Y == MainSVI_CENTER_Y) return;
            double delta = MainSVI_CENTER_Y - center.Y;
            Wall.SetVerticalOffset(Wall.VerticalOffset + delta);
            MainSVI.Center = new Point(MainSVI_CENTER_X, MainSVI_CENTER_Y);
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

        private TestShape _prevSelected;
        private void TestShapeUp(TestShape shape)
        {
            if (_prevSelected != null)
            {
                _prevSelected.Fill = new SolidColorBrush(Colors.Black);
            }
            shape.Fill = new SolidColorBrush(Colors.Yellow);
            _prevSelected = shape;
            this.NameSelected(shape.Name);
        }

        private void NameSelected(NameInfo name)
        {
            Helpers.ChangeImageSource(_mainWindow.CurrImage, "/data/Images/Thumbnail/" + name.Block.ToString("D5") + "_512.jpg");
            _mainWindow.name.Text = name.PanelName;
            _mainWindow.dob.Text = "DOB: " + name.Dob;
            //_mainWindow.dob.Visibility = (name.Dob.Equals("")) ? Visibility.Hidden : Visibility.Visible;
            _mainWindow.dod.Text = "DOD: " + name.Dod;
            //_mainWindow.dod.Visibility = (name.Dod.Equals("")) ? Visibility.Hidden : Visibility.Visible;
            _mainWindow.city.Text = "City: " + name.CityFull;
            //_mainWindow.city.Visibility = (name.CityFull.Equals("")) ? Visibility.Hidden : Visibility.Visible;
            _mainWindow.othercities.Text = "Other Cities: " + name.OtherCities;
            //_mainWindow.othercities.Visibility = (name.OtherCities.Equals("")) ? Visibility.Hidden : Visibility.Visible;

            foreach (NameInfo otherName in _blockToNames[name.Block])
            {
                //TODO: _mainWindow.RelatedNames
            }
        }

        public void Window_SizeChanged(Size newSize)
        {
            this.Width = newSize.Width * 2 / 3;
            MainSVI_CENTER_X = this.Width / 2;
            MainSVI_CENTER_Y = newSize.Height / 2;
            MainSVI.Center = new Point(MainSVI_CENTER_X, MainSVI_CENTER_Y);
        }
    }

    class TestShape : IVirtualChild
    {
        private NameInfo _nameInfo;
        public NameInfo Name { get { return _nameInfo; } }

        Rect _bounds;
        public Brush Fill { get { return (_visual == null) ? null : _visual.Background; } set { if (_visual != null) { _visual.Background = value; } } }
        public Brush Stroke { get; set; }
        public string Label { get; set; }
        TextBlock _visual;
        string _text;
        public event EventHandler BoundsChanged;
        Storyboard st = new Storyboard();
        DoubleAnimation da = new DoubleAnimation();

        public TestShape(Rect bounds, NameInfo name)
        {
            _nameInfo = name;
            _text = name.PanelName;
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
                _visual = new TextBlock();
                _visual.FontSize = 20;
                _visual.Text = _text;
                _visual.Foreground = Brushes.Gray;
                _visual.PreviewTouchUp += new EventHandler<TouchEventArgs>(t_TouchUp);
                _visual.PreviewMouseUp += new MouseButtonEventHandler(t_PreviewMouseUp);
                //st.Children.Add(da);
                //Storyboard.SetTarget(da, t);
                //Storyboard.SetTargetProperty(da, new PropertyPath(TextBlock.OpacityProperty));
                //st.Begin();
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
