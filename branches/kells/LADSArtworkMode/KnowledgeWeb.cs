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
    public class KnowledgeWeb
    {
        bool done = false;
        public bool selectionCurve = false;
        bool curveDrawMode = false;
        Point startPoint = new Point();
        Point beforePoint = new Point();
        Point fixedCenter = new Point();
        public DispatcherTimer curveTimer = null;
        List<Point> curvePoints = new List<Point>();
        List<Line> curveDraw = new List<Line>();
        public List<ScatterViewItem> sviList = new List<ScatterViewItem>();
        public int count = 0;

        double minXin = double.MaxValue;
        double maxXin = double.MinValue;
        double minYin = double.MaxValue;
        double maxYin = double.MinValue;
        
        double maxX = double.MinValue;
        double minX = double.MaxValue;
        double maxY = double.MinValue;
        double minY = double.MaxValue;

        public WebStack currentOpen = null;

        public LADSArtworkMode.ArtworkModeWindow artwork;

        FloatingSearchBox bx;

        Rectangle r = new Rectangle();
        bool sizeChanged = false;

        List<SingleWeb> vertexList = new List<SingleWeb>();
        double scaleFactor = 1;
        double scaleX = 1;
        double scaleY = 1;
        double oldX = 0;
        double oldY = 0;
        double currentSize = 0;
        double canvasH = 0;
        double canvasW = 0;
        double sides = 800;

        double hDiff = 0;
        double wDiff = 0;
        double centerStartX = 0;
        double centerStartY = 0;

        int screenW = 0;
        int screenH = 0;
        
        int currentIndex = 0;

        public List<WebGroup> grp = new List<WebGroup>();
        ScatterViewItem searchBox = new ScatterViewItem();
        ScatterViewItem moveLayer = new ScatterViewItem();
        ScatterView topScatter = new ScatterView();
        ScatterView innerLayer = new ScatterView();
        ScatterView hudScatter = new ScatterView();

        bool searchBoxPresent = false;

        Canvas topCanvas;
        Canvas lineCanvas = new Canvas();

        SurfaceButton searchButton = new SurfaceButton();

        public KnowledgeWeb(Canvas c, double height, double width, String fileName, LADSArtworkMode.ArtworkModeWindow window)
        {
            artwork = window;
            centerStartX = width * 1.5;
            centerStartY = height * 1.5;

            screenW = (int)width;
            screenH = (int)height;

            sides = Math.Max(width / 3,15);

            //first scatterview where the large scatterview item lives
            canvasH = height;
            canvasW = width;

            topCanvas = c;
            c.ClipToBounds = false;

            topScatter.Height = canvasH *3;
            topScatter.Width = canvasW *3;
            topCanvas.Children.Add(topScatter);
            Canvas.SetLeft(topScatter, -canvasW);
            Canvas.SetTop(topScatter, -canvasH);

            hudScatter.Height = canvasH;
            hudScatter.Width = canvasW;
            topCanvas.Children.Add(hudScatter);

            //create the search button and the search box
            //createSearchButton();
            createSearchBox();

            //create the large scatterview item that controls panning and zooming
            topScatter.Items.Add(moveLayer);

            moveLayer.Height = canvasH;
            moveLayer.Width = canvasW;

            moveLayer.Orientation = 0;
            moveLayer.CanRotate = false;
            moveLayer.Center = new Point(canvasW / 2, canvasH / 2);
            moveLayer.SizeChanged += new SizeChangedEventHandler(moveLayer_SizeChanged);
            //moveLayer.Background = Brushes.Transparent;
            moveLayer.ShowsActivationEffects = false;
            moveLayer.PreviewTouchDown += new EventHandler<TouchEventArgs>(moveLayer_PreviewTouchDown);
            moveLayer.PreviewTouchUp += new EventHandler<TouchEventArgs>(moveLayer_PreviewTouchUp);
            moveLayer.PreviewTouchMove += new EventHandler<TouchEventArgs>(moveLayer_PreviewTouchMove);

            DependencyPropertyDescriptor dpd1 = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd1.AddValueChanged(moveLayer, ScatterviewMainChanged);

            //create the canvas that will contain all the things inside the large scatterview
            Canvas otherCanvas = new Canvas();
            moveLayer.Content = otherCanvas;

            //save the variables that will calculate ratio
            currentSize = canvasH * canvasW;
            oldY = canvasH;
            oldX = canvasW;

            r.Width = canvasW;
            r.Height = canvasH;
            r.Fill = Brushes.Transparent;
            otherCanvas.Children.Add(r);

            otherCanvas.Children.Add(lineCanvas);
            otherCanvas.Children.Add(innerLayer);

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ScatterViewItem.CenterProperty, typeof(ScatterViewItem));
            dpd.AddValueChanged(moveLayer, ScatterViewCenterChanged);

            moveLayer.Center = new Point(width * 1.5, height * 1.5);

           /* List<WebGroup> temp = new List<WebGroup>();
            temp = ((FloatingSearchBox)searchBox.Content).GroupList;
            foreach (WebGroup i in temp)
            {
               //TODO: check the filename
            }*/
        }

        private void ScatterviewMainChanged(Object sender, EventArgs e)
        {
            if (curveDrawMode)
            {
                done = true;
                ScatterViewItem currentScatter = sender as ScatterViewItem;
                currentScatter.Center = fixedCenter;
            }
        }

        public bool checkInside(Point pt)
        {
            if (pt.X < minX)
                return false;
            if (pt.Y < minY)
                return false;
            if (pt.X > maxX)
                return false;
            if (pt.Y > maxY)
                return false;

            return true;
        }

        public bool checkInside2(Point pt)
        {
            if (pt.X < minXin)
                return false;
            if (pt.Y < minYin)
                return false;
            if (pt.X > maxXin)
                return false;
            if (pt.Y > maxYin)
                return false;

            return true;
        }

        void moveLayer_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (count > 10000)
            {
                ScatterViewItem currentScatter = sender as ScatterViewItem;
                currentScatter.Center = fixedCenter;

                e.Handled = true;
                curvePoints.Add(e.TouchDevice.GetCenterPosition(topCanvas));
                Ellipse c = new Ellipse();
                c.Height = 10;
                c.Width = 10;
                c.Fill = Brushes.Black;

                Line newLine = new Line();
                newLine.X1 = beforePoint.X;
                newLine.Y1 = beforePoint.Y;
                newLine.X2 = e.TouchDevice.GetCenterPosition(topCanvas).X;
                newLine.Y2 = e.TouchDevice.GetCenterPosition(topCanvas).Y;
                newLine.StrokeThickness = 2;
                newLine.Stroke = Brushes.Black;
                topCanvas.Children.Add(newLine);
                beforePoint = e.TouchDevice.GetCenterPosition(topCanvas);
                curveDraw.Add(newLine);

                Point temp = e.TouchDevice.GetCenterPosition(innerLayer);

                if (newLine.X2 < minX)
                    minX = newLine.X2;
                if (newLine.X2 > maxX)
                    maxX = newLine.X2;
                if (newLine.Y2 < minY)
                    minY = newLine.Y2;
                if (newLine.Y2 > maxY)
                    maxY = newLine.Y2;

                if (temp.X < minXin)
                    minXin = temp.X;
                if (temp.X > maxXin)
                    maxXin = temp.X;
                if (temp.Y < minYin)
                    minYin = temp.Y;
                if (temp.Y > maxYin)
                    maxYin = temp.Y;
            }
            else if (curveTimer != null)
            {
                double movement = Math.Abs(e.TouchDevice.GetCenterPosition(topCanvas).X - startPoint.X) + Math.Abs(e.TouchDevice.GetCenterPosition(topCanvas).Y - startPoint.Y);
                if (movement > 50)
                {
                    curveDrawMode = false;
                    count = 0;
                    curveTimer.Stop();
                    curveTimer = null;
                }
            }
        }

        void moveLayer_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if (curveTimer != null)
            {
                curveDrawMode = false;
                double temp = Math.Abs(e.TouchDevice.GetCenterPosition(topCanvas).X - curvePoints[0].X) + Math.Abs(e.TouchDevice.GetCenterPosition(topCanvas).Y - curvePoints[0].Y);
                if (temp < 50)
                {
                    selectionCurve = true;
                }
                else
                {
                    foreach (Line i in curveDraw)
                        topCanvas.Children.Remove(i);

                    curveDraw.Clear();
                    curvePoints.Clear();
                    selectionCurve = false;
                    minX = double.MaxValue;
                    minY = double.MaxValue;
                    maxX = double.MinValue;
                    maxY = double.MinValue;

                    minXin = double.MaxValue;
                    minYin = double.MaxValue;
                    maxXin = double.MinValue;
                    maxYin = double.MinValue;

                }
                count = 0;
                curveTimer.Stop();
                curveTimer = null;
            }
        }

        void moveLayer_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            toggleStackAnim(null);
            toggleSearchOff();

            ScatterViewItem currentScatter = sender as ScatterViewItem;
            fixedCenter = currentScatter.Center;

            if (curveTimer == null)
            {
                if (selectionCurve)
                {
                    List<ScatterViewItem> svList = new List<ScatterViewItem>();
                    if (checkInside(e.TouchDevice.GetCenterPosition(topCanvas)))
                    {
                        Canvas scatterCanvas = new Canvas();
                        scatterCanvas.Height = 100;
                        scatterCanvas.Width = 100;
                        double offsetCount = 0;

                        foreach (ScatterViewItem i in sviList)
                        {
                            if(checkInside2(i.Center))
                            {
                                svList.Add(i);
                                WebStack.sviContent content = (WebStack.sviContent)i.Tag;
                                if (content.used)
                                {
                                    svList.Clear();
                                    break;
                                }
                                Image temp = new Image();
                                temp.Source = content.im.Source;
                                temp.Height = 100;
                                temp.Width = 100;

                                scatterCanvas.Children.Add(temp);
                                Canvas.SetLeft(temp, offsetCount);
                                offsetCount += 10;
                            }
                        
                        }

                        if (svList.Count > 0)
                        {
                            WebStack.sviContent content = (WebStack.sviContent)sviList[0].Tag;
                            LADSArtworkMode.DockableItem item = new LADSArtworkMode.DockableItem(artwork.MainScatterView, artwork, artwork.Bar, content.im.Source, svList,this);
                            item.Center = e.TouchDevice.GetCenterPosition(artwork.MainScatterView);
                            item.Content = scatterCanvas;
                            item.CaptureTouch(e.TouchDevice);
                        }
                    }

                    minX = double.MaxValue;
                    minY = double.MaxValue;
                    maxX = double.MinValue;
                    maxY = double.MinValue;

                    minXin = double.MaxValue;
                    minYin = double.MaxValue;
                    maxXin = double.MinValue;
                    maxYin = double.MinValue;

                    foreach (Line i in curveDraw)
                      topCanvas.Children.Remove(i);

                    curveDraw.Clear();
                    curvePoints.Clear();
                    selectionCurve = false;
                }
                else
                {
                    curvePoints.Clear();

                    curvePoints.Add(e.TouchDevice.GetCenterPosition(topCanvas));
                    startPoint = curvePoints[curvePoints.Count - 1];
                    beforePoint = startPoint;

                    curveTimer = new DispatcherTimer();
                    count = 0;
                    curveTimer.Tick += new EventHandler(curveTimer_Tick);
                    curveTimer.Start();
                }
            }
        }

        void curveTimer_Tick(object sender, EventArgs e)
        {
            count++;
            if (count > 10000)
            {
                curveDrawMode = true;
            }
        }

        public void toggleStackAnim(WebStack st)
        {
            if (currentOpen != null)
            {
                if (st != null && st != currentOpen)
                    currentOpen.compactIn();
                else
                {
                    foreach (ScatterViewItem svi in currentOpen.ScatterViewItems())
                    {
                        if (svi.AreAnyTouchesOver)
                            return;
                    }

                    currentOpen.compactIn();
                }
            }

            if (st != null)
                currentOpen = st;
            else
                currentOpen = null;
        }


        public void addKeyWords(WebGroup g)
        {
            bx.SearchOnGroup(g);
            toggleSearchOn();
        }

        public void SelectArtwork(String filePath)
        {
            char[] delim = { '\\', '/' };
            String[] path = filePath.Split(delim);
            String xmlPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + "Data/Images/DeepZoom/" + path[path.Length-1] + "/dz.xml";
            //String xmlPath = @"C:\Users\fadeputr\Desktop\LADS - Copy\GCNav\bin\Debug\Data\Images\DeepZoom\gari0001.bmp\dz.xml";

            artwork.MultiImage.SetImageSource(@xmlPath);
            artwork.MultiImage.ResetArtwork();

            artwork.MultiImageThumb.SetImageSource(@xmlPath);

            //artwork.activateKW.ClearValue(SurfaceButton.BackgroundProperty);
            artwork.goBack();
        }

        public void Hide()
        {
            topScatter.Visibility = Visibility.Hidden;
            hudScatter.Visibility = Visibility.Hidden;
            searchButton.Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            topScatter.Visibility = Visibility.Visible;
            hudScatter.Visibility = Visibility.Visible;
            searchButton.Visibility = Visibility.Visible;
        }

        private void ScatterViewCenterChanged(Object sender, EventArgs e)
        {
            if (sizeChanged)
            {
                sizeChanged = false;
                return;
            }

            ScatterViewItem currentScatter = sender as ScatterViewItem;
            double x = currentScatter.Center.X;
            double y = currentScatter.Center.Y;

            if (x < (centerStartX - wDiff/2) - 300)
                x = centerStartX - wDiff / 2 - 300;
            else if (x > (centerStartX + wDiff/2) + 300)
                x = centerStartX + wDiff/2 + 300;

            if (y < (centerStartY - hDiff/2)- 300)
                y = centerStartY - hDiff/2 - 300;
            else if (y > (centerStartY + hDiff/2) + 300)
                y = centerStartY + hDiff/2 + 300;

            currentScatter.Center = new Point(x, y);
        }

        private void createSearchButton()
        {
            searchButton.Content = "Search";
            searchButton.Width = 100;
            searchButton.Height = 50;
            searchButton.Click += new RoutedEventHandler(searchButton_Click);
            topCanvas.Children.Add(searchButton);
            Canvas.SetLeft(searchButton, canvasW - searchButton.Width);
            Canvas.SetTop(searchButton, 0);
        }

        private void createSearchBox()
        {
            //grp = WebXMLReader.LoadFromXML(@"C:\LADS Compiled\GCNav\Knowledge Web\Knowledge Web\Collection2.xml", @"C:\LADS Compiled\GCNav\GCNav\bin\Debug\Data\Images\");
            grp = WebXMLReader.LoadFromXML(@"Data\NewCollection.xml", @"Data\Images\", @"Metadata\");

            bx = new FloatingSearchBox(grp, this);

            searchBox.Content = bx;
            searchBox.Height = 451;
            searchBox.Width = 622;
            searchBox.Center = new Point(canvasW - searchBox.Width / 2, 300);
            searchBox.Orientation = 0;
        }

        void toggleSearchOn()
        {
            if (!searchBoxPresent)
            {
                hudScatter.Items.Add(searchBox);
                searchBoxPresent = true;
            }
            
        }

        void toggleSearchOff()
        {
            if (searchBoxPresent)
            {
                hudScatter.Items.Remove(searchBox);
                searchBoxPresent = false;
            }
        }

        void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!searchBoxPresent)
            {
                hudScatter.Items.Add(searchBox);
                searchBoxPresent = true;
            }
            else
            {
                hudScatter.Items.Remove(searchBox);
                searchBoxPresent = false;
            }
        }

        public void addGroup(String filePath)
        {
            char[] delim = { '\\' ,'/'};
            String[] path = filePath.Split(delim);
            foreach (WebGroup i in bx.GroupList)
            {
                String[] otherPath = i.Filename.Split(delim);
                if (otherPath[otherPath.Length-1].Equals(path[path.Length-1]))
                {
                    addGroup(i);
                    break;
                }
            }
        }

        public void BindLineToScatterViewItems(Line line, ScatterViewItem origin, ScatterViewItem destination)
        {
            // Bind line.(X1,Y1) to origin.ActualCenter
            BindingOperations.SetBinding(line, Line.X1Property, new Binding
            {
                Source = origin,
                Path = new PropertyPath("ActualCenter.X")
            });

            BindingOperations.SetBinding(line, Line.Y1Property, new Binding
            {
                Source = origin,
                Path = new PropertyPath("ActualCenter.Y")
            });
            // Bind line.(X2,Y2) to destination.ActualCenter  
            BindingOperations.SetBinding(line, Line.X2Property, new Binding
            {
                Source = destination,
                Path = new PropertyPath("ActualCenter.X")
            });
            BindingOperations.SetBinding(line, Line.Y2Property, new Binding
            {
                Source = destination,
                Path = new PropertyPath("ActualCenter.Y")
            });
        }

        void moveLayer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (curveTimer != null)
            {
                curveTimer.Stop();
                count = 0;
                curveTimer = null;
            }

            sizeChanged = true;
            ScatterViewItem currentScatter = sender as ScatterViewItem;

            if (currentScatter.Height < screenH)
            {
                currentScatter.Height = screenH;
                currentScatter.Width = screenW;
                e.Handled = true;
                return;
            }
            double sizeNow = currentScatter.Height * currentScatter.Width;

            scaleX = currentScatter.Width / oldX;
            scaleY = currentScatter.Height / oldY;

            canvasH *= scaleY;
            canvasW *= scaleX;

            oldX = currentScatter.Width;
            oldY = currentScatter.Height;

            scaleFactor = sizeNow / currentSize;
            currentSize = sizeNow;

            sides *= scaleX ;

            hDiff = currentScatter.Height - screenH;
            wDiff = currentScatter.Width - screenW;

            foreach (SingleWeb i in vertexList)
            {
                i.setSide(scaleX, scaleY);
            }

            r.Height = currentScatter.Height;
            r.Width = currentScatter.Width;
        }

        public void addGroup(WebGroup grp)
        {
            Image mainArtwork = new Image();
            mainArtwork.Source = new BitmapImage(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + @grp.Filename, UriKind.Absolute));
            int groups = 0;
            List<Image> groupThumb = new List<Image>();

            List<Image> temp = null;
            if ((temp = grp.getGroup("A")).Count > 0)
            {
                groups++;
                groupThumb.Add(temp[0]);
            }

            temp = null;
            if ((temp = grp.getGroup("B")).Count > 0)
            {
                groups++;
                groupThumb.Add(temp[0]);
            }

            temp = null;
            if ((temp = grp.getGroup("C")).Count > 0)
            {
                groups++;
                groupThumb.Add(temp[0]);
            }

            temp = null;
            if ((temp = grp.getGroup("D")).Count > 0)
            {
                groups++;
                groupThumb.Add(temp[0]);
            }

            bool exists = false;

            if (vertexList.Count > 0)
            {
                foreach (SingleWeb i in vertexList)
                {
                    if (i.file.Equals(grp.Filename))
                    {
                        exists = true;
                        break;
                    }
                }
            }
            
            if(!exists)
            {
                Random rnd = new Random();
                int h = 0;
                int w = 0;

                h = (int)(centerStartY - moveLayer.Center.Y) + (int)(moveLayer.Height/2);
                w = (int)(centerStartX - moveLayer.Center.X) + (int)(moveLayer.Width/2);

                bool check = false;

                while(!check)
                {
                    bool cont = false;

                    foreach (SingleWeb i in vertexList)
                    {
                        if (i.centerX - sides / 2 <= w && w <= i.centerX + sides / 2 &&
                            i.centerY - sides / 2 <= h && h <= i.centerY + sides / 2)
                        {
                            cont = true;
                            break;
                        }
                    }

                    if (cont)
                    {
                        w = (int)(centerStartX - moveLayer.Center.X) + (int)(moveLayer.Width / 2) - (int)(screenW / 2) + (int)(screenW * 0.1) + rnd.Next((int)(screenW * 0.6));
                        h = (int)(centerStartY - moveLayer.Center.Y) + (int)(moveLayer.Height / 2) - (int)(screenH/2) + (int)(screenH * 0.1) + rnd.Next((int)(screenH * 0.6));
                        continue;
                    }
                    check = true;
                }

                SingleWeb current = new SingleWeb(innerLayer, w, h, currentIndex++, sides, groupThumb, mainArtwork, groups, lineCanvas, grp.Filename, this, grp.Filename, grp, artwork);
                vertexList.Add(current);

                if (vertexList.Count > 1)
                {
                    for (int i = 0; i < currentIndex - 1; i++)
                    {
                        if (grp.hasAnyKeywordOf(vertexList[i].webGroup.Keywords()))
                        {
                            Line newLine = new Line();
                            newLine.StrokeThickness = 1;
                            newLine.Stroke = Brushes.Black;

                            BindLineToScatterViewItems(newLine, vertexList[i].mainArtwork, current.mainArtwork);
                            lineCanvas.Children.Add(newLine);
                        }
                    }
                }
            }
        }
    }
}
