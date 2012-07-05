using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace LADSArtworkMode.TourEvents
{
    class FadeOutInkEvent : TourEvent
    {
        public FadeOutInkEvent(InkCanvas canvas, double durationParam)
        {
            type = TourEvent.Type.fadeOutInk;
            duration = durationParam;
        }

        public InkCanvas inkCanvas { get; set; }
    }
}
