using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetControl.Models
{
    public interface IContestModel
    {
        Team Team { get; }
        int Score { get; }
        int NumberLives { get; }
        int WaveNumber { get; }
        int BestScore { get; }

        event Action TeamInfoUpdated;

        void IncreaseNumLives();
        void DecreaseNumLives();
        void IncreaseWaveNumber();
        void DecreaseWaveNumber();
        void SaveChanges();
        void SelectTeam(Team team);
        void CompleteWave(bool succeeded, int score);
    }

    public class ContestModel : IContestModel
    {
        private readonly ITeamDatabaseSerializer _db;
        private readonly ILogger _log;

        public ContestModel(ILogger log, ITeamDatabaseSerializer db)
        {
            _log = log;
            _db = db;
        }

        public Team Team { get; private set; }
        public int Score { get; private set; }
        public int NumberLives { get; private set; }
        public int WaveNumber { get; private set; }
        public int BestScore { get; private set; }

        public event Action TeamInfoUpdated;

        public void SelectTeam(Team team)
        {
            Team = team;
            NumberLives = 3;
            WaveNumber = 1;
            Score = 0;
            BestScore = 0;
            RaiseTeamInfoUpdated();
        }

        public void CompleteWave(bool succeeded, int score)
        {
            _log.Log("Team completed wave {0} || score for round: {1}, success: {2}", WaveNumber, score, succeeded);
            BestScore = Math.Max(BestScore, Score + score);
            if (succeeded)
            {
                WaveNumber++;
                Score += score;
            }
            else
            {
                NumberLives--;
            }
        }

        public void IncreaseNumLives()
        {
            NumberLives++;
            RaiseTeamInfoUpdated();
        }

        public void DecreaseNumLives()
        {
            NumberLives--;
            RaiseTeamInfoUpdated();
        }

        public void IncreaseWaveNumber()
        {
            WaveNumber++;
            RaiseTeamInfoUpdated();
        }

        public void DecreaseWaveNumber()
        {
            WaveNumber--;
            RaiseTeamInfoUpdated();
        }

        public void SaveChanges()
        {
            _log.Log("Team completed: {0} with score {1}\n", Team.Name, BestScore);
            _db.Update(db =>
            {
                var team = db.Teams.FirstOrDefault(x => x.Guid == Team.Guid);
                if (team != null)
                {
                    if (BestScore > team.QualScore)
                    {
                        team.QualScore = BestScore;
                    }
                }
            });
        }

        private void RaiseTeamInfoUpdated()
        {
            if (TeamInfoUpdated != null)
                TeamInfoUpdated();
        }
    }
}
