using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slammer.Models;
using ES.LoggingTools;
using ES.Windows;
using Plugins;
using Autofac;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Activities.Rules.Design;
using System.Collections.Concurrent;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace Slammer
{
    public class SlammerUIHandler
    {
        private readonly ILog _log;
        private readonly WindowTools _windowTools;
        private readonly ProcessMem _processMem;
        private KeyboardHook _keyboardHook = new KeyboardHook();
        private IGame _game;
        private Process _process;
        private blank _blank = new blank();
        private IKeyHandler _keyHandler;
        
        private List<property> _currentValues;
        private KeyRegistrar _hotKeyList;
        private ScanResults _items;
        private readonly IComponentContext _container;
        private BackgroundWorker _worker = null;
        private ConcurrentQueue<QueueMessage> _threadQueue;
        public SlammerUI _ui;

        public SlammerUIHandler(ILog log,
                        WindowTools windowTools,
                        ProcessMem processMem,
                        IComponentContext container,
                        IKeyHandler keyHandler)
        {
            _log = log;
            _windowTools = windowTools;
            _processMem = processMem;
            _container = container;
            _keyHandler = keyHandler;

            try
            {
                _game = _container.Resolve<IGame>();
                _log.Info("Found plugin for " + _game.name);
            }
            catch
            {
                _log.Fatal("Error - no plugins for any games found");
                return;
            }
            _log.Warn("Initializing plugin, please load " + _game.name);
            try
            {
                _process = _windowTools.GetProcess(_game.exe, new TimeSpan(0, 1, 0))[0];
                _ui.gameName = _game.name;
            }
            catch
            {
                _log.Fatal("Error - unable to load game");
                return;
            }
        }

        public void Scan(object sender, EventArgs e)
        {
            _ui.scanning = true;
            Cursor.Current = Cursors.WaitCursor;
            _items = new ScanResults(_game.CalculateStatLocations(_process), _process, _game);
            _ui.scanning = false;
            Cursor.Current = Cursors.Default;
            updateItems();
        }

        public void ViewProfile(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "rules (*.rules)|*.rules";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                RuleSet ruleSet = null;
                if (File.Exists(ofd.FileName))
                {
                    XmlTextReader rulesReader = new XmlTextReader(ofd.FileName);
                    WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                    ruleSet = (RuleSet)serializer.Deserialize(rulesReader);
                    rulesReader.Close();

                    RuleSetDialog ruleSetDialog = new RuleSetDialog(typeof(keyHandlerState), null, ruleSet);
                    ruleSetDialog.Height = 560;
                    ruleSetDialog.Show();
                }
            }
        }


        public void EditCommon(object sender, EventArgs e)
        {
            RuleSet ruleSet = null;

            XmlTextReader rulesReader = new XmlTextReader("Common.rules");
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            ruleSet = (RuleSet)serializer.Deserialize(rulesReader);

            rulesReader.Close();

            RuleSetDialog ruleSetDialog = new RuleSetDialog(typeof(keyHandlerState), null, ruleSet);
            DialogResult result = ruleSetDialog.ShowDialog();
            ruleSet = ruleSetDialog.RuleSet;

            if (result == DialogResult.OK)
            {
                // Serialize to a .rules file
                serializer = new WorkflowMarkupSerializer();

                XmlWriter rulesWriter = XmlWriter.Create("Common.rules");
                serializer.Serialize(rulesWriter, ruleSet);
                rulesWriter.Close();
            }
        }

        public void ManageProfile(object sender, EventArgs e)
        {
            var role = _game.GetProfileName(_process, _items.CurrentId());

            RuleSet ruleSet = null;
            if (File.Exists(role + ".rules"))
            {
                XmlTextReader rulesReader = new XmlTextReader(role + ".rules");
                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                ruleSet = (RuleSet)serializer.Deserialize(rulesReader);
                rulesReader.Close();
            }
            else
            {
                XmlTextReader rulesReader = new XmlTextReader("Common.rules");
                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                ruleSet = (RuleSet)serializer.Deserialize(rulesReader);

                rulesReader.Close();
            }

            RuleSetDialog ruleSetDialog = new RuleSetDialog(typeof(keyHandlerState), null, ruleSet);
            DialogResult result = ruleSetDialog.ShowDialog();
            ruleSet = ruleSetDialog.RuleSet;

            if (result == DialogResult.OK)
            {
                // Serialize to a .rules file
                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();

                XmlWriter rulesWriter = XmlWriter.Create(role + ".rules");
                serializer.Serialize(rulesWriter, ruleSet);
                rulesWriter.Close();
            }
        }

        public void Stop(object sender, EventArgs e)
        {
            _keyboardHook.Dispose();
            if (_worker != null) _worker.CancelAsync();
            _ui.enabled = false;
        }

        public void Start(object sender, EventArgs e)
        {
            _ui.enabled = true;
            var role = _game.GetProfileName(_process, _items.CurrentId());
            try
            {
                RuleSet ruleSet = null;
                XmlTextReader rulesReader = new XmlTextReader(role + ".rules");
                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                ruleSet = (RuleSet)serializer.Deserialize(rulesReader);
                rulesReader.Close();

                _hotKeyList = new KeyRegistrar();
                var start = _keyHandler.RegisterKeys(ref _hotKeyList, ruleSet);
                try
                {
                    _keyboardHook.Dispose();
                }
                catch { ;}
                _keyboardHook = new KeyboardHook();
                _keyboardHook.KeyPressed += keyboardHook_KeyPressed;

                foreach (var hKey in _hotKeyList.keyList)
                    _keyboardHook.RegisterHotKey(hKey.modifier, hKey.key);

                _threadQueue = new ConcurrentQueue<QueueMessage>();
                if (_worker != null) _worker.CancelAsync();

                _worker = new BackgroundWorker();
                _worker.WorkerSupportsCancellation = true;
                _worker.WorkerReportsProgress = true;
                _worker.DoWork += (obj, eventArgs) => start(_threadQueue, _process, _items.CurrentId(), obj as BackgroundWorker, ruleSet);
                _worker.ProgressChanged += _worker_ProgressChanged;
                _worker.RunWorkerAsync();
            }
            catch
            {
                var l = string.Format("Role {0} was not located", role);
            }
        }

        public void keyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            _threadQueue.Enqueue(new QueueMessage() { keyEvent = e });
        }

        public void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _ui.processing = ((KeyHandlerProgress)e.UserState) == KeyHandlerProgress.processing;
        }

        public void updateItems()
        {
            if (_items != null)
            {
                _currentValues = _items.Current();
                _ui.count = _items.length;
                _ui.index = _items.current;
            }
            else
            {
                _currentValues = new blank().ConvertToPropertyList();
                _ui.count = 0;
                _ui.index = 0;
            }
            _ui.stats = _currentValues;
        }

        public void Previous(object sender, EventArgs e)
        {
            _items.Previous();
            updateItems();
        }

        public void Next(object sender, EventArgs e)
        {
            _items.Next();
            updateItems();
        }

        public void filterTake(int row) { filterTake( _currentValues[row] ); }
        public void filterTake(property p)
        {
            _items.Take(p);
            updateItems();
        }

        public void filterExclude(int row) { filterExclude( _currentValues[row] ); } 
        public void filterExclude(property p)
        {
            _items.Exclude(p);
            updateItems();
        }

        public property currentProperty(int row)
        {
            if (row > 0)
            {
                return _currentValues[row];
            }
            return null;
        }

        public List<string> distinctValues(property p)
        {
            return _items.GetDistinctValues(p);
        }

    }
}
