using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetControl
{
    public class StubSerialPortListProvider : ISerialPortListProvider
    {
        public string[] GetPortNames()
        {
            return new string[]
            {
                "COM1",
                "COM2"
            };
        }
    }
}
