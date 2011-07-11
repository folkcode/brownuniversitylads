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
using DexterLib;
//using JockerSoft.Media;
//using Microsoft.Expression.Encoder.EncoderObject;
//using Microsoft.Expression.Encoder;

namespace LADSArtworkMode
{
    public class DockableItem : ScatterViewItem
    {
        public bool isDocked;
        ScatterView mainScatterView;
        ArtworkModeWindow win;
        MouseEventHandler mousem;
        SurfaceListBox bar;
        double barX;
        public double oldHeight;
        public double oldWidth;
        public Image image;
        WorkspaceElement wke;
        int wke_index;
        Image dockImage;
        private Point rootPoint;
        private double barImageWidth;
        private double barImageHeight;
        private double actualWKEWidth;
        private Point pt;
        private ScatterViewItem dockedItem = null;
        private bool knowledgeWebUse = false;
        Boolean touchDown;
        Boolean isAnimating;
        AssociatedDocListBoxItem aldbi;
        private List<ScatterViewItem> svLists;
        bool knowledgeStack = false;
        Knowledge_Web.KnowledgeWeb kw;
        private String imageURIPath;
        private LADSVideoBubble vidBub;
        private Helpers _helpers;

        /// <summary>
        /// used by knowledge web
        /// </summary>
        public DockableItem(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar, ImageSource img, List<ScatterViewItem> svList, KnowledgeWeb web)
        {
            kw = web;
            svLists = svList;
            knowledgeWebUse = true;
            knowledgeStack = true;

            aldbi = null;
            image = new Image();
            image.Source = img;

            this.isAnimating = false;
            this.AddChild(image);
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;
            this.MinHeight = 80;
            this.MinWidth = 80;

            isDocked = false;
            touchDown = false;
            //this.CanScale = true;


            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, RemoveListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);

            mainScatterView.Items.Add(this);
            this.SetCurrentValue(HeightProperty, image.Height);
            this.SetCurrentValue(WidthProperty, image.Width);

            this.PreviewTouchDown += new EventHandler<TouchEventArgs>(barversTouchDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(barversTouchDown);
            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();
            this.Opacity = 0.5;
            _helpers = new Helpers();
        }

        /// <summary>
        /// used by knowledge web 
        /// </summary>
        public DockableItem(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar, ImageSource img, ref ScatterViewItem saveScatter, KnowledgeWeb web)
        {
            kw = web;
            knowledgeWebUse = true;
            dockedItem = saveScatter;

            aldbi = null;
            image = new Image();
            image.Source = img;

            this.isAnimating = false;
            this.AddChild(image);
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;
            this.MinHeight = 80;
            this.MinWidth = 80;

            isDocked = false;
            touchDown = false;
            //this.CanScale = true;


            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, RemoveListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);

            mainScatterView.Items.Add(this);
            this.SetCurrentValue(HeightProperty, image.Height);
            this.SetCurrentValue(WidthProperty, image.Width);

            this.PreviewTouchDown += new EventHandler<TouchEventArgs>(barversTouchDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(barversTouchDown);
            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();
            this.Opacity = 0.5;
            _helpers = new Helpers();
        }

        /// <summary>
        /// Dockable item that doesn't preload an image, used by the knowledge web
        /// </summary>
        public DockableItem(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar, ImageSource img)
        {
            aldbi = null;
            image = new Image();
            image.Source = img;
            _helpers = new Helpers();

            this.isAnimating = false;
            this.AddChild(image);
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;
            this.MinHeight = 80;
            this.MinWidth = 80;
            isDocked = false;
            touchDown = false;
            //this.CanScale = true;


            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, RemoveListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);

            mainScatterView.Items.Add(this);
            this.SetCurrentValue(HeightProperty, image.Height);
            this.SetCurrentValue(WidthProperty, image.Width);

            this.PreviewTouchDown += new EventHandler<TouchEventArgs>(barversTouchDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(barversTouchDown);
            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();
            _helpers = new Helpers();
        }

        /// <summary>
        /// used by artwork mode, including the tour authoring & playback system
        /// </summary>
        public DockableItem(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar, String imageURIPathParam)
        {
            Console.WriteLine("Constructor 1");
            image = new Image();
            aldbi = null;
            _helpers = new Helpers();
            /*BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(imageURIPathParam, UriKind.Absolute);
            bi3.EndInit();
            image.Source = bi3;*/

            FileStream stream = new FileStream(imageURIPathParam, FileMode.Open);
            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            image.Source = wpfImage.Source;

            this.isAnimating = false;
            this.Background = Brushes.LightGray;
            this.AddChild(image);
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;
            this.MinHeight = 80;
            this.MinWidth = 80;
            isDocked = false;
            touchDown = false;
            //this.CanScale = true;
            //aldbi = _aldbi;

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, RemoveListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);

            mainScatterView.Items.Add(this);
            this.SetCurrentValue(HeightProperty, image.Height);
            this.SetCurrentValue(WidthProperty, image.Width);



            this.PreviewTouchDown += new EventHandler<TouchEventArgs>(barversTouchDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(barversTouchDown);
            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();

            Random rnd = new Random();

            // remove white background of ScatterViewItem to allow for transparency (added by jcchin on 4/4/2011)
            this.Background = new SolidColorBrush(Colors.Transparent);
            //this.BorderBrush = new SolidColorBrush(Colors.Transparent); // not needed...making background transparent is enough for this
            //this.ShowsActivationEffects = false; // not needed...activation effects are fine for this
            RoutedEventHandler loadedEventHandler = null;
            loadedEventHandler = new RoutedEventHandler(delegate
            {
                this.Loaded -= loadedEventHandler;
                try
                {
                    Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
                    ssc = this.Template.FindName("shadow", this) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
                    ssc.Visibility = Visibility.Hidden;
                }
                catch (Exception exc) { }

            });
            this.Loaded += loadedEventHandler;

            imageURIPath = imageURIPathParam;

            stream.Close();
            
        }

        /// <summary>
        /// used by artwork mode
        /// </summary>
        public DockableItem(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar, String imageURIPathParam, AssociatedDocListBoxItem _aldbi)
        {
            Console.WriteLine("Constructor 2");
            image = new Image();
            aldbi = null;
            _helpers = new Helpers();

            /*BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(imageURIPathParam, UriKind.Absolute);
            bi3.EndInit();
            image.Source = bi3;*/

            FileStream stream = new FileStream(imageURIPathParam, FileMode.Open);
            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            image.Source = wpfImage.Source;
            stream.Close();
            Console.WriteLine(image.Height);
            Console.WriteLine(image.Width);

            this.isAnimating = false;
            this.Background = Brushes.LightGray;
            this.AddChild(image);
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;
            this.MinHeight = 80;
            this.MinWidth = 80;
            isDocked = false;
            touchDown = false;
            //this.CanScale = true;
            aldbi = _aldbi;

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, RemoveListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);

            mainScatterView.Items.Add(this);
            this.SetCurrentValue(HeightProperty, image.Height);
            this.SetCurrentValue(WidthProperty, image.Width);

            this.PreviewTouchDown += new EventHandler<TouchEventArgs>(barversTouchDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(barversTouchDown);

            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();

            Random rnd = new Random();
            Point pt = new Point(rnd.Next((int)(win.ActualWidth * .2 + image.ActualWidth * 3), (int)(win.ActualWidth - image.ActualWidth * 3 - 100)),
                                                          rnd.Next((int)(image.ActualHeight * 3), (int)(win.ActualHeight * .8 - image.ActualHeight * 3)));
            this.SetCurrentValue(CenterProperty, pt);
            Console.WriteLine(pt.X + " " + pt.Y);
            this.Orientation = rnd.Next(-20, 20);

            //Canvas.SetZIndex(this, 95);

            imageURIPath = imageURIPathParam;
            
        }

        //constructor for videos
        public DockableItem(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar, string _targetVid, AssociatedDocListBoxItem _aldbi, LADSVideoBubble _video, VideoItem _vidctrl)
        {
            //_video
            vidBub = _video;
            _helpers = new Helpers();

            image = new Image();
            aldbi = null;

            //might be able to edit this and get rid of generating thumbnail here - only need to generate once (in content authoring)
            //DexterLib.MediaDet md = new MediaDet();
            //md.Filename = _targetVid;
            //md.CurrentStream = 0;
            //string fBitmapName = _targetVid;
            //fBitmapName = fBitmapName.Remove(fBitmapName.Length - 4, 4);
            //fBitmapName += ".bmp";
            //md.WriteBitmapBits(0, 320, 240, fBitmapName);
            
            //BitmapImage bmp = new BitmapImage();
            //bmp.BeginInit();
            //bmp.UriSource = new Uri(fBitmapName, UriKind.RelativeOrAbsolute);
            //bmp.EndInit();
            //image.Source = bmp;
            String thumbFileName = _targetVid;

            int decrement = System.IO.Path.GetExtension(thumbFileName).Length;
            thumbFileName = thumbFileName.Remove(thumbFileName.Length - decrement, decrement);

            //thumbFileName = thumbFileName.Remove(thumbFileName.Length - 4, 4);
            thumbFileName += ".bmp";
            thumbFileName = System.IO.Path.GetFileName(thumbFileName);
            thumbFileName = "C:\\Users\\Public\\Documents\\3rdLADS\\GCNav\\bin\\Debug\\Data\\Videos\\Metadata\\" + thumbFileName;

            FileStream stream = new FileStream(thumbFileName, FileMode.Open);
            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            image.Source = wpfImage.Source;
            stream.Close();
            Console.WriteLine(image.Height);
            Console.WriteLine(image.Width);

            this.isAnimating = false;
            this.Background = Brushes.LightGray;
            this.AddChild(vidBub);
            this.UpdateLayout();
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;

            this.SizeChanged += new SizeChangedEventHandler(ScaleVideo);
            isDocked = false;
            touchDown = false;
            //this.CanScale = true;
            aldbi = _aldbi;

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, RemoveListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);
            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();

            mainScatterView.Items.Add(this);
            //this.SetCurrentValue(HeightProperty, vidBub.Height);
            //this.SetCurrentValue(WidthProperty, vidBub.Width);

            this.PreviewTouchDown += new EventHandler<TouchEventArgs>(barversTouchDown);
            this.PreviewMouseDown += new MouseButtonEventHandler(barversTouchDown);

            Random rnd = new Random();
            Point pt = new Point(rnd.Next((int)(win.ActualWidth * .2 + vidBub.ActualWidth * 3), (int)(win.ActualWidth - vidBub.ActualWidth * 3 - 100)),
                                                           rnd.Next((int)(vidBub.ActualHeight * 3), (int)(win.ActualHeight * .8 - vidBub.ActualHeight * 3)));
            this.SetCurrentValue(CenterProperty, pt);
            Console.WriteLine(pt.X + " " + pt.Y);
            this.Orientation = rnd.Next(-20, 20);

            //Canvas.SetZIndex(this, 95);
            imageURIPath = _targetVid;
            MediaElement vid = new MediaElement();
            vid = vidBub.getVideo();
            vid.MediaOpened += new RoutedEventHandler(video_MediaOpened);
            this.MinHeight = 100;
            Console.WriteLine("ActualWidth " + vidBub.ActualWidth);
            //imageURIPath = imageURIPathParam;

        }

        public void removeDockability()
        {

            this.PreviewTouchUp -= new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp -= new MouseButtonEventHandler(AddtoDock);

            this.PreviewMouseWheel -= new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
        }

        private void video_MediaOpened(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("DOCKABLE EVENT HANDLER");
            vidBub.setPreferredSize(vidBub.getWidth(), vidBub.getHeight());
            this.Height = vidBub.getHeight();
            this.Width = vidBub.getWidth();
        }

        //scales video with pinch zoom
        public void ScaleVideo(object sender, EventArgs e)
        {
            vidBub.Resize(this.ActualWidth, this.ActualHeight, false);
        }


        public String GetImageURIPath
        {
            get { return imageURIPath; }
            set { imageURIPath = value; }
        }

        public void DockableItem_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double newWidth = 0;
            double newHeight = 0;

            newWidth = ((double)e.Delta)/5.0 + this.ActualWidth;
            newHeight = this.ActualHeight * newWidth / this.ActualWidth;

            if (newWidth < 40 || newHeight < 40) return;

            this.Height = newHeight;
            this.Width = newWidth;
            

        }


        public void AddtoDock(object sender, EventArgs e)
        {
            touchDown = false;
            DockableItem item = sender as DockableItem;
            Console.WriteLine("AddToDock Called");
            Helpers helpers = new Helpers();

            //if (isDocked) isDocked = false;
            if (this.Center.Y > (win.ActualHeight * .8) && !isDocked && this.Center.X > win.ActualWidth * .2 && !this.isAnimating)
            {
                if (helpers.IsVideoFile(imageURIPath))
                {
                    vidBub.pauseVideo();
                }
                if (knowledgeWebUse)
                {
                    if (knowledgeStack)
                    {
                        foreach (ScatterViewItem i in svLists)
                        {
                            WebStack.sviContent content = (WebStack.sviContent)i.Tag;
                            content.used = true;
                            content.r.Visibility = Visibility.Visible;
                            for (int j = 0; j < kw.sviList.Count; j++)
                            {
                                WebStack.sviContent content2 = (WebStack.sviContent)kw.sviList[j].Tag;
                                if (content.im.Source == content2.im.Source)
                                {
                                    content2.used = true;
                                    kw.sviList[j].Tag = content2;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        WebStack.sviContent content = (WebStack.sviContent)dockedItem.Tag;

                        for (int j = 0; j < kw.sviList.Count; j++)
                        {
                            WebStack.sviContent content2 = (WebStack.sviContent)kw.sviList[j].Tag;
                            if (content.im.Source == content2.im.Source)
                            {
                                content2.used = true;
                                kw.sviList[j].Tag = content2;
                                break;
                            }
                        }
                        content.r.Visibility = Visibility.Visible;
                        //dockedItem.Tag = content;
                    }
                }

                this.isAnimating = true;
                barImageHeight = bar.ActualHeight * .8;
                barImageWidth = bar.ActualHeight * this.Width / this.Height;

                if (knowledgeStack)
                {
                    Canvas scatterCanvas = new Canvas();
                    scatterCanvas.Height = 100;
                    scatterCanvas.Width = 100;
                    double offsetCount = 0;

                    foreach (ScatterViewItem i in svLists)
                    {
                        WebStack.sviContent content = (WebStack.sviContent)i.Tag;
                        Image temp = new Image();
                        temp.Source = content.im.Source;
                        temp.Height = 100;
                        temp.Width = 100;

                        scatterCanvas.Children.Add(temp);
                        Canvas.SetLeft(temp, offsetCount);
                        offsetCount += 10;
                    }

                    wke = new WorkspaceElement();
                    wke.SetCurrentValue(BackgroundProperty, bar.GetValue(BackgroundProperty));

                    wke.Content = scatterCanvas;
                    wke.Opacity = 0;
                    wke.Background = Brushes.LightGray;

                    wke.item = this;
                    win.DockedItems.Add(wke);
                    bar.Items.Add(wke);
                }
                else
                {

                    dockImage = new Image();
                    dockImage.Source = this.image.Source;
                    dockImage.SetCurrentValue(HeightProperty, barImageHeight);
                    dockImage.SetCurrentValue(WidthProperty, barImageWidth);
                    //dockImage.Opacity = 0.0;
                    wke = new WorkspaceElement();
                    wke.SetCurrentValue(BackgroundProperty, bar.GetValue(BackgroundProperty));

                    wke.Content = dockImage;
                    wke.Opacity = 0;
                    wke.Background = Brushes.LightGray;

                    wke.item = this;
                    win.DockedItems.Add(wke);
                    bar.Items.Add(wke);
                }

                Point startPoint = wke.TransformToAncestor(win.getMain()).Transform(new Point(0, 0));
                Point relPoint = wke.TransformToAncestor(bar).Transform(new Point(0, 0));
                rootPoint = new Point(startPoint.X + relPoint.X, startPoint.Y + relPoint.Y);

                Console.WriteLine("x:" + rootPoint.X + " y:" + rootPoint.Y);

                PointAnimation anim1 = new PointAnimation();
                anim1.Completed += anim1Completed;
                anim1.From = new Point(this.Center.X, this.Center.Y);
                Console.WriteLine("this.Center.X : " + this.Center.X + " AND this.Center.Y : " + this.Center.Y);
                anim1.To = new Point(rootPoint.X + win.BarOffset + barImageWidth / 2.0, rootPoint.Y + barImageHeight / 2.0);
                anim1.Duration = new Duration(TimeSpan.FromSeconds(.4));
                anim1.FillBehavior = FillBehavior.Stop;

                isDocked = true;
                oldHeight = this.Height;
                oldWidth = this.Width;
                DoubleAnimation heightAnim = new DoubleAnimation();
                heightAnim.From = this.Height;
                heightAnim.To = barImageHeight;
                heightAnim.Duration = new Duration(TimeSpan.FromSeconds(.4));
                heightAnim.FillBehavior = FillBehavior.Stop;
                DoubleAnimation orientAnim = new DoubleAnimation();
                orientAnim.From = this.ActualOrientation;
                orientAnim.To = 0;
                orientAnim.Duration = new Duration(TimeSpan.FromSeconds(.4));
                orientAnim.FillBehavior = FillBehavior.Stop;
                DoubleAnimation widthAnim = new DoubleAnimation();
                widthAnim.From = this.Width;
                widthAnim.To = barImageWidth;// barVersion.Width;
                widthAnim.Duration = new Duration(TimeSpan.FromSeconds(.4));
                widthAnim.FillBehavior = FillBehavior.Stop;
                this.BeginAnimation(CenterProperty, anim1);
                this.BeginAnimation(MaxHeightProperty, heightAnim);
                this.BeginAnimation(MaxWidthProperty, widthAnim);
                this.BeginAnimation(OrientationProperty, orientAnim);
            }
            else if (knowledgeWebUse)
            {
                ScatterView sv = this.Parent as ScatterView;
                sv.Items.Remove(this);
            }
        }

        public void anim1Completed(object sender, EventArgs e)
        {
            Console.WriteLine("ANIM 1");
            isDocked = true;
            this.isAnimating = false;
            this.Opacity = 0;
            wke.Opacity = 1.0;
            this.SetCurrentValue(CenterProperty, new Point(rootPoint.X + win.BarOffset + barImageWidth / 2.0, rootPoint.Y + barImageHeight / 2.0));
            this.CanMove = false;
            //this.SetCurrentValue(CenterProperty, pt);
            //dockImage.Opacity = 1.0;
            double imgRatio = oldHeight / oldWidth;
            this.SetCurrentValue(HeightProperty, barImageHeight * .9);
            this.SetCurrentValue(WidthProperty, barImageWidth * .9);
            this.CanRotate = false;
            this.Orientation = 0;
            actualWKEWidth = wke.ActualWidth;
            win.BarOffset += wke.ActualWidth;
            Console.WriteLine("ACTUAL WIDTH " + wke.ActualWidth);
            Console.WriteLine(win.BarOffset);
            //flushItems();
        }

        public void barversTouchDown(object sender, EventArgs e)
        {
            touchDown = true;
            if (isDocked && this.Center.X > win.ActualWidth * .2 && !this.isAnimating && win.bottomPanelVisible)
            {
                if (knowledgeWebUse)
                {
                    if (knowledgeStack)
                    {
                        foreach (ScatterViewItem i in svLists)
                        {
                            WebStack.sviContent content = (WebStack.sviContent)i.Tag;
                            content.used = true;
                            content.r.Visibility = Visibility.Collapsed;
                            for (int j = 0; j < kw.sviList.Count; j++)
                            {
                                WebStack.sviContent content2 = (WebStack.sviContent)kw.sviList[j].Tag;
                                if (content.im.Source == content2.im.Source)
                                {
                                    content2.used = false;
                                    kw.sviList[j].Tag = content2;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        WebStack.sviContent content = (WebStack.sviContent)dockedItem.Tag;

                        for (int j = 0; j < kw.sviList.Count; j++)
                        {
                            WebStack.sviContent content2 = (WebStack.sviContent)kw.sviList[j].Tag;
                            if (content.im.Source == content2.im.Source)
                            {
                                content2.used = false;
                                kw.sviList[j].Tag = content2;
                                break;
                            }
                        }
                        content.r.Visibility = Visibility.Collapsed;
                    }
                }

                //this.SetCurrentValue(HeightProperty, dockImage.Height);
                this.isAnimating = true;
                this.Opacity = 1.0;
                this.CanRotate = true;
                this.isDocked = false;
                this.CanMove = true;

                DoubleAnimation heightAnim = new DoubleAnimation();
                heightAnim.From = this.ActualHeight;
                heightAnim.To = oldHeight; // barVersion.Height;
                heightAnim.Duration = new Duration(TimeSpan.FromSeconds(.3));
                heightAnim.FillBehavior = FillBehavior.Stop;
                DoubleAnimation widthAnim = new DoubleAnimation();
                widthAnim.From = this.ActualWidth;
                widthAnim.To = oldWidth; // barVersion.Width;
                widthAnim.Duration = new Duration(TimeSpan.FromSeconds(.3));
                widthAnim.FillBehavior = FillBehavior.Stop;
                this.BeginAnimation(HeightProperty, heightAnim);
                this.BeginAnimation(WidthProperty, widthAnim);

                //wke.Children.RemoveAt(0);
                //dockImage.Opacity = 0;
                wke.Opacity = 0;
                DoubleAnimation dockwidthAnim = new DoubleAnimation();
                dockwidthAnim.Completed += anim2Completed;
                dockwidthAnim.From = barImageWidth;
                dockwidthAnim.To = 0; // barVersion.Width;
                dockwidthAnim.Duration = new Duration(TimeSpan.FromSeconds(.3));
                dockwidthAnim.FillBehavior = FillBehavior.Stop;

                if (!knowledgeStack)
                {
                    dockImage.BeginAnimation(WidthProperty, dockwidthAnim);
                }
                else
                {
                    this.isAnimating = false;
                    this.isDocked = false;
                    bar.Items.Remove(wke);
                    win.DockedItems.Remove(wke);

                }
                win.BarOffset -= actualWKEWidth;
                int dex = win.DockedItems.IndexOf(wke);
                for (int i = dex + 1; i < win.DockedItems.Count; i++)
                {
                    WorkspaceElement w = win.DockedItems[i] as WorkspaceElement;
                    w.item.SetCurrentValue(CenterProperty, new Point(w.item.Center.X - actualWKEWidth, w.item.Center.Y));
                }
            }
        }
        public void anim2Completed(object sender, EventArgs e)
        {
            Console.WriteLine("ANIM 2");
            this.isAnimating = false;
            this.isDocked = false;
            bar.Items.Remove(wke);
            win.DockedItems.Remove(wke);
            //flushItems();
        }

        public void RemoveListener(object sender, EventArgs e)
        {
            Helpers helpers = new Helpers();
            if (!this.isDocked && this.Center.X > win.ActualWidth - 100 && !touchDown && this.Center.Y < win.ActualHeight * .7)
            {
                PointAnimation anim1 = new PointAnimation();
                anim1.Completed += anim3Completed;
                anim1.From = new Point(this.Center.X, this.Center.Y);
                anim1.To = new Point(win.ActualWidth + 1000, this.Center.Y);
                anim1.Duration = new Duration(TimeSpan.FromSeconds(.4));
                anim1.FillBehavior = FillBehavior.Stop;
                if (aldbi != null) aldbi.opened = false;
                this.BeginAnimation(CenterProperty, anim1);
                if (_helpers.IsVideoFile(imageURIPath))
                {
                    vidBub.pauseVideo();
                }
            }

            // jcchin - tour prep debug info
            /*Console.WriteLine("dockItem.ActualCenter.X = " + this.ActualCenter.X + ", dockItem.ActualCenter.Y = " + this.ActualCenter.Y);
            Console.WriteLine("dockItem.Scale = " + this.ActualWidth / this.image.Source.Width);*/
        }

        public void anim3Completed(object sender, EventArgs e)
        {
            Console.WriteLine("ANIM 3");
            aldbi = null;
            mainScatterView.Items.Remove(this);
        }


        //public void flushItems()
        //{
        //    foreach (WorkspaceElement w in win.DockedItems)
        //    {
        //        if (!w.item.isDocked)
        //        {
        //            win.DockedItems.Remove(w);
        //            bar.Items.Remove(w);
        //        }
        //    }
        // }



    }

    //for the images on the left panel that you click to add
    public class AssociatedDocListBoxItem : SurfaceListBoxItem
    {
        Image image;
        Label label;
        ArtworkModeWindow _lb;
        DockPanel dp;
        String scatteruri;
        public Boolean opened;
        private Helpers _helpers;

        public AssociatedDocListBoxItem(String labeltext, String imageUri, String _scatteruri, ArtworkModeWindow lb)
        {
            Console.WriteLine("labeltext: " + labeltext);
            Console.WriteLine("imageUri: " + imageUri);
            Console.WriteLine("_scatteruri: " + _scatteruri);
            _helpers = new Helpers();


            scatteruri = _scatteruri;
            _lb = lb;
            opened = false;
            dp = new DockPanel();
            this.Content = dp;
            //if image
            if (_helpers.IsImageFile(_scatteruri))
            {
                image = new Image();
            _helpers = new Helpers();

            FileStream stream = new FileStream(imageUri, FileMode.Open);

            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            stream.Close();

                wpfImage.SetCurrentValue(DockPanel.DockProperty, Dock.Left);

                wpfImage.SetCurrentValue(HeightProperty, 50.0);
                wpfImage.SetCurrentValue(WidthProperty, 50 * wpfImage.Source.Width / wpfImage.Source.Height);

                dp.Children.Add(wpfImage);

                label = new Label();
                label.Content = labeltext;
                label.FontSize = 18;
                label.SetCurrentValue(DockPanel.DockProperty, Dock.Right);
                dp.Children.Add(label);
                //.GotFocus += new RoutedEventHandler(onTouch);
                this.PreviewTouchDown += new EventHandler<TouchEventArgs>(onTouch);
                this.PreviewMouseDown += new MouseButtonEventHandler(onTouch);
                lb.getAssociatedDocToolBar().Items.Add(this);
            }

            //equivalent for videos
            else if (_helpers.IsVideoFile(_scatteruri))
            {
                image = new Image();
                
                imageUri = System.IO.Path.GetFullPath(imageUri);
                int decrement = System.IO.Path.GetExtension(imageUri).Length ;
                imageUri = imageUri.Remove(imageUri.Length - decrement, decrement);
                imageUri += ".bmp";
                FileStream stream = new FileStream(imageUri, FileMode.Open);

                System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
                System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
                stream.Close();

                wpfImage.SetCurrentValue(DockPanel.DockProperty, Dock.Left);

                wpfImage.SetCurrentValue(HeightProperty, 50.0);
                wpfImage.SetCurrentValue(WidthProperty, 50 * wpfImage.Source.Width / wpfImage.Source.Height);

                dp.Children.Add(wpfImage);

                label = new Label();
                label.Content = labeltext;
                label.FontSize = 18;
                label.SetCurrentValue(DockPanel.DockProperty, Dock.Right);
                dp.Children.Add(label);
                //.GotFocus += new RoutedEventHandler(onTouch);
                this.PreviewTouchDown += new EventHandler<TouchEventArgs>(onTouch);
                this.PreviewMouseDown += new MouseButtonEventHandler(onTouch);
                lb.getAssociatedDocToolBar().Items.Add(this);

                //sketchy code for not really creating thumbnails
                //MediaElement thumVid = new MediaElement();
                //thumVid.Source = new Uri(scatteruri, UriKind.RelativeOrAbsolute);
                //Console.WriteLine("scatteruri is: " + scatteruri);

                //thumVid.LoadedBehavior = MediaState.Manual;
                //thumVid.ScrubbingEnabled = true;
                //thumVid.Play();
                //thumVid.Pause();

                //thumVid.Position = new TimeSpan(0, 0, 0, 0);
                //thumVid.SetCurrentValue(DockPanel.DockProperty, Dock.Left);
                //thumVid.SetCurrentValue(HeightProperty, 50.0);
                //thumVid.SetCurrentValue(WidthProperty, 50 * thumVid.Width / thumVid.Height);

                //dp.Children.Add(thumVid);

                //label = new Label();
                //label.Content = labeltext;
                //label.FontSize = 18;
                //label.SetCurrentValue(DockPanel.DockProperty, Dock.Right);
                //dp.Children.Add(label);
                ////.GotFocus += new RoutedEventHandler(onTouch);
                //this.PreviewTouchDown += new EventHandler<TouchEventArgs>(onTouch);
                //this.PreviewMouseDown += new MouseButtonEventHandler(onTouch);
                //lb.getAssociatedDocToolBar().Items.Add(this);
            }
        }

        public void onTouch(object sender, EventArgs e)
        {
            Console.WriteLine(this.opened);
            if (!this.opened)
            {
                //if it's an image, do this:
                if (_helpers.IsImageFile(scatteruri))
                {
                    new DockableItem(_lb.getMainScatterView(), _lb, _lb.getBar(), scatteruri, this);
                }
                else if (_helpers.IsVideoFile(scatteruri))
                {
                    //perhaps the initializatoin of this bubble should have the height and width of the thumbnail... if I could extract one...
                    new DockableItem(_lb.getMainScatterView(), _lb, _lb.getBar(), scatteruri, this, new LADSVideoBubble(scatteruri, 500, 500), new VideoItem()); //video-specific constructor
                }
                else
                { //not image or video...
                }

                //for all, do this:
                this.opened = true;
                Console.WriteLine(this.opened + " inside");

            }
        }


        //revisions on Noah's WorkTop code for getting thumbnails from video

        //public override Label GetThumbnail()
        //{
        //    Label l = new Label();
        //    Grid g = new Grid();

        //    Image iconOverlay = Properties.Resources.movie.LoadImage();
        //    iconOverlay.Opacity = .5;
        //    iconOverlay.Width = 25;
        //    iconOverlay.Height = 25 * iconOverlay.Source.Height / iconOverlay.Source.Width;

        //    RenderTargetBitmap frame = new RenderTargetBitmap((int)_video.ActualWidth + 1, (int)_video.ActualHeight + 1, 96, 96, PixelFormats.Default);
        //    frame.Render(_video);
        //    Image frameImage = new Image();
        //    frameImage.Source = frame;
        //    frameImage.Width = LockedAspect > 1 ? 100 : 100 * LockedAspect;
        //    frameImage.Height = LockedAspect > 1 ? 100 / LockedAspect : 100;

        //    g.Children.Add(frameImage);
        //    g.Children.Add(iconOverlay);

        //    l.Content = g;
        //    return l;
        //}

    }
}
