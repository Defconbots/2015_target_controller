using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TargetControl.Models
{
    public interface ILogger
    {
        void Log(string format, params object[] args);
    }

    public class Logger : ILogger
    {
        public void Log(string format, object[] args)
        {
            var formatted = string.Format(format, args) + Environment.NewLine;
            File.AppendAllText("DEFCONBOTS.TXT", formatted);
        }
    }
}
