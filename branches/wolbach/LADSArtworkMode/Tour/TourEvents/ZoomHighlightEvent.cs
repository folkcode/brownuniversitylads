using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;

namespace LADSArtworkMode.TourEvents
{
    class ZoomHighlightEvent : TourEvent
    {
        public ZoomHighlightEvent(SurfaceInkCanvas canvas, double absoluteScaleParam, double zoomToArtworkPointXParam, double zoomToArtworkPointYParam, double durationParam, double opacity)
        {
            type = TourEvent.Type.zoomHighlight;
            absoluteScale = absoluteScaleParam;
            zoomToArtworkPointX = zoomToArtworkPointXParam;
            zoomToArtworkPointY = zoomToArtworkPointYParam;
            duration = durationParam;
            this.opacity = opacity;
        }

        public double opacity
        {
            get;
            set;
        }

        public double absoluteScale
        {
            get;
            set;
        }

        public SurfaceInkCanvas inkCanvas { get; set; }

        public double zoomToArtworkPointX
        {
            get;
            set;
        }

        public double zoomToArtworkPointY
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
