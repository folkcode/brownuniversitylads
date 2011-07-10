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
    /// Interaction logic for StartCard.xaml
    /// </summary>
    public partial class StartCard : UserControl
    {
        public StartCard()
        {
            InitializeComponent();
            this.setImagePath("\\Data\\Startup\\StartCard_FooterLogos.png");
        }

        /// <summary>
        /// sets the path of the image to be displayed at the bottom of the start card
        /// </summary>
        /// <param name="p"></param>
        public void setImagePath(String p)
        {
            image.Source = (new BitmapImage(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + p, UriKind.Absolute))); ;
        }
    }
}
