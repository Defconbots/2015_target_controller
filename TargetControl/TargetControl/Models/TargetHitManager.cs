using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace TargetControl
{
    public interface ITargetHitManager
    {
        event Action<char, string> OnHit;
        void SetRedLed(char address, bool state);
        void SetBlueLed(char address, bool state);
    }

    public class TargetHitManager : ITargetHitManager
    {
        private readonly ISerialCommandInterface _serialInterface;
        private readonly DispatcherTimer _timer;
        private readonly List<Target> _targets;
        private int _nextHitRead = 0;
        private bool _recvPacketLately;

        public TargetHitManager(ISerialCommandInterface serialInterface, DispatcherTimer timer)
        {
            _serialInterface = serialInterface;
            _timer = timer;

            _targets = Enumerable.Range(1, 3)
                .Select(x => new Target((char)('0' + x)))
                .ToList();

            _serialInterface.DataReceived += OnInfoReceived;

            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public event Action<char, string> OnHit;

        public void SetRedLed(char address, bool state)
        {
            var target = _targets.FirstOrDefault(x => x.Address == address);
            if (target != null)
            {
                target.RedLedCmd = state;
            }
        }

        public void SetBlueLed(char address, bool state)
        {
            var target = _targets.FirstOrDefault(x => x.Address == address);
            if (target != null)
            {
                target.BlueLedCmd = state;
            }
        }

        private void OnInfoReceived(SCIReadData data)
        {
            var target = _targets.FirstOrDefault(x => x.Address == data.Address);
            if (target != null)
            {
                if (data.Device == 'R')
                {
                    target.RedLedFb = (data.DataL != '0');
                }
                else if (data.Device == 'B')
                {
                    target.BlueLedFb = (data.DataL != '0');
                }
                else if (data.Device == 'I')
                {
                    var hitId = string.Format("{0}{1}", data.DataH, data.DataL);
                    if (hitId != "00")
                    {
                        _serialInterface.Write(data.Address, data.Device, '0', '0');
                        if (OnHit != null)
                        {
                            OnHit(data.Address, hitId);
                        }
                    }
                }
            }

            _recvPacketLately = true;
            Poll();
        }

        private void OnTimerTick(object sender, EventArgs eventArgs)
        {
            if (!_recvPacketLately)
            {
                Poll();
            }
        }

        private void Poll()
        {
            _recvPacketLately = false;
            var needsRedUpdate = _targets.FirstOrDefault(x => x.RedLedCmd != x.RedLedFb);
            if (needsRedUpdate != null)
            {
                var intensity = needsRedUpdate.RedLedCmd ? "99" : "00";
                _serialInterface.Write(needsRedUpdate.Address, 'R', intensity[0], intensity[1]);
                return;
            }

            var needsBlueUpdate = _targets.FirstOrDefault(x => x.BlueLedCmd != x.BlueLedFb);
            if (needsBlueUpdate != null)
            {
                var intensity = needsBlueUpdate.BlueLedCmd ? "99" : "00";
                _serialInterface.Write(needsBlueUpdate.Address, 'B', intensity[0], intensity[1]);
                return;
            }

            _serialInterface.Read(_targets[_nextHitRead].Address, 'I');
            _nextHitRead = (_nextHitRead + 1)%_targets.Count;
        }
    }

    public class Target
    {
        public Target(char address)
        {
            Address = address;
        }

        public char Address { get; private set; }
        public bool RedLedCmd { get; set; }
        public bool BlueLedCmd { get; set; }
        public bool RedLedFb { get; set; }
        public bool BlueLedFb { get; set; }
    }
}