using Caliburn.Micro;

namespace TargetControl
{
    public sealed class TerminalViewModel : Screen, IMainScreenTabItem
    {
        private readonly ISerial _serial;

        public TerminalViewModel()
        {
            DisplayName = "Terminal";
        }
    }
}