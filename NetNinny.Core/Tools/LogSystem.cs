using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetNinny.Tools
{
    /// <summary>
    /// Represents the verbosity level of the log.
    /// </summary>
    public enum LogVerbosity
    {
        UserInfo = 0x01,
        Error = 0x02,
        Warning = 0x04,
        Verbose = 0x08,

        UserInfo_Error = UserInfo | Error,
        UserInfo_Error_Warning = UserInfo | Error | Warning,
        UserInfo_Error_Warning_Verbose = UserInfo | Error | Warning | Verbose
    }


    /// <summary>
    /// Represents a log entry.
    /// </summary>
    public class LogEntry
    {
        public LogVerbosity Verbosity { get; set; }
        public int ClientId { get; set; }
        public string Message { get; set; }
        public LogEntry(string message, int clientId, LogVerbosity verbosity)
        {
            Message = message;
            ClientId = clientId;
            Verbosity = verbosity;
        }
    }

    /// <summary>
    /// Class used to dispatch log message.
    /// </summary>
    public class LogSystem
    {

        public delegate void LogDelegate(LogEntry entry);

        int ps;
        static int s = 0;
        public LogSystem()
        {
            ps = s;
            s++;
        }

        /// <summary>
        /// Event fired when a new entry is added to the log.
        /// </summary>
        public event LogDelegate Log;

        /// <summary>
        /// Adds a new entry to this log.
        /// </summary>
        public void LogEntry(LogEntry entry)
        {
            if(Log != null)
                Log(entry);
        }

        /// <summary>
        /// Adds a new entry to this log.
        /// </summary>
        public void LogEntry(string message, int clientId, LogVerbosity verbosity)
        {
            if(Log != null)
                Log(new LogEntry(message, clientId, verbosity));
        }

        /// <summary>
        /// Adds a new entry to this log, with UserInfo verbosity.
        /// </summary>
        public void LogInfo(string message, int clientId)
        {
            if (Log != null)
                Log(new LogEntry(message, clientId, LogVerbosity.UserInfo));
        }

        /// <summary>
        /// Adds a new entry to this log, with Error verbosity.
        /// </summary>
        public void LogError(string message, int clientId)
        {
            if (Log != null)
                Log(new LogEntry(message, clientId, LogVerbosity.Error));
        
        }

        /// <summary>
        /// Adds a new entry to this log, with Verbose verbosity.
        /// </summary>
        public void LogVerbose(string message, int clientId)
        {
            if (Log != null)
                Log(new LogEntry(message, clientId, LogVerbosity.Verbose));
        }
    }
}
