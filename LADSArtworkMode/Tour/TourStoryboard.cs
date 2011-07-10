using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;

namespace LADSArtworkMode.Tour
{
    /// <summary>
    /// TourStoryboard - subclass of WPF's Storyboard
    /// </summary>
    class TourStoryboard : Storyboard
    {
        public TourStoryboard()
        {
        }

        public String displayName
        {
            get;
            set;
        }

        public String description
        {
            get;
            set;
        }

        public double totalDuration
        {
            get;
            set;
        }
    }
}
