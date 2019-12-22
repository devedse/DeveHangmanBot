using DeveCoolLib.Logging;
using DeveCoolLib.Logging.Appenders;
using System.Collections.Generic;
using Telegram.Bot;

namespace DeveHangmanBot.TelegramBot.TelegramLogging
{
    public static class TelegramLoggerFactory
    {
        public static ILogger CreateLogger(TelegramBotClient bot, long chatId, params ILogger[] extraLoggers)
        {
            var consoleLogger = new ConsoleLogger(LogLevel.Verbose);
            var telegramLogger = new TelegramLogger(LogLevel.Information, bot, chatId);

            var loggerList = new List<ILogger>() { consoleLogger, telegramLogger };
            loggerList.AddRange(extraLoggers);
            var multiLogger = new MultiLoggerAppender(loggerList);

            return multiLogger;
        }
    }
}
