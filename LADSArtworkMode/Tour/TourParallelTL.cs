using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation.Controls;

namespace LADSArtworkMode
{
    /// <summary>
    /// TourParallelTL - subclass of WPF's ParallelTimeline, imeplements TourTL interface
    /// </summary>
    class TourParallelTL : ParallelTimeline, TourTL
    {
        public TourParallelTL()
        {
        }

        public TourTLType type
        {
            get;
            set;
        }

        public String displayName
        {
            get;
            set;
        }

        public String file
        {
            get;
            set;
        }
        public SurfaceInkCanvas inkCanvas
        {
            get;
            set;
        }
    }
}
