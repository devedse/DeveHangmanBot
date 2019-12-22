using DeveCoolLib.Logging;
using System;
using Telegram.Bot;

namespace DeveHangmanBot.TelegramBot.TelegramLogging
{
    public class TelegramLogger : ILogger
    {
        private readonly LogLevel _levelToLog;
        private readonly TelegramBotClient _bot;
        private readonly long _chatId;

        public TelegramLogger(LogLevel levelToLog, TelegramBotClient bot, long chatId)
        {
            _levelToLog = levelToLog;
            _bot = bot;
            _chatId = chatId;
        }

        public void Write(string str, LogLevel logLevel = LogLevel.Information, ConsoleColor color = ConsoleColor.Gray)
        {
            if ((int)logLevel >= (int)_levelToLog)
            {
                _bot.SendTextMessageAsync(_chatId, str).GetAwaiter().GetResult();
            }
        }

        public void WriteError(string str, LogLevel logLevel = LogLevel.Error)
        {
            Write(str, logLevel, ConsoleColor.Red);
        }

        public void EmptyLine()
        {
            Write(string.Empty, LogLevel.LogAlways);
        }
    }
}
