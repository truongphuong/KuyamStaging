using System;
using System.Diagnostics;
using log4net;

namespace Kuyam.Utility
{
    public static class LogHelper
    {
        #region Provite Properties
        private static readonly ILog InfoLog;
        private static readonly ILog ErrorLog;
        private static readonly ILog SmsLog;
        private static readonly ILog ImportExportCompanytLog;
        #endregion


        #region Constructors

        /// <summary>
        /// Initializes the <see cref="LogHelper"/> class.
        /// </summary>
        static LogHelper()
        {
            log4net.Config.XmlConfigurator.Configure();
            InfoLog = LogManager.GetLogger("Info");
            ErrorLog = LogManager.GetLogger("Error");
            SmsLog = LogManager.GetLogger("SMS");
            ImportExportCompanytLog = LogManager.GetLogger("ImportCompany");

        }
        #endregion
        

        #region Info logging method
        /// <summary>
        /// Log info message
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Info(string message)
        {
            InfoLog.Info(UpdateMessage(message));
        }

        /// <summary>
        /// Log info with exception
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public static void Info(string message, Exception ex)
        {
            InfoLog.Info(UpdateMessage(message), ex);
        }

        #endregion


        #region Error logging method

        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Error(string message)
        {
            message = UpdateMessage(message);
            ErrorLog.Error(message);
            InfoLog.Error(message);
        }

        /// <summary>
        /// Log error message with exception
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public static void Error(string message, Exception ex)
        {
            message = UpdateMessage(message);
            ErrorLog.Error(message, ex);
            InfoLog.Error(message, ex);
        }

        #endregion


        #region SMS logging

        /// <summary>
        /// Log SMS message
        /// </summary>
        /// <param name="message">The message.</param>
        public static void SMS(string message)
        {
            SmsLog.Info(UpdateMessage(message));
        }

        /// <summary>
        /// Log SMS sent fail
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public static void SMS(string message, Exception ex)
        {
            SmsLog.Error(UpdateMessage(message), ex);
        }
        #endregion

        #region ImportExport logging

        /// <summary>
        /// Log SMS message
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ImportExportCompanyInfo(string message)
        {
            ImportExportCompanytLog.Info(UpdateMessage(message));
        }

        /// <summary>
        /// Log import company fail
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public static void ImportExportCompanyError(string message)
        {
            ImportExportCompanytLog.Error(UpdateMessage(message));
        }
        /// <summary>
        /// Log import company fail
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public static void ImportExportCompanyError(string message, Exception ex)
        {
            ImportExportCompanytLog.Error(UpdateMessage(message), ex);
        }
        #endregion

        #region Utitlities

        /// <summary>
        /// Processes the error.
        /// </summary>
        /// <param name="pExceptionInClass">The p exception in class.</param>
        /// <param name="pException">The p exception.</param>
        public static void ProcessError(Type pExceptionInClass, Exception pException)
        {
            
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="loggerName">Name of the logger.</param>
        /// <returns></returns>
        public static ILog GetLogger(string loggerName)
        {
            return LogManager.GetLogger(loggerName);
        }

        #endregion

        /// <summary>
        /// Updates the message to log by adding method call.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static string UpdateMessage(string message)
        {
            StackTrace stackTrace = new StackTrace();

            try
            {
                string methodName = stackTrace.GetFrame(2).GetMethod().Name;
                var declaringType = stackTrace.GetFrame(2).GetMethod().DeclaringType;
                if (declaringType != null)
                {
                    string methodClass = declaringType.FullName;
                    methodName = string.Format("{0}.{1}", methodClass, methodName);
                }
                return string.Format("{0} - {1}", methodName, message);
            }
            catch
            {
                return message;
            }
        }
    }
}
