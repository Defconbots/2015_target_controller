using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace TargetControl.Controls
{
    public class LockedToggleButton : ToggleButton
    {
        protected override void OnClick()
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
    }
}
