using DeveCoolLib.Logging;
using DeveHangmanBot.Config;
using DeveHangmanBot.TelegramBot.TelegramLogging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DeveHangmanBot.TelegramBot
{
    public class DeveHangmanTelegramBot
    {
        private readonly TelegramBotClient _bot;
        private readonly BotConfig _botConfig;
        private readonly ILogger[] _extraLoggers;

        private readonly ConcurrentDictionary<long, ChatState> _chatStates = new ConcurrentDictionary<long, ChatState>();

        private readonly GlobalBotState _globalBotState = new GlobalBotState();

        private readonly ILogger _logger;

        public DeveHangmanTelegramBot(BotConfig botConfig, params ILogger[] extraLoggers)
        {
            _botConfig = botConfig;
            _extraLoggers = extraLoggers;
            _logger = TelegramLoggerFactory.CreateLogger(_extraLoggers);

            _bot = new TelegramBotClient(botConfig.TelegramBotToken);

            _bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnMessageEdited += BotOnMessageReceived;
            //_bot.OnInlineQuery += BotOnInlineQueryReceived;
            //_bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            _bot.OnReceiveError += BotOnReceiveError;
        }

        public async Task Start()
        {
            var me = await _bot.GetMeAsync();
            Console.Title = me.Username;

            _bot.StartReceiving();
            Console.WriteLine("Bot started :)");

            while (true)
            {
                await Task.Delay(60000);
            }

            //_bot.StopReceiving();
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            var errorMessage = $"Error in TelegramBot: {receiveErrorEventArgs.ApiRequestException.ToString()}";
            Console.WriteLine(errorMessage);
            foreach (var logger in _extraLoggers)
            {
                logger.WriteError(errorMessage);
            }
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null)
            {
                return;
            }

            try
            {
                switch (message.Type)
                {
                    case MessageType.Text:
                        await HandleTxt(message);
                        return;
                    case MessageType.Photo:
                    case MessageType.Unknown:
                    case MessageType.Audio:
                    case MessageType.Video:
                    case MessageType.Voice:
                    case MessageType.Document:
                    case MessageType.Sticker:
                    case MessageType.Location:
                    case MessageType.Contact:
                    case MessageType.Venue:
                    case MessageType.Game:
                    case MessageType.VideoNote:
                    case MessageType.Invoice:
                    case MessageType.SuccessfulPayment:
                    case MessageType.WebsiteConnected:
                    case MessageType.ChatMembersAdded:
                    case MessageType.ChatMemberLeft:
                    case MessageType.ChatTitleChanged:
                    case MessageType.ChatPhotoChanged:
                    case MessageType.MessagePinned:
                    case MessageType.ChatPhotoDeleted:
                    case MessageType.GroupCreated:
                    case MessageType.SupergroupCreated:
                    case MessageType.ChannelCreated:
                    case MessageType.MigratedToSupergroup:
                    case MessageType.MigratedFromGroup:
                    case MessageType.Poll:
                    default:
                        return;
                }

            }
            catch (Exception ex)
            {
                await _bot.SendTextMessageAsync(message.Chat.Id, $"Er is iets goed naar de klote gegaan, contact Davy:{Environment.NewLine}{ex.ToString()}");
            }

        }

        private async Task HandleTxt(Message message)
        {
            var txt = message.Text;
            var chatUser = message.From.FirstName;


            var currentChatId = message.Chat.Id;

            var curChat = _chatStates.GetOrAdd(currentChatId, (i) =>
            {
                _logger.Write($"Added new chat group: {currentChatId}");
                return new ChatState(_logger, _globalBotState, currentChatId);
            });

            _globalBotState.AllUsers.TryAdd(message.From.Id, message.From);




            if (txt.Equals("/help", StringComparison.OrdinalIgnoreCase))
            {
                await LogAndRespond(currentChatId, $"Hello {message.From.FirstName}{Environment.NewLine}Some usefull data:{Environment.NewLine}BotId: {_botConfig.TelegramBotToken.Split(':').FirstOrDefault()}{Environment.NewLine}ChatId: {message.Chat.Id}{Environment.NewLine}UserId: {message.From.Id}{Environment.NewLine}Version: {Assembly.GetEntryAssembly().GetName().Version}");
            }
            else if (message.From.Id == BotConstants.DevedseId && txt.Equals("/update", StringComparison.OrdinalIgnoreCase))
            {
                //Admin commands only allowed by Devedse
                var task = Task.Run(async () =>
                {
                    for (int i = 5; i > 0; i--)
                    {
                        await LogAndRespond(currentChatId, $"Killing app in {i} seconds...");
                        await Task.Delay(1000);
                    }
                    await LogAndRespond(currentChatId, "Killing app now");

                    Environment.Exit(0);
                });
            }
            else if (message.From.Id == 239844924L && txt.StartsWith("/broadcast", StringComparison.OrdinalIgnoreCase))
            {
                //Admin commands only allowed by Devedse
                var cmd = "/broadcast ";
                if (txt.Length > cmd.Length)
                {
                    var remainder = txt.Substring(cmd.Length);
                    foreach (var chatstate in _chatStates)
                    {
                        await _bot.SendTextMessageAsync(chatstate.Key, remainder);
                    }
                }
            }
            else
            {
                await curChat.HandleMessage(_bot, message);
            }
            return;
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await _bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }

        public async Task LogAndRespond(long chatId, string msg)
        {
            _logger.Write(msg);
            await _bot.SendTextMessageAsync(chatId, msg);
        }
    }
}
