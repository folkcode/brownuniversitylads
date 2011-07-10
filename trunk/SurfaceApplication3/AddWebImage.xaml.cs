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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Xml;

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
