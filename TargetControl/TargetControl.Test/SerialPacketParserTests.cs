using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

namespace TargetControl.Test
{
    [TestFixture]
    public class SerialPacketParserTests
    {
        private SerialPacketParser _parser;
        private Mock<ISerialPacketHandler> _handler;

        [SetUp]
        public void Setup()
        {
            _parser = new SerialPacketParser();
            _handler = new Mock<ISerialPacketHandler>();

            _handler.Setup(x => x.CheckPacket(It.IsAny<string>()))
                .Returns((int?)null);
        }

        [Test]
        public void WhenAddingOneByte_ExpectPacketCheckCalled()
        {
            _parser.AddData("1", _handler.Object);

            _handler.Verify(x => x.CheckPacket("1"));
        }

        [Test]
        public void WhenHandlerReturnsPacketLength_ExpectDataRemoved()
        {
            _handler.Setup(x => x.CheckPacket("1234")).Returns(4);

            _parser.AddData("1234", _handler.Object);
            _parser.AddData("5678", _handler.Object);

            _handler.Verify(x => x.CheckPacket("5678"));
        }

        [Test]
        public void WhenAddingByteByByte_ExpectWholeString()
        {
            _parser.AddData("1", _handler.Object);
            _parser.AddData("2", _handler.Object);

            _handler.Verify(x => x.CheckPacket("12"));
        }
    }
}
