using System.Windows;
using System.Windows.Controls;

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
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
