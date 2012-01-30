using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode
{
    class HSV
    {

        double m_h;
        public double H
        {
            get { return m_h; }
            set { m_h = value; }
        }

        double m_s;
        public double S
        {
            get { return m_s; }
            set { m_s = value; }
        }

        double m_v;
        public double V
        {
            get { return m_v; }
            set { m_v = value; }
        }

        public HSV()
        {
            m_h = 0;
            m_s = 0;
            m_v = 0;
        }

        public HSV(double h, double s, double v)
        {
            m_h = h;
            m_s = s;
            m_v = v;
        }

    }
}
