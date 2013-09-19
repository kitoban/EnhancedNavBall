using UnityEngine;

namespace EnhancedNavBall
{
    enum LogLevel
    {
        None = 0,
        Minimal = 1,
        Diagnostic = 2
    }

    static class Utilities
    {
        static LogLevel loggingLevel = LogLevel.None;

        public static void DebugLog(LogLevel logLevel, string log)
        {
            if (logLevel <= loggingLevel)
            {
                Debug.Log(log);
            }
        }
    }
}
