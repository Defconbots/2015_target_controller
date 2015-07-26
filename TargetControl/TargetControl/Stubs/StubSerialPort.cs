using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TargetControl
{
    public class StubSerialPort : ISerial
    {
        private bool _isOpen;
        private DispatcherTimer _timer;
        private string _outBuf;

        public string ComPort { get; set; }
        public int BaudRate { get; set; }
        public event Action<string> SerialDataSent
        {
            add { }
            remove { }
        }

        public event Action<string> SerialDataReceived;

        public void Open()
        {
            _isOpen = true;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += SendRemaining;
            _timer.Start();
        }

        public void SendPacket(string buf)
        {
            if (_isOpen)
            {
                if (buf == "[1R01]" || buf == "[1R00]")
                    _outBuf += "<1>";
                else if (buf == "{1I}")
                    _outBuf += "(12)";
                else if (buf == "{0V}")
                    _outBuf += "(45)";
            }
        }

        private void SendRemaining(object sender, EventArgs e)
        {
            var save = _outBuf;
            _outBuf = string.Empty;
            SerialDataReceived(save);
        }
    }
}
