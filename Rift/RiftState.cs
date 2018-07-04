using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ES.Windows;
using System.Diagnostics;
using Plugins;

namespace Rift
{

    public class RiftState : IGameStats
    {
        internal int _health;
        internal int _targetHealth;
        internal string _name;
        internal string _class;
        internal string _role;
        internal string _target;
        internal int _points;
        internal bool _onCD;
        internal int _energy;
        internal int _mana;
        internal bool _inCombat;
        internal string _targetId;

        public int health
        {
	        get { return _health; }
        }

        public string name
        {
	        get { return _name; }
        }

        public string classs
        {
	        get { return _class; }
        }

        public string role
        {
	        get { return _role; }
        }

        public string target
        {
	        get { return _target; }
        }

        public int targetHealth
        {
	        get { return _targetHealth; }
        }

        public int points
        {
	        get { return _points; }
        }

        public bool onCD
        {
	        get { return _onCD; }
        }

        public int energy
        {
	        get { return _energy; }
        }

        public int mana
        {
	        get { return _mana; }
        }

        public bool inCombat
        {
            get { return _inCombat; }
        }

        public string targetId
        {
            get { return _targetId; }
        }
    }

    public class Rift : IGame
    {
        private readonly ProcessMem _processMem;
        private readonly WindowTools _windowTools;

        public Rift(ProcessMem processMem,
                            WindowTools windowTools)
        {
            _processMem = processMem;
            _windowTools = windowTools;
        }

        public string name
        {
	        get { return "rift"; }
        }

        public string exe
        {
	        get { return "rift"; }
        }

        public Dictionary<IntPtr,IGameStats> CalculateStatLocations(Process process)
        {

            var retVal = new Dictionary<IntPtr, IGameStats>();

            var plist = _processMem.AobScan(process, new byte[] { 0x53, 0x6C, 0x61, 0x6D, 0x6D, 0x65, 0x72, 0x3D });
            foreach (var p in plist)
            {
                try
                {
                    retVal.Add(p,RetrieveStats(process, p));
                }
                catch (ArgumentException) { /* do nothing */ ; }
            }
            return retVal;
        }

        public string GetProfileName(Process process, IntPtr location)
        {
            var stats = this.RetrieveStats(process, location);
            return stats.classs + "-" +  stats.role;
        }

        public IGameStats RetrieveStats(Process process, IntPtr location)
        {
            byte[] mem;
            int bRead;
            string s;
            string[] sA;
            var retVal = new RiftState();

            mem = _processMem.ReadAdress(process, location, 200, out bRead);
            s = System.Text.ASCIIEncoding.ASCII.GetString(mem);
            if (s.Contains('|'))
            {
                var x = s.Split('|')[0];
                sA = x.Split(',');
                if (sA.Count() > 12)
                {
                    int.TryParse(sA[1], out retVal._health);
                    retVal._name = sA[2].Trim();
                    retVal._class = sA[3].Trim();
                    retVal._role = sA[4].Trim();
                    retVal._target = sA[5].Trim();
                    int.TryParse(sA[6], out retVal._points);
                    retVal._onCD = sA[7]=="1";
                    int.TryParse(sA[8], out retVal._energy);
                    int.TryParse(sA[9], out retVal._mana);
                    int.TryParse(sA[10], out retVal._targetHealth);
                    retVal._inCombat = sA[11] == "1";
                    retVal._targetId = sA[12].Trim();
                    return retVal;
                }
            }
            return null;
        }
    }
}
