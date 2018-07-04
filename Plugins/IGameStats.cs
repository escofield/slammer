using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins
{

    public class property
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public static class ExtensionMethods
    {
        public static List<property> ConvertToPropertyList(this IGameStats igs)
        {
            return typeof(IGameStats).GetProperties().Select( p => new property() { name = p.Name, value = p.GetValue(igs, null).ToString() }).ToList();
        }

        public static void UpdateValues(this List<property> a, List<property> b)
        {
            if (a.Count > 0)
            {
                foreach (var p in a)
                {
                    p.value = b.Where(x => x.name == p.name).Select(x => x.value).SingleOrDefault();
                }
            }
            
        }
    }

    public interface IGameStats
    {

        int health
        {
            get;
        }

        string name
        {
            get;
        }

        string classs
        {
            get;
        }

        string role
        {
            get;
        }

        string target
        {
            get;
        }

        int targetHealth
        {
            get;
        }

        int points
        {
            get;
        }

        bool onCD
        {
            get;
        }

        int energy
        {
            get;
        }

        int mana
        {
            get;
        }

        bool inCombat
        {
            get;
        }

        string targetId
        {
            get;
        }
    }
}
