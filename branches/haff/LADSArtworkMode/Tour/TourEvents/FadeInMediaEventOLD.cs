using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode.TourEvents
{
    public class FadeInMediaEventOLD :  TourEvent
    {
        //public FadeInMediaEvent(DockableItem mediaParam, double fadeInMediaToScreenPointXParam, double fadeInMediaToScreenPointYParam, double absoluteScaleParam, double durationParam)
        public FadeInMediaEventOLD(DockableItem mediaParam, double durationParam)
        {
            type = TourEvent.Type.fadeInMedia;
            media = mediaParam;

            /*fadeInMediaToScreenPointX = fadeInMediaToScreenPointXParam;
            fadeInMediaToScreenPointY = fadeInMediaToScreenPointYParam;
            absoluteScale = absoluteScaleParam;*/
            duration = durationParam;
        }

        public DockableItem media
        {
            get;
            set;
        }

        /*public double fadeInMediaToScreenPointX
        {
            get;
            set;
        }

        public double fadeInMediaToScreenPointY
        {
            get;
            set;
        }

        public double absoluteScale
        {
            get;
            set;
        }*/

        public double duration
        {
            get;
            set;
        }
    }
}
