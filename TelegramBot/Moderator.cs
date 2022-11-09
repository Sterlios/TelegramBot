using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Moderator
    {
        private const string _applyButton = "apply";
        private const string _cancelButton = "cancel";
        private const string _addModeratorButton = "addModerator";
        private const string _canсelModeratorButton = "canсelModerator";

        private IReplyMarkup _videoMarkup;
        private IReplyMarkup _moderatorMarkup;
        private ITelegramBotClient _bot;

        public User User { get; private set; }
        public Chat Chat { get; private set; }

        private Moderator(ITelegramBotClient bot)
        {
            _bot = bot;

            InitializeButtons(ref _moderatorMarkup, _addModeratorButton, _canсelModeratorButton);
            InitializeButtons(ref _videoMarkup, _applyButton, _cancelButton);
        }

        public Moderator(ITelegramBotClient bot, User user) : this(bot)
        {
            User = user;
        }

        public Moderator(ITelegramBotClient bot, Chat chat) : this(bot)
        {
            Chat = chat;
        }

        public async Task ReceiveAddModeratorQuery(Moderator newModerator)
        {
            await _bot.SendTextMessageAsync(Chat, $"Стать модератором желает " +
                $"{newModerator.Chat.FirstName} " +
                $"{newModerator.Chat.LastName} " +
                $"(@{newModerator.Chat.Username}) " +
                $"id: {newModerator.Chat.Id}", 
                replyMarkup: _moderatorMarkup);
        }

        public async Task RecieveUserMessage(Message message)
        {
            Message memberMessage = _bot.ForwardMessageAsync(Chat, message.Chat, message.MessageId).Result;

            if (message.Type == MessageType.Video || message.Type == MessageType.VideoNote)
            {
                await _bot.SendTextMessageAsync(Chat,
                    $"Видео от пользователя " +
                    $"{message.Chat.FirstName} " +
                    $"{message.Chat.LastName} " +
                    $"(@{message.Chat.Username})",
                    replyMarkup: _videoMarkup, replyToMessageId: memberMessage.MessageId);
            }
        }

        public async Task SendApplyingAddModerator(Update query, Moderator newModerator)
        {
            User Moderator = query.CallbackQuery.From;
            string message = $"Пользователь " +
                $"{newModerator.Chat.FirstName} " +
                $"{newModerator.Chat.LastName} " +
                $"(@{newModerator.Chat.Username}) " +
                $"id: {newModerator.Chat.Id}\n" +
                $"Добавлен в модераторы\n" +
                $"Добавивший модератор: " +
                $"{Moderator.FirstName} " +
                $"{Moderator.LastName} " +
                $"(@{Moderator.Username})";

            await _bot.SendTextMessageAsync(Chat, message);
        }

        public async Task SendAnswer(string message)
        {
            await _bot.SendTextMessageAsync(Chat, message);
        }

        private void InitializeButtons(ref IReplyMarkup markup, string applyButton, string cancelButton)
        {
            markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Принять", applyButton),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Отклонить", cancelButton),
                },
            });
        }
    }
}
