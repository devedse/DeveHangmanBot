namespace DeveHangmanBot.Config
{
    public class BotConfig
    {
        public string TelegramBotToken { get; set; }

        public string GoogleApiKey { get; set; }
        public string GoogleCxToken { get; set; }

        public string GiphyApiKey { get; set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(TelegramBotToken);

    }
}
