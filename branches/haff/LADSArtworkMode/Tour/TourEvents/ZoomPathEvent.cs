using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;

namespace LADSArtworkMode.TourEvents
{
    class ZoomPathEvent : TourEvent
    {
        public ZoomPathEvent(SurfaceInkCanvas canvas, double absoluteScaleParam, double zoomToArtworkPointXParam, double zoomToArtworkPointYParam, double durationParam)
        {
            type = TourEvent.Type.zoomPath;
            absoluteScale = absoluteScaleParam;
            zoomToArtworkPointX = zoomToArtworkPointXParam;
            zoomToArtworkPointY = zoomToArtworkPointYParam;
            duration = durationParam;
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
