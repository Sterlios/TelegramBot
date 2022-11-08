using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class СompetitionBot
    {
        private ITelegramBotClient _bot;
        private List<ChatId> _moderators = new List<ChatId>();
        private Chat _channel;
        private const string _applyButton = "apply";
        private const string _cancelButton = "cancel";

        public СompetitionBot(string token)
        {
            _bot = new TelegramBotClient(token);
            _moderators.Add(474698824); // @alinavolynets
            //_moderators.Add(737444990); // @Kuzmin_Anton_S
            _channel = _bot.GetChatAsync("@TestForBotNa").Result;
            //_channel = _bot.GetChatAsync("@testcontestexample").Result;
        }

        public void Start()
        {
            Console.WriteLine("Запущен бот " + _bot.GetMeAsync().Result.FirstName);

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
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                await HandleMassage(update);
                return;
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                await HandleButton(update);
                return;
            }
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private async Task HandleMassage(Update update)
        {
            if (update.Message.Type == MessageType.Text ||
                update.Message.Type == MessageType.Video || 
                update.Message.Type == MessageType.VideoNote)
            {
                foreach (var moderator in _moderators)
                {
                    await SendToModerator(moderator, update);
                }

                await _bot.SendTextMessageAsync(update.Message.Chat, "Сообщение отправлено модератору!");
            }
        }

        private async Task SendToModerator(ChatId moderator, Update update)
        {
            Message memberMessage = _bot.ForwardMessageAsync(moderator, update.Message.Chat, update.Message.MessageId).Result;

            if (update.Message.Type == MessageType.Video || update.Message.Type == MessageType.VideoNote)
            {
                var buttons = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Принять", _applyButton),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Отклонить", _cancelButton),
                    },
                });

                Message buttonCallBack = _bot.SendTextMessageAsync(moderator,
                    $"Видео от пользователя " +
                    $"{update.Message.Chat.FirstName} " +
                    $"{update.Message.Chat.LastName} " +
                    $"(@{update.Message.Chat.Username})",
                    replyMarkup: buttons, replyToMessageId: memberMessage.MessageId).Result;
            }
        }

        private async Task HandleButton(Update update)
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
                default:
                    break;
            }
        }

        private async Task SendAnswers(Update update, string answer)
        {
            await _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat,
                $"Видео от пользователя " +
                $"{update.CallbackQuery.Message.Chat.FirstName} " +
                $"{update.CallbackQuery.Message.Chat.LastName} " +
                $"(@{update.CallbackQuery.Message.Chat.Username})\n" +
                $"{answer}",
                replyToMessageId: update.CallbackQuery.Message.MessageId - 1);
            await _bot.DeleteMessageAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId);
            await _bot.SendTextMessageAsync(update.CallbackQuery.Message.ReplyToMessage.ForwardFrom.Id, answer);
        }
    }
}