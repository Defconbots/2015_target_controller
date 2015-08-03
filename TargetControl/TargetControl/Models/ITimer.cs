using System;
using System.Windows.Threading;

namespace TargetControl
{
    public interface ITimer
    {
        TimeSpan Interval { get; set; }
        bool IsEnabled { get; set; }
        event EventHandler Tick;

        void Start();
        void Stop();
    }

    public class UITimer : ITimer
    {
        private readonly DispatcherTimer _timer;

        public UITimer(DispatcherTimer timer)
        {
            _timer = timer;
        }

        public event EventHandler Tick
        {
            add { _timer.Tick += value; }
            remove { _timer.Tick -= value; }
        }

        public TimeSpan Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public bool IsEnabled
        {
            get { return _timer.IsEnabled; }
            set { _timer.IsEnabled = value; }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}