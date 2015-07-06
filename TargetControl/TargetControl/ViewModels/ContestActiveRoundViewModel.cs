using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Caliburn.Micro;
using TargetControl.Models;

namespace TargetControl
{
    public interface IContestActiveRoundViewModel : IContestStateMachineViewModel
    {
        TimeSpan ElapsedTime { get; }
        Team Team { get; set; }
        int NumberLives { get; set; }
        int WaveNumber { get; set; }
        int InitialScore { get; set; }
        int InitialBestScore { get; set; }
        int TotalScore { get; }
        int WaveScore { get; }
        void Start();
    }

    public sealed class ContestActiveRoundViewModel : Screen, IContestActiveRoundViewModel
    {
        private readonly IContest _contest;
        private readonly ITimer _timer;
        private readonly Func<IContestPendingRoundViewModel> _pendingFunc;
        private readonly DateTime _startTime;

        public ContestActiveRoundViewModel(IContest contest, ITimer timer, Func<IContestPendingRoundViewModel> pendingFunc)
        {
            _contest = contest;
            _timer = timer;
            _pendingFunc = pendingFunc;

            Targets = new BindableCollection<ContestActiveRoundTargetViewModel>();

            _startTime = DateTime.Now;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
            _timer.Start();

            _contest.WaveDataUpdated += RefreshTargets;
        }

        public event Action<IContestStateMachineViewModel> ChangeState;

        public TimeSpan ElapsedTime
        {
            get { return DateTime.Now - _startTime; }
        }

        public Team Team { get; set; }

        public int NumberLives { get; set; }

        public int WaveNumber { get; set; }

        public int InitialScore { get; set; }

        public int InitialBestScore { get; set; }

        public int TotalScore
        {
            get { return InitialScore + _contest.WaveData.Score; }
        }

        public int WaveScore
        {
            get { return _contest.WaveData.Score; }
        }

        public int MaxScore
        {
            get { return Math.Max(InitialBestScore, TotalScore); }
        }

        public BindableCollection<ContestActiveRoundTargetViewModel> Targets { get; private set; }

        public void Start()
        {
            _contest.Start(Team.HitId.ToString("00"), WaveNumber);
        }

        public void Pause()
        {
            _contest.Stop();
        }

        public void Save()
        {
            Stop();

            if (Targets.All(x => x.Health == 0))
            {
                WaveNumber++;
                InitialScore += _contest.WaveData.Score;
            }
            else
            {
                NumberLives--;
                InitialBestScore = Math.Max(InitialBestScore, InitialScore + _contest.WaveData.Score);
            }

            GoToPending();
        }

        public void Abort()
        {
            Stop();
            GoToPending();
        }

        private void GoToPending()
        {
            if (ChangeState != null)
            {
                var vm = _pendingFunc();
                vm.Team = Team;
                vm.WaveNumber = WaveNumber;
                vm.Score = InitialScore;
                vm.BestScore = InitialBestScore;
                vm.NumberLives = NumberLives;
                ChangeState(vm);
            }
        }

        private void Stop()
        {
            _timer.Stop();
            _contest.WaveDataUpdated -= RefreshTargets;
        }

        private void RefreshTargets()
        {
            Targets.Clear();
            Targets.AddRange(_contest.WaveData.Targets.Select(x => new ContestActiveRoundTargetViewModel
            {
                Health = x.Health
            }));

            NotifyOfPropertyChange(() => WaveScore);
            NotifyOfPropertyChange(() => TotalScore);
            NotifyOfPropertyChange(() => MaxScore);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => ElapsedTime);
        }
    }

    public class ContestActiveRoundTargetViewModel : PropertyChangedBase
    {
        private int _health;

        public int Health
        {
            get { return _health; }
            set
            {
                if (value == _health) return;
                _health = value;
                NotifyOfPropertyChange(() => Health);
            }
        }
    }
}