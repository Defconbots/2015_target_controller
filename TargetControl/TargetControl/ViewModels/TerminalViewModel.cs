using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;

namespace TargetControl
{
    public sealed class TerminalViewModel : Screen, IMainScreenTabItem
    {
        private readonly ISerial _serial;
        private string _terminalOutputText;
        private string _terminalInputText;

        public TerminalViewModel(ISerial serial)
        {
            _serial = serial;
            _serial.SerialDataReceived += OnSerialDataReceived;
            _serial.SerialDataSent += OnSerialDataSent;
            DisplayName = "Terminal";
        }

        public string TerminalOutputText
        {
            get { return _terminalOutputText; }
            set
            {
                if (value == _terminalOutputText) return;
                _terminalOutputText = value;
                NotifyOfPropertyChange(() => TerminalOutputText);
            }
        }

        public string TerminalInputText
        {
            get { return _terminalInputText; }
            set
            {
                if (value == _terminalInputText) return;
                _terminalInputText = value;
                NotifyOfPropertyChange(() => TerminalInputText);
            }
        }

        public void OpenSerialPort()
        {
            _serial.Open();
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var text = TerminalInputText;
                _serial.SendPacket(text);
                TerminalInputText = string.Empty;
            }
        }

        private void OnSerialDataReceived(string serialData)
        {
            TerminalOutputText += serialData + "\n";
        }

        private void OnSerialDataSent(string serialData)
        {
            TerminalOutputText += serialData + "\n";
        }
    }
}