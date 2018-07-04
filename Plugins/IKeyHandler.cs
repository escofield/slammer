using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using System.Workflow.Activities.Rules;

namespace Plugins
{

    public class KeySentInfo
    {
        public DateTime lastSent;
        public TimeSpan frequency;
        public TimeSpan remaining;

        public KeySentInfo(TimeSpan frequency)
        {
            this.frequency = frequency;
        }

        public KeySentInfo() { ;}
    }

    public enum KeyHandlerProgress
    {
        idle,
        processing
    }


    public class keyHandlerState
    {
        public IGameStats previous { get; set; }
        public IGameStats current { get; set; }
        public Keys? queued = null;
        public Keys? calculatedKey = null;
        public bool sendingKeys = false;
        public Dictionary<string, object> cache = new Dictionary<string, object>();
        public KeyRegistrar keyRegistrar;
        public Dictionary<Keys, KeySentInfo> history = new Dictionary<Keys,KeySentInfo>();
        public List<Keys> sent = new List<Keys>();
        public KeySequence sequence = new KeySequence();
        public bool first = false;
    }

    public class QueueMessage
    {
        public KeyPressedEventArgs keyEvent;
    }

    public delegate void KeyProcess(ConcurrentQueue<QueueMessage> queue, Process process, IntPtr id, BackgroundWorker backgroundWorker, RuleSet profile);

    public interface IKeyHandler
    {
        KeyProcess RegisterKeys(ref KeyRegistrar keyRegistrar, RuleSet profile);
    }
}
