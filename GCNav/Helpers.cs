using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;

namespace GCNav
{
    public class Helpers
    {
        public delegate void MapEventHandler(Object Sender, MapEventArgs e);
        public delegate void ImageLoadedHandler(Object sender, ImageLoadedEventArgs e);
        public delegate void ImageSelectedHandler(Object sender, ImageSelectedEventArgs e);
        
        public static DoubleAnimation makeDoubleAnimation(double from, double to, double seconds)
        {
            DoubleAnimation myAnimation = new DoubleAnimation();
            myAnimation.From = from;
            myAnimation.To = to;
            myAnimation.Duration = new Duration(TimeSpan.FromSeconds(seconds));
            return myAnimation;
        }

        public class MapEventArgs : EventArgs
        {
            private List<ImageData> images;
            public MapEventArgs(List<ImageData> i)
            {
                images = i;
            }
            public List<ImageData> getImages()
            {
                return images;
            }
        }

        public class ImageLoadedEventArgs : EventArgs
        {
            private ImageData data;
            public ImageLoadedEventArgs(ImageData i)
            {
                data = i;
            }
            public ImageData getImage()
            {
                return data;
            }
        }

        public class ImageSelectedEventArgs : EventArgs
        {
            private ImageData data;
            public ImageSelectedEventArgs(ImageData i)
            {
                data = i;
            }
            public ImageData getImage()
            {
                return data;
            }
        }
    }
}
