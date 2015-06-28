using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TargetControl.Models
{
    public interface ITeamDatabaseSerializer
    {
        TeamDatabase Database { get; }
        event Action DatabaseUpdated;
        void Update(Action<TeamDatabase> updateFunc);
    }

    /// <summary>
    /// This is the "database" that holds the information about the teams
    /// </summary>
    public class TeamDatabaseSerializer : ITeamDatabaseSerializer
    {
        private const string DatabaseFileName = "teams.json";

        public TeamDatabase Database { get; private set; }

        public event Action DatabaseUpdated;

        public TeamDatabaseSerializer()
        {
            Database = new TeamDatabase();

            if (File.Exists(DatabaseFileName))
            {
                var settingsJson = File.ReadAllText(DatabaseFileName);
                Database = JsonConvert.DeserializeObject<TeamDatabase>(settingsJson);
            }
        }

        public void Update(Action<TeamDatabase> updateFunc)
        {
            updateFunc(Database);

            var serialized = JsonConvert.SerializeObject(Database, Formatting.Indented);
            File.WriteAllText(DatabaseFileName, serialized);

            if (DatabaseUpdated != null)
            {
                DatabaseUpdated();
            }
        }
    }

    public class TeamDatabase
    {
        public TeamDatabase()
        {
            Teams = new List<Team>();
        }

        public List<Team> Teams { get; set; }
    }
}
