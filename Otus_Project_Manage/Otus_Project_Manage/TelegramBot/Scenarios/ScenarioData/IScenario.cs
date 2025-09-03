
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Otus_Project_Manage
{
    public interface IScenario
    {
        /// <summary>
        /// Тип сценария.
        /// </summary>
        ScenarioTypes ScenarioType { get; }

        /// <summary>
        /// Метод обработки сценария.
        /// </summary>
        /// <param name="telegramMessageService">Объект для отправки и получения сообщений из текущего бота.</param>
        /// <param name="userScenario">Информация о выполнении сценария пользователем.</param>
        /// <returns>Возвращает статус сценария.</returns>
        Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario);
    }
}
