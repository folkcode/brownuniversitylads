using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace LADSArtworkMode.TourEvents
{
    class FadeInInkEvent : TourEvent
    {
        public FadeInInkEvent(InkCanvas canvas, double durationParam)
        {
            type = TourEvent.Type.fadeInInk;
            duration = durationParam;
        }

        public InkCanvas inkCanvas { get; set; }
    }
}
