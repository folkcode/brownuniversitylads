using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for helpWindow.xaml
    /// </summary>
    public partial class helpWindow : UserControl
    {
        public helpWindow()
        {
            InitializeComponent();
        }

        public void ShowHelp(bool isCatalog)
        {
            this.Visibility = Visibility.Visible;
            if (isCatalog)
            {
                this.Catalog_SurfaceButton_Click(this, null);
            }
            else
            {
                this.Artwork_SurfaceButton_Click(this, null);
            }

        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void Catalog_SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            Catalog_SurfaceButton.Background = new SolidColorBrush(Color.FromRgb(0xff, 0xf6, 0x8b));
            Catalog_SurfaceButton.Foreground = new SolidColorBrush(Colors.Black);
            CatalogHelp.Visibility = Visibility.Visible;
            Artwork_SurfaceButton.Background = new SolidColorBrush(Color.FromRgb(0x24, 0x52, 0x4a));
            Artwork_SurfaceButton.Foreground = new SolidColorBrush(Colors.White);
            ArtworkHelp.Visibility = Visibility.Collapsed;
        }

        private void Artwork_SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            Artwork_SurfaceButton.Background = new SolidColorBrush(Color.FromRgb(0xff, 0xf6, 0x8b));
            Artwork_SurfaceButton.Foreground = new SolidColorBrush(Colors.Black);
            ArtworkHelp.Visibility = Visibility.Visible;
            Catalog_SurfaceButton.Background = new SolidColorBrush(Color.FromRgb(0x24, 0x52, 0x4a));
            Catalog_SurfaceButton.Foreground = new SolidColorBrush(Colors.White);
            CatalogHelp.Visibility = Visibility.Collapsed;
        }
    }
}
