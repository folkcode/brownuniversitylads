using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;

namespace LADSArtworkMode.TourEvents
{
    class FadeInHighlightEvent: TourEvent
    {
        public FadeInHighlightEvent(SurfaceInkCanvas canvas, double durationParam, double opacity)
        {
            type = TourEvent.Type.fadeInHighlight;
            duration = durationParam;
            inkCanvas = canvas;
            this.opacity = opacity;
        }

        public double opacity { get; set; }

        public SurfaceInkCanvas inkCanvas { get; set; }
    }
}
