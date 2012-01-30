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

        public TourTL copy()
        {
            TourParallelTL tl = new TourParallelTL();
            tl.type = type;
            tl.displayName = displayName;
            tl.file = file;
            tl.inkCanvas = inkCanvas;
            return tl;
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
