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
using System.ComponentModel;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Threading;

namespace Knowledge_Web
{
    public class TimerInfo
    {
        public Point downPoint;
        public int ticks;
    }

    public class SingleWeb
    {
        LADSArtworkMode.ArtworkModeWindow artwork;
        String filePath;

        public WebGroup webGroup;
        KnowledgeWeb kw;
        Grid mainGrid = new Grid();
        List<groupArtwork> groupList = new List<groupArtwork>();
        List<int> connectionList = new List<int>();
        public ScatterViewItem mainArtwork = new ScatterViewItem();
        ScatterView parentScatter;
        Image mainIm;

        bool sizeChanged = false;

        public double centerX;
        public double centerY;

        Rectangle mainRect = new Rectangle();

        int groupNo = 0;
        int vertIndex = 0;

        public double height;
        public double width;

        double startHeight;
        double startWidth;

        public String file;

        public struct timerAndRect
        {
            public DispatcherTimer timer;
            public Rectangle rect;
            public int ind;
        }

        public struct groupArtwork
        {
            public WebStack st;
            public ScatterViewItem sv;
            public double angle;
            public Grid groupGrid;
            public Image im;
        }

        public struct counterAndRect
        {
            public Rectangle rect;
            public int count;
            public int ind;
        }

        public SingleWeb(ScatterView parent, double xStart, double yStart, int index, double sides, List<Image> ims, Image main, int group, Canvas lineCanvas, String fileName, KnowledgeWeb web, string path, WebGroup g, LADSArtworkMode.ArtworkModeWindow art)
        {
            artwork = art;
            webGroup = g;

            filePath = path;

            kw = web;

            file = fileName;

            centerX = xStart;
            centerY = yStart;

            groupNo = group;
            vertIndex = index;

            int counter = 0;


            mainArtwork.Content = mainGrid;

            mainArtwork.Center = new Point(xStart, yStart);
            mainArtwork.MinHeight = 1;
            mainArtwork.MinWidth = 1;
            mainArtwork.SizeChanged += new SizeChangedEventHandler(mainArtwork_SizeChanged);
            mainArtwork.PreviewTouchDown += new EventHandler<TouchEventArgs>(mainArtwork_PreviewTouchDown);
            mainArtwork.PreviewTouchUp += new EventHandler<TouchEventArgs>(mainArtwork_PreviewTouchUp);
            mainArtwork.PreviewTouchMove += new EventHandler<TouchEventArgs>(mainArtwork_PreviewTouchMove);
            double xDist = sides * 0.4;
            double yDist = sides * 0.4;

            mainArtwork.Width = xDist;
            mainArtwork.Height = yDist;

            kw.sviList.Add(mainArtwork);

            main.Width = mainArtwork.Width - 10;
            main.Height = mainArtwork.Height - 10;
            mainIm = main;

            WebStack.sviContent content = new WebStack.sviContent();
            content.g = mainGrid;
            content.im = main;
            content.r = mainRect;

            content.r.Width = main.ActualWidth + 10;
            content.r.Height = main.ActualHeight + 10;
            content.r.Visibility = Visibility.Collapsed;
            content.r.Fill = Brushes.Green;

            content.g.Children.Add(content.r);
            content.g.Children.Add(content.im);

            content.used = false;

            mainArtwork.Tag = content;

            startHeight = mainArtwork.Height;
            startWidth = mainArtwork.Width;

            mainArtwork.CanRotate = false;
            mainArtwork.Orientation = 0;

            DependencyPropertyDescriptor dpd1 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd1.AddValueChanged(mainArtwork, ScatterviewMainChanged);

            parent.Items.Add(mainArtwork);

            String[] enumerate = { "A", "B", "C", "D" };

            for(int j = 0; j < group; j++)
            {
                Image i = ims[j];
                groupArtwork current = new groupArtwork();
                current.sv = new ScatterViewItem();
                current.sv.MinHeight = 1;
                current.sv.MinWidth = 1;
                current.groupGrid = new Grid();

                current.sv.Width = sides * 0.05;
                current.sv.Height = sides * 0.05;
                current.angle = 225 + counter * 90;
                current.sv.Center = new Point(xStart + Math.Cos((current.angle/180.0)*Math.PI) * xDist, yStart + Math.Sin((current.angle/180.0)*Math.PI)*yDist);
                current.sv.CanRotate = false;
                current.sv.Orientation = 0;
                //current.sv.SizeChanged += new SizeChangedEventHandler(sv_SizeChanged);
                //current.sv.PreviewTouchDown += new EventHandler<TouchEventArgs>(sv_TouchDown);
                //current.sv.PreviewTouchUp += new EventHandler<TouchEventArgs>(sv_PreviewTouchUp);
                //current.sv.PreviewTouchMove += new EventHandler<TouchEventArgs>(sv_PreviewTouchMove);

                timerAndRect temp2 = new timerAndRect();
                temp2.rect = new Rectangle();
                i.Height = current.sv.Height - 10;
                i.Width = current.sv.Width - 10;
                temp2.ind = counter+1;

                temp2.rect.Height = i.ActualHeight + 10;
                temp2.rect.Width = i.ActualWidth + 10;

                current.sv.Tag = temp2;

                current.sv.Content = current.groupGrid;
                current.groupGrid.Children.Add(temp2.rect);
                current.groupGrid.Children.Add(i);
                current.im = i;

                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
                //dpd.AddValueChanged(current.sv, ScatterViewCenterChanged);

                current.st = new WebStack(parent, current.sv.Center, webGroup, enumerate[j],kw,artwork,j);

                //parent.Items.Add(current.sv);

                groupList.Add(current);
                counter++;
            }

            height = sides;
            width = sides;
            parentScatter = parent;
        }

      

        /**
         * START OF SV TOUCH METHODS
         **/
        void sv_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            timerAndRect current = (timerAndRect)currentScatter.Tag;
            if (current.timer != null)
            {
                current.timer.Stop();
                counterAndRect temp = (counterAndRect)current.timer.Tag;

                current.timer = null;
                currentScatter.Tag = current;
            }
        }

        void sv_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            timerAndRect current = (timerAndRect)currentScatter.Tag;
            if (current.timer != null)
            {
                current.timer.Stop();
                counterAndRect temp = (counterAndRect)current.timer.Tag;
                if (temp.count < 1000)
                {
                    kw.SelectArtwork(filePath);
                    /*if (current.rect.Visibility == Visibility.Collapsed)
                        current.rect.Visibility = Visibility.Visible;
                    else
                        current.rect.Visibility = Visibility.Collapsed;
                    current.rect.Fill = Brushes.Red;

                    Console.WriteLine("goto artwork mode");
                    */
                }
                current.timer = null;
                currentScatter.Tag = current;
            }
        }

        void sv_TouchDown(object sender, TouchEventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            timerAndRect current = (timerAndRect)currentScatter.Tag;

            if ((DispatcherTimer)current.timer == null)
            {
                current.timer = new DispatcherTimer();
                current.timer.Tick += new EventHandler(timer_Tick_Group);
                current.timer.Start();
                counterAndRect temp = new counterAndRect();
                temp.count = 0;
                temp.rect = current.rect;
                temp.ind = current.ind;
                current.timer.Tag = temp;
                currentScatter.Tag = current;
            }
        }

        void sv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            timerAndRect current = (timerAndRect)currentScatter.Tag;

            DispatcherTimer currentTimer = current.timer;
            if (currentTimer != null)
            {
                currentTimer.Stop();
                currentScatter.Tag = current;
            }

            groupArtwork currentGrp = groupList[current.ind - 1];

            currentGrp.groupGrid.Height = currentScatter.Height;
            currentGrp.groupGrid.Width = currentScatter.Width;

            currentGrp.im.Height = currentScatter.Height - 10;
            currentGrp.im.Width = currentScatter.Width - 10;

            current.rect.Width = currentGrp.im.ActualWidth + 10;
            current.rect.Height = currentGrp.im.ActualHeight + 10;
        }
        
        void timer_Tick_Group(object sender, EventArgs e)
        {
            DispatcherTimer currentTimer = sender as DispatcherTimer;
            counterAndRect current = (counterAndRect)currentTimer.Tag;
            groupArtwork currentGrp = groupList[current.ind - 1];
            if (current.count == 10000)
            {
                kw.addKeyWords(webGroup);
                /*current.rect.Height = currentGrp.im.ActualHeight + 10;
                current.rect.Width = currentGrp.im.ActualWidth + 10;
                if (current.rect.Visibility == Visibility.Visible)
                    current.rect.Visibility = Visibility.Collapsed;
                else
                    current.rect.Visibility = Visibility.Visible;
                current.rect.Fill = Brushes.Green;*/
            }
            current.count++;
            currentTimer.Tag = current;
        }

        /**
        * END OF SV TOUCH METHODS
        **/

        /**
        * START OF MAIN TOUCH METHODS
        **/

        void mainArtwork_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            kw.toggleStackAnim(null);
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            WebStack.sviContent content = (WebStack.sviContent)currentScatter.Tag;
            DispatcherTimer currentTimer = content.time;
            if (content.time != null)
            {
                content.time.Stop();
                TimerInfo ti = (TimerInfo)(content.time.Tag);
                content.time = null;
                currentTimer.Tag = null;
                currentScatter.Tag = content;
                if (ti.ticks < 1000)
                {
                    kw.SelectArtwork(filePath);
                    /*if (mainRect.Visibility == Visibility.Collapsed)
                        mainRect.Visibility = Visibility.Visible;
                    else
                        mainRect.Visibility = Visibility.Collapsed;
                    mainRect.Fill = Brushes.Red;

                    Console.WriteLine("goto artwork mode");*/
                }

            }
        }

        void mainArtwork_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            kw.toggleStackAnim(null);
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            WebStack.sviContent content = (WebStack.sviContent)currentScatter.Tag;
            if (content.time != null)
            {
                Point tp = e.GetTouchPoint(parentScatter).Position;
                Point ot = ((TimerInfo)content.time.Tag).downPoint;
                if (((tp.X - ot.X) * (tp.X - ot.X)) + ((tp.Y - ot.Y) * (tp.Y - ot.Y)) > 64)
                {
                    content.time.Stop();
                    content.time = null;
                    currentScatter.Tag = content;
                }
            }
        }
       

        void mainArtwork_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (kw.curveTimer != null)
            {
                kw.curveTimer.Stop();
                kw.count = 0;
                kw.curveTimer = null;
            }
            kw.toggleStackAnim(null);
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            WebStack.sviContent content = (WebStack.sviContent)currentScatter.Tag;
            if (content.time == null && !kw.selectionCurve)
            {
                content.time = new DispatcherTimer();
                TimerInfo ti = new TimerInfo();
                ti.downPoint = e.GetTouchPoint(parentScatter).Position;
                ti.ticks = 0;
                content.time.Tick += new EventHandler(timer_Tick);
                content.time.Tag = ti;
                content.time.Start();
                currentScatter.Tag = content;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer currentTimer = sender as DispatcherTimer;
            TimerInfo ti = (TimerInfo)currentTimer.Tag;
            if (ti.ticks == 10000)
            {
                kw.addKeyWords(webGroup);
                /*mainRect.Height = mainIm.ActualHeight + 10;
                mainRect.Width = mainIm.ActualWidth+ 10;
                if (mainRect.Visibility == Visibility.Visible)
                    mainRect.Visibility = Visibility.Collapsed;
                else
                    mainRect.Visibility = Visibility.Visible;
                mainRect.Fill = Brushes.Green;*/
            }

            ti.ticks++;
        }


        void mainArtwork_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;

            centerX = currentScatter.Center.X;
            centerY = currentScatter.Center.Y;
            WebStack.sviContent content = (WebStack.sviContent)currentScatter.Tag;
            DispatcherTimer currentTimer = content.time;
            if (currentTimer != null)
            {
                currentTimer.Stop();
                content.time = null;
                currentScatter.Tag = content;
            }

            double scale = currentScatter.Width / startWidth;
            startWidth =  currentScatter.Width;
            sizeChanged = true;

            mainGrid.Height = currentScatter.Height;
            mainGrid.Width = currentScatter.Width;
            mainIm.Height = currentScatter.Height - 10;
            mainIm.Width = currentScatter.Width - 10;
            mainRect.Height = mainIm.ActualHeight + 10;
            mainRect.Width = mainIm.ActualWidth + 10;

            mainChanged(currentScatter.Width / 0.4, scale);
            //throw new NotImplementedException();
        }


        private void ScatterviewMainChanged(Object sender, EventArgs e)
        {
            if (sizeChanged)
            {
                sizeChanged = false;
                return;
            }

            ScatterViewItem currentScatter = sender as ScatterViewItem;
            double xDir = currentScatter.Center.X - centerX;
            double yDir = currentScatter.Center.Y - centerY;
            
            centerX = currentScatter.Center.X;
            centerY = currentScatter.Center.Y;

            for(int i = 0; i < groupList.Count; i++)
            {
                groupList[i].sv.Center = new Point(groupList[i].sv.Center.X + xDir, groupList[i].sv.Center.Y + yDir);

                groupList[i].st.Reposition(new Point(groupList[i].st._center.X + xDir, groupList[i].st._center.Y + yDir), 1);
            }
        }
        
        private void ScatterViewCenterChanged(Object sender, EventArgs e)
        {
            ScatterViewItem currentScatter = sender as ScatterViewItem;
            
            double x = currentScatter.Center.X;
            double y = currentScatter.Center.Y;

            if (x > mainArtwork.Center.X + width/2)
                x = mainArtwork.Center.X + width/2;
            else if (x < mainArtwork.Center.X - width/2)
                x = mainArtwork.Center.X - width/2;

            if (y > mainArtwork.Center.Y + height / 2)
                y = mainArtwork.Center.Y + height / 2;
            else if (y < mainArtwork.Center.Y - height / 2)
                y = mainArtwork.Center.Y - height / 2;

            currentScatter.Center = new Point(x, y);

            groupArtwork current = groupList[ ((timerAndRect)currentScatter.Tag).ind-1];
            current.angle = Math.Atan2((currentScatter.Center.Y - mainArtwork.Center.Y), (currentScatter.Center.X - mainArtwork.Center.X));
            current.angle = (current.angle / Math.PI) * 180;
            groupList[((timerAndRect)currentScatter.Tag).ind - 1] = current;
        }

        public void mainChanged(double sides, double scale)
        {
            double xDist = sides * 0.4;
            double yDist = sides * 0.4;

            for (int j = 0; j < groupNo; j++)
            {
                groupList[j].sv.Width *= scale;
                groupList[j].sv.Height *= scale;
                if (j == 0)
                {
                    groupList[j].st.Reposition(new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist)), scale);
                    groupList[j].sv.Center = new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist));
                }
                else if (j == 1)
                {
                    groupList[j].st.Reposition(new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.7), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist *0.8)), scale);
                    groupList[j].sv.Center = new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.7), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist *0.8));
                }
                else if (j == 2)
                {
                    groupList[j].st.Reposition(new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.5), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist * 0.4)), scale);
                    groupList[j].sv.Center = new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.5), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist * 0.4));
                }
                else if (j == 3)
                {
                    groupList[j].st.Reposition(new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.8), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist * 0.8)), scale);
                    groupList[j].sv.Center = new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.8), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist * 0.8));
                }
            }

            height = sides;
            width = sides;
        }

        /**
        * END OF MAIN TOUCH METHODS
        **/

        public void setSide(double scaleX, double scaleY)
        {
            double sides = height * scaleX;

            double xDist = sides * 0.4;
            double yDist = sides * 0.4;

            centerX *= scaleX;
            centerY *= scaleY;

            mainArtwork.Center = new Point(centerX, centerY);

            double widthChange = xDist/mainArtwork.Width;
            double heightChange = yDist / mainArtwork.Height;

            mainArtwork.Width = xDist;
            mainArtwork.Height = yDist;

            for (int j = 0; j < groupNo; j++)
            {
                groupList[j].sv.Width *= widthChange;
                groupList[j].sv.Height *= widthChange;

                if (j == 0)
                {
                    groupList[j].st.Reposition(new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist)), widthChange);
                    groupList[j].sv.Center = new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist));
                }
                else if (j == 1)
                {
                    groupList[j].st.Reposition(new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.7), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist *0.8)), widthChange);
                    groupList[j].sv.Center = new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.7), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist *0.8));
                }
                else if (j == 2)
                {
                    groupList[j].st.Reposition(new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.5), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist * 0.4)), widthChange);
                    groupList[j].sv.Center = new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.5), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist * 0.4));
                }
                else if (j == 3)
                {
                    groupList[j].st.Reposition(new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.8), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist * 0.8)), widthChange);
                    groupList[j].sv.Center = new Point(mainArtwork.Center.X + Math.Cos((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Width/4 + */xDist * 0.8), mainArtwork.Center.Y + Math.Sin((groupList[j].angle / 180.0) * Math.PI) * (/*groupList[j].sv.Height/4 +*/ yDist * 0.8));
                }
            }

            height = sides;
            width = sides;
        }
    }
}
