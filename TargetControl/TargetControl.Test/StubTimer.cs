using System;

namespace TargetControl.Test
{
    public class StubTimer : ITimer
    {
        public TimeSpan Interval { get; set; }

        public bool IsEnabled { get; set; }

        public event EventHandler Tick;

        public void Start()
        {
            IsEnabled = true;
        }

        public void Stop()
        {
            IsEnabled = false;
        }

        public void RaiseTick()
        {
            if (IsEnabled)
            {
                if (Tick != null)
                {
                    Tick(this, new EventArgs());
                }
            }
        }
    }
}