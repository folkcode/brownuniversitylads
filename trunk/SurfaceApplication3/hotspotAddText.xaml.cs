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
using System.Windows.Shapes;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for hotspotAddText.xaml
    /// </summary>
    public partial class hotspotAddText : Window
    {
        private hotspotAdd hotspotControl;
        public hotspotAddText()
        {
            InitializeComponent();
        }
        public void setParentControl(hotspotAdd add)
        {
            hotspotControl = add;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            String caption = title.Text;
            String description = Text.Text;
            hotspotControl.setHotspotInfo(caption + "/" + "text" +"/" + description);
            this.Close();
            hotspotControl.saveHotspotInfo();
            hotspotControl.ModifyText.IsEnabled = true;
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            title.Text = "";
            Text.Text = "";
            return;
        }
    }
}
