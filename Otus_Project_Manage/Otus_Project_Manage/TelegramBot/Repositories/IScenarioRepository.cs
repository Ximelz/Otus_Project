using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface IScenarioRepository
    {
        /// <summary>
        /// Получение сценария обработки команды на основании входящего условия.
        /// </summary>
        /// <param name="predicate">Условие выборки сценария.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденный сценарий.</returns>
        Task<IScenario> GetScenario(Func<IScenario, bool> predicate, CancellationToken ct);


        /// <summary>
        /// Добавление сценария обработки команды в репозиторий.
        /// </summary>
        /// <param name="scenario">Сохраняемый сценарий.</param>
        /// <param name="ct">Токен отмены.</param>
        Task AddScenario(IScenario scenario, CancellationToken ct);

        /// <summary>
        /// Метод получения данных о сценарии на основании входящего условия.
        /// </summary>
        /// <param name="predicate">Условие выборки данных пользователя о сценарии.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Возвращает данные о сценарим на основании входящего условия.</returns>
        Task<UserScenarioData?> GetUserScenarioData(Func<UserScenarioData, bool> predicate, CancellationToken ct);

        /// <summary>
        /// Метод сохранения данных о сценарии пользовтеля.
        /// </summary>
        /// <param name="userScenarioData">Данные о сценарии пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        Task SaveUserScenarioData(UserScenarioData userScenarioData, CancellationToken ct);

        /// <summary>
        /// Метод удаление данных о сценарии пользовтеля.
        /// </summary>
        /// <param name="predicate">Условие выборки для удаления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task DeleteUserScenarioData(Func<UserScenarioData, bool> predicate, CancellationToken ct);

        /// <summary>
        /// Метод получения пользовательских данных о сценарии.
        /// </summary>
        /// <param name="predicate">Условие выборки для пользовательских данных о сценарии.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список пользовательских данных о сценарии.</returns>
        Task<IReadOnlyList<UserScenarioData>?> GetUserScenarioDatas(Func<UserScenarioData, bool> predicate, CancellationToken ct);
    }
}
