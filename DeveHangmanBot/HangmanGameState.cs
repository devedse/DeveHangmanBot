﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace DeveHangmanBot
{
    public class HangmanGameState
    {
        private readonly ChatState _chatState;

        public string Word { get; }

        public List<char> GuessedLetters { get; }

        public HangmanGameState(ChatState chatState, string word)
        {
            this._chatState = chatState;
            Word = word;
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
                    sb.Append("_");
                }
                sb.Append(" ");
            }

            await bot.SendTextMessageAsync(_chatState.ChatId, sb.ToString());

            return guessesIncorrect == 0;
        }
    }
}
