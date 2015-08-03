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
    public class TargetHitManagerTests
    {
        private Mock<ISerialCommandInterface> _serialInterface;
        private StubTimer _timer;
        private TargetHitManager _hitManager;

        [SetUp]
        public void Setup()
        {
            _serialInterface = new Mock<ISerialCommandInterface>();
            _timer = new StubTimer();
            _hitManager = new TargetHitManager(_serialInterface.Object, _timer);
        }

        [Test]
        public void WhenNotEnabled_ExpectNoSerialTraffic()
        {
            _timer.RaiseTick();

            _serialInterface.Verify(x => x.Read(It.IsAny<char>(), It.IsAny<char>()), Times.Never());
            _serialInterface.Verify(x => x.Write(It.IsAny<char>(), It.IsAny<char>(), It.IsAny<char>(), It.IsAny<char>()), Times.Never());
        }

        [Test]
        public void WhenNotEnabledAndHitReceived_ExpectNoSerialTraffic()
        {
            _serialInterface.Raise(x => x.DataReceived += null, new SCIReadData
            {
                Address = '1',
                Device = 'I',
            });

            _serialInterface.Verify(x => x.Read(It.IsAny<char>(), It.IsAny<char>()), Times.Never());
            _serialInterface.Verify(x => x.Write(It.IsAny<char>(), It.IsAny<char>(), It.IsAny<char>(), It.IsAny<char>()), Times.Never());
        }
    }
}
