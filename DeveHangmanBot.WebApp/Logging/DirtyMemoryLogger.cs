using DeveCoolLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeveHangmanBot.WebApp.Logging
{
    public class DirtyMemoryLogger : ILogger
    {
        private static object Lockject = new object();

        public static List<string> LoggingLines = new List<string>();
        private readonly LogLevel _levelToLog;

        public DirtyMemoryLogger(LogLevel levelToLog)
        {
            _levelToLog = levelToLog;
        }

        public void EmptyLine()
        {

        }

        public void Write(string str, LogLevel logLevel = LogLevel.Information, ConsoleColor color = ConsoleColor.Gray)
        {
            if ((int)logLevel >= (int)_levelToLog)
            {
                lock (Lockject)
                {
                    LoggingLines.Add(str);
                }
            }
        }

        public void WriteError(string str, LogLevel logLevel = LogLevel.Error)
        {
            Write(str, logLevel);
        }
    }
}
