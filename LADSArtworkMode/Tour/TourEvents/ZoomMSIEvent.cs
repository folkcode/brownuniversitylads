using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepZoom.Controls;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// ZoomMSIEvent - subclass of TourEvent
    /// </summary>
    public class ZoomMSIEvent : TourEvent
    {
        public ZoomMSIEvent(MultiScaleImage msiParam, double absoluteScaleParam, double zoomToMSIPointXParam, double zoomToMSIPointYParam, double durationParam)
        {
            type = TourEvent.Type.zoomMSI;

            msi = msiParam;
            absoluteScale = absoluteScaleParam;
            zoomToMSIPointX = zoomToMSIPointXParam;
            zoomToMSIPointY = zoomToMSIPointYParam;
            duration = durationParam;
        }
        public override TourEvent copy()
        {
            return new ZoomMSIEvent(msi, absoluteScale, zoomToMSIPointX, zoomToMSIPointY, duration);
        }

        public MultiScaleImage msi
        {
            get;
            set;
        }

        public double absoluteScale
        {
            get;
            set;
        }

        public double zoomToMSIPointX
        {
            get;
            set;
        }

        public double zoomToMSIPointY
        {
            get;
            set;
        }

        /*public double duration
        {
            get;
            set;
        }*/
    }
}
