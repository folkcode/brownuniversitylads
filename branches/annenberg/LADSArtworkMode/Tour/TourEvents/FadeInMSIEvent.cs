using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepZoom.Controls;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// (NOT IN USE - at least for now) FadeInMSIEvent - subclass of TourEvent
    /// </summary>
    public class FadeInMSIEvent :  TourEvent
    {
        public FadeInMSIEvent(MultiScaleImage msiParam, double durationParam)
        {
            type = TourEvent.Type.fadeInMSI;
            msi = msiParam;

            duration = durationParam;
        }

        public MultiScaleImage msi
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
