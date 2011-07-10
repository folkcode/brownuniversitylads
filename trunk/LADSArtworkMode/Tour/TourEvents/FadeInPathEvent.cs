using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Microsoft.Surface.Presentation.Controls;

namespace LADSArtworkMode.TourEvents
{
    class FadeInPathEvent : TourEvent
    {
        public FadeInPathEvent(SurfaceInkCanvas canvas, double durationParam)
        {
            type = TourEvent.Type.fadeInPath;
            duration = durationParam;
            inkCanvas = canvas;
        }

        public SurfaceInkCanvas inkCanvas { get; set; }
    }
}
