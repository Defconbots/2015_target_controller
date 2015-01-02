using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetControl
{
    public class SerialCommandInterface : ISerialPacketHandler
    {
        private ISerial _serial;
        private readonly ISerialPacketParser _serialParser;
        private char _lastAddress;
        private char _lastDevice;
        private char _lastDataH;
        private char _lastDataL;

        public SerialCommandInterface(ISerial serial, ISerialPacketParser serialParser)
        {
            _serial = serial;
            _serialParser = serialParser;

            _serial.SerialDataReceived += OnSerialDataReceived;
        }

        public event Action<SCIReadData> DataReceived = delegate { }; 

        public void Read(char address, char device)
        {
            var buf = string.Format("{{{0}{1}}}", address, device);
            _serial.SendPacket(buf);
            _lastAddress = address;
            _lastDevice = device;
        }

        public void Write(char address, char device, char dataH, char dataL)
        {
            var buf = string.Format("[{0}{1}{2}{3}]", address, device, dataH, dataL);
            _serial.SendPacket(buf);
            _lastAddress = address;
            _lastDevice = device;
            _lastDataH = dataH;
            _lastDataL = dataL;
        }

        public int? CheckPacket(string buf)
        {
            if (buf.Length == 0)
            {
                return null;
            }

            if (buf[0] == '(')
            {
                if (buf.Length < 4)
                {
                    return null;
                }

                if (buf[3] != ')')
                {
                    return -1;
                }

                DataReceived(new SCIReadData
                {
                    Address = _lastAddress,
                    Device = _lastDevice,
                    DataH = buf[1],
                    DataL = buf[2]
                });
                return 4;
            }

            if (buf[0] == '<')
            {
                if (buf.Length < 3)
                {
                    return null;
                }

                if (buf[1] != _lastAddress || buf[2] != '>')
                {
                    return -1;
                }

                DataReceived(new SCIReadData
                {
                    Address = _lastAddress,
                    Device = _lastDevice,
                    DataH = _lastDataH,
                    DataL = _lastDataL
                });
                return 3;
            }

            return -1;
        }

        private void OnSerialDataReceived(string data)
        {
            _serialParser.AddData(data, this);
        }
    }

    public struct SCIReadData
    {
        public char Address { get; set; }
        public char Device { get; set; }
        public char DataH { get; set; }
        public char DataL { get; set; }
    }
}
