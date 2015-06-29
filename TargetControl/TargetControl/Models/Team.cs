using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TargetControl.Models
{
    public class TeamMember // this should probably be a view model if you intend on editing it in the UI
    {
        public string Name { get; set; }
    }

    public class Team
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public int HitId { get; set; }
        public List<TeamMember> Members { get; set; }
        public int QualScore { get; set; }
        public int FinalScore { get; set; }
    }
}
