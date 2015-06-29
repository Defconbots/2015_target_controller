using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TargetControl
{
    public class CurrentWaveData
    {
        public List<WaveTarget> Targets { get; set; }

        public int Score { get; set; }
    }

    public class WaveTarget
    {
        public WaveTarget(char address, int health)
        {
            Address = address;
            Health = health;
        }

        public char Address { get; set; }

        public int Health { get; set; }
    }

    public interface IContest : IDisposable
    {
        CurrentWaveData WaveData { get; set; }
        event Action WaveDataUpdated;
        void Start(string teamId, int waveNumber);
        void Resume();
        void Stop();
    }

    public class Contest : IContest
    {
        private const int ScoreForMiscall = 10000;
        private const int ScoreForCalledShot = 150000;
        private const int ScoreForHit = 100000;

        private readonly ITargetHitManager _targetHitManager;
        private readonly DispatcherTimer _resetTimer;
        private static readonly TimeSpan HitOffDelay = TimeSpan.FromSeconds(3);
        private string _teamId;

        public Contest(ITargetHitManager targetManager, DispatcherTimer resetTimer)
        {
            _resetTimer = resetTimer;
            _targetHitManager = targetManager;

            _resetTimer = resetTimer;
            _resetTimer.Interval = HitOffDelay;
            _resetTimer.Tick += OnResetTick;

            _targetHitManager.OnHit += OnHit;

            WaveData = new CurrentWaveData
            {
                Targets = Enumerable.Range(1, 3)
                    .Select(i => new WaveTarget((char)('0' + i), 1))
                    .ToList()
            };

            OnResetTick(null, null);
        }

        public CurrentWaveData WaveData { get; set; }

        public event Action WaveDataUpdated;

        public void Dispose()
        {
            _resetTimer.Stop();
            _targetHitManager.OnHit -= OnHit;
        }

        public void Start(string teamId, int waveNumber)
        {
            _teamId = teamId;
            WaveData = new CurrentWaveData
            {
                Targets = Enumerable.Range(1, 3)
                    .Select(i => new WaveTarget((char)('0' + i), waveNumber))
                    .ToList()
            };
            Resume();

            if (WaveDataUpdated != null)
            {
                WaveDataUpdated();
            }
        }

        public void Resume()
        {
            _targetHitManager.SetSpeed(TargetSpeed.Go);
            UpdateTargetsToNormal();
        }

        public void Stop()
        {
            _targetHitManager.SetSpeed(TargetSpeed.Stop);
            foreach (var target in WaveData.Targets)
            {
                _targetHitManager.SetRedLed(target.Address, false);
                _targetHitManager.SetBlueLed(target.Address, false);
            }
        }

        private void OnResetTick(object sender, EventArgs e)
        {
            UpdateTargetsToNormal();
            _resetTimer.Stop();
        }

        private void UpdateTargetsToNormal()
        {
            foreach (var target in WaveData.Targets)
            {
                _targetHitManager.SetRedLed(target.Address, false);
                _targetHitManager.SetBlueLed(target.Address, target.Health > 0);
            }
        }

        private void OnHit(char address, string hitId, TargetHitType hitType)
        {
            if (_resetTimer.IsEnabled)
            {
                return;
            }

            Console.WriteLine("**HIT** {0} {1}", address, hitId);
            if (hitId != _teamId)
            {
                Console.WriteLine("-- wrong hit id");
                return;
            }

            var hitTarget = WaveData.Targets.FirstOrDefault(x => x.Address == address);
            if (hitTarget != null)
            {
                if (hitTarget.Health > 0)
                {
                    if (hitType == TargetHitType.Miscalled)
                    {
                        WaveData.Score -= ScoreForMiscall;
                    }
                    else
                    {
                        hitTarget.Health--;
                        WaveData.Score += hitType == TargetHitType.Called ? ScoreForCalledShot : ScoreForHit;
                    }
                }
                else
                {
                    Console.WriteLine("-- already at zero health");
                    return;
                }
            }

            _resetTimer.Start();
            foreach (var target in WaveData.Targets)
            {
                _targetHitManager.SetRedLed(target.Address, address == target.Address);
                _targetHitManager.SetBlueLed(target.Address, false);
            }

            if (WaveDataUpdated != null)
            {
                WaveDataUpdated();
            }
        }
    }
}
