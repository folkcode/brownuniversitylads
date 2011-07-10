using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;

namespace LADSArtworkMode
{
    public class Hotspot
    {
        String name;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        String ArtworkName;
        public String artworkName
        {
            get { return ArtworkName; }
            set{ ArtworkName = value;}
        }

        double positionX;
        public double PositionX
        {
            get { return positionX; }
            set { positionX = value; }
        }

        double positionY;
        public double PositionY
        {
            get { return positionY; }
            set { positionY = value; }
        }

        String description;
        public String Description
        {
            get { return description; }
            set { description = value; }
        }

        XmlNode m_xmlNode;

        public XmlNode XmlNode
        {
            get { return m_xmlNode; }
            set { m_xmlNode = value; }
        }

        String m_type; // image or text

        public String Type
        {
            get { return m_type; }
            set { m_type = value; }
        }



    }
}
