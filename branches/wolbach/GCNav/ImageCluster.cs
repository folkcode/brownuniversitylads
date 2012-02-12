using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace GCNav
{
    public class ImageCluster : Grid
    {
        private int bordPadding = 2;
        private int bordMargin = 3;
        private int bordThickness = 4;
        public int size_padding_constant { get { return 2 * (bordMargin + bordPadding + bordThickness); } }
        public int minYear = int.MaxValue;
        public int maxYear = int.MinValue;
        private int _numRows;
        private int curRow = 0;
        private List<ImageData> images;
        StackPanel[] stacks;
        public ImageCluster(int rows)
        {
            _numRows = rows;
            stacks = new StackPanel[_numRows];
            //add rows to grid
            for (int i = 0; i < _numRows; i++)
            {
                this.RowDefinitions.Add(new RowDefinition());

                stacks[i] = new StackPanel();
                stacks[i].Orientation = Orientation.Horizontal;
                stacks[i].HorizontalAlignment = HorizontalAlignment.Center;
                this.Children.Add(stacks[i]);
                Grid.SetRow(stacks[i], i);

            }
            images = new List<ImageData>();
            //this.Background = new SolidColorBrush(Colors.Red);  // For debugging.
        }

        //used to determine width for collisions while populating timeline
        //returns the longest row width.  nevermind the name.
        public double topRowWidth()
        {
            double maxW = 0;
            for (int i = 0; i < _numRows; i++)
            {
                double w = 0;
                foreach (Border b in stacks[i].Children)
                {
                    w += ((ImageData)((Canvas)b.Child).Children[0]).Width;
                    w += size_padding_constant;
                }
                maxW = (maxW > w) ? maxW : w;
            }
            return maxW;
        }

        // Used to determine width for collisions while populating timeline.
        // Returns the longest row width.
        public double longestRowWidth()
        {
            double maxW = 0;
            for (int i = 0; i < _numRows; i++)
            {
                double w = 0;
                foreach (Border b in stacks[i].Children)
                {
                    w += ((ImageData)((Canvas)b.Child).Children[0]).Width;
                    w += size_padding_constant;
                }
                maxW = (maxW > w) ? maxW : w;
            }
            return maxW;
        }

        public List<ImageData> getImages()
        {
            return images;
        }
        public int getSize()
        {
            return images.Count;
        }

        public void addImage(ImageData img)
        {
            minYear = Math.Min(img.year, minYear);
            maxYear = Math.Max(img.year, maxYear);
            images.Add(img);
            //create border thing, add image
            Border bord = new Border();

            bord.BorderBrush = Brushes.White; // jcchin
            //bord.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x2d, 0x0c)); // jcchin - commented out

            bord.Background = new SolidColorBrush(Color.FromRgb(0x00, 0x2d, 0x0c));
            bord.CornerRadius = new CornerRadius(5);
            bord.HorizontalAlignment = HorizontalAlignment.Center;
            bord.VerticalAlignment = VerticalAlignment.Center;
            bord.Padding = new Thickness(bordPadding);
            bord.Margin = new Thickness(bordMargin);
            bord.BorderThickness = new Thickness(bordThickness);
            

            TextBlock tb = new TextBlock();
            tb.FontSize = 12;
            tb.Width = img.Width;
            tb.Background = new SolidColorBrush(Color.FromArgb(127,0,0,0));
            //tb.Height = 12;
            tb.TextWrapping = TextWrapping.NoWrap;
            tb.TextTrimming = TextTrimming.WordEllipsis;
            tb.Foreground = new SolidColorBrush(Color.FromRgb(0xff,0xff,0xff));
            tb.Text = img.artist;
            tb.VerticalAlignment = VerticalAlignment.Bottom;
            Canvas.SetBottom(tb, 0);

            tb.IsHitTestVisible = false;

            Canvas c = new Canvas();
            c.Height = img.Height;
            c.Width = img.Width;
            c.Children.Add(img);
            

            c.Children.Add(tb);
            bord.Child = c;
            img.setCluster(this);

            //add border thing to row
            stacks[curRow].Children.Add(bord);

            //increment row
            curRow++;
            curRow %= _numRows;
        }
    }
}
