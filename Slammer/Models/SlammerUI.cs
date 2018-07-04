using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;

namespace Slammer.Models
{
    public class SlammerUI
    {
        public string gameName { get; set; }
        public bool enabled { get; set; }
        public bool processing { get; set; }
        public List<property> stats { get; set; }
        public bool scanning { get; set; }
        public int count { get; set; }
        public int index { get; set; }
    }
}
