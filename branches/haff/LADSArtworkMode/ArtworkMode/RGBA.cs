using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode
{
    class RGBA
    {
        double m_r;

        public double R
        {
            get { return m_r; }
            set { m_r = value; }
        }

        double m_g;

        public double G
        {
            get { return m_g; }
            set { m_g = value; }
        }
        double m_b;

        public double B
        {
            get { return m_b; }
            set { m_b = value; }
        }
        double m_a;

        public double A
        {
            get { return m_a; }
            set { m_a = value; }
        }

        public RGBA()
        {
            m_a = 255;
            m_r = 0;
            m_g = 0;
            m_b = 0;
        }

        public RGBA(double r, double g, double b, double a)
        {
            m_a = a;
            m_r = r;
            m_g = g;
            m_b = b;
        }
    }
}
