using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Plugins
{
    public interface IGame
    {
        string name { get; }
        string exe { get; }

        Dictionary<IntPtr, IGameStats> CalculateStatLocations(Process process);
        IGameStats RetrieveStats(Process process, IntPtr location);
        string GetProfileName(Process process, IntPtr location);
    }
}
