using DeveCoolLib.Logging;
using DeveHangmanBot.Config;
using DeveHangmanBot.TelegramBot.TelegramLogging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace DeveHangmanBot.TelegramBot
{
    public class DeveHangmanTelegramBot
    {
        private readonly TelegramBotClient _bot;
        private readonly BotConfig _botConfig;
        private readonly ILogger[] _extraLoggers;

        private Dictionary<long, ChatState> _chatStates = new Dictionary<long, ChatState>();


        public DeveHangmanTelegramBot(BotConfig botConfig, params ILogger[] extraLoggers)
        {
            _botConfig = botConfig;
            _extraLoggers = extraLoggers;
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

            var logger = TelegramLoggerFactory.CreateLogger(_bot, message.Chat.Id, _extraLoggers);

            try
            {
                switch (message.Type)
                {
                    case MessageType.Text:
                        await HandleTxt(logger, message);
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

        private async Task HandleTxt(ILogger logger, Message message)
        {
            var txt = message.Text;
            var chatUser = message.From.FirstName;


            var currentChatId = message.Chat.Id;

            ChatState curChat;
            _chatStates.TryGetValue(currentChatId, out curChat);
            if (curChat == null)
            {
                curChat = new ChatState(currentChatId);
                _chatStates.Add(currentChatId, curChat);
            }




            if (txt.Equals("/help", StringComparison.OrdinalIgnoreCase))
            {
                logger.Write($"Hello {message.From.FirstName}{Environment.NewLine}Some usefull data:{Environment.NewLine}BotId: {_botConfig.TelegramBotToken.Split(':').FirstOrDefault()}{Environment.NewLine}ChatId: {message.Chat.Id}{Environment.NewLine}UserId: {message.From.Id}{Environment.NewLine}Version: {Assembly.GetEntryAssembly().GetName().Version}");
            }
            else if (txt.Equals("/update", StringComparison.OrdinalIgnoreCase) && message.From.Id == 239844924L)
            {
                var task = Task.Run(async () =>
                {
                    for (int i = 5; i > 0; i--)
                    {
                        logger.Write($"Killing app in {i} seconds...");
                        await Task.Delay(1000);
                    }
                    logger.Write("Killing app now");

                    Environment.Exit(0);
                });
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
    }
}
