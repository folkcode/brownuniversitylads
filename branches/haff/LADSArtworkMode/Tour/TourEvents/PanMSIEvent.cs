using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepZoom.Controls;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// (NO LONGER IN USE - ZoomMSIEvent takes care of this) PanMSIEvent - subclass of TourEvent
    /// </summary>
    public class PanMSIEvent :  TourEvent
    {
        public PanMSIEvent(MultiScaleImage msiParam, double panToMSIPointXParam, double panToMSIPointYParam, double durationParam)
        {
            type = TourEvent.Type.panMSI;

            msi = msiParam;
            panToArtworkPointX = panToMSIPointXParam;
            panToArtworkPointY = panToMSIPointYParam;
            duration = durationParam;
        }

        public MultiScaleImage msi
        {
            get;
            set;
        }

        public double panToArtworkPointX
        {
            get;
            set;
        }

        public double panToArtworkPointY
        {
            get;
            set;
        }

        public double duration
        {
            get;
            set;
        }
    }
}
