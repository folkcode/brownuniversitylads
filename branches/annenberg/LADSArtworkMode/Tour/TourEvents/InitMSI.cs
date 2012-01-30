using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepZoom.Controls;

namespace LADSArtworkMode.TourEvents
{
    /// <summary>
    /// (NOT IN USE - can't set msi properties if the msi is not visible) InitMSIEvent - subclass of TourEvent
    /// </summary>
    public class InitMSIEvent :  TourEvent
    {
        public InitMSIEvent(MultiScaleImage msiParam, double initMSIPointXParam, double initMSIPointYParam, double absoluteScaleParam)
        {
            type = TourEvent.Type.initMSI;
            msi = msiParam;

            initMSIPointX = initMSIPointXParam;
            initMSIPointY = initMSIPointYParam;
            absoluteScale = absoluteScaleParam;
        }

        public MultiScaleImage msi
        {
            get;
            set;
        }

        public double initMSIPointX
        {
            get;
            set;
        }

        public double initMSIPointY
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
