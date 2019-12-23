using System.Collections.Generic;
using Telegram.Bot.Types;

namespace DeveHangmanBot
{
    public class GlobalBotState
    {
        public Dictionary<long, User> AllUsers { get; set; } = new Dictionary<long, User>();
    }
}
