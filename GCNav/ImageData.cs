using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;

namespace GCNav
{
    /// <summary>
    /// straightforward image class stores all the necessary information of an image, created when the collection is loaded
    /// </summary>
    public class ImageData : Image
    {
        /*a list of buttons containing location information for the image, used on the map*/
        private List<MapControl.MapButton> _locButtons;
        private List<newMap.newMapButton> _newlocButtons;
        private List<String> _locButtonInfo;
        public List<String> keywords;
        public int year {get;set;}
        public String artist {get;set;}
        public String medium {get;set;}
        public String title {get;set;}
        public String xmlpath { get; set; }

        /*seperate file for thumbnail, don't want to load the real high resolution image for thumbnail, just in case it eats up all the memory*/
        public String thumbpath { get; set; }
        /*use to pass the filename of the image to artwork mode. Ex: "gari0001.bmp", "gari0043.bmp"*/
        public String filename { get; set; }

        public ImageData(String path)
        {
            BitmapImage myImage = new BitmapImage();
            myImage.BeginInit();
            myImage.UriSource = new Uri(path,UriKind.Relative);
            myImage.EndInit();

            _locButtons = new List<MapControl.MapButton>(); //mapButtons for the old mapControl
            _newlocButtons = new List<newMap.newMapButton>(); //mapButtons for new mapControl
            _locButtonInfo = new List<String>();
     
        
            this.Source = myImage;
            this.Height = 45;
            this.Stretch = Stretch.Uniform;

            this.PreviewTouchDown += new EventHandler<TouchEventArgs>(ImageData_TapGestureHandler);
            this.MouseDown += new MouseButtonEventHandler(ImageData_TapGestureHandler);
        }
        public void setLocButtonInfo(String info)
        {
            _locButtonInfo.Add(info);
        }
        public List<String> getLocButtonInfo()
        {
            return _locButtonInfo;
        }

        public void addButton(MapControl.MapButton button)
        {
            _locButtons.Add(button);
            
        }
        public void addButton(newMap.newMapButton button)
        {
            _newlocButtons.Add(button);
        }

        public List<MapControl.MapButton> getLocButtons()
        {
            return _locButtons;
        }
        public List<newMap.newMapButton> getnewButtons()
        {
            return _newlocButtons;
        }

        public void setSize(int height)
        {
            this.Height = height;
            this.Width = height / this.Source.Height * this.Source.Width;
        }

        public void setSizebyWidth(int width)
        {
            this.Width = width;
            this.Height = width / this.Source.Width * this.Source.Height;
        }

        public void setSizeByPercent(double zoomPercent)
        {
            this.Height = this.Height * zoomPercent;
            this.Width = this.Width * zoomPercent;
        }

        private void ImageData_TapGestureHandler(object sender, EventArgs e)
        {
            _parent.imageSelected(this);
        }

        private Navigator _parent;
        public void setParent(Navigator nav)
        {
            _parent = nav;
        }

        public Navigator getParent()
        {
            return _parent;
        }

        private ImageCluster _cluster;
        public void setCluster(ImageCluster c)
        {
            _cluster = c;
        }

        public ImageCluster getCluster()
        {
            return _cluster;
        }

    }
}
