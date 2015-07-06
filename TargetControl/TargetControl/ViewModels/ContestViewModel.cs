using System;
using System.Windows.Threading;
using Caliburn.Micro;
using SimpleInjector.Advanced.Internal;
using TargetControl.Models;

namespace TargetControl
{
    public interface IContestStateMachineViewModel
    {
        event Action<IContestStateMachineViewModel> ChangeState;
    }

    public sealed class ContestViewModel : Screen, IMainScreenTabItem
    {
        private IContestStateMachineViewModel _currentState;

        public ContestViewModel(Func<IContestSelectTeamViewModel> initialState)
        {
            DisplayName = "Contest";

            ChangeState(initialState());
        }

        public IContestStateMachineViewModel CurrentState
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

        private void ChangeState(IContestStateMachineViewModel state)
        {
            if (_currentState != null)
            {
                _currentState.ChangeState -= ChangeState;
            }

            state.ChangeState += ChangeState;
            CurrentState = state;
        }
    }
}