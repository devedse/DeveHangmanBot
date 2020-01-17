using DeveCoolLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;

namespace DeveHangmanBot
{
    public class HangmanGameState
    {
        private readonly ILogger _logger;
        private readonly ChatState _chatState;
        private readonly List<string> _potentialWords;

        public string Word { get; }

        public List<char> GuessedLetters { get; }

        public HangmanGameState(ILogger logger, ChatState chatState, List<string> potentialWords)
        {
            _logger = logger;
            _chatState = chatState;
            _potentialWords = potentialWords;

            var random = new Random();
            Word = potentialWords[random.Next(potentialWords.Count)];

            GuessedLetters = new List<char>();
        }

        public async Task<bool> HandleGuess(TelegramBotClient bot, string msg)
        {
            msg = msg.ToLowerInvariant().Trim();

            var result = false;

            if (msg.Length == 1)
            {
                GuessedLetters.Add(msg[0]);
                result = await PrintHang(bot);
            }
            else if (msg.Equals(Word))
            {
                GuessedLetters.AddRange(msg);
                result = await PrintHang(bot);
            }


            return result;
        }

        public async Task<bool> PrintHang(TelegramBotClient bot)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Word:");

            var guessProgress = GetCurrentGuessedProgress();
            sb.Append(guessProgress.wordToWrite);

            await bot.SendTextMessageAsync(_chatState.ChatId, sb.ToString());

            return guessProgress.guessesIncorrect == 0;
        }

        private (string wordToWrite, int guessesIncorrect) GetCurrentGuessedProgress(string notFoundLetter = "_", string spaceBetween = " ")
        {
            var sb = new StringBuilder();

            int guessesIncorrect = 0;

            for (int i = 0; i < Word.Length; i++)
            {
                var curChar = Word[i];
                if (GuessedLetters.Contains(curChar))
                {
                    sb.Append(curChar);
                }
                else
                {
                    guessesIncorrect++;
                    sb.Append(notFoundLetter);
                }
                sb.Append(spaceBetween);
            }

            return (sb.ToString(), guessesIncorrect);
        }

        public async Task<bool> GiveHint(TelegramBotClient bot, string msg)
        {
            var allLetters = Word.Distinct();
            var allRemainingLetters = allLetters.Except(GuessedLetters).Distinct().ToList();
            if (allRemainingLetters.Count > 1)
            {
                var r = new Random();
                await HandleGuess(bot, allRemainingLetters[r.Next(allRemainingLetters.Count)].ToString());
                return false;
            }
            else
            {
                await bot.SendTextMessageAsync(_chatState.ChatId, "Boy, foh real?, there's only one letter remaining, you damn shitnoob! -1000 points just for asking.");
                return true;
            }
        }

        public async Task Cheat(TelegramBotClient bot)
        {
            var regexPart = GetCurrentGuessedProgress(".", "");
            var wordProgress = $"^{regexPart.wordToWrite}$";
            var regex = new Regex(wordProgress, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var sb = new StringBuilder();
            sb.AppendLine("If you wouldn't be so shit, you'd know that these would be possible:");
            var possibilities = _potentialWords.Distinct().Where(t => regex.IsMatch(t)).ToList();

            foreach (var possibility in possibilities.Distinct())
            {
                sb.AppendLine(possibility);
            }

            await bot.SendTextMessageAsync(_chatState.ChatId, sb.ToString());
        }
    }
}
