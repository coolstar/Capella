using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Capella
{
    public class TimeLabel : Label
    {
        private Timer timer;
        public TimeLabel()
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += (sender, e) =>
                {
                    if (this.IsVisible)
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            updateTime();
                        }));
                };
            timer.Start();
        }

        ~TimeLabel()
        {
            Console.WriteLine("Timer Destroyed");
            timer.Stop();
            timer.Close();
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.RegisterAttached("time",
            typeof(DateTime), typeof(TimeLabel),
            new FrameworkPropertyMetadata(DateTime.Now, new PropertyChangedCallback(TimePropertyChanged)));

        private DateTime _time = DateTime.Now;
        public DateTime time
        {
            set
            {
                _time = value;
                updateTime();
            }
            get
            {
                return _time;
            }
        }

        private static void TimePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TimeLabel timeLabel = obj as TimeLabel;

            if (timeLabel != null)
            {
                timeLabel.time = Convert.ToDateTime(e.NewValue);
            }
        }

        private void updateTime()
        {
            TimeSpan span = DateTime.Now - time;
            if (span.TotalSeconds < 0)
                this.Content = "0s";
            else if (span.TotalSeconds < 60)
            {
                this.Content = ((int)span.TotalSeconds) + "s";
            }
            else if (span.TotalMinutes < 60)
            {
                this.Content = ((int)span.TotalMinutes) + "m";
                timer.Interval = 60 * 1000;
            }
            else if (span.TotalHours < 24)
            {
                this.Content = ((int)span.TotalHours) + "h";
                timer.Interval = 60 * 1000;
            }
            else
            {
                this.Content = ((int)span.TotalDays) + "d";
                timer.Interval = 60 * 1000;
            }
        }
    }
}
