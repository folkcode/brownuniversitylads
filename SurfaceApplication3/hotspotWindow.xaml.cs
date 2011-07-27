using System.Windows;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for hotspotWindow.xaml
    /// </summary>
    public partial class hotspotWindow : Window
    {
        public hotspotWindow()
        {
            InitializeComponent();
            hotspot.setParentWindow(this);
        }
    }
}
