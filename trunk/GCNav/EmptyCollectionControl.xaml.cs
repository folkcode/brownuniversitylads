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
    /// Interaction logic for EmptyCollectionControl.xaml
    /// If the collection is empty, alerts the user that he/she must populate the collection to use LADS
    /// </summary>
    public partial class EmptyCollectionControl : UserControl
    {
        public EmptyCollectionControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
