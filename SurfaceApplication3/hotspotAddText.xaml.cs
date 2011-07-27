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
            this.Closed +=new EventHandler(hotspotAddText_Closed);
        }
        public void setParentControl(hotspotAdd add)
        {
            hotspotControl = add;
        }
        public void hotspotAddText_Closed(object sender, EventArgs e)
        {
            hotspotControl.newWindowIsOpened = false;
        }

        //Save and close the window when the user complete the infomation
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (title.Text != null && Text.Text != null)
            {
                String caption = title.Text;
                String description = Text.Text;
                hotspotControl.setHotspotInfo(caption + "/" + "text" + "/" + description);
                this.Close();
                hotspotControl.saveHotspotInfo();
       
                hotspotControl.Edit.IsEnabled = true;
                hotspotControl.AddText.IsEnabled = false;
                hotspotControl.AddAudio.IsEnabled = true;
                hotspotControl.AddImage.IsEnabled = true;
                hotspotControl.AddVideo.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Caption and description cannot be empty!");
                hotspotControl.newWindowIsOpened = false;
                return;
            }
        }

        //Cancel all the changes
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            title.Text = "";
            Text.Text = "";
            hotspotControl.newWindowIsOpened = false;
            this.Close();
        }
    }
}
