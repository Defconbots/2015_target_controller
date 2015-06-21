using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

using MahApps.Metro.Controls;

using TargetControl.Models;

namespace TargetControl
{
    public sealed class TeamsViewModel : Screen, IMainScreenTabItem
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly TeamDatabaseSerializer _db;

        public BindableCollection<Team> Teams { get; set; }

        public TeamsViewModel(IEventAggregator eventAggregator, TeamDatabaseSerializer db)
        {
            _eventAggregator = eventAggregator;
            _db = db;
            DisplayName = "Teams";

            Teams = new BindableCollection<Team>();

            _db.DatabaseUpdated += UpdateFromDatabase;
            _db.Load();
        }

        public void AddTeam()
        {
            _eventAggregator.PublishOnUIThread(new ShowFlyoutEvent
            {
                ViewModel = new AddTeamViewModel(AddTeamFinal),
                Position = Position.Left,
                IsModal = true
            });
        }

        public void EditTeam(Team team)
        {
            _eventAggregator.PublishOnUIThread(new ShowFlyoutEvent
            {
                ViewModel = new AddTeamViewModel(EditTeamFinal, team),
                Position = Position.Left,
                IsModal = true
            });
        }

        public void RemoveTeam(Team team)
        {
            _db.Update(db => db.Teams.RemoveAll(t => t.Guid == team.Guid));
        }

        public void AddTeamFinal(AddTeamViewModel teamVM)
        {
            var team = new Team
            {
                Guid = teamVM.Guid,
                Name = teamVM.TeamName,
                HitId = teamVM.HitId,
                Members = teamVM.Members.Select(x => new TeamMember
                {
                    Name = x.Name,
                }).ToList()
            };

            _db.Update(db => db.Teams.Add(team));

            _eventAggregator.PublishOnUIThread(new RemoveFlyoutEvent
            {
                ViewModel = teamVM,
            });
        }

        public void EditTeamFinal(AddTeamViewModel teamVM)
        {
            _db.Update(db =>
            {
                var team = db.Teams.FirstOrDefault(t => t.Guid == teamVM.Guid);
                if (team != null)
                {
                    team.Name = teamVM.TeamName;
                    team.HitId = teamVM.HitId;
                    team.Members = teamVM.Members.Select(x => new TeamMember
                    {
                        Name = x.Name,
                    }).ToList();
                }
            });

            _eventAggregator.PublishOnUIThread(new RemoveFlyoutEvent
            {
                ViewModel = teamVM,
            });
        }

        private void UpdateFromDatabase()
        {
            Teams.Clear();
            foreach (var team in _db.Database.Teams)
            {
                Teams.Add(team);
            }
        }
    }

}
