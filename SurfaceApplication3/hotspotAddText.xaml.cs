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
           // Console.Out.WriteLine("called");
            hotspotControl.newWindowIsOpened = false;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (title.Text != null && Text.Text != null)
            {
                String caption = title.Text;
                String description = Text.Text;
                hotspotControl.setHotspotInfo(caption + "/" + "text" + "/" + description);
                this.Close();
                hotspotControl.saveHotspotInfo();
            //    hotspotControl.AddText.IsEnabled = false;
            //    hotspotControl.AddImage.IsEnabled = true;
            //    hotspotControl.AddAudio.IsEnabled = true;
            //    hotspotControl.AddVideo.IsEnabled = true;
                hotspotControl.Edit.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Caption and descriptions can not be empty!");
                hotspotControl.newWindowIsOpened = false;
                return;
            }
           // hotspotControl.ModifyText.IsEnabled = true;
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            title.Text = "";
            Text.Text = "";
            hotspotControl.newWindowIsOpened = false;
            this.Close();
        }
    }
}
