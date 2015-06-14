using System;
using System.Windows.Threading;
using Caliburn.Micro;

namespace TargetControl
{
    public sealed class ContestViewModel : Screen, IMainScreenTabItem
    {
        private readonly Contest _contest;

        public ContestViewModel(Contest contest)
        {
            _contest = contest;
            DisplayName = "Contest";
        }
    }
}