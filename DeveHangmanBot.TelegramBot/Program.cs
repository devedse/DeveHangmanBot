using DeveHangmanBot.Config;
using System;
using System.Threading.Tasks;

namespace DeveHangmanBot.TelegramBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = BotConfigLoader.LoadFromStaticFile();

            var bot = new DeveHangmanTelegramBot(config);
            await bot.Start();
        }
    }
}
