using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;
using System.Windows.Forms;
using System.Threading;
using ES.LoggingTools;
using ES.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Workflow.Activities.Rules;

namespace KeyHandler
{
    public class HotKeyHandler : IKeyHandler
    {
        private readonly IGame _game;
        private readonly ILog _log;
        private readonly WindowTools _windowTools;
        keyHandlerState _keyHandlerState;
        System.Media.SoundPlayer _enable = new System.Media.SoundPlayer(".\\Sounds\\on.wav");
        System.Media.SoundPlayer _disable = new System.Media.SoundPlayer(".\\Sounds\\off.wav");
        System.Media.SoundPlayer _fire = new System.Media.SoundPlayer(".\\Sounds\\fire.wav");

        public HotKeyHandler(ILog log
                        ,WindowTools windowTools
                        ,IGame game)
        {       
            _game = game;
            _log = log;
            _windowTools = windowTools;
        }

        public KeyProcess RegisterKeys(ref KeyRegistrar keyRegistrar, RuleSet profile)
        {
            _keyHandlerState = new keyHandlerState() { current = null, previous = null };
            _keyHandlerState.keyRegistrar = keyRegistrar;
            RuleValidation validation = new RuleValidation(typeof(keyHandlerState), null);
            RuleExecution execution = new RuleExecution(validation, _keyHandlerState);
            profile.Execute(execution);
            return Start;
        }

        public void Start(ConcurrentQueue<QueueMessage> queue, Process process, IntPtr id, BackgroundWorker backgroundWorker, RuleSet profile)
        {
            bool previousSendingKeys = false;
            DateTime notify = DateTime.Now;
            TimeSpan delay = new TimeSpan(0, 0, 1);
            QueueMessage qm;

            while (backgroundWorker.CancellationPending == false)
            {

                _keyHandlerState.previous = _keyHandlerState.current;
                _keyHandlerState.current = _game.RetrieveStats(process, id);
                if (_keyHandlerState.current == null)
                {
                    _log.Error("Unable to load game stats");
                    Thread.Sleep(500);
                    continue;
                }

                bool keySent = _keyHandlerState.previous!=null && _keyHandlerState.previous.onCD == false && _keyHandlerState.current.onCD == true;

                if(keySent) {
                    if (_keyHandlerState.calculatedKey == _keyHandlerState.queued)
                        _keyHandlerState.queued = null;
                    var trackedKeyHistory = _keyHandlerState.history.Where(x => x.Key == _keyHandlerState.calculatedKey).SingleOrDefault();
                    if(trackedKeyHistory.Value != null) trackedKeyHistory.Value.lastSent = DateTime.Now;
                    if (_keyHandlerState.calculatedKey.HasValue)
                    {
                        _keyHandlerState.sent.Insert(0, _keyHandlerState.calculatedKey.Value);
                        if (_keyHandlerState.sent.Count > 30)
                        {
                            _keyHandlerState.sent.RemoveAt(30);
                        }
                    }
                    _keyHandlerState.calculatedKey = null;
                }

                if (queue.TryDequeue(out qm))
                    _keyHandlerState.queued = qm.keyEvent.Key;

                foreach(var x in _keyHandlerState.history.Where(y => y.Value.lastSent != null))
                {
                    var y = x.Value.lastSent.Add(x.Value.frequency).Subtract(DateTime.Now);
                    x.Value.remaining = y.TotalSeconds > 0 ? y : new TimeSpan();
                }

                RuleValidation validation = new RuleValidation(typeof(keyHandlerState), null);
                RuleExecution execution = new RuleExecution(validation, _keyHandlerState);
                profile.Execute(execution);
                _keyHandlerState.first = false;

                if (_keyHandlerState.sendingKeys != previousSendingKeys)
                {
                    backgroundWorker.ReportProgress(0, _keyHandlerState.sendingKeys ? KeyHandlerProgress.processing : KeyHandlerProgress.idle);
                    if (_keyHandlerState.sendingKeys)
                        _enable.Play();
                    else
                        _disable.Play();
                }
                previousSendingKeys = _keyHandlerState.sendingKeys;

                if (_keyHandlerState.calculatedKey != null && _keyHandlerState.sendingKeys)
                {
                    if (DateTime.Now > notify.Add(delay))
                    {
                        _fire.Play();
                        _log.Debug("sending key {0}", Enum.GetName(typeof(Keys), _keyHandlerState.calculatedKey));
                    }
                    _windowTools.SendKey(process, (Keys)_keyHandlerState.calculatedKey);
                }

                Thread.Sleep(200);
            }
        }
    }
}
