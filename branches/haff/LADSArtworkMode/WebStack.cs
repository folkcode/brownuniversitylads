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
using System.IO;
using System.Windows.Threading;
using System.Windows.Automation.Peers;

namespace Knowledge_Web
{
    public class WebStack
    {
        private bool animationOn = false;
        private int groupNo = 0;
        private KnowledgeWeb kw;
        private LADSArtworkMode.ArtworkModeWindow artwork;

        private const int INITIAL_THUMB_HEIGHT = 100;
        private const double OVERLAP_HEIGHT = 0.1;
        private const double OVERLAP_WIDTH = 0.1;
        private double maxDist = INITIAL_THUMB_HEIGHT;
        public Point _center;

        private ScatterView _sv;
        List<BitmapImage> _images;

        bool animated = false;

        List<ScatterViewItem> _svis = new List<ScatterViewItem>();

        List<Point> _pointList = new List<Point>();

        public IEnumerable<ScatterViewItem> ScatterViewItems()
        {
            foreach (ScatterViewItem svi in _svis)
                yield return svi;
        }

        public struct sviContent
        {
            public DispatcherTimer time;
            public Grid g;
            public Rectangle r;
            public Image im;
            public bool used;
        }

        public struct counterAndCenter
        {
            public bool used;
            public TouchDevice t;
            public int counter;
            public ImageSource s;
            public ScatterViewItem svi;
        }

        public struct ScatterandCenter
        {
            public ScatterViewItem sv;
            public Point pt;
        }

        public WebStack(ScatterView sv, Point origin, WebGroup gr, String group, KnowledgeWeb knowledge, LADSArtworkMode.ArtworkModeWindow art, int g)
        {
            groupNo = g;
            artwork = art;
            kw = knowledge;
            _sv = sv;
            _images = gr.getGroupBitmap(group);
            if (_images == null)
                return;

            foreach (BitmapImage i in _images)
            {
                ScatterViewItem svi = new ScatterViewItem();
                svi.MinHeight = 1;
                svi.MinWidth = 1;
                Image img = new Image();
                img.Source = i;

                sviContent content = new sviContent();
                content.g = new Grid();
                content.g.Height = INITIAL_THUMB_HEIGHT;
                content.g.Width = INITIAL_THUMB_HEIGHT;

                content.im = img;

                content.r = new Rectangle();
                content.r.Height = INITIAL_THUMB_HEIGHT;
                content.r.Width = INITIAL_THUMB_HEIGHT;
                content.r.Visibility = Visibility.Collapsed;

                content.g.Children.Add(content.r);
                content.g.Children.Add(content.im);

                content.used = false;

                svi.Content = content.g;
                svi.Tag = content;

                svi.Height = INITIAL_THUMB_HEIGHT;
                svi.Width = INITIAL_THUMB_HEIGHT;
                svi.Orientation = 0;
                svi.CanRotate = false;
                svi.CanMove = false;
                svi.CanScale = false;
                svi.PreviewTouchUp += new EventHandler<System.Windows.Input.TouchEventArgs>(svi_PreviewTouchUp);
                svi.PreviewTouchDown += new EventHandler<TouchEventArgs>(svi_PreviewTouchDown);
                svi.SizeChanged += new SizeChangedEventHandler(svi_SizeChanged);

                knowledge.sviList.Add(svi);

                _svis.Add(svi);
                _sv.Items.Add(svi);
            }

            PositionScatterViewItems(origin);
        }

        void svi_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            sviContent content = (sviContent)currentScatter.Tag;

            content.g.Height = currentScatter.Height;
            content.g.Width = currentScatter.Width;

            content.im.Width = currentScatter.Width - 10;
            content.im.Height = currentScatter.Height - 10;

            content.r.Width = content.im.ActualWidth + 5;
            content.r.Height = content.im.ActualHeight + 5; ;
            content.r.Fill = Brushes.Green;
        }

        void svi_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            sviContent content = (sviContent)currentScatter.Tag;

            if (kw.curveTimer != null)
            {
                kw.curveTimer.Stop();
                kw.curveTimer = null;
                kw.count = 0;
            }

            if (content.time == null)
            {
                counterAndCenter temp1 = new counterAndCenter();
                temp1.t = e.TouchDevice;
                temp1.counter = 0;
                temp1.s = content.im.Source;
                temp1.svi = currentScatter;

                content.time = new DispatcherTimer();
                content.time.Tick += new EventHandler(temp_Tick);
                content.time.Tag = temp1;
                content.time.Start();

                currentScatter.Tag = content;
            }
        }

        void temp_Tick(object sender, EventArgs e)
        {
            DispatcherTimer currentTimer = sender as DispatcherTimer;
            counterAndCenter current = (counterAndCenter)currentTimer.Tag;
            WebStack.sviContent content = (WebStack.sviContent)current.svi.Tag;

            if (current.counter == 2000 && !content.used)
            {
                //TODO make sure you cant do it more than once! WHAT IS A WORKSPACEELEMENT AND HOW DO I GET IT ???

                //LADSArtworkMode.DockableItem item = new LADSArtworkMode.DockableItem(artwork.MainScatterview, artwork, artwork.Bar, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Metadata\\riso000695_1md.jpg");
                LADSArtworkMode.DockableItem item = new LADSArtworkMode.DockableItem(artwork.MainScatterView, artwork, artwork.Bar, current.s, ref current.svi, kw);
                item.Center = current.t.GetCenterPosition(artwork.MainScatterView);
                Image temp = new Image();
                temp.Source = current.s;
                item.image.Source = current.s;
                item.Content = temp;
                item.CaptureTouch(current.t);

                currentTimer.Stop();
                sviContent temp2 = ((sviContent)(current.svi.Tag));
                temp2.time = null;
                current.svi.Tag = temp2;
            }
            current.counter++;
            currentTimer.Tag = current;
        }

        void svi_PreviewTouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            sviContent content = (sviContent)currentScatter.Tag;

            if (content.time != null)
            {
                content.time.Stop();
                content.time = null;
            }

            if (!animated)
                spreadOut();
        }

        public void Reposition(Point center, double scale)
        {
            _center = center;
            maxDist = double.MinValue;
            foreach (ScatterViewItem svi in _svis)
            {
                svi.Width *= scale;
                svi.Height *= scale;
                if (svi.Width > maxDist)
                    maxDist = svi.Width;

                if (svi.Height > maxDist)
                    maxDist = svi.Height;
            }

            PositionScatterViewItems(center);
        }

        private void PositionScatterViewItems(Point origin)
        {
            double height = 0;
            double width = 0;

            foreach (ScatterViewItem svi in _svis)
            {
                height += (OVERLAP_HEIGHT * svi.ActualHeight);
                width += (OVERLAP_WIDTH * svi.ActualWidth);
            }

            Point p = new Point(origin.X - (0.5 * width), origin.Y - (0.5 * height));
            foreach (ScatterViewItem svi in _svis)
            {
                svi.Center = new Point(p.X + (0.5 * svi.ActualWidth), p.Y + (0.5 * svi.ActualHeight));
                p = new Point(p.X + (OVERLAP_WIDTH * svi.ActualWidth), p.Y + (OVERLAP_HEIGHT * svi.ActualHeight));
            }

        }

        public void spreadOut()
        {
            if (_svis.Count == 1)
                return;

            if (animationOn)
                return;

            if(!animated)
                kw.toggleStackAnim(this);

            animated = true;
           
            List<ScatterViewItem> sv1 = new List<ScatterViewItem>();
            List<ScatterViewItem> sv2 = new List<ScatterViewItem>();
            Point sv1C = new Point();
            Point sv2C = new Point();

            if (_svis.Count % 2 == 0)
            {
                bool pick = false;
                for (int i = 0; i < _svis.Count; i++)
                {
                    _pointList.Add(_svis[i].Center);
                    if (pick)
                        sv1.Add(_svis[i]);
                    else
                        sv2.Add(_svis[i]);
                    pick = !pick;
                }
            }
            else
            {
                double x = _svis[0].Width;
                double y = _svis[0].Height;

                sv1C.X = Math.Max(x, y);
                sv1C.Y = Math.Max(x, y);
                sv2C.X = Math.Max(x, y);
                sv2C.Y = -Math.Max(x, y);

                _pointList.Add(_svis[0].Center);

                bool pick = false;
                for (int i = 1; i < _svis.Count; i++)
                {
                    _pointList.Add(_svis[i].Center);
                    if (pick)
                        sv1.Add(_svis[i]);
                    else
                        sv2.Add(_svis[i]);
                    pick = !pick;
                }
            }

            spreadEven(sv1, sv2, sv1C, sv2C);

        }

        public void spreadEven(List<ScatterViewItem> sv1, List<ScatterViewItem> sv2, Point startSv1, Point startSv2 )
        {
            Point current = _center;
            double x = startSv1.X;
            double y = startSv1.Y;

            if (y == 0)
            {
                y = Math.Max(sv2[0].Width, sv2[0].Height);
            }

            foreach (ScatterViewItem i in sv1)
            {
                Point dest = new Point(); ;
                if (groupNo == 0)
                    dest = new Point(current.X /*+ x + i.Width/2*/, current.Y + y + i.Height / 2);
                else if (groupNo == 1)
                    dest = new Point(current.X /*+ x + i.Width/2*/, current.Y + y + i.Height / 2);
                else if (groupNo == 2)
                    dest = new Point(current.X /*+ x + i.Width/2*/, current.Y - y - i.Height / 2);
                else
                    dest = new Point(current.X /*+ x + i.Width/2*/, current.Y - y - i.Height / 2);
             
                x = Math.Max(i.Width / 2, i.Height / 2);
                y = Math.Max(i.Width / 2, i.Height / 2);
                current = dest;

                PointAnimation anim1 = new PointAnimation();
                anim1.From = new Point(i.Center.X, i.Center.Y);
                anim1.To = dest;
                anim1.Duration = new Duration(TimeSpan.FromSeconds(.4));
                anim1.Completed += new EventHandler(anim1_Completed);

                i.BeginAnimation(ScatterContentControlBase.CenterProperty, anim1);
                animationOn = true;
            }

            current = _center;
            x = startSv2.X;
            y = startSv2.Y;

            if (x == 0)
            {
                x = Math.Max(sv1[0].Width, sv1[0].Height);
            }

            foreach (ScatterViewItem i in sv2)
            {
                Point dest = new Point(); ;
                if(groupNo == 0)
                    dest = new Point(current.X + x + i.Width / 2, current.Y /*+ y - i.Height / 2*/);
                else if(groupNo == 1)
                    dest = new Point(current.X - x - i.Width / 3, current.Y /*+ y - i.Height / 2*/);
                else if(groupNo == 2)
                    dest = new Point(current.X - x - i.Width / 2, current.Y /*+ y - i.Height / 2*/);
                else
                    dest = new Point(current.X + x + i.Width / 2, current.Y /*+ y - i.Height / 2*/);

                x = Math.Max(i.Width / 2, i.Height / 2);
                y = -Math.Max(i.Width / 2, i.Height / 2);
                current = dest;

                PointAnimation anim1 = new PointAnimation();
                anim1.From = new Point(i.Center.X, i.Center.Y);
                anim1.To = dest;
                anim1.Duration = new Duration(TimeSpan.FromSeconds(.4));
                anim1.Completed += new EventHandler(anim1_Completed);

                i.BeginAnimation(ScatterContentControlBase.CenterProperty, anim1);
                animationOn = true;
            }
        }

        void anim1_Completed(object sender, EventArgs e)
        {
            animationOn = false;
        }
     
        public void compactIn()
        {
            animated = false;
            int count = 0;
            foreach (ScatterViewItem i in _svis)
            {
                PointAnimation anim1 = new PointAnimation();
                anim1.From = new Point(i.Center.X, i.Center.Y);

                anim1.To = _pointList[count];
                anim1.Duration = new Duration(TimeSpan.FromSeconds(.4));
                anim1.FillBehavior = FillBehavior.Stop;
                anim1.Completed +=new EventHandler(anim1_Completed);

                i.BeginAnimation(ScatterContentControlBase.CenterProperty, anim1);
                animationOn = true;
                count++;
            }
            _pointList.Clear();
        }
    }
}
