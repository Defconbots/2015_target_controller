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
        Team Team { get; }
        int NumberLives { get; }
        int WaveNumber { get; }
        int TotalScore { get; }
        int WaveScore { get; }
        void Start();
    }

    public sealed class ContestActiveRoundViewModel : Screen, IContestActiveRoundViewModel
    {
        private readonly IContest _contest;
        private readonly IContestModel _contestModel;
        private readonly ITimer _timer;
        private readonly Func<IContestPendingRoundViewModel> _pendingFunc;
        private readonly DateTime _startTime;

        public ContestActiveRoundViewModel(IContest contest, IContestModel contestModel, ITimer timer, Func<IContestPendingRoundViewModel> pendingFunc)
        {
            _contest = contest;
            _contestModel = contestModel;
            _timer = timer;
            _pendingFunc = pendingFunc;

            Targets = new BindableCollection<ContestActiveRoundTargetViewModel>();

            _startTime = DateTime.Now;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
            _timer.Start();

            _contest.WaveDataUpdated += RefreshTargets;
            _contestModel.TeamInfoUpdated += OnTeamInfoUpdated;
        }

        public event Action<IContestStateMachineViewModel> ChangeState;

        public TimeSpan ElapsedTime
        {
            get { return DateTime.Now - _startTime; }
        }

        public Team Team
        {
            get { return _contestModel.Team; }
        }

        public int NumberLives
        {
            get { return _contestModel.NumberLives; }
        }

        public int WaveNumber
        {
            get { return _contestModel.WaveNumber; }
        }

        public int TotalScore
        {
            get { return _contestModel.Score + _contest.WaveData.Score; }
        }

        public int WaveScore
        {
            get { return _contest.WaveData.Score; }
        }

        public int MaxScore
        {
            get { return Math.Max(_contestModel.BestScore, TotalScore); }
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

            var succeeded = Targets.All(x => x.Health == 0);
            _contestModel.CompleteWave(succeeded, _contest.WaveData.Score);

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
                ChangeState(vm);
            }
        }

        private void Stop()
        {
            _timer.Stop();
            _contest.Stop();
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

        private void OnTeamInfoUpdated()
        {
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