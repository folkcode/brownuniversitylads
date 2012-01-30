using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    public class OpenMediaEvent :  TourEvent
    {
        public OpenMediaEvent(DockableItem mediaParam, double openMediaToScreenPointXParam, double openMediaToScreenPointYParam, double absoluteScaleParam)
        {
            type = TourEvent.Type.openMedia;
            media = mediaParam;

            openMediaToScreenPointX = openMediaToScreenPointXParam;
            openMediaToScreenPointY = openMediaToScreenPointYParam;
            absoluteScale = absoluteScaleParam;
        }

        public DockableItem media
        {
            get;
            set;
        }

        public double openMediaToScreenPointX
        {
            get;
            set;
        }

        public double openMediaToScreenPointY
        {
            get;
            set;
        }

        public double absoluteScale
        {
            get;
            set;
        }
    }
}
