using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// (NO LONGER USED) InitMediaEvent - subclass of TourEvent
    /// </summary>
    public class InitMediaEvent :  TourEvent
    {
        public InitMediaEvent(DockableItem mediaParam, double initMediaToScreenPointXParam, double initMediaToScreenPointYParam, double absoluteScaleParam)
        {
            type = TourEvent.Type.initMedia;
            media = mediaParam;

            initMediaToScreenPointX = initMediaToScreenPointXParam;
            initMediaToScreenPointY = initMediaToScreenPointYParam;
            absoluteScale = absoluteScaleParam;
        }

        public DockableItem media
        {
            get;
            set;
        }

        public double initMediaToScreenPointX
        {
            get;
            set;
        }

        public double initMediaToScreenPointY
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
