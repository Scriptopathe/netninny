using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetNinny.Core;
using NetNinny.Tools;
using NetNinny.Filtering;
namespace NetNinny.UI
{
    public partial class NetNinnyForm : Form
    {
        #region Variables
        NetNinny.Core.ProxyServer m_server;
        volatile List<LogEntry> m_entryQueue;
        object m_entryQueueMutex = new object();
        #endregion
        /// <summary>
        /// Creates a new instance of the net ninny form.
        /// </summary>
        public NetNinnyForm()
        {
            InitializeComponent();

            m_entryQueue = new List<LogEntry>();
            Application.Idle += Application_Idle;
            m_toggleButton.Click += m_toggleButton_Click;
            // Initialises server logs.
            m_server = new Core.ProxyServer();
            m_server.LogSys.Log += LogSys_Log;
        }

        /// <summary>
        /// Happens at application idle time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Application_Idle(object sender, EventArgs e)
        {
            lock (m_entryQueueMutex)
            {
                foreach (LogEntry entry in m_entryQueue)
                {
                    AddLogEntry(entry);
                }
                m_entryQueue.Clear();
            }
        }

        /// <summary>
        /// Toggles on / off server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_toggleButton_Click(object sender, EventArgs e)
        {
            if(m_server.IsRunning)
            {
                StopServer();
            }
            else
            {
                StartServer();
            }
        }

        /// <summary>
        /// Happens when a log entry is fired.
        /// </summary>
        /// <param name="entry"></param>
        void LogSys_Log(Tools.LogEntry entry)
        {
            lock (m_entryQueueMutex)
            {
                m_entryQueue.Add(entry);
            }
        }
        /// <summary>
        /// Adds a log entry to the ui.
        /// </summary>
        /// <param name="entry"></param>
        void AddLogEntry(Tools.LogEntry entry)
        {
            int id = entry.ClientId - 1;
            try
            {
                string name = "Log(" + (id + 1).ToString() + ")";
                if (!m_logTabs.TabPages.ContainsKey(id.ToString()))
                {
                    TextBox ctrl = new TextBox();
                    ctrl.Multiline = true;
                    ctrl.ReadOnly = true;
                    ctrl.Dock = DockStyle.Fill;
                    ctrl.Name = "log";
                    ctrl.ScrollBars = ScrollBars.Both;
                    m_logTabs.TabPages.Add(id.ToString(), name);
                    m_logTabs.TabPages[id.ToString()].Controls.Add(ctrl);
                }
                var page = m_logTabs.TabPages[id.ToString()];
                var control = page.Controls["log"];
                if (entry.Verbosity == LogVerbosity.Error)
                {
                    control.BackColor = Color.FromArgb(255, 220, 220);
                    control.ForeColor = Color.FromArgb(100, 0, 0);
                    page.Text = name + ":Error";
                }
                control.Text += entry.Message + "\r\n";
            }
            catch(Exception e)
            {
                int a = 5;
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void StopServer()
        {
            // Disable inputs
            m_browseButton.Enabled = true;
            m_filtersTextbox.Enabled = true;
            m_portUpdown.Enabled = true;
            m_browseButton.Enabled = true;
            // Updates texts
            m_statusText.Text = "Stopped.";
            m_toggleButton.Text = "Start";

            m_server.Stop();
        }
        /// <summary>
        /// Starts the server.
        /// </summary>
        public void StartServer()
        {
            // Disable inputs
            m_browseButton.Enabled = false;
            m_filtersTextbox.Enabled = false;
            m_portUpdown.Enabled = false;
            m_browseButton.Enabled = false;
            // Updates texts
            m_statusText.Text = "Running...";
            m_toggleButton.Text = "Stop";
            // Clear log pages.
            m_logTabs.TabPages.Clear();

            // Starts the actual server
            string filtersCollectionFile = m_filtersTextbox.Text;
            FilterCollection collection = null;

            // Gets the filter collection.
            if (filtersCollectionFile != "")
            {
                try { collection = FilterCollection.FromFile(filtersCollectionFile); }
                catch { Console.WriteLine("Error reading the filters file. Default filters loaded."); }
            }

            if (collection == null)
                collection = new FilterCollection() { "SpongeBob", "Norrköping", "Paris Hilton", "Britney Spears" };

            // ------ FEATURE 7: Configurable proxy port ------ 
            int port = (int)m_portUpdown.Value;
            
            m_server.Start(port, "127.0.0.1", collection);

        }
    }
}
