using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace TimelineSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Timer _timer;
        private DateTime _currentDate;

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                OnPropertyChanged(nameof(CurrentDate));
            }
        }

        public TimeLineEvent PulsingEvent { get; set; }

        public List<object> TimeEvents { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();

            StartDate = DateTime.Now;
            _currentDate = StartDate;
            EndDate = StartDate.AddHours(5);

            GenerateEvents();

            DataContext = this;
            _timer = new Timer(OnTimeTick, null, 1000, 1000);
        }

        private void OnPropertyChanged(string propName)
        {
            Dispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName)));
        }

        public void OnTimeTick(object state)
        {
            CurrentDate = CurrentDate.AddSeconds(1);
            PulsingEvent.Duration = TimeSpan.FromMinutes(CurrentDate.Second % 10);
        }

        public void GenerateEvents()
        {
            var eventList = new List<object>();
            Random random = new Random();
            var max = (int)(EndDate - StartDate).TotalMinutes;

            PulsingEvent = new TimeLineEvent
            {
                Time = StartDate.AddSeconds(-1 * StartDate.Second).AddMinutes(3),
                Duration = TimeSpan.FromMinutes(1)
            };

            eventList.Add(new TimeLineEvent
            {
                Time = StartDate.AddSeconds(-1 * StartDate.Second).AddMinutes(1)
            });

            eventList.Add(new TimeLineEvent
            {
                Time = StartDate.AddSeconds(-1 * StartDate.Second).AddMinutes(3),
                Duration = TimeSpan.FromMinutes(8)
            });

            for (int i = 0; i < 1000; i++)
            {
                double time = random.NextDouble() * max;
                var newEvent = new TimeLineEvent
                {
                    Time = StartDate.AddMinutes(time)
                };
                eventList.Add(newEvent);
            }
            eventList.Add(new TimeLineEvent
            {
                Time = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, EndDate.Hour, EndDate.Minute, 0)
            });
            
            TimeEvents = eventList;
        }
    }

    public class TimeLineEvent : INotifyPropertyChanged
    {
        private DateTime _time;
        private TimeSpan? _duration;

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime Time
        {
            get { return _time; }
            set
            {
                _time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        public TimeSpan? Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        private void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
