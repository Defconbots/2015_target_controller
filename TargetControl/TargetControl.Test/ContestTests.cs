using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

namespace TargetControl.Test
{
    [TestFixture]
    public class ContestTests
    {
        private Mock<ITargetHitManager> _targetManager;
        private StubTimer _resetHitTimer;
        private StubTimer _resetSpeedTimer;
        private StubTimer _cooldownTimer;
        private Contest _contest;

        [SetUp]
        public void Setup()
        {
            _targetManager = new Mock<ITargetHitManager>();
            _resetHitTimer = new StubTimer();
            _resetSpeedTimer = new StubTimer();
            _cooldownTimer = new StubTimer();
            _contest = new Contest(_targetManager.Object, _resetHitTimer, _resetSpeedTimer, _cooldownTimer);
            _contest.Start("25", 1);
        }

        [Test]
        public void WhenCalledShot_ExpectScoreIncreased()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Called);

            Assert.AreEqual(150000, _contest.WaveData.Score);
        }

        [Test]
        public void WhenWildcardShot_ExpectScoreIncreased()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Wildcard);

            Assert.AreEqual(100000, _contest.WaveData.Score);
        }

        [Test]
        public void WhenMisCalledShot_ExpectScoreDecreased()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Miscalled);

            Assert.AreEqual(-10000, _contest.WaveData.Score);
        }

        [Test]
        public void WhenCalledShotOnDeadTarget_ExpectTrainSameSpeed()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Wildcard);
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Called);

            _targetManager.Verify(x => x.SetSpeed(TargetSpeed.Stop), Times.Never());
        }

        [Test]
        public void WhenCalledShot_ExpectTrainStopped()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Called);

            _targetManager.Verify(x => x.SetSpeed(TargetSpeed.Stop));
        }

        [Test]
        public void WhenElapsedAfterCalledShot_ExpectTrainReturnsToNormal()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Called);
            _targetManager.ResetCalls();
            _resetSpeedTimer.RaiseTick();

            _targetManager.Verify(x => x.SetSpeed(TargetSpeed.Normal));
        }

        [Test]
        public void WhenMisCalledShot_ExpectTrainSpedUp()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Miscalled);

            _targetManager.Verify(x => x.SetSpeed(TargetSpeed.Fast));
        }

        [Test]
        public void WhenElapsedAfterMisCalledShot_ExpectTrainReturnsToNormal()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Miscalled);
            _targetManager.ResetCalls();
            _resetSpeedTimer.RaiseTick();

            _targetManager.Verify(x => x.SetSpeed(TargetSpeed.Normal));
        }

        [Test]
        public void WhenCalledShotBefore30Seconds_ExpectTrainNotStopped()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Called);
            _resetSpeedTimer.RaiseTick();
            _targetManager.ResetCalls();

            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Called);

            _targetManager.Verify(x => x.SetSpeed(TargetSpeed.Stop), Times.Never());
        }

        [Test]
        public void WhenCalledShotAfter30Seconds_ExpectTrainStopped()
        {
            _targetManager.Raise(x => x.OnHit += null, '1', "25", TargetHitType.Called);
            _resetHitTimer.RaiseTick();
            _resetSpeedTimer.RaiseTick();
            _cooldownTimer.RaiseTick();

            _targetManager.ResetCalls();
            _targetManager.Raise(x => x.OnHit += null, '2', "25", TargetHitType.Called);

            _targetManager.Verify(x => x.SetSpeed(TargetSpeed.Stop));
        }
    }
}
