using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetControl
{
    public interface ISerial
    {
        event Action<string> SerialDataSent;

        event Action<string> SerialDataReceived;

        string ComPort { get; set; }

        int BaudRate { get; set; }
        
        void Open();

        void SendPacket(string buf);
    }

    public class SerialPortSerial : ISerial
    {
        private SerialPort _serialPort;

        public string ComPort { get; set; }
        
        public int BaudRate { get; set; }

        public event Action<string> SerialDataSent;

        public event Action<string> SerialDataReceived;

        public void SendPacket(string buf)
        {
            if (_serialPort != null)
            {
                _serialPort.Write(buf);

                if (SerialDataSent != null)
                {
                    SerialDataSent(buf);
                }
            }
        }

        public void Open()
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= OnDataReceived;
                _serialPort.Close();
                _serialPort.Dispose();
            }

            _serialPort = new SerialPort(ComPort, BaudRate);
            _serialPort.DataReceived += OnDataReceived;
            _serialPort.Open();
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            if (SerialDataReceived != null)
            {
                SerialDataReceived(_serialPort.ReadExisting());
            }
        }
    }
}
