using System;

namespace TargetControl
{
    public interface ISerialPacketParser
    {
        void AddData(string data, ISerialPacketHandler handler);
    }

    public interface ISerialPacketHandler
    {
        int? CheckPacket(string data);
    }

    public class SerialPacketParser : ISerialPacketParser
    {
        private string _buf = string.Empty;

        public void AddData(string data, ISerialPacketHandler handler)
        {
            _buf += data;

            while (true)
            {
                var length = handler.CheckPacket(_buf);
                if (length == null)
                    return;

                _buf = length > 0 ? _buf.Substring((int)length) : _buf.Substring(1);
            }
        }
    }
}