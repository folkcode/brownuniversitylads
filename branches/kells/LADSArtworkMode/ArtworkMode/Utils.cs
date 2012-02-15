using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode
{
    class Utils
    {
        static public double max(double a, double b, double c)
        {
            if (a < b)
            {
                if (b < c) return c;
                return b;
            }
            else
            {
                if (a < c) return c;
                return a;
            }
        }

        static public double min(double a, double b, double c)
        {
            if (a < b)
            {
                if (a < c) return a;
                return c;
            }
            else // a >= b
            {
                if (b > c) return c;
                return b ;
            }
        }
    }
}
