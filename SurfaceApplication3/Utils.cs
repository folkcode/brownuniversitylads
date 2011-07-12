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
using System.IO;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Xml;
using System.Text.RegularExpressions;
using DeepZoom;


namespace SurfaceApplication3
{
    static class Utils
    {
        public static void setAspectRatio(Canvas canv, Rectangle rect, Image img, Image wpfImg, double thick)
        {
            // set the aspect ratio:

            Double height = wpfImg.Source.Height;
            Double width = wpfImg.Source.Width;

            Double ratio = img.Width / img.Height;
            Console.Out.WriteLine("ratio" + ratio);
            if (width / height > ratio)
            {
                img.Height = img.Width * height / width;
                rect.Height = img.Height + thick*2;
                double diff = (canv.Height - img.Height) / 2;
                Canvas.SetTop(img, diff);
                Canvas.SetTop(rect, diff - thick);
                Console.Out.WriteLine("height" + img.Height);

            }
            else
            {
                img.Width = img.Height * width / height;
                rect.Width = img.Width + thick*2;
                double diff = (canv.Width - img.Width)/2;
                Canvas.SetLeft(img, diff);
                Canvas.SetLeft(rect, diff - thick);
                Console.Out.WriteLine("width" + img.Width);
            }
        }
    }
}
