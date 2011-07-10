using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    public class PanArtEvent :  TourEvent
    {
        public PanArtEvent(double panToArtworkPointXParam, double panToArtworkPointYParam, double durationParam)
        {
            type = TourEvent.Type.panArt;

            panToArtworkPointX = panToArtworkPointXParam;
            panToArtworkPointY = panToArtworkPointYParam;
            duration = durationParam;
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
