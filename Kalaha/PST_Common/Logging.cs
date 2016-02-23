//
// Logging
//
// Definition of a class to log stuff, implemented as a singleton.
//

using System;


namespace PST_Common
{
    public sealed class Logging
    {
        // --- Enums ---

        [Flags]
        public enum LogLevel
        {
            Off = 0x00,
            Error = 0x01,
            Info  = 0x02,
            Debug = 0x04,
            DeepDebug = 0x08
        }


        // --- Attributes of the class Logging ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile Logging _inst;
        private static object _syncRoot = new Object();
        
        const string _logFilename = "Kalaha.log";
        private static readonly AsyncLock m_lock = new AsyncLock(); 

        private LogLevel _logLevel;


        // --- Methods of the class Logging ---

       /// <summary>The constructor of the class. This is declared private because only
        /// the Instance property method may call it.</summary>
        private Logging() { }

        /// <summary>
        /// If the single instance has not yet been created, yet, creates the instance.
        /// This approach ensures that only one instance is created and only when the instance is needed.
        /// This approach uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        /// This double-check locking approach solves the thread concurrency problems while avoiding an exclusive
        /// lock in every call to the Instance property method. It also allows you to delay instantiation
        /// until the object is first accessed.
        /// </summary>
        /// <returns>The single instance of the class</returns>
        public static Logging I
        {
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                            _inst = new Logging();
                    }
                }

                return _inst;
            }
        }

        /// <summary>
        /// Initialization in this case means opening the log file if the logLevel is set to more than "Off".
        /// </summary>
        /// <param name="logLevel">If the logSate is set active, the logging is done.</param>
        public async void Initialize(LogLevel logLevel)
        {
            _logLevel = logLevel;

            if (_logLevel != LogLevel.Off)
            {
                try
                {
                    await StorageHelper.AppendFileAsync<string>(_logFilename,
                                                                "\n----------- Kalaha started at " + DateTime.Now +
                                                                " -----------\n");
                    SetLogLevel(_logLevel);
                 }
                catch (Exception)
                {
                    // Catch away all exceptions as we do not want any disturbance just because the logging is not possible.
                }
            }
        }

        /// <summary>
        /// Static method to be called from anywhere in order to write a string into the log file.
        /// </summary>
        /// <param name="message">The message to be written. Needs a '\n' if a newline shall be written into the file.</param>
        /// <param name="logLevel">The log level this message has. Default is LogLevel.Debug.</param>
        public async void LogMessage(string message, LogLevel logLevel = LogLevel.Debug)
        {
            if ((_logLevel & logLevel) > LogLevel.Off)
            {
                try
                {
                    // Write the message into the file. This is done using some sophisticated locking in order
                    // for multiple write operation to still have the right order of lines in the file:
                    using (var releaser = await m_lock.LockAsync())
                    {
                        await StorageHelper.AppendFileAsync<string>(_logFilename,
                                                                    DateTime.Now.ToString("HH:mm:ss") + ' ' + message);
                    }
                }
                catch (Exception)
                {
                    // Catch away all exceptions as we do not want any disturbance just because the logging is not possible.
                }
            }
        }

        public void SetLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;

            if (_logLevel != LogLevel.Off)
            {
                LogMessage("LogLevel set to " + _logLevel + ".\n", _logLevel);
            }
        }

        /// <returns>The currently set log level</returns>
        public LogLevel GetLogLevel()
        {
            return (_logLevel);
        }
        /*
                /// <summary>Overrides the standard ToString() method by returning a nice text.</summary>
                /// <returns>The respective string</returns>
                public override string ToString()
                {
                    string retStr = "";

                    if (_logLevel == LogLevel.Off) {
                        retStr = "Off";
                    }
                    else
                    {
                        string addStr = "";
                        if ((_logLevel & LogLevel.Error) > 0)
                        {
                            retStr += (addStr + "Error");
                            addStr = "+";
                        }

                        if ((_logLevel & LogLevel.Info) > 0)
                        {
                            retStr += (addStr + "Info");
                            addStr = "|";
                        }

                        if ((_logLevel & LogLevel.Debug) > 0)
                        {
                            retStr += (addStr + "Debug");
                            addStr = "|";
                        }

                        if ((_logLevel & LogLevel.DeepDebug) > 0)
                        {
                            retStr += (addStr + "DeepDebug");
                        }
                    }
        
                    return retStr;
                }
                        */
    } // class Logging

}
