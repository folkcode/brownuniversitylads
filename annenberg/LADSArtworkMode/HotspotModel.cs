using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LADSArtworkMode
{
    public class HotspotModel
    {
        private ObservableCollection<Hotspot> m_HotspotObservableCollection = null;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection <Hotspot> Classes
        {
            get { return m_HotspotObservableCollection; }
            set
            {
                m_HotspotObservableCollection = value;
                this.sendPropertyChanged("Classes");
            }
        }

        private void sendPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }

        }
    }
}
