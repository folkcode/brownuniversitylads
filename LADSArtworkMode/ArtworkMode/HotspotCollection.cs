using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Controls;
using DeepZoom.Controls;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace LADSArtworkMode
{
    /// <summary>
    /// Hold and nvigate through hotspots.
    /// </summary>
    public class HotspotCollection
    {
        XmlNodeList m_hotspotList;
        //XmlNodeList m_name;
        // XmlNodeList m_positionX;
        // XmlNodeList m_positionY;
        // XmlNodeList m_description;
        HotspotDetailsControl[] m_hotspotDetails;


        public HotspotDetailsControl[] HotspotDetails
        {
            get { return m_hotspotDetails; }
            set { m_hotspotDetails = value; }
        }

        HotspotIconControl[] m_hotspotIcons;

        public void removeHotspotIcons()
        {
            if (HotspotIcons == null) return;
            foreach (HotspotIconControl icon in HotspotIcons)
            {
                if (icon != null)
                {
                    icon.DetailControl.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void reAddHotspotIcons()
        {
            if (HotspotIcons == null) return;
            foreach (HotspotIconControl icon in HotspotIcons)
            {
                if (icon != null)
                {
                    icon.DetailControl.Visibility = Visibility.Visible;
                }
            }
        }

        public HotspotIconControl[] HotspotIcons
        {
            get { return m_hotspotIcons; }
            set { m_hotspotIcons = value; }
        }
        Hotspot[] m_hotspots;

        public Hotspot[] Hotspots
        {
            get { return m_hotspots; }
            set { m_hotspots = value; }
        }

        Boolean[] m_isSelected; // maintain a list of selected hotspots to be loaded into the listbox

        public Boolean[] IsSelected
        {
            get { return m_isSelected; }
            set { m_isSelected = value; }
        }

        Boolean[] m_isOnScreen;

        public Boolean[] IsOnScreen
        {
            get { return m_isOnScreen; }
            set { m_isOnScreen = value; }
        }

        public HotspotCollection()
        {
           // m_model = new HotspotModel();
        }

        /// <summary>
        /// Read the hotspot XML file and save to m_hotspots.
        /// </summary>
        public bool loadDocument(String filename)
        {
            //XmlNodeList hotspotList;
            XmlDocument doc = new XmlDocument();
            try
            {
                try
                {
                    doc.Load(filename);
                }
                catch (Exception e)
                {
                    
                    return false;
                }
                m_hotspotList = doc.SelectNodes("//hotspot");
                //MessageBox.Show(m_hotspotList[0].ChildNodes[0].InnerText);
                m_hotspotIcons = new HotspotIconControl[m_hotspotList.Count];
                m_hotspotDetails = new HotspotDetailsControl[m_hotspotList.Count];
                m_hotspots = new Hotspot[m_hotspotList.Count] ;
                m_isSelected = new Boolean [m_hotspotList.Count];
                m_isOnScreen = new Boolean[m_hotspotList.Count];

                for (int i = 0; i < m_hotspots.Length; i++)
                {
                    m_hotspots[i] = new Hotspot();
                    m_hotspots[i].Name = m_hotspotList[i].ChildNodes[0].InnerText;
                    m_hotspots[i].PositionX = (double)Convert.ToDouble(m_hotspotList[i].ChildNodes[1].InnerText);
                    m_hotspots[i].PositionY = (double)Convert.ToDouble(m_hotspotList[i].ChildNodes[2].InnerText);
                    m_hotspots[i].Type = m_hotspotList[i].ChildNodes[3].InnerText;
                    m_hotspots[i].Description = m_hotspotList[i].ChildNodes[4].InnerText;
                   // Console.WriteLine(m_hotspotList[i].ChildNodes[4].InnerText);
                    m_hotspots[i].XmlNode = m_hotspotList[i];
                    m_isSelected[i] = true;
                    m_isOnScreen[i] = false;
                    //Console.Out.WriteLine("filename" + filename);
                    String fileName = this.getArtworkName(filename);
                    m_hotspots[i].artworkName = fileName;
                }
            }
            catch (Exception ex)
            {
                return false;
                MessageBox.Show(ex.ToString());
            }
            return true;

        }

        public String getArtworkName(String filename)
        {
            String[] str = Regex.Split(filename, "XMLFiles"); //this is to get the artWork name for the hotspot
            String fileName = str[1].Substring(1,str[1].Length - 5);
            Console.Out.WriteLine(fileName);
            return fileName;
        }

        /// <summary>
        /// Display a hotspot icon on screen.
        /// </summary>
        public void loadHotspotIcon(int index, Canvas canvasParent, ScatterView scatterViewParent, MultiScaleImage msi)
        {
            if (index >= 0 && index < m_hotspotIcons.Length)
            {
                m_hotspotIcons[index] = new HotspotIconControl(canvasParent, scatterViewParent, m_hotspots[index], msi);
                m_hotspotIcons[index].displayOnScreen(msi);
                m_isOnScreen[index] = true;

                m_hotspotDetails[index] = m_hotspotIcons[index].DetailControl; // jcchin - key line that was missing
            }
        }

        /// <summary>
        /// Display all hotspot icons on screen.
        /// </summary>
        public void loadAllHotspotsIcon(Canvas canvasParent, ScatterView scatterViewParent, MultiScaleImage msi)
        {
            if (m_hotspots != null)
            {
                for (int i = 0; i < m_hotspots.Length; i++)
                {
                    if (m_isSelected[i] == true)
                        loadHotspotIcon(i, canvasParent, scatterViewParent, msi);

                }
            }
        }

        /// <summary>
        /// Specify a hotspot location with respect to its artwork.
        /// </summary>
        public void updateHotspotLocation(int index, Canvas canvasParent, ScatterView scatterViewParent, MultiScaleImage msi) // new - jcchin
        {
            if (index >= 0 && index < m_hotspotIcons.Length)
            {
                if (m_isOnScreen[index] == true)
                {
                    m_hotspotIcons[index].updateScreenLocation(msi);

                    if (m_hotspotDetails[index].IsOnScreen == true)
                    {
                        m_hotspotDetails[index].updateScreenLocation(msi);
                    }
                }
            }
        }

        /// <summary>
        /// Specify all hotspots locations with respect to its artwork.
        /// </summary>
        public void updateHotspotLocations(Canvas canvasParent, ScatterView scatterViewParent, MultiScaleImage msi) // new - jcchin
        {
            if (m_hotspots != null)
            {
                for (int i = 0; i < m_hotspots.Length; i++)
                {
                    updateHotspotLocation(i, canvasParent, scatterViewParent, msi);
                }
            }
        }


        /// <summary>
        /// Remove a hotspot at position 'index' from screen
        /// </summary>
        public void unloadHotspotIcon(int index)
        {
            if (m_hotspots != null)
            {
                if (index >= 0 && index < m_hotspotIcons.Length)
                {
                    if (m_isOnScreen[index] == true)
                    {
                        m_hotspotIcons[index].removeFromScreen();
                        m_isOnScreen[index] = false;
                    }
                }
            }
        }

        /// <summary>
        /// Remove all hotspots from screen.
        /// </summary>
        public void unloadAllHotspotsIcon()
        {
            if (m_hotspots != null)
            {
                for (int i = 0; i < m_hotspots.Length; i++)
                {
                    if (m_isOnScreen[i] == true)
                        m_hotspotIcons[i].removeFromScreen();
                }
            }
        }


        /// <summary>
        /// Search the hotspot collection for hotspots with name corresponding to the provided keywords.
        /// </summary>
        public void search(String keyword)
        {
            if (keyword =="" || keyword == null)
            {
                if (m_isSelected == null) return;
                for (int i = 0; i < m_isSelected.Length; i++)
                {
                    m_isSelected[i] = true;
                }
            }
            for (int i = 0; i < m_isSelected.Length; i++)
            {
                if (m_isSelected == null) return;
                m_isSelected[i] = m_hotspots[i].Name.ToLower().Contains(keyword);
            }
        }
    }

}
