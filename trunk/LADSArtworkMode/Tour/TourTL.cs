using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation.Controls;

namespace LADSArtworkMode
{
    enum TourTLType
    {
        artwork,
        media,
        audio,
        path,
        highlight
    }

    /// <summary>
    /// TourTL interface - implemented by TourParllelTL and TourMediaTL
    /// </summary>
    interface TourTL
    {
        TourTLType type
        {
            get;
            set;
        }

        TourTL copy();

        String displayName
        {
            get;
            set;
        }

        // filename of artwork or associated medium
        String file
        {
            get;
            set;
        }

        
    }
}
