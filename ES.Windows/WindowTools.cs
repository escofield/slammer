using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ES.LoggingTools;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace ES.Windows
{
    public class WindowTools
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        //This is a replacement for Cursor.Position in WinForms
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;
        private const int VK_TAB = 0x09;
        private const int VK_ENTER = 0x0D;
        private const int VK_UP = 0x26;
        private const int VK_DOWN = 0x28;
        private const int VK_RIGHT = 0x27;

        private readonly ILog _log;

        public WindowTools(ILog log)
        {
            _log = log;
        }

        public Process[] GetProcess(string name, TimeSpan timeout)
        {
            _log.Info(string.Format("Finding process {0}", name));
            var start = DateTime.Now;
            Process[] ps;
            while (((ps = System.Diagnostics.Process.GetProcessesByName(name)).Length == 0) && start.Add(timeout) > DateTime.Now)
            {
                Thread.Sleep(500);
            }
            if (ps.Length == 0)
            {
                _log.Error(string.Format("Unable to locate {0}", name));
            }
            return ps;
        }

        public void BringToFront(IntPtr handle){
            SetForegroundWindow(handle);
        }

        public void SendKey(Process process, Keys key)
        {
            PostMessage(process.MainWindowHandle, WM_KEYDOWN, (IntPtr)key, IntPtr.Zero);
        }

        public void SendKeys(string keys)
        {
            System.Windows.Forms.SendKeys.SendWait(keys);
        }

        //This simulates a left mouse click
        public void LeftMouseClick(Point pos)
        {
            SetCursorPos(pos.X, pos.Y);
            System.Threading.Thread.Sleep(300);
            mouse_event(MOUSEEVENTF_LEFTDOWN, pos.X, pos.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, pos.X, pos.Y, 0, 0);
        }

        public Color GetColorAt(Point p)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, p.X, p.Y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }
    }
}
