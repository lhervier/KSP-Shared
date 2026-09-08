using System.Reflection;
using UnityEngine;

namespace com.github.lhervier.ksp.shared 
{
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4,
        Trace = 5
    }

    public class ModLogger 
    {
        private static readonly string PREFIX = "[" + Constants.ModName + "]";
        private static LogLevel _logLevel = LogLevel.Info;
        
        private readonly string additionalPrefix = "";
        
        public static void SetLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        /// <summary>
        /// Whether a message logged at <paramref name="level"/> would be written.
        ///
        /// Meant to be tested before building the message itself: Log() checks the level too, but only
        /// once it has been called, that is to say once the interpolated string has been built. On a code
        /// path that runs at every frame, that string is the whole cost of a log that goes nowhere.
        /// </summary>
        public bool IsEnabled(LogLevel level) => level <= _logLevel;

        public bool IsErrorEnabled => IsEnabled(LogLevel.Error);
        public bool IsWarningEnabled => IsEnabled(LogLevel.Warning);
        public bool IsInfoEnabled => IsEnabled(LogLevel.Info);
        public bool IsDebugEnabled => IsEnabled(LogLevel.Debug);
        public bool IsTraceEnabled => IsEnabled(LogLevel.Trace);
        
        public ModLogger() 
        {
        }
        
        public ModLogger(string additionalPrefix) : this()
        {
            this.additionalPrefix = "[" + additionalPrefix + "]";
        }


        public void Log(string message, LogLevel level) 
        {
            if (IsEnabled(level))
            {
                string levelPrefix;
                switch (level)
                {
                    case LogLevel.Error:
                        levelPrefix = "[ERR ]";
                        break;
                    case LogLevel.Warning:
                        levelPrefix = "[WARN]";
                        break;
                    case LogLevel.Info:
                        levelPrefix = "[INFO]";
                        break;
                    case LogLevel.Debug:
                        levelPrefix = "[DBG ]";
                        break;
                    case LogLevel.Trace:
                        levelPrefix = "[TRC ]";
                        break;
                    default:
                        levelPrefix = "";
                        break;
                }
                if( level == LogLevel.Error )
                {
                    Debug.LogError(PREFIX + levelPrefix + this.additionalPrefix + " " + message);
                }
                else if( level == LogLevel.Warning )
                {
                    Debug.LogWarning(PREFIX + levelPrefix + this.additionalPrefix + " " + message);
                } else {
                    Debug.Log(PREFIX + levelPrefix + this.additionalPrefix + " " + message);
                }
            }
        }

        public void LogError(string message) => Log(message, LogLevel.Error);
        public void LogWarning(string message) => Log(message, LogLevel.Warning);
        public void LogInfo(string message) => Log(message, LogLevel.Info);
        public void LogDebug(string message) => Log(message, LogLevel.Debug);
        public void LogTrace(string message) => Log(message, LogLevel.Trace);
    }
}