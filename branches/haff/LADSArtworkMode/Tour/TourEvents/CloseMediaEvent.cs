using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    public class CloseMediaEvent :  TourEvent
    {
        public CloseMediaEvent(DockableItem mediaParam)
        {
            type = TourEvent.Type.closeMedia;
            media = mediaParam;
        }

        public DockableItem media
        {
            get;
            set;
        }
    }
}
