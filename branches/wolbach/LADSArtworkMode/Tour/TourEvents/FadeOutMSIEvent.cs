using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepZoom.Controls;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// (NOT IN USE - at least for now) FadeOutMSIEvent - subclass of TourEvent
    /// </summary>
    public class FadeOutMSIEvent :  TourEvent
    {
        public FadeOutMSIEvent(MultiScaleImage msiParam, double durationParam)
        {
            type = TourEvent.Type.fadeOutMSI;
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
