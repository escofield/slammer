using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plugins
{
    public class KeySequence
    {
        private List<Keys> _sequence = new List<Keys>();
        private int _index;

        public void StartAt(Keys k) {  _index = _sequence.IndexOf(k); }
        public Keys Start(int i) { this.index = i; return _sequence[_index]; }

        public int index 
        { 
            get { return _index; } 
            set 
            {
                if(value > _sequence.Count - 1) value = _sequence.Count - 1;
                if(value < 0) value = 0;
                _index = value;
            } 
        }

        public void Add(Keys k)
        {
            _sequence.Add(k);
        }

        public void Sequence(List<Keys> k)
        {
            _sequence = k;
        }

        public Keys Next()
        {
            if (_index + 1 > _sequence.Count - 1)
            {
                _index = 0;
                return _sequence[_index];
            }
            return _sequence[++_index];
        }

        //public Keys Previous()
        //{
        //    if (_index - 1 < 0) _index = _sequence.Count - 1;
        //    return _sequence[--_index];
        //}

        public Keys Current()
        {
            return _sequence[_index];
        }

        public void Increment()
        {
            _index++;
            if (_index + 1 > _sequence.Count) _index = 0;
        }

    }
}

