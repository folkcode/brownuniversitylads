using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;

namespace LADSArtworkMode
{

    /// <summary>
    /// Provide functions to do image manipulation.
    /// </summary>
    class ImageProcessingTools
    {
        // red - green - blue color channels
        int m_imageWidth;

        public int ImageWidth
        {
            get { return m_imageWidth; }
            set { m_imageWidth = value; }
        }

        int m_imageHeight;

        public int ImageHeight
        {
            get { return m_imageHeight; }
            set { m_imageHeight = value; }
        }

        RGBA[] m_RGB;

        public RGBA[] RGB
        {
            get { return m_RGB; }
            set { m_RGB = value; }
        }

        // hue - saturation - brightness color channels
        HSV[] m_HSV;

        public HSV[] HSV
        {
            get { return m_HSV; }
            set { m_HSV = value; }
        }


        // hold the current brightness
        int m_currentBrightness;
        public int CurrentBrightness
        {
            get { return m_currentBrightness; }
            set { m_currentBrightness = value; }
        }

        // hold the current contrast
        int m_currentContrast;
        public int CurrentContrast
        {
            get { return m_currentContrast; }
            set { m_currentContrast = value; }
        }

        // hold the current saturation
        int m_currentSaturation;
        public int CurrentSaturation
        {
            get { return m_currentSaturation; }
            set { m_currentSaturation = value; }
        }

        Byte[] m_sourceBytes;

        public Byte[] SourceBytes
        {
            get { return m_sourceBytes; }
            set { m_sourceBytes = value; }
        }

        bool m_modified;

        public bool Modified
        {
            get { return m_modified; }
            set { m_modified = value; }
        }

        Byte[] m_modifiedBytes;

        public Byte[] ModifiedBytes
        {
            get { return m_modifiedBytes; }
            set { m_modifiedBytes = value; }
        }


        List<String> m_history;

        public List<String> History
        {
            get { return m_history; }
            set { m_history = value; }
        }

        /*  // the current artwork that uses the tool
          Artwork m_artwork;
          internal Artwork Artwork
          {
              get { return m_artwork; }
              set { m_artwork = value; }
          }*/

        /// <summary>
        /// Default constructor
        /// </summary>
        public ImageProcessingTools()
        {
            m_currentBrightness = 0;
            m_currentContrast = 0;
            m_currentSaturation = 0;
            m_modified = false;


        }

        /// <summary>
        /// adjust brightness.
        /// not used.
        /// </summary>
        public void adjustBrightness(int value)
        {
            double adjustment = (double)(value - m_currentBrightness) / (50.0 * 3);

            if (value == m_currentBrightness)
                return;
            for (int y = 0; y < m_imageHeight; y++)
            {
                for (int x = 0; x < m_imageWidth; x++)
                {
                    m_RGB[x + y * m_imageWidth].R += adjustment;
                    m_RGB[x + y * m_imageWidth].G += adjustment;
                    m_RGB[x + y * m_imageWidth].B += adjustment;

                    if (value > m_currentBrightness) // lightening
                    {

                        if (m_RGB[x + y * m_imageWidth].R > 1.0)
                            m_RGB[x + y * m_imageWidth].R = 1.0;
                        if (m_RGB[x + y * m_imageWidth].G > 1.0)
                            m_RGB[x + y * m_imageWidth].G = 1.0;
                        if (m_RGB[x + y * m_imageWidth].B > 1.0)
                            m_RGB[x + y * m_imageWidth].B = 1.0;
                    }
                    else
                    {
                        if (m_RGB[x + y * m_imageWidth].R < 0.0)
                            m_RGB[x + y * m_imageWidth].R = 0.0;
                        if (m_RGB[x + y * m_imageWidth].G < 0.0)
                            m_RGB[x + y * m_imageWidth].G = 0.0;
                        if (m_RGB[x + y * m_imageWidth].B < 0.0)
                            m_RGB[x + y * m_imageWidth].B = 0.0;
                    }


                }
            }

            m_currentBrightness = value;
        }

        /// <summary>
        /// adjust contrast.
        /// not used.
        /// </summary>
        public void adjustContrast(int value)
        {
            double r, g, b;
            if (value == m_currentContrast)
                return;

            double contrast = (double)Math.Pow((100.0 + value - m_currentContrast) / 100.0, 3);
            for (int y = 0; y < m_imageHeight; y++)
            {
                for (int x = 0; x < m_imageWidth; x++)
                {
                    r = m_RGB[x + y * m_imageWidth].R;
                    g = m_RGB[x + y * m_imageWidth].G;
                    b = m_RGB[x + y * m_imageWidth].B;
                    //Blue
                    //Converts Blue value to a value between 0 and 1
                    //Centers that value over 0 (value will be between -.5 and .5)
                    b -= 0.5f;
                    //Adjust the value by contrast (value will be between -127.5 and 127.5)
                    //(Value will usually be between -1 and 1)
                    b *= contrast;
                    //Value will be between -127 and 128
                    b += 0.5f;
                    //Clamp value
                    if (b > 1)
                        b = 1;
                    else if (b < 0)
                        b = 0;

                    //Green
                    g -= 0.5f;
                    g *= contrast;
                    g += 0.5f;
                    if (g > 1)
                        g = 1;
                    else if (g < 0)
                        g = 0;

                    //Red
                    r -= 0.5f;
                    r *= contrast;
                    r += 0.5f;
                    if (r > 1)
                        r = 1;
                    else if (r < 0)
                        r = 0;

                    m_RGB[x + y * m_imageWidth].R = r;
                    m_RGB[x + y * m_imageWidth].G = g;
                    m_RGB[x + y * m_imageWidth].B = b;
                }
            }
            m_currentContrast = value;
        }


        /// <summary>
        /// adjust saturation
        /// not used
        /// </summary>
        public void adjustSaturation(int value)
        {
            double adjustment = (double)(value - m_currentSaturation) / (50.0 * 2);

            m_HSV = RGBtoHSV(m_RGB);
            for (int x = 0; x < ImageWidth; x++)
            {
                for (int y = 0; y < ImageHeight; y++)
                {
                    m_HSV[x + y * ImageWidth].S += adjustment;
                    if (m_HSV[x + y * ImageWidth].S > 1.0)
                        m_HSV[x + y * ImageWidth].S = 1.0;
                    else if (m_HSV[x + y * ImageWidth].S < 0.0)
                        m_HSV[x + y * ImageWidth].S = 0.0;

                }
            }
            m_RGB = HSVtoRGB(m_HSV);
            m_currentSaturation = value;
        }



        /*public RGBA[] byteToRGBA(byte[] input, int width, int height)
        {
            RGBA[] output = new RGBA[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    try
                    {
                        output[x + y * width] = new RGBA();
                        output[x + y * width].B = (input[4 * x + y * (4 * width) + 0] / 255.0);
                        output[x + y * width].G = (input[4 * x + y * (4 * width) + 1] / 255.0);
                        output[x + y * width].R = (input[4 * x + y * (4 * width) + 2] / 255.0);
                        output[x + y * width].A = 255; // (input[4 * x + y * (4 * width) + 3]);
                        // For now, the tools are applied by capturing the current screen, manipulate the resulting image and save it onto a new overlay. 
                        // alpha channel is set to 255 to prevent "double layer" effect created by nonconsistency between the original image and the new overlay.
                        // ALPHA CHANNEL SHOULD BE SET TO THE INPUT ALPHA CHANNEL WHEN MANIPULATING THE ORIGINAL IMAGE
                    }
                    catch (NullReferenceException e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            return output;
        }

        public byte[] RGBAToByte(RGBA[] input)
        {
            byte[] output = new byte[input.Length * 4];
            for (int x = 0; x < ImageWidth; x++)
            {
                for (int y = 0; y < ImageHeight; y++)
                {
                    int inputIndex = (ImageWidth - 1 - x) + (ImageHeight - 1 - y) * ImageWidth;
                    int outputIndex = 4 * (x) + (y) * (4 * ImageWidth);
                    output[outputIndex + 2] = (byte)(input[inputIndex].R * 255.0);
                    output[outputIndex + 1] = (byte)(input[inputIndex].G * 255.0);
                    output[outputIndex + 0] = (byte)(input[inputIndex].B * 255.0);
                    output[outputIndex + 3] = (byte)(input[inputIndex].A);
                }
            }
            return output;
        }*/

        /// <summary>
        /// Change from RGB color to HSV color
        /// not used.
        /// </summary>
        public HSV[] RGBtoHSV(RGBA[] input)
        {
            HSV[] output = new HSV[input.Length];
            double min, max, delta;
            double r, g, b, h, s, v;

            for (int x = 0; x < ImageWidth; x++)
            {
                for (int y = 0; y < ImageHeight; y++)
                {
                    output[x + y * ImageWidth] = new HSV();
                    r = input[x + y * ImageWidth].R;
                    g = input[x + y * ImageWidth].G;
                    b = input[x + y * ImageWidth].B;

                    min = Utils.min(r, g, b);
                    max = Utils.max(r, g, b);
                    v = max;				// v

                    delta = max - min;

                    if (max != 0)
                    {
                        s = delta / max;		// s
                        if (r == max)
                            h = (g - b) / delta;		// between yellow & magenta
                        else if (g == max)
                            h = 2 + (b - r) / delta;	// between cyan & yellow
                        else
                            h = 4 + (r - g) / delta;	// between magenta & cyan
                        h *= 60;				// degrees
                        if (h < 0)
                            h += 360;
                    }
                    else
                    {
                        // r = g = b = 0		// s = 0, v is undefined
                        s = 0;
                        h = -1;
                    }

                    output[x + y * ImageWidth].H = h;
                    output[x + y * ImageWidth].S = s;
                    output[x + y * ImageWidth].V = v;

                }
            }
            return output;
        }


        /// <summary>
        /// change from HSV to RGB
        /// not used.
        /// </summary>
        public RGBA[] HSVtoRGB(HSV[] input)
        {
            RGBA[] output = new RGBA[input.Length];

            int i;
            double f, p, q, t;
            double h, s, v, r, g, b;

            for (int x = 0; x < ImageWidth; x++)
            {
                for (int y = 0; y < ImageHeight; y++)
                {
                    output[x + y * ImageWidth] = new RGBA();
                    h = input[x + y * ImageWidth].H;
                    s = input[x + y * ImageWidth].S;
                    v = input[x + y * ImageWidth].V;

                    r = g = b = 0;

                    if (s == 0)
                    {
                        // achromatic (grey)
                        r = g = b = v;

                    }
                    else
                    {

                        h /= 60.0;			// sector 0 to 5
                        i = (int)h;
                        f = h - i;			// factorial part of h
                        p = v * (1 - s);
                        q = v * (1 - s * f);
                        t = v * (1 - s * (1 - f));

                        switch (i)
                        {
                            case 0:
                                r = v;
                                g = t;
                                b = p;
                                break;
                            case 1:
                                r = q;
                                g = v;
                                b = p;
                                break;
                            case 2:
                                r = p;
                                g = v;
                                b = t;
                                break;
                            case 3:
                                r = p;
                                g = q;
                                b = v;
                                break;
                            case 4:
                                r = t;
                                g = p;
                                b = v;
                                break;
                            case 5:		// case 5:
                                r = v;
                                g = p;
                                b = q;
                                break;
                            case 6:
                                r = v;
                                g = p;
                                b = q;
                                break;
                        }
                    }
                    output[x + y * ImageWidth].R = r;
                    output[x + y * ImageWidth].G = g;
                    output[x + y * ImageWidth].B = b;
                    // output[x + y * ImageWidth].A = 255;
                }
            }
            return output;

        }

        /// <summary>
        /// adjust the Brightness from input 'source' and output a Byte array
        /// </summary>
        public Byte[] adjustBrightnessByteLevel(int value, Byte[] source)
        {
          //  if (value == m_currentBrightness)
              //  return source;

            double adjustment = (double)(value) / (100.0 * 3);
            adjustment *= (double)255;
            int r, g, b;
            //Byte[] result = new Byte[source.Length];
            Byte[] result = (Byte[]) source.Clone();
            for (int y = 0; y < m_imageHeight; y++)
            {
                for (int x = 0; x < m_imageWidth; x++)
                {

                    // int outputIndex = 4 * (ImageWidth - 1 - x) + 4 * (ImageHeight - 1 - y) * ImageWidth;
                    //int inputIndex = 4 * (x) + (y) * (4 * ImageWidth);
                    b = source[4 * x + y * (4 * m_imageWidth) + 0];
                    g = source[4 * x + y * (4 * m_imageWidth) + 1];
                    r = source[4 * x + y * (4 * m_imageWidth) + 2];

                    b += (int)adjustment;
                    g += (int)adjustment;
                    r += (int)adjustment;

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
                    result[4 * x + y * (4 * m_imageWidth) + 0] = (byte)b;
                    result[4 * x + y * (4 * m_imageWidth) + 1] = (byte)g;
                    result[4 * x + y * (4 * m_imageWidth) + 2] = (byte)r;
                }
            }


            m_currentBrightness = value;
            return result;
        }


        /// <summary>
        /// adjust the Contrast from input 'source' and output a Byte array
        /// </summary>
        public Byte[] adjustContrastByteLevel(int value, Byte[] source)
        {
            double r, g, b;
         //   if (value == m_currentContrast)
              //  return source;
            Byte[] result = (Byte[])source.Clone();
            double contrast = (double)Math.Pow((100 + value) / 100.0, 2);
            
            for (int y = 0; y < m_imageHeight; y++)
            {
                for (int x = 0; x < m_imageWidth; x++)
                {
                    r = source[4 * x + y * (4 * m_imageWidth) + 2] / 255.0;
                    g = source[4 * x + y * (4 * m_imageWidth) + 1] / 255.0;
                    b = source[4 * x + y * (4 * m_imageWidth) + 0] / 255.0;
                    //Blue
                    //Converts Blue value to a value between 0 and 1
                    //Centers that value over 0 (value will be between -.5 and .5)
                    b -= 0.5f;
                    //Adjust the value by contrast (value will be between -127.5 and 127.5)
                    //(Value will usually be between -1 and 1)
                    b *= contrast;
                    //Value will be between -127 and 128
                    b += 0.5f;
                    //Clamp value
                    if (b > 1)
                        b = 1;
                    else if (b < 0)
                        b = 0;

                    //Green
                    g -= 0.5f;
                    g *= contrast;
                    g += 0.5f;
                    if (g > 1)
                        g = 1;
                    else if (g < 0)
                        g = 0;

                    //Red
                    r -= 0.5f;
                    r *= contrast;
                    r += 0.5f;
                    if (r > 1)
                        r = 1;
                    else if (r < 0)
                        r = 0;

                    result[4 * x + y * (4 * m_imageWidth) + 2] = (byte)(r * 255);
                    result[4 * x + y * (4 * m_imageWidth) + 1] = (byte)(g * 255);
                    result[4 * x + y * (4 * m_imageWidth) + 0] = (byte)(b * 255);
                }
            }
            m_currentContrast = value;
            return result;
        }

        /// <summary>
        /// adjust the Saturation from input 'source' and output a Byte array
        /// </summary>
        public Byte[] adjustSaturationByteLevel(int value, Byte[] source)
        {
            //if (value == m_currentSaturation)
                //return source;
            double adjustment = (double)(value) / (100.0 * 2);
            Byte[] result = (Byte[])source.Clone();
            double min, max, delta;
            double r, g, b, h, s, v;
            int i;
            double f, p, q, t;

            for (int x = 0; x < ImageWidth; x++)
            {
                for (int y = 0; y < ImageHeight; y++)
                {
                            // change from RGB to HSV
                            r = source[4 * x + y * (4 * m_imageWidth) + 2] / 255.0;
                            g = source[4 * x + y * (4 * m_imageWidth) + 1] / 255.0;
                            b = source[4 * x + y * (4 * m_imageWidth) + 0] / 255.0;

                            min = Utils.min(r, g, b);
                            max = Utils.max(r, g, b);
                            v = max;				// v

                            delta = max - min;

                            if (max != 0)
                            {
                                s = delta / max;		// s
                                if (r == max)
                                    h = (g - b) / delta;		// between yellow & magenta
                                else if (g == max)
                                    h = 2 + (b - r) / delta;	// between cyan & yellow
                                else
                                    h = 4 + (r - g) / delta;	// between magenta & cyan
                                h *= 60;				// degrees
                                if (h < 0)
                                    h += 360;
                            }
                            else
                            {
                                // r = g = b = 0		// s = 0, v is undefined
                                s = 0;
                                h = -1;
                            }

                            // adjust saturation:
                            s += adjustment;
                            if (s > 1.0)
                                s = 1.0;
                            else if (s < 0.0)
                                s = 0.0;

                            // change back from HSV to RGB:
                            r = g = b = 0;

                            if (s == 0)
                            {
                                // achromatic (grey)
                                r = g = b = v;

                            }
                            else
                            {

                                h /= 60.0;			// sector 0 to 5
                                i = (int)h;
                                f = h - i;			// factorial part of h
                                p = v * (1 - s);
                                q = v * (1 - s * f);
                                t = v * (1 - s * (1 - f));

                                switch (i)
                                {
                                    case 0:
                                        r = v;
                                        g = t;
                                        b = p;
                                        break;
                                    case 1:
                                        r = q;
                                        g = v;
                                        b = p;
                                        break;
                                    case 2:
                                        r = p;
                                        g = v;
                                        b = t;
                                        break;
                                    case 3:
                                        r = p;
                                        g = q;
                                        b = v;
                                        break;
                                    case 4:
                                        r = t;
                                        g = p;
                                        b = v;
                                        break;
                                    case 5:		// case 5:
                                        r = v;
                                        g = p;
                                        b = q;
                                        break;
                                    case 6:
                                        r = v;
                                        g = p;
                                        b = q;
                                        break;
                                }
                            }


                            result[4 * x + y * (4 * m_imageWidth) + 2] = (byte)(r * 255.0);
                            result[4 * x + y * (4 * m_imageWidth) + 1] = (byte)(g * 255.0);
                            result[4 * x + y * (4 * m_imageWidth) + 0] = (byte)(b * 255.0);

                }
            }
            m_currentSaturation = value;
            return result;


        }



        /// <summary>
        /// Apply image manipulation tools to the artwork, based on the history of usage.
        /// </summary>
        public Byte[] applyFilter(String type, int BrightnessValue, int ContrastValue, int SaturationValue)
        {
            Byte[] result = (Byte[]) m_sourceBytes.Clone();
            Thread MulThread;
            if (m_history.Contains(type))
                m_history.Remove(type);
            m_history.Add(type);
            foreach (String ttype in m_history)
            {
                if (ttype.Equals("Brightness"))
                {
                    m_currentBrightness = 0;
                    result = adjustBrightnessByteLevel(BrightnessValue,result);
                }
                else if (ttype.Equals("Contrast"))
                {
                    m_currentContrast = 0;
                    result = adjustContrastByteLevel(ContrastValue,result);
                }
                else
                {
                    m_currentSaturation = 0;
                    result = adjustSaturationByteLevel(SaturationValue, result);
                

                }
            }
            //while (!MulThread.ThreadState.
            return result;

        }
    }

}
