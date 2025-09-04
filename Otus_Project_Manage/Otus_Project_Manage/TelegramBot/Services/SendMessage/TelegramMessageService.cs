using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class TelegramMessageService : ITelegramMessageService
    {
        public TelegramMessageService(ITelegramBotClient bot, Update update, ProjectUser user, CancellationToken ct)
        {
            _user = user;
            _botClient = bot;
            _update = update;
            _ct = ct;
            telegramId = update.Type == UpdateType.CallbackQuery ? update.CallbackQuery.From.Id : update.Message.From.Id;
        }

        private ITelegramBotClient _botClient;
        private CancellationToken _ct;
        private Update _update;
        private ProjectUser _user;
        private long telegramId;
        public ITelegramBotClient bot => _botClient;

        public CancellationToken ct => _ct;

        public Update update => _update;

        public ProjectUser user => _user;

        public Task<string> GetMessage()
        {
            _ct.ThrowIfCancellationRequested();

            if (update.Type == UpdateType.CallbackQuery)
                return Task.FromResult(_update.CallbackQuery.Data);

            return Task.FromResult(_update.Message.Text);
        }

        public async Task SendMessage(string message)
        {
            _ct.ThrowIfCancellationRequested();

            await _botClient.SendMessage(chatId: telegramId,
                                         text: message,
                                         parseMode: ParseMode.Html,
                                         cancellationToken: _ct);
        }

        public async Task SendMessageWithDefaultKeyboard(string message)
        {
            _ct.ThrowIfCancellationRequested();

            KeyboardTypes keyboardType = KeyboardCommands.GetKeyoardTypeByRole(_user.role);
            ReplyKeyboardMarkup keyboard = KeyboardCommands.GetKeyboardMarkup(keyboardType);


            await _botClient.SendMessage(chatId: telegramId,
                                         text: message,
                                         replyMarkup: keyboard,
                                         parseMode: ParseMode.Html,
                                         cancellationToken: _ct);
        }

        public async Task SendMessageByKeyboardType(string message, KeyboardTypes keyboardType)
        {
            _ct.ThrowIfCancellationRequested();

            ReplyKeyboardMarkup keyboard = KeyboardCommands.GetKeyboardMarkup(keyboardType);

            await _botClient.SendMessage(chatId: telegramId,
                                         text: message,
                                         replyMarkup: keyboard,
                                         parseMode: ParseMode.Html,
                                         cancellationToken: _ct);
        }

        public async Task SendMessageWithKeyboard(string message, ReplyMarkup keyboard)
        {
            _ct.ThrowIfCancellationRequested();

            await _botClient.SendMessage(chatId: telegramId,
                                         text: message,
                                         replyMarkup: keyboard,
                                         parseMode: ParseMode.Html,
                                         cancellationToken: _ct);
        }

        public async Task SetUserCommands()
        {
            _ct.ThrowIfCancellationRequested();

            Chat chat;

            if (update.Type == UpdateType.CallbackQuery)
                chat = update.CallbackQuery.Message.Chat;
            else
                chat = update.Message.Chat;

            List<BotCommand> currentCommands = KeyboardCommands.GetUserCommands(user);

            await _botClient.SetMyCommands(currentCommands, BotCommandScope.Chat(chat.Id));
        }
    }
}
