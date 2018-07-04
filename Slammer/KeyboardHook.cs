using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Plugins;

namespace Slammer
{
public sealed class KeyboardHook : IDisposable
{
    // Registers a hot key with Windows.
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    // Unregisters the hot key with Windows.
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    /// <summary>
    /// Represents the window that is used internally to get the messages.
    /// </summary>
    private class Window : NativeWindow, IDisposable
    {
        private static int WM_HOTKEY = 0x0312;

        public Window()
        {
            // create the handle for the window.
            this.CreateHandle(new CreateParams());
        }

        /// <summary>
        /// Overridden to get the notifications.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // check if we got a hot key pressed.
            if (m.Msg == WM_HOTKEY)
            {
                // get the keys.
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);

                // invoke the event to notify the parent.
                if (KeyPressed != null)
                    KeyPressed(this, new KeyPressedEventArgs(modifier, key));
            }
        }

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            this.DestroyHandle();
        }

        #endregion
    }

    private Window _window = new Window();
   
    private List<hotKey> hotKeys = new List<hotKey>();

    public KeyboardHook()
    {
        // register the event of the inner native window.
        _window.KeyPressed += delegate(object sender, KeyPressedEventArgs args)
        {
            if (KeyPressed != null)
                KeyPressed(this, args);
        };
    }

    /// <summary>
    /// Registers a hot key in the system.
    /// </summary>
    /// <param name="modifier">The modifiers that are associated with the hot key.</param>
    /// <param name="key">The key itself that is associated with the hot key.</param>
    public void RegisterHotKey(KeyModifier modifier, Keys key)
    {
        // increment the counter.
        var hk = new hotKey(key, modifier);
        hotKeys.Add(hk);
        // register the hot key.
        if (!RegisterHotKey(_window.Handle, hk.id, (uint)modifier, (uint)key))
            throw new InvalidOperationException("Couldn’t register the hot key.");
    }

    /// <summary>
    /// A hot key has been pressed.
    /// </summary>
    public event EventHandler<KeyPressedEventArgs> KeyPressed;

    #region IDisposable Members

    public void Pause()
    {
        foreach (var hk in hotKeys)
        {
            UnregisterHotKey(_window.Handle, hk.id);
        }
    }

    public void Start()
    {
        foreach (var hk in hotKeys)
        {
            RegisterHotKey(_window.Handle, hk.id, (uint)hk.modifier, (uint)hk.key);
        }
    }

    public void Dispose()
    {
        // unregister all the registered hot keys.
        foreach (var hk in hotKeys)
        {
            UnregisterHotKey(_window.Handle, hk.id);
        }
        hotKeys = null;
        // dispose the inner native window.
        _window.Dispose();
    }

    #endregion
}
}
