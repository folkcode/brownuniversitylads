using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    public class ZoomArtEvent : TourEvent
    {
        public ZoomArtEvent(double absoluteScaleParam, double zoomToArtworkPointXParam, double zoomToArtworkPointYParam, double durationParam)
        {
            type = TourEvent.Type.zoomArt;

            absoluteScale = absoluteScaleParam;
            zoomToArtworkPointX = zoomToArtworkPointXParam;
            zoomToArtworkPointY = zoomToArtworkPointYParam;
            duration = durationParam;
        }

        public double absoluteScale
        {
            get;
            set;
        }

        public double zoomToArtworkPointX
        {
            get;
            set;
        }

        public double zoomToArtworkPointY
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
