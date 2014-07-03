// This class and all other files in this assembly are the property and copyright of Mark II Software, LLC
// Copyright (c)2008-2011 Mark II Software, LLC.  All Rights Reserved.
// You may not use, copy, decompile, or in any way reference this file or functionality without the express written
// consent of an officer of the Mark II Software, LLC

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace M2.Util
{
    public static class Logger
    {
        [DllImport("kernel32.dll")]
        static extern void OutputDebugString(string lpOutputString);

        private static EventLog _el = new EventLog();
        private static string _defaultSource = null;
        private static string _destData = null;
        private static string _lastMessage = null;

        public enum DestinationType
        {
            WindowsEventLog,
            Screen,
            File,
            DebugStream
        }

        public enum Severity
        {
            Debug,
            Information,
            Warning,
            Error,
            Disabled
        }

        private static DestinationType _dest = DestinationType.WindowsEventLog;

        public static Severity MinLoggingLevel { get; set; }
        public static Severity MinSMSNotificationLevel { get; set; }
        public static string SpecificEventLog { get; set; }

		private static bool _initialized = false;

        public static string LoggingFilePath
        {
            get { return _destData; }
            set { _destData = value; }
        }

        static Logger()
        {
            MinLoggingLevel = Severity.Debug;
        }

        public static void Init(DestinationType dest, string defaultSource, string destData = null, Logger.Severity minLoggingLevel = Severity.Warning, bool clearFile = false)
        {
            _dest = dest;
            _defaultSource = defaultSource;
            MinLoggingLevel = minLoggingLevel;
            LoggingFilePath = _destData;

            switch (dest)
            {
                case DestinationType.WindowsEventLog:
                    _el.Log = "Application";
                    break;
            }

            if (clearFile)
            {
                ClearLogFile();
            }

            _lastMessage = null;

			_initialized = true;
        }

        public static void ClearLogFile()
        {
            File.WriteAllText(_destData, "");
        }

        public static string GetLastMessage()
        {
            return _lastMessage;
        }

        public static void Debug(string message)
        {
            Log(message, Severity.Debug);
        }

        public static void Debug(string source, string message)
        {
            Log(source, message, Severity.Debug);
        }

        public static void Debug(string source, string message, int id)
        {
            Log(source, message, Severity.Debug, id);
        }

        public static void Info(string message)
        {
            Log(message, Severity.Information);
        }

        public static void Info(string source, string message)
        {
            Log(source, message, Severity.Information);
        }

        public static void Info(string source, string message, int id)
        {
            Log(source, message, Severity.Information, id);
        }

        public static void Warning(string message)
        {
            Log(message, Severity.Warning);
        }

        public static void Warning(string source, string message)
        {
            Log(source, message, Severity.Warning);
        }

        public static void Warning(string source, string message, int id)
        {
            Log(source, message, Severity.Warning, id);
        }
        public static void Error(string message)
        {
            Log(message, Severity.Error);
        }

        public static void Error(string source, string message)
        {
            Log(source, message, Severity.Error);
        }

        public static void Error(string source, string message, int id)
        {
            Log(source, message, Severity.Error, id);
        }

        public static void Log(string message)
        {
            Log(_defaultSource, message, Severity.Information);
        }

        public static void Log(string message, Severity severity)
        {
            Log(_defaultSource, message, severity);
        }

        public static void Log(string source, string message, Severity severity)
        {
            Log(source, message, severity, 0);
        }

        public static void Log(string source, string message, Severity severity, int id)
        {
            if ((int)severity < (int)MinLoggingLevel || !_initialized)
                return;

            string fullMsg = message;
            Console.WriteLine(fullMsg);

            string detailedMessage = String.Format("{0} [{1}] {2}\r\n", DateTime.Now.ToString("HH:mm:ss.ffff"), severity.ToString(), message);
            
            if ((int)severity >= (int)MinSMSNotificationLevel && SMSClient.IsActive)
                SMSClient.Send(detailedMessage);

            _lastMessage = detailedMessage;

            switch (_dest)
            {
                case DestinationType.Screen:
                    MessageBoxIcon icon = MessageBoxIcon.Information;
                    switch (severity)
                    {
                        case Severity.Debug:
                        case Severity.Information:
                            icon = MessageBoxIcon.Information;
                            break;

                        case Severity.Error:
                            icon = MessageBoxIcon.Error;
                            break;

                        case Severity.Warning:
                            icon = MessageBoxIcon.Warning;
                            break;
                    }
                    MessageBox.Show(null, fullMsg, "", MessageBoxButtons.OK, icon);
                    break;

                case DestinationType.WindowsEventLog:
                    EventLogEntryType type = EventLogEntryType.Information;
                    switch (severity)
                    {
                        case Severity.Debug:
                        case Severity.Information:
                            type = EventLogEntryType.Information;
                            break;

                        case Severity.Error:
                            type = EventLogEntryType.Error;
                            break;

                        case Severity.Warning:
                            type = EventLogEntryType.Warning;
                            break;
                    }

                    _el.Source = source;
                    if (SpecificEventLog != null)
                    {
                        _el.Log = SpecificEventLog;
                        SetupEventLog(SpecificEventLog);
                    }
                    else
                    {
                        _el.Log = "Application";
                    }
                    _el.WriteEntry(message, type, id);
                    break;

                case DestinationType.File:
                    File.AppendAllText(_destData, detailedMessage);
                    break;

                case DestinationType.DebugStream:
                    OutputDebugString(detailedMessage);
                    break;
            }
        }

        private static void SetupEventLog(string logName)
        {
            if (!EventLog.SourceExists(logName))
            {
                EventLog.CreateEventSource(logName, "Application");
            }
        }
    }
}
