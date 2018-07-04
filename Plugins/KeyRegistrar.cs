using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Plugins
{
    public class KeyRegistrar
    {
        private List<hotKey> _keyList = new List<hotKey>();
        public List<hotKey> keyList
        {
            get { return _keyList; }
        }

        public void Add(hotKey key)
        {
            _keyList.Add(key);
        }
    }
}
