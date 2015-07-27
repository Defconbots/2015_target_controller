using System;
using System.Linq;
using Caliburn.Micro;
using TargetControl.Models;

namespace TargetControl
{
    public interface IContestPendingRoundViewModel : IContestStateMachineViewModel
    {
    }

    public sealed class ContestPendingRoundViewModel : Screen, IContestPendingRoundViewModel
    {
        private readonly IContestModel _contestModel;
        private readonly Func<IContestSelectTeamViewModel> _selectTeam;
        private readonly Func<IContestActiveRoundViewModel> _activeFunc;

        public ContestPendingRoundViewModel(
            IContestModel contestModel,
            Func<IContestSelectTeamViewModel> selectTeam,
            Func<IContestActiveRoundViewModel> activeFunc)
        {
            _contestModel = contestModel;
            _selectTeam = selectTeam;
            _activeFunc = activeFunc;

            _contestModel.TeamInfoUpdated += OnTeamInfoUpddated;
        }

        private void OnTeamInfoUpddated()
        {
            NotifyOfPropertyChange(() => Team);
            NotifyOfPropertyChange(() => Score);
            NotifyOfPropertyChange(() => BestScore);
            NotifyOfPropertyChange(() => NumberLives);
            NotifyOfPropertyChange(() => WaveNumber);
        }

        public event Action<IContestStateMachineViewModel> ChangeState;

        public Team Team
        {
            get { return _contestModel.Team; }
        }

        public int Score
        {
            get { return _contestModel.Score; }
        }

        public int BestScore
        {
            get { return _contestModel.BestScore; }
        }

        public bool IsFinal
        {
            get { return _contestModel.IsFinal; }
            set { _contestModel.IsFinal = value; }
        }

        public int NumberLives
        {
            get { return _contestModel.NumberLives; }
        }

        public int WaveNumber
        {
            get { return _contestModel.WaveNumber; }
        }

        public void IncreaseNumLives()
        {
            _contestModel.IncreaseNumLives();
        }

        public void DecreaseNumLives()
        {
            _contestModel.DecreaseNumLives();
        }

        public void IncreaseWaveNumber()
        {
            _contestModel.IncreaseWaveNumber();
        }

        public void DecreaseWaveNumber()
        {
            _contestModel.DecreaseWaveNumber();
        }

        public void SaveResults()
        {
            _contestModel.SaveChanges();

            if (ChangeState != null)
            {
                var vm = _selectTeam();
                ChangeState(vm);
            }
        }

        public void StartRound()
        {
            if (ChangeState != null)
            {
                var vm = _activeFunc();
                vm.Start();
                ChangeState(vm);
            }
        }
    }
}