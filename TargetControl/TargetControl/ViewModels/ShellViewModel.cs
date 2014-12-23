using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Caliburn.Micro;

using MahApps.Metro;
using MahApps.Metro.Controls;

using TargetControl.Models;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace TargetControl
{
    public sealed class ShellViewModel : Conductor<IMainScreenTabItem>.Collection.OneActive,
        IHandle<ShowFlyoutEvent>,
        IHandle<RemoveFlyoutEvent>
    {
        public BindableCollection<IMainScreenTabItem> Tabs { get; set; }
        public BindableCollection<FlyoutViewModel> Flyouts { get; set; }

        public ShellViewModel(IEnumerable<IMainScreenTabItem> tabs, IEventAggregator eventAggregator)
        {
            DisplayName = "DEFCONBOTS CONTEST CONTROLLER";

            Tabs = new BindableCollection<IMainScreenTabItem>(tabs);
            Flyouts = new BindableCollection<FlyoutViewModel>();

            eventAggregator.Subscribe(this);
        }

        public void Handle(ShowFlyoutEvent message)
        {
            Flyouts.Add(new FlyoutViewModel
            {
                ViewModel = message.ViewModel,
                Position = message.Position,
                IsOpen = true,
                Header = "TEST",
                IsModal = true
            });
        }

        public void Handle(RemoveFlyoutEvent message)
        {
            var flyout = Flyouts.FirstOrDefault(x => x.ViewModel == message.ViewModel);
            if (flyout != null)
            {
                flyout.IsOpen = false;
            }
        }
    }

    public interface IMainScreenTabItem
    {
    }

    public struct RemoveFlyoutEvent
    {
        public IScreen ViewModel { get; set; }
    }

    public struct ShowFlyoutEvent
    {
        public IScreen ViewModel { get; set; }

        public Position Position { get; set; }
    }

    public class FlyoutViewModel : PropertyChangedBase
    {
        private bool _isOpen;
        public IScreen ViewModel { get; set; }

        public string Header { get; set; }

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (value.Equals(_isOpen))
                {
                    return;
                }
                _isOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public Position Position { get; set; }

        public bool IsModal { get; set; }
    }
}
