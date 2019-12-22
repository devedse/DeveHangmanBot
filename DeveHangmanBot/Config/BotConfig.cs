namespace DeveHangmanBot.Config
{
    public class BotConfig
    {
        public string TelegramBotToken { get; set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(TelegramBotToken);
    }
}
