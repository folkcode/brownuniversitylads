using System;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for AddWebImage.xaml
    /// </summary>
    public partial class AddWebImage : SurfaceWindow
    {
        private MetaDataEntry _metaEntry;
        public AddWebImage(MetaDataEntry metaEntry)
        {
            InitializeComponent();
            _metaEntry = metaEntry;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            String url = url_tag.Text;
            _metaEntry.setURL(url);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
