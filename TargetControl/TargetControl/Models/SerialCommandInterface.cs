using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetControl
{
    public interface ISerialCommandInterface
    {
        event Action<SCIReadData> DataReceived;
        void Read(char address, char device);
        void Write(char address, char device, char dataH, char dataL);
        void Connect();
    }

    public class SerialCommandInterface : ISerialPacketHandler, ISerialCommandInterface
    {
        private readonly ISerial _serial;
        private readonly ISerialPacketParser _serialParser;
        private char _lastAddress;
        private char _lastDevice;
        private char _lastDataH;
        private char _lastDataL;
        private Stopwatch _stopwatchTime;

        public SerialCommandInterface(ISerial serial, ISerialPacketParser serialParser)
        {
            _serial = serial;
            _serialParser = serialParser;
            _stopwatchTime = new Stopwatch();

            _serial.SerialDataReceived += OnSerialDataReceived;
        }

        public event Action<SCIReadData> DataReceived = delegate { };

        public void Connect()
        {
            _serial.Open();
        }

        public void Read(char address, char device)
        {
            var buf = string.Format("{{{0}{1}}}", address, device);
            _serial.SendPacket(buf);
            _lastAddress = address;
            _lastDevice = device;
            _stopwatchTime.Restart();
        }

        public void Write(char address, char device, char dataH, char dataL)
        {
            var buf = string.Format("[{0}{1}{2}{3}]", address, device, dataH, dataL);
            _serial.SendPacket(buf);
            _lastAddress = address;
            _lastDevice = device;
            _lastDataH = dataH;
            _lastDataL = dataL;
            _stopwatchTime.Restart();
        }

        public int? CheckPacket(string buf)
        {
            if (buf.Length == 0)
            {
                return null;
            }

            if (buf[0] == '(')
            {
                if (buf.Length < 6)
                {
                    return null;
                }

                if (buf[1] != _lastAddress || buf[5] != ')')
                {
                    return -1;
                }

                Console.WriteLine("took {0}ms", _stopwatchTime.ElapsedMilliseconds);
                DataReceived(new SCIReadData
                {
                    Address = buf[1],
                    Device = buf[2],
                    DataH = buf[3],
                    DataL = buf[4]
                });
                return 6;
            }

            if (buf[0] == '<')
            {
                if (buf.Length < 4)
                {
                    return null;
                }

                if (buf[1] != _lastAddress ||
                    buf[2] != _lastDevice ||
                    buf[3] != '>')
                {
                    return -1;
                }

                Console.WriteLine("took {0}ms", _stopwatchTime.ElapsedMilliseconds);
                DataReceived(new SCIReadData
                {
                    Address = _lastAddress,
                    Device = _lastDevice,
                    DataH = _lastDataH,
                    DataL = _lastDataL
                });
                return 4;
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
