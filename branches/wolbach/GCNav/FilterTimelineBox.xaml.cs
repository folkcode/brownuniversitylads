using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation.Controls;
using System.Globalization;

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
        List<String> _categories; // jcchin
        //List<String> _years;

        Dictionary<string, Dictionary<int, List<int>>> _yearsDic; // jcchin
        DateTimeFormatInfo _dateInfo; // jcchin

        DateTime _lastOpened;
     
        public FilterTimelineBox()
        {
            InitializeComponent();
            _lastOpened = DateTime.UtcNow;

            filtCategoryList.Visibility = Visibility.Visible;

            _dateInfo = new DateTimeFormatInfo(); // jcchin
        }

        public void init(Navigator Nav)
        {
            _nav = Nav;
            _imageCollection = _nav.getImageCollection();

            _artists = new List<String>();
            _mediums = new List<String>();
            _categories = new List<String>(); // jcchin
            //_years = new List<String>();
            _yearsDic = new Dictionary<string, Dictionary<int, List<int>>>();

            //Populate filter category lists
            foreach (ImageData img in _imageCollection)
            {
                String artist = img.artist;
                String medium = img.medium;
                String category = img.category; // jcchin
                String year = (img.year).ToString();
                int month = img.month;
                int day = img.day;

                if (!_artists.Contains(artist))
                    _artists.Add(artist);
                if (!_mediums.Contains(medium))
                    _mediums.Add(medium);
                if (!_categories.Contains(category)) // jcchin
                    _categories.Add(category);
                /*if (!_years.Contains(year))
                {
                    _years.Add(year);
                }*/

                // jcchin
                if (!_yearsDic.ContainsKey(year))
                {
                    _yearsDic.Add(year, new Dictionary<int, List<int>>());
                }
                Dictionary<int, List<int>> currYear;
                _yearsDic.TryGetValue(year, out currYear);
                
                if (!currYear.ContainsKey(month))
                {
                    currYear.Add(month, new List<int>());
                }
                List<int> currMonth;
                currYear.TryGetValue(month, out currMonth);

                if (!currMonth.Contains(day))
                {
                    currMonth.Add(day);
                }
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
            /*if (filtCategoryList.IsVisible)
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
            }*/
        }

        private void filtCategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (filtCategoryList.SelectedIndex)
            {  
                /*case 0:
                    populateFilterList(_artists);
                    break;
                case 1:
                    populateFilterList(_mediums);
                    break;*/
                case 0:
                    populateFilterList(_categories);
                    filtItemListGrid_date.Visibility = Visibility.Collapsed;
                    filtItemListGrid.Visibility = Visibility.Visible;
                    break;
                case 1:
                    populateFilterList_years();
                    filtItemListGrid.Visibility = Visibility.Collapsed;
                    filtItemListGrid_date.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void populateFilterList(List<String> theItems)
        {
            filtItemList.Items.Clear();
            filtItemList_years.Items.Clear();
            filtItemList_months.Items.Clear();
            filtItemList_days.Items.Clear();

            for (int i = 0; i < theItems.Count; i++)
            {
                SurfaceListBoxItem b = new SurfaceListBoxItem();
                b.Content = theItems[i];
                b.Background =new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5e5e5e"));
                filtItemList.Items.Add(b);
            }
        }

        // jcchin
        private void populateFilterList_years()
        {
            filtItemList.Items.Clear();
            filtItemList_years.Items.Clear();
            filtItemList_months.Items.Clear();
            filtItemList_days.Items.Clear();

            Dictionary<String, Dictionary<int, List<int>>>.KeyCollection.Enumerator yearsEnum = _yearsDic.Keys.GetEnumerator();
            while (yearsEnum.MoveNext())
            {
                SurfaceListBoxItem b = new SurfaceListBoxItem();
                b.Content = yearsEnum.Current;
                b.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5e5e5e"));
                filtItemList_years.Items.Add(b);
            }
        }

        private void populateFilterList_months(string selectedYear)
        {
            filtItemList_months.Items.Clear();
            filtItemList_days.Items.Clear();

            Dictionary<int, List<int>> monthsDic = new Dictionary<int,List<int>>();
            _yearsDic.TryGetValue(selectedYear, out monthsDic);

            if (monthsDic != null)
            {
                Dictionary<int, List<int>>.KeyCollection.Enumerator monthsEnum = monthsDic.Keys.GetEnumerator();
                List<int> monthsList = new List<int>();
                while (monthsEnum.MoveNext())
                {
                    monthsList.Add(monthsEnum.Current);
                }
                if (monthsList != null)
                {
                    monthsList.Sort();

                    foreach (int month in monthsList)
                    {
                        if (month != 0)
                        {
                            SurfaceListBoxItem b = new SurfaceListBoxItem();

                            b.Content = _dateInfo.GetMonthName(month);
                            b.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5e5e5e"));
                            filtItemList_months.Items.Add(b);
                        }
                    }
                }
            }
        }

        private void populateFilterList_days(string selectedYear, int selectedMonth)
        {
            filtItemList_days.Items.Clear();

            Dictionary<int, List<int>> monthsDic = new Dictionary<int,List<int>>();
            _yearsDic.TryGetValue(selectedYear, out monthsDic);
            List<int> daysList = new List<int>();
            if (monthsDic != null)
            {
                monthsDic.TryGetValue(selectedMonth, out daysList);

                if (daysList != null)
                {
                    daysList.Sort();

                    foreach (int day in daysList)
                    {
                        if (day != 0)
                        {
                            SurfaceListBoxItem b = new SurfaceListBoxItem();
                            b.Content = day.ToString();
                            b.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5e5e5e"));
                            filtItemList_days.Items.Add(b);
                        }
                    }
                }
            }
        }

        private void filtItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((filtItemList.SelectedItems.Count == 0) && (filtItemList_years.SelectedItems.Count == 0))
            {
                _nav.ImagesSelected(_imageCollection);
                return;
            }
            List<ImageData> imgs = new List<ImageData>();
            switch (filtCategoryList.SelectedIndex)
            {
                /*case 0:
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
                    break;*/
                case 0:
                    // categories
                    for (int i = 0; i < _imageCollection.Count; i++)
                    {
                        foreach (SurfaceListBoxItem item in filtItemList.SelectedItems)
                        {
                            if (_imageCollection[i].category == (string)((SurfaceListBoxItem)item).Content)
                                imgs.Add(_imageCollection[i]);
                        }
                    }
                    break;
                case 1:
                    //years
                    string selectedYear = (string)((SurfaceListBoxItem)filtItemList_years.SelectedItem).Content;
                    this.populateFilterList_months(selectedYear);

                    for (int i = 0; i < _imageCollection.Count; i++)
                    {
                        if (_imageCollection[i].year.ToString() == selectedYear)
                        {
                                imgs.Add(_imageCollection[i]);
                        }
                    }

                    break;
                default:
                    break;
            }
            _nav.ImagesSelected(imgs);
        }

        private void filtItemList_months_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ImageData> imgs = new List<ImageData>();
            string selectedYear = "";
            if (filtItemList_months.SelectedItem != null)
            {
                selectedYear = (string)((SurfaceListBoxItem)filtItemList_years.SelectedItem).Content;
            }
            int selectedMonth = 0;
            if (filtItemList_months.SelectedItem != null)
            {
                selectedMonth = DateTime.ParseExact(((string)((SurfaceListBoxItem)filtItemList_months.SelectedItem).Content), "MMMM", CultureInfo.CurrentCulture).Month;
            }

            this.populateFilterList_days(selectedYear, selectedMonth);

            for (int i = 0; i < _imageCollection.Count; i++)
            {
                if (_imageCollection[i].year.ToString() == selectedYear)
                {
                    if ((_imageCollection[i].month == selectedMonth))  // account for images with no month! || (_imageCollection[i].month == 0)
                    {
                        imgs.Add(_imageCollection[i]);
                    }
                }
            }

            _nav.ImagesSelected(imgs);
        }

        private void filtItemList_days_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ImageData> imgs = new List<ImageData>();
            string selectedYear = "";
            if (filtItemList_months.SelectedItem != null)
            {
                selectedYear = (string)((SurfaceListBoxItem)filtItemList_years.SelectedItem).Content;
            }
            int selectedMonth = 0;
            if (filtItemList_months.SelectedItem != null)
            {
                selectedMonth = DateTime.ParseExact(((string)((SurfaceListBoxItem)filtItemList_months.SelectedItem).Content), "MMMM", CultureInfo.CurrentCulture).Month;
            }
            int selectedDay = 0;
            if (filtItemList_days.SelectedItem != null) {
                selectedDay = Convert.ToInt32((string)((SurfaceListBoxItem)filtItemList_days.SelectedItem).Content);
            }

            for (int i = 0; i < _imageCollection.Count; i++)
            {
                if (_imageCollection[i].year.ToString() == selectedYear)
                {
                    if ((_imageCollection[i].month == selectedMonth)) 
                    {
                        if ((_imageCollection[i].day == selectedDay)) // account for images with a month but no day! || (_imageCollection[i].day == 0)
                        {
                            imgs.Add(_imageCollection[i]);
                        }
                    }
                    /*else if (_imageCollection[i].month == 0)  // account for images with no month!
                    {
                        imgs.Add(_imageCollection[i]);
                    }*/
                }
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
            filtItemList_years.SelectedIndex = -1;

            filtItemList_months.SelectedIndex = -1;
            filtItemList_months.Items.Clear();
            filtItemList_days.SelectedIndex = -1;

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
