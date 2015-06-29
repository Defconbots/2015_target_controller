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
        private readonly ITeamDatabaseSerializer _db;
        private readonly Func<IContestPendingRoundViewModel> _pendingRoundFunc;

        public ContestSelectTeamViewModel(ITeamDatabaseSerializer db, Func<IContestPendingRoundViewModel> pendingRoundFunc)
        {
            _db = db;
            _pendingRoundFunc = pendingRoundFunc;
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
                var vm = _pendingRoundFunc();
                vm.Team = SelectedTeam;
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