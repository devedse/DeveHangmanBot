using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Dictionary<string, int> Points = new Dictionary<string, int>();

        public ChatState(long chatId)
        {
            ChatId = chatId;
        }

        public async Task HandleMessage(TelegramBotClient bot, Message message)
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
                if (msg == "/stop")
                {
                    BotActive = false;
                    CurrentGame = null;
                    await bot.SendTextMessageAsync(ChatId, "Bot is now inactive");
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
                        var random = new Random();
                        var chosenWord = words[random.Next(words.Count)];

                        CurrentGame = new HangmanGameState(this, chosenWord);
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
                        sb.AppendLine($"{point.Key}: {point.Value}");
                    }

                    await bot.SendTextMessageAsync(ChatId, sb.ToString());
                }
                else if (CurrentGame != null)
                {
                    var correct = await CurrentGame.HandleGuess(bot, msg);

                    if (correct)
                    {
                        await bot.SendTextMessageAsync(ChatId, $"You fucking did it {message.From.Username}, 10 points to gryffindor");
                        AddPoints(message.From.Username, 10);
                    }
                }
            }
        }

        private void AddPoints(string user, int points)
        {
            if (Points.ContainsKey(user))
            {
                Points[user] += points;
            }
            else
            {
                Points.Add(user, points);
            }
        }

        public async Task DisplayHelp(TelegramBotClient bot)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Type /words to see available word lists");
            sb.AppendLine("Type /play 1 2 3 to start a game in the bot");

            await bot.SendTextMessageAsync(ChatId, sb.ToString());
        }

        public async Task DisplayWordLists(TelegramBotClient bot)
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
    }
}
