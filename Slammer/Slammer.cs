using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using ES.LoggingTools;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using Autofac;
using Slammer.Models;
using Plugins;
using System.Net;
using System.Web.Script.Serialization;
using Gma.UserActivityMonitor;
using ES.Windows;

namespace Slammer
{
    public partial class Slammer : Form
    {
        private readonly SlammerUIHandler _slammerUI;
        private readonly ILog _log;
        private readonly SlammerUI _ui;
        private property _gridRowFilter;
        private readonly WindowTools _windowTools;
        private int _minionI = 1;
        public Slammer(ILog log,
                        SlammerUIHandler slammerUI,
                        WindowTools windowTools
                      )
        {
            _log = log;
            _log.Info("Initializing Slammer");
            _ui = new SlammerUI();
            _slammerUI = slammerUI;
            _slammerUI._ui = _ui;
            _windowTools = windowTools;

            InitializeComponent();
            this.tabControl1.SelectedIndexChanged += visibleTabChanged;
            this.manageProfile.Click += new System.EventHandler(_slammerUI.ManageProfile);
            this.stopProfile.Click += new System.EventHandler(_slammerUI.Stop);
            this.startProfile.Click += new System.EventHandler(_slammerUI.Start);
            this.Next.Click += new System.EventHandler(_slammerUI.Next);
            this.Previous.Click += new System.EventHandler(_slammerUI.Previous);
            this.takeValue.Click += new System.EventHandler(this.filterTake);
            this.removeValue.Click += new System.EventHandler(this.filterExclude);
            this.Scan.Click += new System.EventHandler(_slammerUI.Scan);
            this.editCommonRulesetToolStripMenuItem.Click += new System.EventHandler(_slammerUI.EditCommon);
            this.reviewAProfileToolStripMenuItem.Click += new System.EventHandler(_slammerUI.ViewProfile);
            this.memScan.Tick += new System.EventHandler(this.memScan_Tick);

            gameStats.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText="Name",  Name = "Stat", DataPropertyName = "name" });
            gameStats.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Value", Name = "Value", DataPropertyName = "value", Width=164 });

            gameStats.ContextMenuStrip = cmsFilter;
            gameStats.MouseDown += gameStats_MouseDown;
            cmsFilter.Opening += cmsFilter_Opening;
            cmsFilter.Closed += cmsFilter_Closed;
            cmsFilter.MouseEnter += cmsFilter_MouseEnter;

            updateUI();

            this.eventList.DoubleClick += new System.EventHandler(this.eventListDoubleClick);
            this.eventsAll.CheckedChanged += new System.EventHandler(this.radioClick);
            this.eventWeekly.CheckedChanged += new System.EventHandler(this.radioClick);
            this.eventsInfinityStones.CheckedChanged += new System.EventHandler(this.radioClick);
            this.unstable.CheckedChanged += new System.EventHandler(this.eventTracker_Tick);
            this.blood.CheckedChanged += new System.EventHandler(this.eventTracker_Tick);
            this.other.CheckedChanged += new System.EventHandler(this.eventTracker_Tick);
            this.volan.CheckedChanged += new System.EventHandler(this.eventTracker_Tick);

            this.GrabMinion1.Paint += GrabMinion1_Paint;
            this.GrabMinion2.Paint += GrabMinion2_Paint;
            this.GrabMinion3.Paint += GrabMinion3_Paint;
            this.GrabMinion4.Paint += GrabMinion4_Paint;
            this.GrabMinion5.Paint += GrabMinion5_Paint;
            this.GrabMinion6.Paint += GrabMinion6_Paint;
            this.GrabMinion7.Paint += GrabMinion7_Paint;
            this.GrabMinion8.Paint += GrabMinion8_Paint;
            this.GrabAdventure.Paint += GrabAdventure_Paint;
            this.GrabClaim.Paint += GrabClaim_Paint;
            this.GrabSend.Paint += GrabSend_Paint;


            this.GrabMinion1.MouseDown += GrabMinion1_MouseDown;
            this.GrabMinion2.MouseDown += GrabMinion2_MouseDown;
            this.GrabMinion3.MouseDown += GrabMinion3_MouseDown;
            this.GrabMinion4.MouseDown += GrabMinion4_MouseDown;
            this.GrabMinion5.MouseDown += GrabMinion5_MouseDown;
            this.GrabMinion6.MouseDown += GrabMinion6_MouseDown;
            this.GrabMinion7.MouseDown += GrabMinion7_MouseDown;
            this.GrabMinion8.MouseDown += GrabMinion8_MouseDown;
            this.GrabClaim.MouseDown += GrabClaim_MouseDown;
            this.GrabAdventure.MouseDown += GrabAdventure_MouseDown;
            this.GrabSend.MouseDown += GrabSend_MouseDown;
            this.adventureTimer.Tick += adventureTimer_Tick;
             _log.Info("Slammer Loaded");
        }


        void visibleTabChanged(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.TabControl)(sender)).SelectedTab.TabIndex == 1)
            {
                eventTracker.Enabled = true;
                eventTracker_Tick(null, null);
            }
            else
            {
                eventTracker.Enabled = false;
            }
        }
        
        void cmsFilter_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

            memScan.Enabled = false;
            if (_gridRowFilter != null)
            {
                var valueItems = _slammerUI.distinctValues(_gridRowFilter).Select(x => new ToolStripMenuItem() { Text = x, CheckOnClick = true, DisplayStyle = ToolStripItemDisplayStyle.ImageAndText }).ToArray();
                filterList.DropDownItems.Clear();
                filterList.DropDownItems.AddRange(valueItems);
                filterList.DropDown.AutoClose = false;

                filterList.DropDownItems.Add(new ToolStripSeparator());

                var execute = new ToolStripMenuItem() { Text = "Execute" };
                execute.Font = new Font(execute.Font.FontFamily, execute.Font.Size, FontStyle.Bold);
                execute.Click += executeFilter_Click;
                filterList.DropDownItems.Add(execute);
            }
            var cancel = new ToolStripMenuItem() { Text = "Cancel" };
            cancel.Click += Cancel_Click;
            filterList.DropDownItems.Add(cancel);
        }


        void CloseContextMenu()
        {
            cmsFilter.AutoClose = true;
            filterList.DropDown.AutoClose = true;
            cmsFilter.Close();
        }

        void executeFilter_Click(object sender, EventArgs e)
        {
            var removeItems = new List<string>();
            
            foreach (var i in filterList.DropDownItems)
            {
                if (i.GetType().IsAssignableTo<ToolStripMenuItem>() == true)
                {
                    var item = (ToolStripMenuItem)i;
                    if (item.Checked == false && item.CheckOnClick == true)
                    {
                        removeItems.Add(item.Text);
                    }
                }
            }
            foreach (var item in removeItems)
            {
                _slammerUI.filterExclude(new property() { value = item, name = _gridRowFilter.name });
            }
            CloseContextMenu();
        }


        void updateUI()
        {
            _slammerUI.updateItems();
            LoadedGame.Text = _ui.gameName;
            startProfile.Visible = !_ui.enabled;
            stopProfile.Visible = _ui.enabled;
            stopProfile.BackColor = (_ui.processing) ? Color.MediumVioletRed : Color.LightBlue;
            gameStats.DataSource = _ui.stats;
            gameStats.Refresh();
            listDetail.Text = string.Format("{0} of {1}", _ui.index, _ui.count);
            gameStats.ClearSelection();

            Next.Enabled = Previous.Enabled = manageProfile.Enabled = startProfile.Enabled = _ui.count > 0;

        }

        private void memScan_Tick(object sender, EventArgs e) { updateUI(); }
        void Cancel_Click(object sender, EventArgs e) { CloseContextMenu(); }
        void gameStats_MouseDown(object sender, MouseEventArgs e) { _gridRowFilter = _slammerUI.currentProperty(gameStats.HitTest(e.X, e.Y).RowIndex); }
        public void filterTake(object sender, EventArgs e) { _slammerUI.filterTake(_gridRowFilter); }
        public void filterExclude(object sender, EventArgs e) { _slammerUI.filterExclude(_gridRowFilter); }
        void cmsFilter_MouseEnter(object sender, EventArgs e) { cmsFilter.AutoClose = false; }
        void cmsFilter_Closed(object sender, ToolStripDropDownClosedEventArgs e) { memScan.Enabled = true; }

        private struct shard{
            public string name;
            public int id;
        }
        List<shard> shards = new List<shard>() {
                new shard() { name="Seastone", id=1701 },
                new shard() { name="Greybriar", id=1702 },
                new shard() { name="Deepwood", id=1704 },
                new shard() { name="Wolfsbane", id=1706 },
                new shard() { name="Faeblight", id=1707 },
                new shard() { name="Laethys", id=1708 },
                new shard() { name="Hailol", id=1721 }
        };
        string url = "http://chat-us.riftgame.com:8080/chatservice/zoneevent/list?shardId=";
        string timeUrl = "http://chat-us.riftgame.com:8080/chatservice/time";
        List<string> hidden = new List<string>();

        private void eventTracker_Tick(object sender, EventArgs e)
        {
            List<WorldEvent> completeEvents = new List<WorldEvent>();
            var request = new WebClient();

            var selectedEventTypes = new List<EventType>();
            if (unstable.Checked) selectedEventTypes.Add(EventType.Unstable);
            if (blood.Checked) selectedEventTypes.Add(EventType.Blood);
            if (volan.Checked) selectedEventTypes.Add(EventType.Volan);
            if (other.Checked) selectedEventTypes.Add(EventType.Other);

            var eventContrib = new List<Contribute>();
            if (eventsAll.Checked) { eventContrib.Add(Contribute.Zero); eventContrib.Add(Contribute.Weekly); eventContrib.Add(Contribute.Stones); }
            if (eventWeekly.Checked) { eventContrib.Add(Contribute.Weekly); }
            if (eventsInfinityStones.Checked) { eventContrib.Add(Contribute.Weekly); eventContrib.Add(Contribute.Stones); }

            foreach(var shard in shards){
                var time = long.Parse(request.DownloadString(timeUrl));
                var json = request.DownloadString(url + shard.id);

                var serializer = new JavaScriptSerializer();
                var jsonResult = (worldEventResponse)serializer.Deserialize(json, typeof(worldEventResponse));

                completeEvents.AddRange(jsonResult.data.Where(x => x.name != null).OrderBy(x => x.started)
                                .Select(x => new WorldEvent() 
                                        { zone = x.zone, 
                                          shard = shard.name, 
                                          name = x.name, 
                                          started = x.started, 
                                          timeLeft = string.Format("{0}:{1:00}",(time - x.started) / 60, (time - x.started) % 60) }));
            }

            var filteredEvents = completeEvents
                            .Where(x => selectedEventTypes.Contains(x.eventType) && eventContrib.Contains(x.contributions) && hidden.Contains(x.id) == false)
                            .OrderBy(x => x.started);
            eventList.Items.Clear();
            foreach (var evnt in filteredEvents)
            {
                var item = new ListViewItem(new string[] { evnt.zone, evnt.shard, evnt.name, evnt.timeLeft, evnt.id });
                switch(evnt.eventType)
                {
                    case EventType.Blood:
                        item.BackColor = Color.FromArgb(178, 32, 32);
                        item.ForeColor = Color.White;
                        eventList.Items.Add(item);
                        break;
                    case EventType.Unstable:
                        item.BackColor = Color.FromArgb(21, 86, 120);
                        item.ForeColor = Color.White;
                        eventList.Items.Add(item);
                        break;
                    case EventType.Volan:
                        item.BackColor = Color.FromArgb(51, 100, 51);
                        item.ForeColor = Color.White;
                        eventList.Items.Add(item);
                        break;
                    case EventType.Other:
                        eventList.Items.Add(item);
                        break;
                }

            }
        }

        private void eventListDoubleClick(object sender, EventArgs e)
        {
            if (((ListView)sender).SelectedItems.Count > 0 && ((ListView)sender).SelectedItems[0].SubItems.Count > 2)
            {
                var item = ((ListView)sender).SelectedItems[0].SubItems;
                hidden.Add(item[4].Text);
            }
            eventTracker_Tick(null, null);
        }

        private void radioClick(object sender, EventArgs e)
        {
            eventTracker_Tick(null, null);
        }

        private void reviewAProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void editCommonRulesetToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected enum GrabType
        {
            none,
            Adventure,
            Claim,
            Minion1,
            Minion2,
            Minion3,
            Minion4,
            Minion5,
            Minion6,
            Minion7,
            Minion8,
            Send
        }

        private Dictionary<GrabType, Point> grabLocations = new Dictionary<GrabType, Point>();
        private bool _startedAdventures = false;

        protected GrabType grabbing = GrabType.none;
        void GrabAdventure_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Adventure, sender, e); }
        void GrabMinion1_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Minion1,sender,e); }
        void GrabMinion2_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Minion2, sender, e); }
        void GrabMinion3_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Minion3, sender, e); }
        void GrabMinion4_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Minion4, sender, e); }
        void GrabMinion5_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Minion5, sender, e); }
        void GrabMinion6_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Minion6, sender, e); }
        void GrabMinion7_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Minion7, sender, e); }
        void GrabMinion8_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Minion8, sender, e); }
        void GrabClaim_Paint(object sender, PaintEventArgs e) { this.Grab_Paint(GrabType.Claim, sender, e); }
        void GrabSend_Paint(object sender, PaintEventArgs e){ this.Grab_Paint(GrabType.Send,sender,e); }


        void Grab_Paint(GrabType t, object sender, PaintEventArgs e)
        {
            if (this.grabbing == t) { return; }
            Cursors.NoMove2D.Draw(e.Graphics, e.ClipRectangle);
        }

        void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            var s = string.Format("X={0}, Y={1}", e.X, e.Y);
            switch (grabbing)
            {
                case GrabType.Adventure:
                    AdventurePostion.Text = s;
                    break;
                case GrabType.Claim:
                    ClaimPosition.Text = s;
                    break;
                case GrabType.Minion1:
                    MinionPosition1.Text = s;
                    break;
                case GrabType.Minion2:
                    MinionPosition2.Text = s;
                    break;
                case GrabType.Minion3:
                    MinionPosition3.Text = s;
                    break;
                case GrabType.Minion4:
                    MinionPosition4.Text = s;
                    break;
                case GrabType.Minion5:
                    MinionPosition5.Text = s;
                    break;
                case GrabType.Minion6:
                    MinionPosition6.Text = s;
                    break;
                case GrabType.Minion7:
                    MinionPosition7.Text = s;
                    break;
                case GrabType.Minion8:
                    MinionPosition8.Text = s;
                    break;
                case GrabType.Send:
                    SendPosition.Text = s;
                    break;
            }
        }

        void GrabMinion1_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Minion1, sender, e); }
        void GrabMinion2_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Minion2, sender, e); }
        void GrabMinion3_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Minion3, sender, e); }
        void GrabMinion4_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Minion4, sender, e); }
        void GrabMinion5_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Minion5, sender, e); }
        void GrabMinion6_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Minion6, sender, e); }
        void GrabMinion7_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Minion7, sender, e); }
        void GrabMinion8_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Minion8, sender, e); }
        void GrabSend_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Send, sender, e); }
        void GrabAdventure_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Adventure, sender, e); }
        void GrabClaim_MouseDown(object sender, MouseEventArgs e) { Grab_MouseDown(GrabType.Claim, sender, e); }

        void Grab_MouseDown(GrabType t, object sender, MouseEventArgs e)
        {
            HookManager.MouseUp += HookManager_MouseUp;
            HookManager.MouseMove += HookManager_MouseMove;
            this.grabbing = t;
            Cursor.Current = Cursors.NoMove2D;
            GrabMinion1.Refresh();
            GrabMinion2.Refresh();
            GrabMinion3.Refresh();
            GrabMinion4.Refresh();
            GrabMinion5.Refresh();
            GrabMinion6.Refresh();
            GrabMinion7.Refresh();
            GrabMinion8.Refresh();
            GrabSend.Refresh();
            GrabAdventure.Refresh();
            GrabClaim.Refresh();
        }

        void HookManager_MouseUp(object sender, MouseEventArgs e)
        {
            HookManager.MouseUp -= HookManager_MouseUp;
            HookManager.MouseMove -= HookManager_MouseMove;
            Cursor.Current = Cursors.Default;

            if(grabLocations.ContainsKey(grabbing)==false)
                grabLocations.Add(grabbing,new Point());
            grabLocations[grabbing] = new Point(e.X, e.Y);
            this.grabbing = GrabType.none;
            GrabMinion1.Refresh();
            GrabMinion2.Refresh();
            GrabMinion3.Refresh();
            GrabMinion4.Refresh();
            GrabMinion5.Refresh();
            GrabMinion6.Refresh();
            GrabMinion7.Refresh();
            GrabMinion8.Refresh();
            GrabSend.Refresh();
            GrabAdventure.Refresh();
            GrabClaim.Refresh();
        }
        private Color _sendColor;

        private void AdventuresStart_Click(object sender, EventArgs e)
        {
            var sendColorPoint = new Point(grabLocations[GrabType.Send].X - 5, grabLocations[GrabType.Send].Y);
            _sendColor = _windowTools.GetColorAt(grabLocations[GrabType.Send]);
            FillSlots();
            var rpeat = int.Parse(repeatCount.Text) -1;
            repeatCount.Text = rpeat.ToString();
          
            adventureTimer.Interval = int.Parse(timerSeconds.Text) * 1000;
            if (adventureTimer.Interval > 40000 && rpeat > 0)
            {
                _startedAdventures = true;
                adventureTimer.Start();
            }
        }

        private void AdventuresStop_Click(object sender, EventArgs e)
        {
            _startedAdventures = false;
            adventureTimer.Stop();
        }

        private void ClaimReward()
        {
            var slots = int.Parse(slotCount.Text);
            for (var x = 0; x < slots; x++)
            {
                _windowTools.LeftMouseClick(grabLocations[GrabType.Claim]);
                Thread.Sleep(500);
            }
        }

        private void FillSlots()
        {
            ClaimReward();
            var slots = int.Parse(slotCount.Text);
            
            for (var x = 0; x < slots; x++)
            {
                _windowTools.LeftMouseClick(grabLocations[GrabType.Adventure]);
                var minionS = 0;
                do
                {
                    if (_minionI > 8) { _minionI = 1; }
                    switch (_minionI)
                    {
                        case 1:
                            _windowTools.LeftMouseClick(grabLocations[GrabType.Minion1]);
                            break;
                        case 2:
                            _windowTools.LeftMouseClick(grabLocations[GrabType.Minion2]);
                            break;
                        case 3:
                            _windowTools.LeftMouseClick(grabLocations[GrabType.Minion3]);
                            break;
                        case 4:
                            _windowTools.LeftMouseClick(grabLocations[GrabType.Minion4]);
                            break;
                        case 5:
                            _windowTools.LeftMouseClick(grabLocations[GrabType.Minion5]);
                            break;
                        case 6:
                            _windowTools.LeftMouseClick(grabLocations[GrabType.Minion6]);
                            break;
                        case 7:
                            _windowTools.LeftMouseClick(grabLocations[GrabType.Minion7]);
                            break;
                        case 8:
                            _windowTools.LeftMouseClick(grabLocations[GrabType.Minion8]);
                            break;
                    }
                    _minionI++;
                    Thread.Sleep(500);
                } while (_sendColor == _windowTools.GetColorAt(grabLocations[GrabType.Send]) && minionS++ < 10);
                Thread.Sleep(500);
                _windowTools.LeftMouseClick(grabLocations[GrabType.Send]);
                Thread.Sleep(500);
            }
        }

        void adventureTimer_Tick(object sender, EventArgs e)
        {
            adventureTimer.Stop();
            FillSlots();
            var rpeat = int.Parse(repeatCount.Text) - 1;
            repeatCount.Text = rpeat.ToString();
            if (rpeat > 0 && _startedAdventures == true)
                adventureTimer.Start();
            else
                _startedAdventures = false;

        }

    }
}
