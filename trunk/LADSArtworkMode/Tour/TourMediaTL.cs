using System;
using System.Windows.Media;

namespace LADSArtworkMode
{
    /// <summary>
    /// TourMediaTL - subclass of WPF's MediaTimeline, implements TourTL interface
    /// </summary>
    class TourMediaTL : MediaTimeline, TourTL
    { 
        public TourMediaTL(Uri uri) : base(uri)
        {
            this.uri = uri;
        }
        public TourTL copy()
        {
            TourMediaTL tl = new TourMediaTL(uri);
            tl.type = type;
            tl.displayName = displayName;
            tl.file = file;
            tl.Duration = Duration;
            return tl;
        }

        public Uri uri
        {
            get;
            set;
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
    }
}
