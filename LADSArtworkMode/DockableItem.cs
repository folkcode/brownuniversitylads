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
using DeepZoom;
using System.IO;
using DexterLib;

namespace LADSArtworkMode
{
    public class DockableItem : ScatterViewItem
    {
        public bool isDocked;
        ScatterView mainScatterView;
        public ArtworkModeWindow win;
        MouseEventHandler mousem;
        SurfaceListBox bar;
        double barX;
        public double oldHeight;
        public double oldWidth;
        public Image image;
        public WorkspaceElement wke;
        int wke_index;
        Image dockImage;
        private Point rootPoint;
        public double barImageWidth;
        public double barImageHeight;
        private double actualWKEWidth;
        private Point pt;
        private ScatterViewItem dockedItem = null;
        private bool knowledgeWebUse = false;
        public Boolean touchDown;
       public Boolean isAnimating;
        AssociatedDocListBoxItem aldbi;
        private List<ScatterViewItem> svLists;
        bool knowledgeStack = false;
        private String imageURIPath;
        private LADSVideoBubble vidBub;
        private Helpers _helpers;
        public string scatteruri;
        public DockedItemInfo info;
        public double aspectRatio;
        public Boolean isVideo = false;
   


        public void resetValues(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar)
        {
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;
            isDocked = false;
        }

        /// <summary>
        /// used by artwork mode, including the tour authoring & playback system
        /// </summary>
        public DockableItem(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar, String imageURIPathParam)
        {
            
            scatteruri = imageURIPathParam;
            image = new Image();
            aldbi = null;
            _helpers = new Helpers();

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
            isDocked = false;
            touchDown = false;

            this.Loaded += new RoutedEventHandler(DockableItem_Loaded);

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, CenterChangedListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);

            mainScatterView.Items.Add(this);
            this.SetCurrentValue(HeightProperty, image.Height);
            this.SetCurrentValue(WidthProperty, image.Width);

            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();

            Random rnd = new Random();

            this.Background = new SolidColorBrush(Colors.Transparent);
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
            scatteruri = imageURIPathParam;
            image = new Image();
            aldbi = null;
            _helpers = new Helpers();

            FileStream stream = new FileStream(imageURIPathParam, FileMode.Open);
            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            image.Source = wpfImage.Source;
            stream.Close();

            this.isAnimating = false;
            this.Background = Brushes.LightGray;
            this.AddChild(image);
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;
            isDocked = false;
            touchDown = false;
            aldbi = _aldbi;

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, CenterChangedListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);

            mainScatterView.Items.Add(this);
            this.SetCurrentValue(HeightProperty, image.Height);
            this.SetCurrentValue(WidthProperty, image.Width);

            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();

            Random rnd = new Random();
            Point pt = new Point(rnd.Next((int)(win.ActualWidth * .2 + image.ActualWidth * 3), (int)(win.ActualWidth - image.ActualWidth * 3 - 100)),
                                                          rnd.Next((int)(image.ActualHeight * 3), (int)(win.ActualHeight * .8 - image.ActualHeight * 3)));
            this.SetCurrentValue(CenterProperty, pt);
            this.Orientation = rnd.Next(-20, 20);

            this.Loaded += new RoutedEventHandler(DockableItem_Loaded);

            imageURIPath = imageURIPathParam;
            
        }

        //constructor for VIDEOS
        public DockableItem(ScatterView _mainScatterView, ArtworkModeWindow _win, SurfaceListBox _bar, string _targetVid, AssociatedDocListBoxItem _aldbi, LADSVideoBubble _video, VideoItem _vidctrl)
        {
            isVideo = true;
            scatteruri = _targetVid;
            vidBub = _video;
            _helpers = new Helpers();

            image = new Image();
            aldbi = null;
            String thumbFileName = _targetVid;

            //the video thumbnail filename is the same name with a different extension. This gets that extension
            int decrement = System.IO.Path.GetExtension(thumbFileName).Length;
            thumbFileName = thumbFileName.Remove(thumbFileName.Length - decrement, decrement);
            thumbFileName += ".bmp";
            thumbFileName = System.IO.Path.GetFileName(thumbFileName);
            thumbFileName = "Data\\Videos\\Metadata\\" + thumbFileName;

            //opens in filestream to prevent errors from the file already being open
            FileStream stream = new FileStream(thumbFileName, FileMode.Open);
            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            image.Source = wpfImage.Source;
            stream.Close();

            this.isAnimating = false;
            this.Background = Brushes.LightGray;
            this.AddChild(vidBub);
            this.UpdateLayout();
            mainScatterView = _mainScatterView;
            bar = _bar;
            win = _win;
            isDocked = false;
            touchDown = false;
            aldbi = _aldbi;

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(this, CenterChangedListener);

            this.PreviewTouchUp += new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp += new MouseButtonEventHandler(AddtoDock);
            this.PreviewMouseWheel += new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
            this.CaptureMouse();

            mainScatterView.Items.Add(this);

            Random rnd = new Random();
            Point pt = new Point(rnd.Next((int)(win.ActualWidth * .2 + vidBub.ActualWidth * 3), (int)(win.ActualWidth - vidBub.ActualWidth * 3 - 100)),
                                                           rnd.Next((int)(vidBub.ActualHeight * 3), (int)(win.ActualHeight * .8 - vidBub.ActualHeight * 3)));
            this.SetCurrentValue(CenterProperty, pt);
            this.Orientation = rnd.Next(-20, 20);

            imageURIPath = _targetVid;
            MediaElement vid = vidBub.getVideo();
            vid.MediaOpened += new RoutedEventHandler(video_MediaOpened);
            vid.Loaded += new RoutedEventHandler(video_MediaOpened);
            this.MinHeight = 100;
        }

        void DockableItem_Loaded(object sender, RoutedEventArgs e)
        {
            changeInitialSize();
        }

        public void changeInitialSize()
        {
            if (win.isTourPlayingOrAuthoring())
                return;
            this.UpdateLayout();
            aspectRatio = (double)this.ActualWidth / (double)this.ActualHeight;
            if (!isVideo)
            {
                this.MinHeight = 80;
                this.MinWidth = 80 * aspectRatio;
            }
            if (aspectRatio>1 && this.ActualHeight < 150)
            {
                this.Height = 150;
                this.Width = aspectRatio * 150;
                
            }
            if (aspectRatio<1 && this.ActualWidth < 150)
            {
                this.Width = 150;
                this.Height = 150 / aspectRatio;

            }
            this.SizeChanged += DockableItem_SizeChanged;
        }

        void DockableItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Helpers helper = new Helpers();
            if (e.NewSize.Width < 150 || e.NewSize.Height < 150)
            {
                this.Width = e.PreviousSize.Width;
                this.Height = e.PreviousSize.Height;
            }
            else if (isVideo) //calls specialized video resize method that also resizes the controls
            {
                vidBub.Resize(e.NewSize.Width, e.NewSize.Width / aspectRatio, false);
                this.Height = vidBub.getVideo().Height + 50;
                this.Width = vidBub.getVideo().Width;
            }
        }

        public void maintainAspectRatio(object sender, EventArgs e)
        {
            this.MinHeight = 80;
            this.MinWidth = 80 * aspectRatio;
        }

        public void removeDockability()
        {
            this.PreviewTouchUp -= new EventHandler<TouchEventArgs>(AddtoDock);
            this.PreviewMouseUp -= new MouseButtonEventHandler(AddtoDock);
            this.PreviewMouseWheel -= new MouseWheelEventHandler(DockableItem_PreviewMouseWheel);
        }

        private void video_MediaOpened(object sender, RoutedEventArgs e)
        {
            this.Height = vidBub.getHeight();
            this.Width = vidBub.getWidth();
            aspectRatio = vidBub.getVideo().Width / vidBub.getVideo().Height;
            this.SizeChanged += DockableItem_SizeChanged;
        }

        public void stopVideo()
        {
            vidBub.pauseVideo();
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

            newWidth = ((double)e.Delta) / 5.0 + this.ActualWidth;
            newHeight = this.ActualHeight * newWidth / this.ActualWidth;

            if (newWidth < 40 || newHeight < 40) return;

            this.Height = newHeight;
            this.Width = newWidth;


        }

        public void AddtoDockFromSaved(double savedOldWidth, double savedOldHeight, double savedWKEWidth, DockedItemInfo oldinfo)
        {
            oldHeight = savedOldHeight;
            oldWidth = savedOldWidth;
            info = oldinfo;
            barImageHeight = bar.ActualHeight * .8;
            barImageWidth = bar.ActualHeight * savedOldWidth / savedOldHeight;
            this.MinHeight = 80;
            this.MinWidth = 80 * savedOldWidth / savedOldHeight;

            dockImage = new Image();
            dockImage.Source = this.image.Source;
            dockImage.SetCurrentValue(HeightProperty, barImageHeight);
            dockImage.SetCurrentValue(WidthProperty, barImageWidth);
            wke = new WorkspaceElement();
            wke.Visibility = Visibility.Visible;
            wke.SetCurrentValue(BackgroundProperty, bar.GetValue(BackgroundProperty));
            wke = new WorkspaceElement();
            wke.Content = dockImage;
            wke.Opacity = 0;
            wke.Background = Brushes.LightGray;
            wke.bar = bar;
            wke.item = this;
            wke.artmodewin = win;
            win.DockedItems.Add(wke);
            win.DockedDockableItems.Add(this);
            bar.Items.Add(wke);
            Point startPoint = wke.TransformToAncestor(win.getMain()).Transform(new Point(0, 0));
            Point relPoint = wke.TransformToAncestor(bar).Transform(new Point(0, 0));
            rootPoint = new Point(startPoint.X + relPoint.X, startPoint.Y + relPoint.Y);
            this.Opacity = .5;
            wke.Opacity = 1.0;
            this.SetCurrentValue(CenterProperty, new Point(rootPoint.X + win.BarOffset + barImageWidth / 2.0, rootPoint.Y + barImageHeight / 2.0));
            this.CanMove = false;
            double imgRatio = oldHeight / oldWidth;
            this.SetCurrentValue(HeightProperty, barImageHeight * .9);
            this.SetCurrentValue(WidthProperty, barImageWidth * .9);
            this.CanRotate = false;
            this.Orientation = 0;

            actualWKEWidth = savedWKEWidth;
            win.BarOffset += savedWKEWidth;
            this.Visibility = Visibility.Hidden;

            this.isDocked = true;
            this.isAnimating = false;
            this.IsHitTestVisible = true;
            
        }

        public void AddtoDock(object sender, EventArgs e)
        {

            touchDown = false;
            DockableItem item = sender as DockableItem;
            Helpers helpers = new Helpers();

            if (this.Center.Y > (win.ActualHeight * .8) && !isDocked && this.Center.X > win.ActualWidth * .2 && !this.isAnimating)
            {
                // Explore mode asset management.
                win.tourExploreManageDock(item);


                this.isAnimating = true;
                this.IsHitTestVisible = false;
                this.SizeChanged -= DockableItem_SizeChanged;
                if (helpers.IsVideoFile(imageURIPath))
                {
                    vidBub.pauseVideo();
                }
                this.isAnimating = true;
                barImageHeight = bar.ActualHeight * .8;
                barImageWidth = bar.ActualHeight * this.Width / this.Height;

                dockImage = new Image();
                dockImage.Source = this.image.Source;
                dockImage.SetCurrentValue(HeightProperty, barImageHeight);
                dockImage.SetCurrentValue(WidthProperty, barImageWidth);
                wke = new WorkspaceElement();
                wke.Visibility = Visibility.Visible;
                wke.SetCurrentValue(BackgroundProperty, bar.GetValue(BackgroundProperty));
                wke = new WorkspaceElement();
                wke.Content = dockImage;
                wke.Opacity = 0;
                wke.Background = Brushes.LightGray;
                wke.bar = bar;
                wke.item = this;
                wke.artmodewin = win;
                win.DockedItems.Add(wke);
                win.DockedDockableItems.Add(this);
                bar.Items.Add(wke);

                Point startPoint = wke.TransformToAncestor(win.getMain()).Transform(new Point(0, 0));
                Point relPoint = wke.TransformToAncestor(bar).Transform(new Point(0, 0));
                rootPoint = new Point(startPoint.X + relPoint.X, startPoint.Y + relPoint.Y);

                PointAnimation anim1 = new PointAnimation();
                anim1.Completed += anim1Completed;
                anim1.From = new Point(this.Center.X, this.Center.Y);
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
        }

        public void anim1Completed(object sender, EventArgs e)
        {
            this.Opacity = .5;
            wke.Opacity = 1.0;
            this.SetCurrentValue(CenterProperty, new Point(rootPoint.X + win.BarOffset + barImageWidth / 2.0, rootPoint.Y + barImageHeight / 2.0));
            this.CanMove = false;
            double imgRatio = oldHeight / oldWidth;
            this.SetCurrentValue(HeightProperty, barImageHeight * .9);
            this.SetCurrentValue(WidthProperty, barImageWidth * .9);
            this.CanRotate = false;
            this.Orientation = 0;
            actualWKEWidth = wke.ActualWidth;
            win.BarOffset += wke.ActualWidth;
            this.Visibility = Visibility.Hidden;

            isDocked = true;
            this.isAnimating = false;
            this.IsHitTestVisible = true;
            if (info == null) info = new DockedItemInfo();
            info.scatteruri = scatteruri;
            info.savedOldHeight = this.oldHeight;
            info.savedOldWidth = this.oldWidth;
            info.savedWKEWidth = actualWKEWidth;
            wke.info = info;
            if (!win.SavedDockedItems.Contains(info))
                win.SavedDockedItems.Add(info);
        }

        public void anim2Completed(object sender, EventArgs e)
        {
            this.isAnimating = false;
            this.isDocked = false;
            bar.Items.Remove(wke);
            win.DockedItems.Remove(wke);
        }

        public void CenterChangedListener(object sender, EventArgs e)
        {
            Helpers helpers = new Helpers();
            if (!this.isDocked && this.Center.X > win.ActualWidth - 100 && !touchDown && this.Center.Y < win.ActualHeight * .7 && !win.isTourPlayingOrAuthoring())
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
        }

        public void anim3Completed(object sender, EventArgs e)
        {
            aldbi = null;
            mainScatterView.Items.Remove(this);
            if (info != null && win.SavedDockedItems.Contains(info))
            win.SavedDockedItems.Remove(info);
        }

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
                this.PreviewTouchDown += new EventHandler<TouchEventArgs>(onTouch);
                this.PreviewMouseDown += new MouseButtonEventHandler(onTouch);
                lb.getAssociatedDocToolBar().Items.Add(this);
            }

            //equivalent for videos
            else if (_helpers.IsVideoFile(_scatteruri))
            {
                if (_helpers.IsDirShowFile(_scatteruri)) //can easily create nice thumbnails of the video using DirectShow
                {
                    image = new Image();

                    imageUri = System.IO.Path.GetFullPath(imageUri);
                    int decrement = System.IO.Path.GetExtension(imageUri).Length;
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
                    this.PreviewTouchDown += new EventHandler<TouchEventArgs>(onTouch);
                    this.PreviewMouseDown += new MouseButtonEventHandler(onTouch);
                    lb.getAssociatedDocToolBar().Items.Add(this);
                }
                //Code for not actually creating thumbnails of videos, but instead creating paused, unplayable media elements to act as thumbnails
                else
                {
                    MediaElement thumVid = new MediaElement();
                    thumVid.Source = new Uri(scatteruri, UriKind.RelativeOrAbsolute);

                    thumVid.LoadedBehavior = MediaState.Manual;
                    thumVid.ScrubbingEnabled = true;
                    thumVid.Play();
                    thumVid.Pause();

                    thumVid.Position = new TimeSpan(0, 0, 0, 0);
                    thumVid.SetCurrentValue(DockPanel.DockProperty, Dock.Left);
                    thumVid.SetCurrentValue(HeightProperty, 50.0);
                    thumVid.SetCurrentValue(WidthProperty, 50 * thumVid.Width / thumVid.Height);

                    dp.Children.Add(thumVid);

                    label = new Label();
                    label.Content = labeltext;
                    label.FontSize = 18;
                    label.SetCurrentValue(DockPanel.DockProperty, Dock.Right);
                    dp.Children.Add(label);
                    this.PreviewTouchDown += new EventHandler<TouchEventArgs>(onTouch);
                    this.PreviewMouseDown += new MouseButtonEventHandler(onTouch);
                    lb.getAssociatedDocToolBar().Items.Add(this);
                }
            }
        }

        public void setLabel(string l)
        {
            label.Content = l;
        }

        public string getLabel()
        {
            return (string)label.Content;
        }

        public void onTouch(object sender, EventArgs e)
        {
            if (!this.opened)
            {
                //if it's an image, do this:
                if (_helpers.IsImageFile(scatteruri))
                {
                    new DockableItem(_lb.getMainScatterView(), _lb, _lb.getBar(), scatteruri, this);
                }
                else if (_helpers.IsVideoFile(scatteruri))
                {
                    new DockableItem(_lb.getMainScatterView(), _lb, _lb.getBar(), scatteruri, this, new LADSVideoBubble(scatteruri, 500, 500), new VideoItem()); //video-specific constructor
                }
                else
                { //not image or video...
                }

                //all kinds of elements must be set to open
                this.opened = true;
            }
        }
    }
}
