using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// FadeOutMediEvent - subclass of TourEvent
    /// </summary>
    public class FadeOutMediaEvent : TourEvent
    {
        public FadeOutMediaEvent(DockableItem mediaParam, double durationParam)
        {
            type = TourEvent.Type.fadeOutMedia;
            media = mediaParam;
            duration = durationParam;
        }

        public override TourEvent copy()
        {
            return new FadeOutMediaEvent(media, duration);
        }

        public DockableItem media
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
