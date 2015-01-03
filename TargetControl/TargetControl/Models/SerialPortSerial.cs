using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PInvokeSerialPort;

namespace TargetControl
{
    public interface ISerial
    {
        void SendPacket(string buf);
        event Action<string> SerialDataReceived;
        void Open(string comPort, int baudRate);
    }
    
    public class SerialPortSerial : ISerial
    {
        private SerialPort _serialPort;

        public event Action<string> SerialDataReceived;

        public void SendPacket(string buf)
        {
            if (_serialPort != null)
            {
                _serialPort.Write(buf);
            }
        }

        public void Open(string comPort, int baudRate)
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= OnDataReceived;
                _serialPort.Close();
                _serialPort.Dispose();
            }

            _serialPort = new SerialPort(comPort, baudRate);
            _serialPort.DataReceived += OnDataReceived;
            _serialPort.Open();
        }

        private void OnDataReceived(byte x)
        {
            if (SerialDataReceived != null)
            {
                SerialDataReceived(((char)x).ToString());
            }
        }
    }
}
