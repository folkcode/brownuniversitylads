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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Surface.Presentation.Controls;

namespace GCNav
{
    /// <summary>
    /// Interaction logic for FilterTimelineBox.xaml
    /// </summary>
    public partial class FilterTimelineBox : UserControl
    {
        Navigator _nav;
        List<ImageData> _imageCollection;


       
        List<String> _artists;
        List<String> _mediums;
        List<String> _years;
        DateTime _lastOpened;
     


        public FilterTimelineBox()
        {
            InitializeComponent();
            timelineFilter.Height = 30;
            _lastOpened = DateTime.UtcNow;

        }

        public void init(Navigator Nav)
        {
            _nav = Nav;
            _imageCollection = _nav.getImageCollection();

            _artists = new List<String>();
            _mediums = new List<String>();
            _years = new List<String>();

            //Populate filter category lists
            foreach (ImageData img in _imageCollection)
            {
                String artist = img.artist;
                String medium = img.medium;
                String year = (img.year).ToString();

                if (!_artists.Contains(artist))
                    _artists.Add(artist);
                if (!_mediums.Contains(medium))
                    _mediums.Add(medium);
                if (!_years.Contains(year))
                    _years.Add(year);
            }
        }
        public void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Double canvasLeft = e.NewSize.Width / 4;
           
            this.Width = e.NewSize.Width / 4;
            
            Canvas.SetLeft(this, canvasLeft);
        }


        public void toggleFilterbox()
        {
            if (filtCategoryList.IsVisible)
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = 250;
                da.To = 30;
                da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                timelineFilter.BeginAnimation(Grid.HeightProperty, da);
                filtCategoryList.Visibility = Visibility.Hidden;
                filtItemList.Visibility = Visibility.Hidden;
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = 30;
                da.To = 250;
                da.Duration = new Duration(TimeSpan.FromSeconds(.4));
                timelineFilter.BeginAnimation(Grid.HeightProperty, da);

                filtCategoryList.Visibility = Visibility.Visible;
                filtItemList.Visibility = Visibility.Visible;
            }
        }

        private void filtCategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine(filtCategoryList.SelectedIndex);
            switch (filtCategoryList.SelectedIndex)
            {  
                case 0:
                    populateFilterList(_artists);
                    break;
                case 1:
                    populateFilterList(_mediums);
                    break;
                case 2:
                    populateFilterList(_years);
                    break;
                default:
                    break;
            }
        }

        private void populateFilterList(List<String> theItems)
        {
            filtItemList.Items.Clear();
            for (int i = 0; i < theItems.Count; i++)
            {
                SurfaceListBoxItem b = new SurfaceListBoxItem();
                b.Content = theItems[i];
                b.Background =new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#665D9D8E"));
                filtItemList.Items.Add(b);
                //filtItemList.Items.Add(theItems[i]);
                
            }
        }


        private void filtItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (filtItemList.SelectedItems.Count == 0)
            {
                _nav.ImagesSelected(_imageCollection);
                return;
            }
            List<ImageData> imgs = new List<ImageData>();
            switch (filtCategoryList.SelectedIndex)
            {
                case 0:
                    //artists
                    for (int i = 0; i < _imageCollection.Count; i++)
                    {
                        foreach (SurfaceListBoxItem item in filtItemList.SelectedItems)
                        {
                            if (_imageCollection[i].artist == (string)((SurfaceListBoxItem)item).Content)
                                imgs.Add(_imageCollection[i]);
                        }

                    }
                    break;
                case 1:
                    //mediums
                    for (int i = 0; i < _imageCollection.Count; i++)
                    {
                        foreach (SurfaceListBoxItem item in filtItemList.SelectedItems)
                        {
                            if (_imageCollection[i].medium == (string)((SurfaceListBoxItem)item).Content)
                                imgs.Add(_imageCollection[i]);
                        }
                    }
                    break;
                case 2:
                    //years
                    for (int i = 0; i < _imageCollection.Count; i++)
                    {
                        foreach (SurfaceListBoxItem item in filtItemList.SelectedItems)
                        {
                            if (_imageCollection[i].year.ToString() == (string)((SurfaceListBoxItem)item).Content)
                                imgs.Add(_imageCollection[i]);
                        }
                    }
                    break;
                default:
                    break;
            }

            _nav.ImagesSelected(imgs);

        }

        private void handle_filt()
        {
            DateTime currUtcTime = DateTime.UtcNow;
            TimeSpan span = currUtcTime.Subtract(_lastOpened);
            if (span.Days > 0 || span.Hours > 0 || span.Minutes > 0 || span.Seconds > 0 || span.Milliseconds > 400)
            {
                toggleFilterbox();
                _lastOpened = currUtcTime;
            }
        }

        private void filt_mousedown(object sender, MouseEventArgs e)
        {
            this.handle_filt();
        }

        private void filt_touchdown(object sender, TouchEventArgs e)
        {
            this.handle_filt();
            e.Handled = true;
        }

        private void reset_timeline()
        {
            _nav.resetZoom();
            _nav.ImagesSelected(_imageCollection);
            filtItemList.SelectedIndex = -1;
            _nav.resetZoom();
        }

        private void reset_touch(object sender, TouchEventArgs e)
        {
            reset_timeline();
        }

        private void reset_mouse(object sender, MouseButtonEventArgs e)
        {
            reset_timeline();
        }
    }
}
