using DeveCoolLib.Logging;
using DeveCoolLib.Threading;
using DeveHangmanBot.Config;
using DeveHangmanBot.ImageStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DeveHangmanBot
{
    public class ChatState
    {
        public HangmanGameState CurrentGame { get; set; }
        public bool BotActive { get; set; } = true;
        public long ChatId { get; }

        public Dictionary<long, int> Points = new Dictionary<long, int>();
        private readonly BotConfig _botconfig;
        private readonly ILogger _logger;
        private readonly GlobalBotState _globalBotState;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        private readonly ImageObtainer _imageObtainer;

        public ChatState(BotConfig botconfig, ILogger logger, GlobalBotState globalBotState, long chatId)
        {
            _botconfig = botconfig;
            _logger = logger;
            _globalBotState = globalBotState;
            ChatId = chatId;

            _imageObtainer = new ImageObtainer(botconfig);
        }

        public async Task HandleMessage(TelegramBotClient bot, Message message)
        {
            //Ensure only one message can be handled at a time
            using (var disposableSemaphore = await _semaphoreSlim.DisposableWaitAsync())
            {
                var msg = message.Text;

                if (!BotActive)
                {
                    switch (msg)
                    {
                        case "/start":
                            BotActive = true;
                            await bot.SendTextMessageAsync(ChatId, "Bot is now active");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (msg.Equals("/stop"))
                    {
                        BotActive = false;
                        CurrentGame = null;
                        await bot.SendTextMessageAsync(ChatId, "Bot is now inactive");
                    }
                    else if (msg.Equals("/stopgame"))
                    {
                        await bot.SendTextMessageAsync(ChatId, "Stopping game");
                        CurrentGame = null;
                    }
                    else if (msg.StartsWith("/play"))
                    {
                        var chosenWordLists = msg.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);

                        var wordLists = WordListInitiator.GetThese(chosenWordLists);
                        var words = wordLists.SelectMany(t => t.Words).ToList();

                        if (words.Count == 0)
                        {
                            await DisplayHelp(bot);
                        }
                        else
                        {
                            CurrentGame = new HangmanGameState(_imageObtainer, _logger, this, words);
                            await CurrentGame.PrintHang(bot);
                        }
                    }
                    else if (msg.Equals("/words"))
                    {
                        await DisplayWordLists(bot);
                    }
                    else if (msg.Equals("/points"))
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Points:");
                        var pointThings = Points.OrderByDescending(t => t.Value);
                        foreach (var point in pointThings)
                        {
                            sb.AppendLine($"{GetName(point.Key)}: {point.Value}");
                        }

                        await bot.SendTextMessageAsync(ChatId, sb.ToString());
                    }
                    else if (msg.Equals("/hint") && CurrentGame != null)
                    {
                        var isNoob = await CurrentGame.GiveHint(bot, msg);
                        if (isNoob)
                        {
                            AddPoints(message.From.Id, -1000);
                        }
                    }
                    else if (msg.Equals("/cheat") && CurrentGame != null && message.From.Id == BotConstants.DevedseId)
                    {
                        await CurrentGame.Cheat(bot);
                    }
                    else if (CurrentGame != null)
                    {
                        var correct = await CurrentGame.HandleGuess(bot, msg);

                        if (correct)
                        {
                            await bot.SendTextMessageAsync(ChatId, $"You fucking did it {GetName(message.From.Id)}, 10 points to gryffindor");
                            AddPoints(message.From.Id, 10);

                            CurrentGame = null;
                        }
                    }
                }
            }
        }

        private void AddPoints(long userId, int points)
        {
            if (Points.ContainsKey(userId))
            {
                Points[userId] += points;
            }
            else
            {
                Points.Add(userId, points);
            }
        }

        private async Task DisplayHelp(TelegramBotClient bot)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Type a letter to guess");
            sb.AppendLine("Type a word to guess the whole word, the bot will respond if you are right!");
            sb.AppendLine("Type /words to see available word lists");
            sb.AppendLine("Type /play 1 2 3 to start a game with a word list in the bot");
            sb.AppendLine("Type /stopgame to stop the current game");

            await bot.SendTextMessageAsync(ChatId, sb.ToString());
        }

        private async Task DisplayWordLists(TelegramBotClient bot)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Available word lists for /play (e.g. /play 1 2)");

            for (int i = 0; i < WordListInitiator.WordLists.Count; i++)
            {
                var curwordlist = WordListInitiator.WordLists[i];
                sb.AppendLine($"{i}: {curwordlist.Name}");
            }

            await bot.SendTextMessageAsync(ChatId, sb.ToString());
        }

        public string GetName(long id)
        {
            var user = _globalBotState.AllUsers[id];
            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName))
            {
                return $"{user.FirstName} {user.LastName}";
            }
            else if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                return user.FirstName;
            }
            else if (!string.IsNullOrWhiteSpace(user.Username))
            {
                return user.Username;
            }
            return user.Id.ToString();
        }
    }
}
