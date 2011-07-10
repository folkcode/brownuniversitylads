using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    public abstract class TourEvent
    {
        public enum Type
        {
            zoomHighlight,
            fadeOutPath,
            fadeInPath,
            zoomPath,
            fadeOutHighlight,
            fadeInHighlight,
            closeMedia,
            openMedia,
            panArt,
            zoomArt,
            initMSI, // (NOT IN USE - can't set msi properties if the msi is not visible)
            fadeInMSI, // not in use
            fadeOutMSI, // not in use
            panMSI, // not in use (zoomMSI takes care of this)
            zoomMSI,
            initMedia,
            fadeInMedia,
            fadeOutMedia,
            panMedia, // not in use (zoomMedia takes care of this)
            zoomMedia,
        }

        public TourEvent.Type type
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
