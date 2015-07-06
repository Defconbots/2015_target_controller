using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using TargetControl.Models;

namespace TargetControl.Test
{
    [TestFixture]
    public class ContestActiveRoundTests
    {
        private StubPendingRoundVM _pendingRound;
        private ITimer _timer;
        private StubContest _contest;

        [SetUp]
        public void Setup()
        {
            _pendingRound = new StubPendingRoundVM();
            _timer = new Mock<ITimer>().Object;
            _contest = new StubContest();
        }

        [Test]
        public void WhenWaveCompleted_ExpectScoreAdded()
        {
            var vm = Create();
            vm.InitialScore = 0;
            _contest.WaveData = new CurrentWaveData
            {
                Score = 123456,
                Targets = new List<WaveTarget>
                {
                    new WaveTarget('0', 0)
                }
            };
            vm.Save();

            Assert.AreEqual(123456, _pendingRound.Score);
        }

        [Test]
        public void WhenWaveNotCompleted_ExpectBestScoreAdded()
        {
            var vm = Create();
            vm.InitialScore = 0;
            vm.InitialBestScore = 100000;
            _contest.WaveData = new CurrentWaveData
            {
                Score = 123456,
                Targets = new List<WaveTarget>
                {
                    new WaveTarget('0', 3)
                }
            };
            vm.Save();

            Assert.AreEqual(0, _pendingRound.Score);
            Assert.AreEqual(123456, _pendingRound.BestScore);
        }

        [Test]
        public void WhenWaveNotCompletedWithBetterBestScore_ExpectBestScoreIgnored()
        {
            var vm = Create();
            vm.InitialScore = 0;
            vm.InitialBestScore = 500000;
            _contest.WaveData = new CurrentWaveData
            {
                Score = 123456,
                Targets = new List<WaveTarget>
                {
                    new WaveTarget('0', 3)
                }
            };
            vm.Save();

            Assert.AreEqual(0, _pendingRound.Score);
            Assert.AreEqual(500000, _pendingRound.BestScore);
        }

        private ContestActiveRoundViewModel Create()
        {
            var vm = new ContestActiveRoundViewModel(_contest, _timer, () => _pendingRound);
            vm.ChangeState += model => { };
            return vm;
        }
    }

    public class StubPendingRoundVM : IContestPendingRoundViewModel
    {
        public event Action<IContestStateMachineViewModel> ChangeState;

        public Team Team { get; set; }

        public int Score { get; set; }

        public int BestScore { get; set; }

        public int NumberLives { get; set; }

        public int WaveNumber { get; set; }
    }

    public class StubContest : IContest
    {
        private CurrentWaveData _waveData;

        public void Dispose()
        {
        }

        public CurrentWaveData WaveData
        {
            get { return _waveData; }
            set
            {
                _waveData = value;
                if (WaveDataUpdated != null)
                    WaveDataUpdated();
            }
        }

        public event Action WaveDataUpdated;
        public void Start(string teamId, int waveNumber)
        {
        }

        public void Resume()
        {
        }

        public void Stop()
        {
        }
    }
}
