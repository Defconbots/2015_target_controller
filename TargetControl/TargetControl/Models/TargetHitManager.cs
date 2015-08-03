using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace TargetControl
{
    public interface ITargetHitManager
    {
        event Action<char, string, TargetHitType> OnHit;
        void SetRedLed(char address, bool state);
        void SetBlueLed(char address, bool state);
        void SetSpeed(TargetSpeed speed);
    }

    public enum TargetHitType
    {
        Wildcard,
        Called,
        Miscalled
    }

    public class TargetHitManager : ITargetHitManager
    {
        private readonly ISerialCommandInterface _serialInterface;
        private readonly DispatcherTimer _timer;
        private readonly List<Target> _targets;
        private TargetSpeed _speedCmd;
        private TargetSpeed _speedFb;
        private int _nextHitRead = 0;
        private bool _recvPacketLately;

        public TargetHitManager(ISerialCommandInterface serialInterface, DispatcherTimer timer)
        {
            _serialInterface = serialInterface;
            _timer = timer;

            _targets = Enumerable.Range(1, Contest.NumTargets)
                .Select(x => new Target((char)('0' + x)))
                .ToList();

            _serialInterface.DataReceived += OnInfoReceived;

            _timer.Interval = TimeSpan.FromMilliseconds(300);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public event Action<char, string, TargetHitType> OnHit;

        public void SetSpeed(TargetSpeed speed)
        {
            _speedCmd = speed;
        }

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
            _recvPacketLately = true;

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
                    var hitType = TargetHitType.Wildcard;
                    if (data.DataH >= 'a' && data.DataH <= 'j')
                    {
                        hitType = TargetHitType.Miscalled;
                        data.DataH -= (char)('a' - '0');
                    }
                    else if (data.DataH >= 'A' && data.DataH <= 'J')
                    {
                        hitType = TargetHitType.Called;
                        data.DataH -= (char)('A' - '0');
                    }

                    var hitId = string.Format("{0}{1}", data.DataH, data.DataL);
                    if (hitId != "00")
                    {
                        if (OnHit != null)
                        {
                            OnHit(data.Address, hitId, hitType);
                        }

                        _serialInterface.Write(data.Address, data.Device, '0', '0');
                        return;
                    }
                }
            }

            if (data.Address == '0')
            {
                if (data.Device == 'S')
                {
                    var speed = "" + data.DataH + data.DataL;
                    _speedFb = speed == "ST" ? TargetSpeed.Stop :
                               speed == "F1" ? TargetSpeed.Fast : 
                               speed == "F2" ? TargetSpeed.Normal :
                                               TargetSpeed.Stop;
                }
            }

            Poll();
        }

        private void OnTimerTick(object sender, EventArgs eventArgs)
        {
            if (!_recvPacketLately)
            {
                Poll();
            }
            _recvPacketLately = false;
        }

        private void Poll()
        {
            if (_speedCmd != _speedFb)
            {
                var speed = _speedCmd == TargetSpeed.Stop ? "ST" :
                    _speedCmd == TargetSpeed.Normal ? "F1" :
                    _speedCmd == TargetSpeed.Fast ? "F2" : "";
                _serialInterface.Write('0', 'S', speed[0], speed[1]);
                return;
            }

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

    public enum TargetSpeed
    {
        Stop,
        Normal,
        Fast
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
        public bool? RedLedFb { get; set; }
        public bool? BlueLedFb { get; set; }
    }
}