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
        private BindableCollection<TerminalLine> _terminalOutputText;
        private string _terminalInputText;

        public TerminalViewModel(ISerial serial)
        {
            _serial = serial;
            _serial.SerialDataReceived += OnSerialDataReceived;
            _serial.SerialDataSent += OnSerialDataSent;
            DisplayName = "Terminal";
            TerminalOutputText = new BindableCollection<TerminalLine>();
        }

        public BindableCollection<TerminalLine> TerminalOutputText
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
            AddText(serialData, true);
        }

        private void OnSerialDataSent(string serialData)
        {
            AddText(serialData, false);
        }

        private void AddText(string serialData, bool isReceived)
        {
            while (TerminalOutputText.Count > 20)
            {
                TerminalOutputText.RemoveAt(0);
            }

            TerminalOutputText.Add(new TerminalLine
            {
                Text = serialData,
                IsReceived = isReceived
            });
        }
    }

    public class TerminalLine
    {
        public string Text { get; set; }
        public bool IsReceived { get; set; }    
    }
}