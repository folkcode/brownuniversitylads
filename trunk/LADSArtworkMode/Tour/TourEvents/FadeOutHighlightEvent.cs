using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;

namespace LADSArtworkMode.TourEvents
{
    class FadeOutHighlightEvent : TourEvent
    {
        public FadeOutHighlightEvent(SurfaceInkCanvas canvas, double durationParam, double opacity)
        {
            type = TourEvent.Type.fadeOutHighlight;
            duration = durationParam;
            inkCanvas = canvas;
            this.opacity = opacity;
        }
        public override TourEvent copy()
        {
            return new FadeOutHighlightEvent(inkCanvas, duration, opacity);
        }
        public double opacity { get; set; }
        public SurfaceInkCanvas inkCanvas { get; set; }
    }
}
