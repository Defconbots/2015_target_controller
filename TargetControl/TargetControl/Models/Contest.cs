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
        public const int NumTargets = 5;

        private static readonly TimeSpan HitOffDelay = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan ResetSpeedDuration = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan TrainStopCooldown = TimeSpan.FromSeconds(30);

        private readonly ITargetHitManager _targetHitManager;
        private readonly ITimer _resetTimer;
        private readonly ITimer _resetSpeedTimer;
        private readonly ITimer _calledShotCooldownTimer;
        private string _teamId;

        public Contest(ITargetHitManager targetManager, ITimer resetTimer, ITimer resetSpeedTimer, ITimer calledShotCooldownTimer)
        {
            _targetHitManager = targetManager;
            _resetTimer = resetTimer;
            _resetSpeedTimer = resetSpeedTimer;
            _calledShotCooldownTimer = calledShotCooldownTimer;

            _resetTimer.Interval = HitOffDelay;
            _resetTimer.Tick += OnResetTick;

            _resetSpeedTimer.Interval = ResetSpeedDuration;
            _resetSpeedTimer.Tick += OnResetSpeedTick;

            _calledShotCooldownTimer.Interval = TrainStopCooldown;
            _calledShotCooldownTimer.Tick += OnCalledShotCooldownComplete;

            _targetHitManager.OnHit += OnHit;

            WaveData = new CurrentWaveData
            {
                Targets = Enumerable.Range(1, NumTargets)
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
                Targets = Enumerable.Range(1, NumTargets)
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
            _targetHitManager.SetSpeed(TargetSpeed.Normal);
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

        private void OnResetSpeedTick(object sender, EventArgs e)
        {
            _targetHitManager.SetSpeed(TargetSpeed.Normal);
            _resetSpeedTimer.Stop();
        }

        private void OnCalledShotCooldownComplete(object sender, EventArgs e)
        {
            _calledShotCooldownTimer.Stop();
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

                        _targetHitManager.SetSpeed(TargetSpeed.Fast);
                        _resetSpeedTimer.Start();
                    }
                    else if (hitType == TargetHitType.Called)
                    {
                        hitTarget.Health--;
                        WaveData.Score += ScoreForCalledShot;

                        if (!_calledShotCooldownTimer.IsEnabled)
                        {
                            _targetHitManager.SetSpeed(TargetSpeed.Stop);
                            _resetSpeedTimer.Start();
                            _calledShotCooldownTimer.Start();
                        }
                    }
                    else
                    {
                        hitTarget.Health--;
                        WaveData.Score += ScoreForHit;
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
                _targetHitManager.SetBlueLed(target.Address, hitType == TargetHitType.Miscalled && target.Health > 0);
            }

            if (WaveDataUpdated != null)
            {
                WaveDataUpdated();
            }
        }
    }
}
