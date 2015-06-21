using System.IO.Ports;
using System.Linq;

using Caliburn.Micro;

namespace TargetControl
{
    public sealed class ManualControlViewModel : Screen, IMainScreenTabItem
    {
        private readonly ISerialCommandInterface _serialInterface;

        private string _voltage;

        public ManualControlViewModel(ISerialCommandInterface serialInterface)
        {
            _serialInterface = serialInterface;
            DisplayName = "Manual";

            var targets = Enumerable.Range(1, 5)
                .Select(x => new ManualControlTargetViewModel((char)('0' + x)));
            Targets = new BindableCollection<ManualControlTargetViewModel>(targets);

            _serialInterface.DataReceived += OnSerialCommandDataReceived;
        }

        public BindableCollection<ManualControlTargetViewModel> Targets { get; set; }

        public string Voltage
        {
            get { return _voltage; }
            set
            {
                _voltage = value;
                NotifyOfPropertyChange();
            }
        }

        public void OpenSerialPort()
        {
            _serialInterface.Connect();
        }

        public void ReadVoltage()
        {
            _serialInterface.Read('0', 'V');
        }

        public void SetSpeed(string speed)
        {
            if (speed != null && speed.Length == 2)
            {
                _serialInterface.Write('0', 'S', speed[0], speed[1]);
            }
        }

        public void ToggleRedLed(ManualControlTargetViewModel target)
        {
            var led = !target.RedLed ? "99" : "00";
            _serialInterface.Write(target.Address, 'R', led[0], led[1]);
        }

        public void ToggleBlueLed(ManualControlTargetViewModel target)
        {
            var led = !target.BlueLed ? "99" : "00";
            _serialInterface.Write(target.Address, 'B', led[0], led[1]);
        }

        public void ReadHitId(ManualControlTargetViewModel target)
        {
            _serialInterface.Read(target.Address, 'I');
        }

        private void OnSerialCommandDataReceived(SCIReadData data)
        {
            var target = Targets.FirstOrDefault(x => x.Address == data.Address);
            if (target != null)
            {
                if (data.Device == 'R')
                {
                    target.RedLed = (data.DataL != '0');
                }
                else if (data.Device == 'B')
                {
                    target.BlueLed = (data.DataL != '0');
                }
                else if (data.Device == 'I')
                {
                    target.HitId = string.Format("{0}{1}", data.DataH, data.DataL);
                }
            }

            if (data.Address == '0')
            {
                if (data.Device == 'V')
                {
                    Voltage = string.Format("{0}{1}", data.DataH, data.DataL);
                }
            }
        }
    }

    public class ManualControlTargetViewModel : PropertyChangedBase
    {
        private readonly char _address;
        private bool _redLed;
        private bool _blueLed;
        private string _hitId;

        public ManualControlTargetViewModel(char address)
        {
            _address = address;
        }

        public string Name
        {
            get { return string.Format("Target #{0}", Address); }
        }

        public bool RedLed
        {
            get { return _redLed; }
            set
            {
                _redLed = value;
                NotifyOfPropertyChange();
            }
        }

        public bool BlueLed
        {
            get { return _blueLed; }
            set
            {
                _blueLed = value;
                NotifyOfPropertyChange();
            }
        }

        public string HitId
        {
            get { return _hitId; }
            set
            {
                _hitId = value;
                NotifyOfPropertyChange();
            }
        }

        public char Address
        {
            get { return _address; }
        }
    }
}