using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plugins
{
    public class hotKey
    {

        private Dictionary<KeyModifier, int> ModifierKeyMap = new Dictionary<KeyModifier, int>() { { KeyModifier.None, 0x0 }, { KeyModifier.Alt, 0x1000 }, { KeyModifier.Control, 0x2000 }, { KeyModifier.Shift, 0x4000 }, { KeyModifier.Win, 0x8000 } };
        public KeyModifier modifier;
        public Keys key;
        
        public int id
        {
            get
            {
                return ModifierKeyMap[this.modifier] | (int)this.key;
            }
        }

        private hotKey() { ;}

        public hotKey(Keys key)
        {
            this.key = key;
            this.modifier = KeyModifier.None;
        }

        public hotKey(Keys key, KeyModifier modifier)
        {
            this.key = key;
            this.modifier = modifier;
        }
    }
    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum KeyModifier : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }


    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private KeyModifier _modifier;
        private Keys _key;

        public KeyPressedEventArgs(KeyModifier modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public KeyModifier Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }
    }

}
