using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Microsoft.Surface.Presentation.Controls;

namespace LADSArtworkMode.TourEvents
{
    class FadeOutPathEvent : TourEvent
    {
        public FadeOutPathEvent(SurfaceInkCanvas canvas, double durationParam)
        {
            type = TourEvent.Type.fadeOutPath;
            duration = durationParam;
            inkCanvas = canvas;
        }
        public override TourEvent copy()
        {
            return new FadeOutPathEvent(inkCanvas, duration);
        }
        public SurfaceInkCanvas inkCanvas { get; set; }
    }
}
