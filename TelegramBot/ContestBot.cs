using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class ContestBot
    {
        private const string _applyButton = "apply";
        private const string _cancelButton = "cancel";
        private const string _addModeratorButton = "addModerator";
        private const string _canсelModeratorButton = "canсelModerator";
        private const string _startCommand = "/start";
        private const string _moderatorCommand = "/moderator";

        private ModeratorsPool _moderators;
        private Chat _channel;
        private Chat _newModeratorID = null;

        private ITelegramBotClient _bot;

        public ContestBot(string token)
        {
            _bot = new TelegramBotClient(token);
            _moderators = new ModeratorsPool(new Moderator(_bot, _bot.GetChatAsync(ModeratorChatID).Result));

            _channel = _bot.GetChatAsync(ChannelChatId).Result;
        }

        public void Start()
        {
            Console.WriteLine("Запущен бот " + _bot.GetMeAsync().Result.FirstName);
            Console.WriteLine("Login " + _bot.GetMeAsync().Result.Username);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            _bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.ReadLine();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            switch (update.Type)
            {
                case UpdateType.Message:
                    await HandleMessageAsync(update);
                    return;
                case UpdateType.CallbackQuery:
                    await HandleButtonAsync(update);
                    return;
                default:
                    break;
            }
        }

        private async Task HandleMessageAsync(Update update)
        {
            switch (update.Message.Text?.ToLower())
            {
                case _startCommand:
                    await _bot.SendTextMessageAsync(update.Message.Chat, "Доброго времени суток!");
                    return;
                case _moderatorCommand:
                    await SendAddModeratorQuery(update);
                    return;
                default:
                    await HandleMassage(update.Message);
                    return;
            }
        }

        private async Task SendAddModeratorQuery(Update update)
        {
            await _moderators.SendAddModeratorQuery(new Moderator(_bot, update.Message.Chat));
            _newModeratorID = update.Message.Chat;
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private async Task HandleMassage(Message message)
        {
            if (message.Type == MessageType.Text ||
                message.Type == MessageType.Video ||
                message.Type == MessageType.VideoNote)
            {
                await _moderators.SendMassage(message);

                await _bot.SendTextMessageAsync(message.Chat, "Сообщение отправлено модератору!");
            }
        }

        private async Task HandleButtonAsync(Update update)
        {
            var pressedButtonID = update.CallbackQuery.Data;

            switch (pressedButtonID)
            {
                case _applyButton:
                    await _bot.ForwardMessageAsync(_channel, update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.ReplyToMessage.MessageId);
                    await SendAnswers(update, "ПРИНЯТО");
                    break;
                case _cancelButton:
                    await SendAnswers(update, "ОТКЛОНЕНО");
                    break;
                case _addModeratorButton:
                    await ApplyAddModerator(update);
                    break;
                default:
                    break;
            }

            await _bot.DeleteMessageAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId);
        }

        private async Task ApplyAddModerator(Update update)
        {
            if (!_moderators.TryAddModerator(update.CallbackQuery.Message.Text))
            {
                await _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat, 
                    "Не удалось пользователя сделать модератором (Возможно такой пользователь уже модератор)", 
                    replyToMessageId: update.CallbackQuery.Message.MessageId);
                return;
            }

            await _moderators.SendApplyingAddModerator(update);
        }

        private async Task SendAnswers(Update update, string answer)
        {
            await _moderators.SendAnswer(update.CallbackQuery.Message.ReplyToMessage, update.CallbackQuery.From, answer);
            //var query = update.CallbackQuery.Message.Chat;

            //await _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat, message, replyToMessageId: update.CallbackQuery.Message.MessageId - 1);
            //await _bot.DeleteMessageAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId);

            //foreach (var moderator in _moderators)
            //{
            //    if (moderator != update.CallbackQuery.Message.Chat)
            //    {
            //        await _bot.SendTextMessageAsync(moderator, message);
            //    }
            //}

            //await _bot.SendTextMessageAsync(update.CallbackQuery.Message.ReplyToMessage.ForwardFrom.Id, answer);
        }
    }
}
