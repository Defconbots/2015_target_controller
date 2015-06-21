using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TargetControl.Models
{
    /// <summary>
    /// This is the "database" that holds the information about the teams
    /// </summary>
    public class TeamDatabaseSerializer
    {
        private const string DatabaseFileName = "teams.json";

        public TeamDatabase Database { get; set; } = new TeamDatabase();

        public event Action DatabaseUpdated;

        public void Load()
        {
            if (File.Exists(DatabaseFileName))
            {
                var settingsJson = File.ReadAllText(DatabaseFileName);
                Database = JsonConvert.DeserializeObject<TeamDatabase>(settingsJson);
            }
        }

        public void Update(Action<TeamDatabase> updateFunc)
        {
            updateFunc(Database);

            var serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(DatabaseFileName, serialized);

            DatabaseUpdated?.Invoke();
        }
    }

    public class TeamDatabase
    {
        public List<Team> Teams { get; set; } = new List<Team>();
    }
}
