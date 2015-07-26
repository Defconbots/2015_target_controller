using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using TargetControl.Models;

namespace TargetControl
{
    public interface IContestSelectTeamViewModel : IContestStateMachineViewModel
    {
        Team SelectedTeam { get; set; }
    }

    public sealed class ContestSelectTeamViewModel : Screen, IContestSelectTeamViewModel
    {
        private readonly IContestModel _contestModel;
        private readonly ITeamDatabaseSerializer _db;
        private readonly Func<IContestPendingRoundViewModel> _pendingRoundFunc;

        public ContestSelectTeamViewModel(ITeamDatabaseSerializer db, Func<IContestPendingRoundViewModel> pendingRoundFunc, IContestModel contestModel)
        {
            _db = db;
            _pendingRoundFunc = pendingRoundFunc;
            _contestModel = contestModel;
            DisplayName = "Contest";
            Teams = new ObservableCollection<Team>();

            _db.DatabaseUpdated += OnDatabaseUpdated;
            OnDatabaseUpdated();
        }

        public Team SelectedTeam { get; set; }

        public ObservableCollection<Team> Teams { get; set; }

        public event Action<IContestStateMachineViewModel> ChangeState;

        public void SelectTeam()
        {
            if (ChangeState != null)
            {
                _contestModel.SelectTeam(SelectedTeam);

                var vm = _pendingRoundFunc();
                ChangeState(vm);
            }
        }

        private void OnDatabaseUpdated()
        {
            Teams.Clear();
            foreach (var team in _db.Database.Teams)
                Teams.Add(team);

            if (SelectedTeam == null)
            {
                SelectedTeam = Teams.FirstOrDefault();
            }
        }
    }
}