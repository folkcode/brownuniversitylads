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
            setText();
        }
        public void setText()
        {

            text.Text = "Tap the TourAuthoring button to start making a new tour. Tap the Catalog button to go back to the catalog. Tap the tool bars on the left panels to explore the details of the artwork.";

        }
        private void Done_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;

        }
    }
}
