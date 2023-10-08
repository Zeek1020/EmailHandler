using MailManagerLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using System.Threading.Tasks;
using S22.Imap;
using System.Linq;
using EmailHandler.Models;


namespace EmailHandler
{
    public partial class Form2 : Form
    {

        MailManagerAsync handler = null;
        public object tally_lock = new object();
        public Dictionary<string, int> mailTally = null;
        protected EventHandler<Dictionary<string, int>> graphDataChanged = null;
        protected int dots = 0;
        protected int messageCount = 0;
        protected object progress_lock = new object();
        protected int messageProgress = 0;
        
        
        public Form2( MailManagerAsync _handler )
        {
            handler = _handler;

            _handler.Options = FetchOptions.HeadersOnly;
            _handler.Condition = SearchCondition.Unseen();

            InitializeComponent();
            backgroundWorker1.RunWorkerAsync();
            this.progressBar1.Enabled = false;
            this.label1.Text = handler.Username;
            TickingTimer checkGraphReady = new TickingTimer();
            checkGraphReady.Interval = 150;
            checkGraphReady.Tick += CheckGraphData;
            checkGraphReady.Tick += UpdateProgressBar;
            checkGraphReady.Tick += UpdateLoadingDots;
            checkGraphReady.Disposed += disposeTimer;
            checkGraphReady.Start();

        }
        #region TimerMethods
        public void disposeTimer( object sender, EventArgs e ) {
            this.label1.Text = handler.Username;
            this.progressBar1.Enabled = false;
            this.progressBar1.Visible = false;
        }
        protected void UpdateProgressBar( object sender, EventArgs e)
        {
            if (this.progressBar1.Enabled)
            {
                this.progressBar1.Value = messageProgress;
            } else
            {
                this.progressBar1.Maximum = messageCount;
                if (messageCount > 0)
                {
                    this.progressBar1.Enabled = true;
                }
            }
        }
        protected void UpdateLoadingDots( object sender, EventArgs e)
        {
            const int INTERVAL_SECONDS = 250;
            const int MAX_DOTS = 3;
            const int MIN_DOTS = 0;
            int interval = (sender as TickingTimer).Interval;
            long ticks = (sender as TickingTimer).NumTicks;

            if ((ticks * interval) % (INTERVAL_SECONDS) == 0)
            {
                StringBuilder strDots = new StringBuilder();
                dots = dots >= MAX_DOTS ? MIN_DOTS : dots + 1;

                for (int i = 0; i < dots; i++) strDots.Append(".");

                this.label1.Text = $"Loading {strDots}";
            }
        }
        protected void CheckGraphData(object sender, EventArgs e ) {
            if (mailTally != null){
                Dictionary<string, int> tally = null;
                lock (tally_lock){
                    tally = mailTally;
                    mailTally = null;
                }
                (sender as TickingTimer).Stop();
                (sender as TickingTimer).Dispose();
                setGraphData(tally);
            }
        }
        #endregion
        public void setGraphData( Dictionary<string, int> dict ) {
            Series series = this.chart1.Series[0];
            foreach (var t in dict)
            {
                var point = new DataPoint();
                point.SetValueY(t.Value);
                point.LegendText = t.Key;
                series.Points.Add(point);
            }
        }
        private async Task<IEnumerable<MailMessage>> getMailMessageTask( IEnumerable<uint> list ) {
            return await Task.Run(() =>
            {
                var messages = handler.getMessagesFromList(list);
                lock( progress_lock)
                {
                    messageProgress += messages.Count();
                }
                return messages;
            });
        }
        private IEnumerable<Task> breakoutMessagesIntoThreads( IEnumerable<uint> idList, int max_threads = 10 ) {
            var tasks = new List<Task>();
            int take = idList.Count() / max_threads;
            for (int i = 0; i < max_threads; i++)
            {
                int skip = i * take;
                IEnumerable<uint> list;
                if (i + 1 == max_threads)
                {
                    list = idList.Skip(skip);
                }
                else
                {
                    list = idList.Skip(skip).Take(take);
                }
                tasks.Add(getMailMessageTask(list));
            }
            return tasks;
        }
        private void bkgGetMail(object sender, DoWorkEventArgs e)
        {
            const int TIMEOUT_SECONDS = 3;

            Dictionary<string, int> tally = new Dictionary<string, int>();
            DateTime dt = DateTime.Now;
            
            while( handler == null)
            {
                if( (dt - DateTime.Now).Seconds > TIMEOUT_SECONDS)
                {
                    (sender as BackgroundWorker).Dispose();
                    return;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            messageCount = handler.MessageIds.Count();
            Debug.WriteLine($"MessageCount: {messageCount}");

            var tasks = breakoutMessagesIntoThreads(handler.MessageIds, 100);
            
            handler.Messages = new List<MailMessage>();
            Debug.WriteLine("Wait all");
            Task.WaitAll( tasks.ToArray() );

            foreach( Task<IEnumerable<MailMessage>> t in tasks )
            {
                handler.Messages =  t.Result.Concat( handler.Messages );
            }

            Debug.WriteLine($"Messages: {handler.Messages.Count()}");

            foreach ( MailMessage message in handler.Messages )
            {
                string from = message.From.Address;

                if (tally.ContainsKey(from))
                {
                    tally[from] += 1;
                }
                else
                {
                    tally.Add(from, 1);
                }
            }

            lock( tally_lock ){
                this.mailTally = tally;
            }
            (sender as BackgroundWorker).Dispose();
        }
    }
}