using System.Windows;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for mapWindow.xaml
    /// </summary>
    public partial class mapWindow : Window
    {
        public mapWindow()
        {
            InitializeComponent();
            mapControl.setParentWindow(this);
            
        }

    }
}
