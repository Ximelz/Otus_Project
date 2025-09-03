using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface IScenarioService
    {
        /// <summary>
        /// Получение сценария обработки команды на основании типа сценария.
        /// </summary>
        /// <param name="scenarioType">Тип сценария.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденный сценарий.</returns>
        Task<IScenario> GetScenarioByType(ScenarioTypes scenarioType, CancellationToken ct);

        /// <summary>
        /// Метод получения данных о сценарии по id пользователя из telegram.
        /// </summary>
        /// <param name="telegramUserId">Id пользователя в telegram.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Возвращает данные о сценарим если у пользователя есть активый сценарий обработки команды.</returns>
        Task<UserScenarioData?> GetScenarioDataByUserId(long telegramUserId, CancellationToken ct);

        /// <summary>
        /// Метод сохранения данных о сценарии пользовтеля.
        /// </summary>
        /// <param name="userScenarioData">Данные о сценарии пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        Task SaveUserScenarioData(UserScenarioData userScenarioData, CancellationToken ct);

        /// <summary>
        /// Метод сброса работы сценария у пользователя.
        /// </summary>
        /// <param name="telegramUserId">Данные о сценарии пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        Task ResetScenarioByUserTelegramId(long telegramUserId, CancellationToken ct);

        /// <summary>
        /// Метод получения всех пользовательских данных об активных сценариях.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список всех сценариев.</returns>
        Task<IReadOnlyList<UserScenarioData>?> GetAllScenatiosData(CancellationToken ct);
    }
}
