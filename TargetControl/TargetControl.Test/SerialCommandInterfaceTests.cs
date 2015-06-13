using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

namespace TargetControl.Test
{
    [TestFixture]
    public class SerialCommandInterfaceTests
    {
        private SerialCommandInterface _sci;
        private Mock<ISerial> _serial;

        [SetUp]
        public void Setup()
        {
            _serial = new Mock<ISerial>();
            _sci = new SerialCommandInterface(_serial.Object, new SerialPacketParser());
        }

        [Test]
        public void WhenConnecting_ExpectOpensSerialPort()
        {
            _sci.Connect();
            
            _serial.Verify(x => x.Open());
        }

        [Test]
        public void WhenSendingRead_ExpectDataSentToSerial()
        {
            _sci.Read('0', 'V');

            _serial.Verify(x => x.SendPacket("{0V}"));
        }

        [Test]
        public void WhenValidReadResponse_ExpectEventWithData()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Read('0', 'V');
            _serial.Raise(x => x.SerialDataReceived += null, "(01)");

            Assert.AreEqual(new SCIReadData
            {
                Address = '0',
                Device = 'V',
                DataH = '0',
                DataL = '1'
            }, data);
        }

        [Test]
        public void WhenValidReadResponseByteByByte_ExpectEventWithData()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Read('0', 'V');
            _serial.Raise(x => x.SerialDataReceived += null, "(");
            _serial.Raise(x => x.SerialDataReceived += null, "0");
            _serial.Raise(x => x.SerialDataReceived += null, "1");
            _serial.Raise(x => x.SerialDataReceived += null, ")");

            Assert.AreEqual(new SCIReadData
            {
                Address = '0',
                Device = 'V',
                DataH = '0',
                DataL = '1'
            }, data);
        }

        [Test]
        public void WhenTwoReadResponses_ExpectEventWithData()
        {
            _sci.Read('0', 'V');
            _serial.Raise(x => x.SerialDataReceived += null, "(01)");

            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Read('1', 'X');
            _serial.Raise(x => x.SerialDataReceived += null, "(23)");

            Assert.AreEqual(new SCIReadData
            {
                Address = '1',
                Device = 'X',
                DataH = '2',
                DataL = '3'
            }, data);
        }

        [Test]
        public void WhenInvalidReadResponse_ExpectNoEventRaised()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Read('0', 'V');
            _serial.Raise(x => x.SerialDataReceived += null, "(0");

            Assert.AreEqual(null, data);
        }

        [Test]
        public void WhenSendingWrite_ExpectDataSentToSerial()
        {
            _sci.Write('1', 'R', '0', '0');

            _serial.Verify(x => x.SendPacket("[1R00]"));
        }

        [Test]
        public void WhenValidWriteResponse_ExpectEventWithData()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Write('2', 'R', '0', '0');
            _serial.Raise(x => x.SerialDataReceived += null, "<2>");

            Assert.AreEqual(new SCIReadData
            {
                Address = '2',
                Device = 'R',
                DataH = '0',
                DataL = '0'
            }, data);
        }

        [Test]
        public void WhenReceivingNothing_ExpectNothingHappens()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _serial.Raise(x => x.SerialDataReceived += null, "");
        
            Assert.IsNull(data);
        }

        [Test]
        public void WhenReceivingInvalidReadResponse_ExpectNothingHappens()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Read('3', 'R');
            _serial.Raise(x => x.SerialDataReceived += null, "(111");
        
            Assert.IsNull(data);
        }

        [Test]
        public void WhenReceivingPartialWriteResponse_ExpectNothingHappens()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Write('3', 'R', '0', '0');
            _serial.Raise(x => x.SerialDataReceived += null, "<1");
        
            Assert.IsNull(data);
        }

        [Test]
        public void WhenReceivingInvalidWriteResponse_ExpectNothingHappens()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Write('3', 'R', '0', '0');
            _serial.Raise(x => x.SerialDataReceived += null, "<1X");
        
            Assert.IsNull(data);
        }

        [Test]
        public void WhenReceivingWriteResponseFromWrongDevice_ExpectNothingHappens()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _sci.Write('3', 'R', '0', '0');
            _serial.Raise(x => x.SerialDataReceived += null, "<1>");
        
            Assert.IsNull(data);
        }

        [Test]
        public void WhenReceivingInvalidResponse_ExpectNothingHappens()
        {
            object data = null;
            _sci.DataReceived += v => data = v;
            _serial.Raise(x => x.SerialDataReceived += null, "X");
        
            Assert.IsNull(data);
        }
    }
}
