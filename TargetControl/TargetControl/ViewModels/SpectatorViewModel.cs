using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

using TargetControl.Models;

namespace TargetControl
{
    public class SpectatorViewModel : Screen
    {
        private readonly IContest _contest;
        private readonly IContestModel _contestModel;
        private readonly ITeamDatabaseSerializer _db;

        public SpectatorViewModel(IContest contest, IContestModel contestModel, ITeamDatabaseSerializer db)
        {
            _contestModel = contestModel;
            _db = db;
            _contest = contest;

            Targets = new BindableCollection<ContestActiveRoundTargetViewModel>();
            Teams = new BindableCollection<Team>();

            _contest.WaveDataUpdated += RefreshTargets;
            _contestModel.TeamInfoUpdated += OnTeamInfoUpdated;

            _db.DatabaseUpdated += UpdateFromDatabase;
            UpdateFromDatabase();

        }

        public BindableCollection<Team> Teams { get; set; }

        //public TimeSpan ElapsedTime
        //{
        //    get { return DateTime.Now - _startTime; }
        //}

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
            NotifyOfPropertyChange(() => Team);
            NotifyOfPropertyChange(() => NumberLives);
            NotifyOfPropertyChange(() => WaveNumber);
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
