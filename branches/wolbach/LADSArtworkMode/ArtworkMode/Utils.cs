using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading; // jcchin
using System.Windows.Controls; // jcchin
using System.Windows.Controls.Primitives; // jcchin
using System.Windows.Media; // jcchin
using System.Windows.Input; // jcchin

namespace LADSArtworkMode
{
    class Utils
    {
        static public double max(double a, double b, double c)
        {
            if (a < b)
            {
                if (b < c) return c;
                return b;
            }
            else
            {
                if (a < c) return c;
                return a;
            }
        }

        static public double min(double a, double b, double c)
        {
            if (a < b)
            {
                if (a < c) return a;
                return c;
            }
            else // a >= b
            {
                if (b > c) return c;
                return b ;
            }
        }

        // jcchin
        public static double Distance(Point start, Point end)
        {
            double xDist = start.X - end.X;
            double yDist = start.Y - end.Y;

            return Math.Sqrt((xDist * xDist) + (yDist * yDist));
        }

        // jcchin
        public static void Soon(Action action)
        {
            DispatcherTimer tmr = new DispatcherTimer();
            tmr.Interval = TimeSpan.FromMilliseconds(50);
            tmr.Tick += delegate(Object sende, EventArgs e)
            {
                tmr.Stop();
                tmr = null;

                action();
            };

            tmr.Start();
        }

        // jcchin
        public static void showPopupAtControl(String msg, UIElement at, TimeSpan duration, FrameworkElement earlyCancel)
        {
            Popup pop = new Popup();
            pop.AllowsTransparency = true;

            TextBlock t = new TextBlock();
            t.Margin = new Thickness(5.0);
            t.Text = msg;
            t.Background = Brushes.Wheat;
            t.Foreground = Brushes.Navy;
            t.Opacity = 0.8;

            pop.Opacity = 0.8;
            pop.Child = t;
            pop.PlacementTarget = at;
            pop.PopupAnimation = PopupAnimation.Scroll;
            pop.IsOpen = true;

            DispatcherTimer tmr = new DispatcherTimer();

            if (earlyCancel != null)
            {
                EventHandler<TouchEventArgs> svi = null;
                svi = delegate(Object sender, TouchEventArgs e)
                {
                    earlyCancel.PreviewTouchDown -= svi;

                    if (tmr.IsEnabled)
                        tmr.Stop();

                    pop.IsOpen = false;
                    pop = null;
                };
                earlyCancel.PreviewTouchDown += svi;

                tmr.Interval = duration;
                tmr.Tick += new EventHandler(delegate(Object sender, EventArgs e)
                {
                    earlyCancel.PreviewTouchDown -= svi;

                    pop.IsOpen = false;
                    pop = null;
                    tmr.Stop();
                });
                tmr.Start();
            }
            else
            {
                tmr.Interval = duration;
                tmr.Tick += new EventHandler(delegate(Object sender, EventArgs e)
                {
                    pop.IsOpen = false;
                    pop = null;
                    tmr.Stop();
                });
                tmr.Start();
            }
        }
    }
}
