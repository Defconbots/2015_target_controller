using System;
using System.Linq;
using Caliburn.Micro;
using TargetControl.Models;

namespace TargetControl
{
    public interface IContestPendingRoundViewModel : IContestStateMachineViewModel
    {
        Team Team { get; set; }
        int Score { get; set; }
        int NumberLives { get; set; }
        int WaveNumber { get; set; }
    }

    public sealed class ContestPendingRoundViewModel : Screen, IContestPendingRoundViewModel
    {
        private readonly ITeamDatabaseSerializer _db;
        private readonly Func<IContestSelectTeamViewModel> _selectTeam;
        private readonly Func<IContestActiveRoundViewModel> _activeFunc;
        private int _numberLives;
        private int _waveNumber;

        public ContestPendingRoundViewModel(ITeamDatabaseSerializer db,
            Func<IContestSelectTeamViewModel> selectTeam,
            Func<IContestActiveRoundViewModel> activeFunc)
        {
            _db = db;
            _selectTeam = selectTeam;
            _activeFunc = activeFunc;

            NumberLives = 3;
            WaveNumber = 1;
            Score = 0;
        }

        public event Action<IContestStateMachineViewModel> ChangeState;

        public Team Team { get; set; }

        public int Score { get; set; }

        public int NumberLives
        {
            get { return _numberLives; }
            set
            {
                if (value == _numberLives) return;
                _numberLives = value;
                NotifyOfPropertyChange(() => NumberLives);
            }
        }

        public int WaveNumber
        {
            get { return _waveNumber; }
            set
            {
                if (value == _waveNumber) return;
                _waveNumber = value;
                NotifyOfPropertyChange(() => WaveNumber);
            }
        }

        public void IncreaseNumLives()
        {
            NumberLives++;
        }

        public void DecreaseNumLives()
        {
            NumberLives--;
        }

        public void IncreaseWaveNumber()
        {
            WaveNumber++;
        }

        public void DecreaseWaveNumber()
        {
            WaveNumber--;
        }

        public void SaveResults()
        {
            _db.Update(db =>
            {
                var team = db.Teams.FirstOrDefault(x => x.Guid == Team.Guid);
                if (team != null)
                {
                    if (Score > team.QualScore)
                    {
                        team.QualScore = Score;
                    }
                }
            });

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
                vm.Team = Team;
                vm.WaveNumber = WaveNumber;
                vm.InitialScore = Score;
                vm.NumberLives = NumberLives;
                vm.Start();
                ChangeState(vm);
            }
        }
    }
}