using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.IO;

namespace DeepZoom.Controls
{
    class Drawing
    {
        public Drawing()
        {
        }

        public static Byte[] DrawImage(Byte[] input, int w, int h, int bit, int value)
        {
            double adj = ((double) value / (100.0 *3)) * 255;
            Byte[] pixels = new Byte[input.Length];
            int r, g, b;
             for (int x = 0; x < w; x++)
             {
                 for (int y = 0; y < h; y++)
                 {
                     int index = (x + y * w) * bit / 8;
                     r = ((int)input[index + 0] + (int)adj);
                     g = ((int)input[index + 1] + (int)adj);
                     b = ((int)input[index + 2] + (int)adj);
                     //pixels[index + 3] = (byte)255;

                     if (value > 0) // lightening
                     {

                         if (r > 255)
                             r = 255;
                         if (g > 255)
                             g = 255;
                         if (b > 255)
                             b = 255;
                     }
                     else
                     {
                         if (r < 0)
                             r = 0;
                         if (g < 0)
                             g = 0;
                         if (b < 0)
                             b = 0;
                     }

                     pixels[index + 0] = (byte)r;
                     pixels[index + 1] = (byte)g;
                     pixels[index + 2] = (byte)b;


                 }
             }
             return pixels;
        }
    }
}
