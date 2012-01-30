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
using Microsoft.Surface.Presentation.Controls.Primitives;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for small_window.xaml
    /// </summary>
    public partial class small_window : UserControl
    {
        public big_window big;
        SurfaceToggleButton itemChecked;
        public small_window()
        {
            
            InitializeComponent();
        }
        public void setBigWindow(big_window bigwindow) {
            big = bigwindow;
        }

        private void SufaceButton_Click(object sender, RoutedEventArgs e)
        { 
        }
        
        private void SurfaceToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (itemChecked != null)
                itemChecked.IsChecked = false;
            itemChecked = e.Source as SurfaceToggleButton;
            itemChecked.IsChecked = true;
            
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = true;

            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"; //Should change the limit of data type

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] filePath = ofd.FileNames;
                string[] safeFilePath = ofd.SafeFileNames;

                for (int i = 0; i < safeFilePath.Length; i++)
                {
                    BitmapImage myBitmapImage = new BitmapImage();
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(@filePath[i]);
                    myBitmapImage.EndInit();

                    //set image source
                    image1.Source = myBitmapImage;
                    title_tag.Text = safeFilePath[i];
                }

            }



        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            big.MetaDataList.Items.Remove(this);
        }

        private void tags_TextChanged(object sender, TextChangedEventArgs e)
        {
            tags.IsReadOnly = true;
            title_tag.IsReadOnly = true;
        }
       
    }
}
