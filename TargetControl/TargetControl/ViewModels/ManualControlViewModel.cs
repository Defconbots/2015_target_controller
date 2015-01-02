using System.Linq;

using Caliburn.Micro;

namespace TargetControl
{
    public sealed class ManualControlViewModel : Screen, IMainScreenTabItem
    {
        private readonly ISerial _serialPort;
        private readonly ISerialCommandInterface _serialInterface;

        private string _selectedSerialPort;

        public ManualControlViewModel(ISerialPortListProvider availableSerialPortsProvider,
            ISerial serialPort,
            ISerialCommandInterface serialInterface)
        {
            _serialPort = serialPort;
            _serialInterface = serialInterface;
            DisplayName = "Manual";

            var availablePorts = availableSerialPortsProvider.GetPortNames();
            AvailableSerialPorts = new BindableCollection<string>(availablePorts);

            var targets = Enumerable.Range(1, 5)
                .Select(x => new ManualControlTargetViewModel((char)('0' + x)));
            Targets = new BindableCollection<ManualControlTargetViewModel>(targets);

            SelectedAvailableSerialPort = AvailableSerialPorts.FirstOrDefault();

            _serialInterface.DataReceived += OnSerialCommandDataReceived;
        }

        public BindableCollection<string> AvailableSerialPorts { get; set; }

        public BindableCollection<ManualControlTargetViewModel> Targets { get; set; }

        public string SelectedAvailableSerialPort
        {
            get { return _selectedSerialPort; }
            set
            {
                _selectedSerialPort = value;
                NotifyOfPropertyChange(() => SelectedAvailableSerialPort);
            }
        }

        public void OpenSerialPort()
        {
            _serialPort.Open(SelectedAvailableSerialPort, 115200);
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
            _serialInterface.Write(target.Address, 'R', '0', target.RedLed ? '0' : '1');
        }

        public void ToggleBlueLed(ManualControlTargetViewModel target)
        {
            _serialInterface.Write(target.Address, 'B', '0', target.BlueLed ? '0' : '1');
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
            }
        }
    }

    public class ManualControlTargetViewModel : PropertyChangedBase
    {
        private readonly char _address;
        private bool _redLed;
        private bool _blueLed;

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

        public char Address
        {
            get { return _address; }
        }
    }
}