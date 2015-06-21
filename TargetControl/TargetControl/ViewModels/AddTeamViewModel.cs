using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Caliburn.Micro;

using TargetControl.Models;

namespace TargetControl
{
    public class AddTeamViewModel : Screen
    {
        private readonly Action<AddTeamViewModel> _complete;
        private string _teamName = string.Empty;
        private string _newMemberName = string.Empty;
        private int _hitId;

        public string Guid { get; private set; }

        public bool IsEdit { get; private set; }

        public string TeamName
        {
            get { return _teamName; }
            set
            {
                if (value != _teamName)
                {
                    _teamName = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string NewMemberName
        {
            get { return _newMemberName; }
            set
            {
                if (value != _newMemberName)
                {
                    _newMemberName = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public int HitId
        {
            get { return _hitId; }
            set
            {
                if (value == _hitId)
                {
                    return;
                }
                _hitId = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<AddTeamMemberViewModel> Members { get; set; }

        public AddTeamViewModel(Action<AddTeamViewModel> complete, Team team = null)
        {
            _complete = complete;
            Members = new BindableCollection<AddTeamMemberViewModel>();

            Guid = System.Guid.NewGuid().ToString();

            IsEdit = team != null;
            if (team != null)
            {
                TeamName = team.Name;
                HitId = team.HitId;
                Members.AddRange(team.Members.Select(m => new AddTeamMemberViewModel(m.Name)));
                Guid = team.Guid;
            }
        }

        public void AddMember()
        {
            Members.Add(new AddTeamMemberViewModel(NewMemberName));
            NewMemberName = string.Empty;
        }

        public void RemoveMember(AddTeamMemberViewModel member)
        {
            Members.Remove(member);
        }

        public void Add()
        {
            if (NewMemberName != string.Empty)
            {
                AddMember();
            }

            _complete(this);
        }

        public void NewMemberNameKeyDown(ActionExecutionContext context)
        {
            var keyArgs = context.EventArgs as KeyEventArgs;

            if (keyArgs != null && keyArgs.Key == Key.Enter)
            {
                AddMember();
            }
        }
    }

    public class AddTeamMemberViewModel : PropertyChangedBase
    {
        public AddTeamMemberViewModel(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
