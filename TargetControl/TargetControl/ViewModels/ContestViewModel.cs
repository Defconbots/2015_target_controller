using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Caliburn.Micro;

using TargetControl.Models;

namespace TargetControl
{
    public sealed class ContestViewModel : Screen, IMainScreenTabItem
    {
        private readonly Func<ContestSelectTeamViewModel> _createSelectTeamViewModel;
        private object _currentState;

        public ContestViewModel(Func<ContestSelectTeamViewModel> createSelectTeamViewModel)
        {
            _createSelectTeamViewModel = createSelectTeamViewModel;
            DisplayName = "Contest";

            SelectTeam();
        }

        private void SelectTeam()
        {
            CurrentState = _createSelectTeamViewModel();
            //CurrentState.TeamSelected += CreateRound;
        }

        public object CurrentState
        {
            get { return _currentState; }
            set
            {
                if (Equals(value, _currentState))
                {
                    return;
                }
                _currentState = value;
                NotifyOfPropertyChange();
            }
        }
    }

    public sealed class ContestSelectTeamViewModel : Screen
    {
        private readonly ITeamDatabaseSerializer _db;

        public ContestSelectTeamViewModel(ITeamDatabaseSerializer db)
        {
            _db = db;
            DisplayName = "Contest";
            Teams = new ObservableCollection<Team>();

            _db.DatabaseUpdated += OnDatabaseUpdated;
            OnDatabaseUpdated();
        }

        public Team SelectedTeam { get; set; }

        public ObservableCollection<Team> Teams { get; set; }

        public void SelectTeam()
        {
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

    public sealed class ContestPendingRoundViewModel : Screen
    {
        public Team Team { get; private set; }
    }
}