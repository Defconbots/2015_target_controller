using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetControl
{
    public interface ISerial
    {
        void SendPacket(string buf);
        event Action<string> SerialDataReceived;
    }
    
    public class SerialPortSerial
    {
    }
}
