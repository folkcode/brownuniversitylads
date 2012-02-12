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

namespace GCNav
{
    /// <summary>
    /// Interaction logic for helpWindow.xaml
    /// </summary>
    public partial class helpWindow : UserControl
    {
        public bool _isNavi; //whether it's in the navigator mode or in the artWorkMode
        public helpWindow()
        {
            InitializeComponent();
            setText();
        }

        public void setText()
        {
          
                text.Text = "Tap the image in the catalog and see more information about the artwork. Tap the thumbnail image on the up right corner and explore more details about the artwork.";
         
        }
        private void Done_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            
        }
    }
}
