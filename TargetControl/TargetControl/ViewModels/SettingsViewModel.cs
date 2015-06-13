using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace TargetControl
{
    public class SettingsViewModel : Screen, IMainScreenTabItem
    {
        private readonly ISerial _serial;
        private readonly ISerialPortListProvider _availableSerialPortsProvider;

        private string _selectedSerialPort;

        public SettingsViewModel(ISerialPortListProvider availableSerialPortsProvider,
            ISerial serial)
        {
            _serial = serial;
            _availableSerialPortsProvider = availableSerialPortsProvider;

            DisplayName = "Settings";

            var availablePorts = availableSerialPortsProvider.GetPortNames();
            AvailableSerialPorts = new BindableCollection<string>(availablePorts);

            _selectedSerialPort = AvailableSerialPorts.FirstOrDefault();
            _serial.BaudRate = 57600;

            LoadSettings();
        }

        public BindableCollection<string> AvailableSerialPorts { get; set; }

        public string SelectedAvailableSerialPort
        {
            get { return _selectedSerialPort; }
            set
            {
                _selectedSerialPort = value;
                _serial.ComPort = _selectedSerialPort;
                NotifyOfPropertyChange(() => SelectedAvailableSerialPort);
                SaveSettings();
            }
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                var settingsJson = File.ReadAllText("settings.json");
                var settings = JsonConvert.DeserializeObject<Settings>(settingsJson);

                SelectedAvailableSerialPort = settings.ComPort;
            }
        }

        private void SaveSettings()
        {
            var settingsJson = JsonConvert.SerializeObject(new Settings
            {
                ComPort = SelectedAvailableSerialPort
            });
            File.WriteAllText("settings.json", settingsJson);
        }
    }

    public class Settings
    {
        public string ComPort { get; set; }
    }
}
