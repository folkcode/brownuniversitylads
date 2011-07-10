using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// ZoomMediaEvent - subclass of TourEvent
    /// </summary>
    public class ZoomMediaEvent : TourEvent
    {
        public ZoomMediaEvent(DockableItem mediaParam, double absoluteScaleParam, double zoomMediaToScreenPointXParam, double zoomMediaToScreenPointYParam, double durationParam)
        {
            type = TourEvent.Type.zoomMedia;
            media = mediaParam;

            absoluteScale = absoluteScaleParam; // What does a ScaleTransform require?
            zoomMediaToScreenPointX = zoomMediaToScreenPointXParam;
            zoomMediaToScreenPointY = zoomMediaToScreenPointYParam;
            duration = durationParam;
        }
        public override TourEvent copy()
        {
            return new ZoomMediaEvent(media, absoluteScale, zoomMediaToScreenPointX, zoomMediaToScreenPointY, duration);
        }

        public DockableItem media
        {
            get;
            set;
        }

        public double absoluteScale
        {
            get;
            set;
        }

        public double zoomMediaToScreenPointX
        {
            get;
            set;
        }

        public double zoomMediaToScreenPointY
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
