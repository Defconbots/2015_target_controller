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

    public class Contest
    {
        private readonly ITargetHitManager _targetHitManager;
        private readonly DispatcherTimer _resetTimer;
        private static readonly TimeSpan HitOffDelay = TimeSpan.FromSeconds(3);

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

        private void OnResetTick(object sender, EventArgs e)
        {
            foreach (var target in WaveData.Targets)
            {
                _targetHitManager.SetRedLed(target.Address, false);
                _targetHitManager.SetBlueLed(target.Address, target.Health > 0);
            }
            _resetTimer.Stop();
        }

        private void OnHit(char address, string hitId)
        {
            if (_resetTimer.IsEnabled)
            {
                return;
            }

            Console.WriteLine("**HIT** {0} {1}", address, hitId);

            var hitTarget = WaveData.Targets.FirstOrDefault(x => x.Address == address);
            if (hitTarget != null)
            {
                if (hitTarget.Health > 0)
                {
                    hitTarget.Health--;
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
        }
    }
}
