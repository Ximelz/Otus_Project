using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public interface ITelegramMessageService
    {
        /// <summary>
        /// Объект telegram бота.
        /// </summary>
        ITelegramBotClient bot { get; }

        /// <summary>
        /// Токен отмены.
        /// </summary>
        CancellationToken ct { get; }

        /// <summary>
        /// Данные о сообщении из telegram.
        /// </summary>
        Update update { get; }

        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        ProjectUser user { get; }

        /// <summary>
        /// Метод обычной отправки сообщения.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        Task SendMessage(string message);

        /// <summary>
        /// Метод отправки сообщение со стандартной клавиатурой под строкой ввода, зависящей от роли пользователя.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        Task SendMessageWithDefaultKeyboard(string message);

        /// <summary>
        /// Метод отправки сообщения с определенным типом клавиатуры.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="keyboardType">Тип клавиатуры.</param>
        Task SendMessageByKeyboardType(string message, KeyboardTypes keyboardType);

        /// <summary>
        /// Метод отправки сообщения с кнопками.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="keyboard">Кнопки с объектами.</param>
        Task SendMessageWithKeyboard(string message, ReplyMarkup keyboard);

        /// <summary>
        /// Метод получения сообщения.
        /// </summary>
        /// <returns>Сообщение.</returns>
        Task<string> GetMessage();

        /// <summary>
        /// Установка команд у пользователя в выпадающем меню.
        /// </summary>
        Task SetUserCommands();

        /// <summary>
        /// Метод отправки сообщения конкретному пользователю.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="user">Пользователь, которому нужно отправить сообщение.</param>
        Task SendMessageToUser(string message, ProjectUser user);
    }
}
