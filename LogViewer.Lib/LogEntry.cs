using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib
{
    /// <summary>
    /// Base class for any LogEntry type
    /// </summary>
    public abstract class LogEntry : IComparable<LogEntry>
    {
        public DateTime Timestamp { get; set; }
        public string TID { get; set; }
        public string Process { get; set; }
        public string Area { get; set; }
        public string Category { get; set; }
        public string EventID { get; set; }
        public LogEntryLevel Level { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// All sorting done using timestamps
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(LogEntry other)
        {
            return this.Timestamp.CompareTo(other.Timestamp);
        }

        /// <summary>
        /// Checks if the log item would 
        /// </summary>
        /// <param name="filterOptions"></param>
        /// <returns></returns>
        public bool Filter(LogEntrySearchOptions filterOptions)
        {
            //level
            if (filterOptions.Level != null)
            {
                if (this.Level != filterOptions.Level)
                    return false;
            }

            //process
            if (filterOptions.Process != null)
            {
                if (!this.Process.ToUpper().Contains(filterOptions.Process.ToUpper()))
                    return false;
            }

            //timestamp 
            if (filterOptions.StartDateTime != null && filterOptions.EndDateTime != null)
            {
                if (this.Timestamp < filterOptions.StartDateTime || this.Timestamp > filterOptions.EndDateTime)
                    return false;
            }
            else if (filterOptions.StartDateTime != null)
            {
                if (this.Timestamp < filterOptions.StartDateTime)
                    return false;
            }
            else if (filterOptions.EndDateTime != null)
            {
                if (this.Timestamp > filterOptions.EndDateTime)
                    return false;
            }

            //tid
            if(filterOptions.TID !=null)
            {
                if (!this.TID.ToUpper().Contains(filterOptions.TID.ToUpper()))
                    return false;
            }

            //area
            if (filterOptions.Area != null)
            {
                if (!this.Area.ToUpper().Contains(filterOptions.Area.ToUpper()))
                    return false;
            }

            //category
            if (filterOptions.Category != null)
            {
                if (!this.Category.ToUpper().Contains(filterOptions.Category.ToUpper()))
                    return false;
            }

            //event id
            if (filterOptions.EventID != null)
            {
                if (!this.EventID.ToUpper().Contains(filterOptions.EventID.ToUpper()))
                    return false;
            }

            //message
            if (filterOptions.Message != null)
            {
                if (!this.Message.ToUpper().Contains(filterOptions.Message.ToUpper()))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the names of the columns
        /// </summary>
        /// <returns></returns>
        public static string[] GetColumnNames()
        {
            return new string[] 
            { 
                "Timestamp",           
                "Process", 
                 "TID",
                "Area", 
                "Category", 
                "EventID", 
                "Level", 
                "Message" 
            };
        }

        /// <summary>
        /// Returns the LogEntryLevel enum from log text
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static LogEntryLevel GetLogEntryLevel(string level)
        {
            switch (level.ToUpper())
            {
                case "ASSERT":
                    return LogEntryLevel.Assert;
                case "CRITICAL":
                    return LogEntryLevel.Critical;
                case "ERROR":
                    return LogEntryLevel.Error;
                case "HIGH":
                    return LogEntryLevel.High;
                case "INFORMATION":
                    return LogEntryLevel.Information;
                case "MEDIUM":
                    return LogEntryLevel.Medium;
                case "MONITORABLE":
                    return LogEntryLevel.Monitorable;
                case "UNEXPECTED":
                    return LogEntryLevel.Unexpected;
                case "VERBOSE":
                    return LogEntryLevel.Verbose;
                case "WARNING":
                    return LogEntryLevel.Warning;
                default:
                    var error = string.Format("Log level '{0}' not recognized!", level);
                    throw new ApplicationException(error);
            }
        }
    }
}
