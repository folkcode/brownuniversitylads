using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// PanMediaEvent - subclass of TourEvent
    /// </summary>
    public class PanMediaEvent :  TourEvent
    {
        public PanMediaEvent(DockableItem mediaParam, double panMediaToMSIPointXParam, double panMediaToMSIPointYParam, double durationParam)
        {
            type = TourEvent.Type.panMedia;
            media = mediaParam;

            panMediaToMSIPointX = panMediaToMSIPointXParam;
            panMediaToMSIPointY = panMediaToMSIPointYParam;
            duration = durationParam;
        }

        public DockableItem media
        {
            get;
            set;
        }

        public double panMediaToMSIPointX
        {
            get;
            set;
        }

        public double panMediaToMSIPointY
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
