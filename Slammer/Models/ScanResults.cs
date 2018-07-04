using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;
using System.Diagnostics;

namespace Slammer.Models
{
    internal class blank : IGameStats
    {
        public int health { get { return 0; } }
        public string name { get { return ""; } }
        public string classs { get { return ""; } }
        public string role { get { return ""; } }
        public string target { get { return ""; } }
        public int targetHealth { get { return 0; } }
        public int points { get { return 0; } }
        public bool onCD { get { return false; } }
        public int energy { get { return 0; } }
        public int mana { get { return 0; } }
        public bool inCombat { get { return false; } }
        public string targetId { get { return ""; } }
    }

    public class ScanResults
    {
        private List<IntPtr> _indexList;
        private int _index;
        private Process _process;
        private IGame _game;

        public ScanResults(Dictionary<IntPtr, IGameStats> list
                            , Process process
                            , IGame game)
        {
            _indexList = list.Keys.ToList();
            _process = process;
            _game = game;
        }
        public int length { get { return _indexList == null ? 0 :_indexList.Count(); } }
        public int current { get { return _index + 1; } }

        public void First() { _index = 0; }

        public void Next() { _index++; }

        public void Previous() { _index--; }

        public IntPtr CurrentId()
        {
            return _indexList[_index];
        }

        public static List<property> BlankProperties()
        {
            return new blank().ConvertToPropertyList();
        }

        public List<property> Current()
        {
            if (_indexList.Count == 0) return ScanResults.BlankProperties();

            if (_index > _indexList.Count() - 1) _index = 0;
            if (_index < 0) _index = _indexList.Count() - 1;

            return GetStats().ConvertToPropertyList();
        }

        private List<KeyValuePair<IntPtr, IGameStats>> build()
        {
            var query = new List<KeyValuePair<IntPtr, IGameStats>>();
            List<IntPtr> newList = new List<IntPtr>();
            foreach (var i in _indexList)
            {
                var gamestats = _game.RetrieveStats(_process, i);
                if (gamestats != null)
                {
                    query.Add(new KeyValuePair<IntPtr, IGameStats>(i, gamestats));
                    newList.Add(i);
                }
            }
            _indexList = newList;
            return query;
        }

        public void Take(property p)
        {
            var cId = _indexList[_index];
            var list = build();
            IEnumerable<IntPtr> query = new List<IntPtr>();
            switch (p.name)
            {
                case "health": query = list.Where(x => x.Value.health.ToString() != p.value).Select(x => x.Key); break;
                case "name": query = list.Where(x => x.Value.name != p.value).Select(x => x.Key); break;
                case "classs": query = list.Where(x => x.Value.classs != p.value).Select(x => x.Key); break;
                case "role": query = list.Where(x => x.Value.role != p.value).Select(x => x.Key); break;
                case "target": query = list.Where(x => x.Value.target != p.value).Select(x => x.Key); break;
                case "targetHealth": query = list.Where(x => x.Value.targetHealth.ToString() != p.value).Select(x => x.Key); break;
                case "points": query = list.Where(x => x.Value.points.ToString() != p.value).Select(x => x.Key); break;
                case "onCD": query = list.Where(x => x.Value.onCD.ToString() != p.value).Select(x => x.Key); break;
                case "energy": query = list.Where(x => x.Value.energy.ToString() != p.value).Select(x => x.Key); break;
                case "mana": query = list.Where(x => x.Value.mana.ToString() != p.value).Select(x => x.Key); break;
                case "inCombat": query = list.Where(x => x.Value.inCombat.ToString() != p.value).Select(x => x.Key); break;
            }
            foreach (var i in query.ToList()) _indexList.Remove(i);
            if (_indexList.Contains(cId))
            {
                _index = _indexList.IndexOf(cId);
            }
            else
            {
                _index = 0;
            }
        }

        public void Exclude(property p)
        {
            var cId = _indexList[_index];
            var list = build();
            IEnumerable<IntPtr> query = new List<IntPtr>();
            switch (p.name)
            {
                case "health": query = list.Where(x => x.Value.health.ToString() == p.value).Select(x => x.Key); break;
                case "name": query = list.Where(x => x.Value.name == p.value).Select(x => x.Key); break;
                case "classs": query = list.Where(x => x.Value.classs == p.value).Select(x => x.Key); break;
                case "role": query = list.Where(x => x.Value.role == p.value).Select(x => x.Key); break;
                case "target": query = list.Where(x => x.Value.target == p.value).Select(x => x.Key); break;
                case "targetHealth": query = list.Where(x => x.Value.targetHealth.ToString() == p.value).Select(x => x.Key); break;
                case "points": query = list.Where(x => x.Value.points.ToString() == p.value).Select(x => x.Key); break;
                case "onCD": query = list.Where(x => x.Value.onCD.ToString() == p.value).Select(x => x.Key); break;
                case "energy": query = list.Where(x => x.Value.energy.ToString() == p.value).Select(x => x.Key); break;
                case "mana": query = list.Where(x => x.Value.mana.ToString() == p.value).Select(x => x.Key); break;
                case "inCombat": query = list.Where(x => x.Value.inCombat.ToString() == p.value).Select(x => x.Key); break;
            }
            foreach (var i in query.ToList()) _indexList.Remove(i);
            if (_indexList.Contains(cId))
            {
                _index = _indexList.IndexOf(cId);
            }
            else
            {
                _index = 0;
            }
        }

        public void Remove()
        {

        }

        public IGameStats GetStats()
        {
            if (_indexList.Count == 0)
            {
                return new blank();
            }

            IGameStats stats = null;
            while(null == (stats = _game.RetrieveStats(_process, _indexList[_index])))
            {
                _indexList.Remove(_indexList[_index]);
                if ( _indexList.Count == 0 )
                    break;
                if (_index > _indexList.Count - 1)
                    _index--;
                if (_index < 0)
                    break;
            }
            if (stats == null)
                return new blank();
            return stats;
        }

        public List<string> GetDistinctValues(property p)
        {
            var list = build();
            IEnumerable<string> query = new List<string>();
            switch (p.name)
            {
                case "health": query = list.Select(x => x.Value.health.ToString()); break;
                case "name": query = list.Select(x => x.Value.name); break;
                case "classs": query = list.Select(x => x.Value.classs); break;
                case "role": query = list.Select(x => x.Value.role); break;
                case "target": query = list.Select(x => x.Value.target); break;
                case "targetHealth": query = list.Select(x => x.Value.targetHealth.ToString()); break;
                case "points": query = list.Select(x => x.Value.points.ToString()); break;
                case "onCD": query = list.Select(x => x.Value.onCD.ToString()); break;
                case "energy": query = list.Select(x => x.Value.energy.ToString()); break;
                case "mana": query = list.Select(x => x.Value.mana.ToString()); break;
                case "inCombat": query = list.Select(x => x.Value.inCombat.ToString()); break;
            }
            return query.Distinct().ToList(); ;
        }
    }
}
