using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// FadeInMediaEvent - subclass of TourEvent
    /// </summary>
    public class FadeInMediaEvent : TourEvent
    {
        public FadeInMediaEvent(DockableItem mediaParam, double fadeInMediaToScreenPointXParam, double fadeInMediaToScreenPointYParam, double absoluteScaleParam, double durationParam)
        {
            type = TourEvent.Type.fadeInMedia;
            media = mediaParam;

            fadeInMediaToScreenPointX = fadeInMediaToScreenPointXParam;
            fadeInMediaToScreenPointY = fadeInMediaToScreenPointYParam;
            absoluteScale = absoluteScaleParam;
            duration = durationParam;
        }

        public override TourEvent copy()
        {
            return new FadeInMediaEvent(media, fadeInMediaToScreenPointX, fadeInMediaToScreenPointY, absoluteScale, duration);
        }

        public DockableItem media
        {
            get;
            set;
        }

        public double fadeInMediaToScreenPointX
        {
            get;
            set;
        }

        public double fadeInMediaToScreenPointY
        {
            get;
            set;
        }

        public double absoluteScale
        {
            get;
            set;
        }
    }
}
