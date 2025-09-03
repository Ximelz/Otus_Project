using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface ITaskService
    {
        /// <summary>
        /// Метод добавления задачи.
        /// </summary>
        /// <param name="task">Задача для добавления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task AddTask(ProjectTask task, CancellationToken ct);

        /// <summary>
        /// Метод обновления задачи.
        /// </summary>
        /// <param name="task">Задача для обновления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task UpdateTask(ProjectTask task, CancellationToken ct);

        /// <summary>
        /// Удаление задачи.
        /// </summary>
        /// <param name="taskId">Id задачи.</param>
        /// <param name="ct">Токен отмены.</param>
        Task DeleteTask(Guid taskId, CancellationToken ct);

        /// <summary>
        /// Список активных задач команды.
        /// </summary>
        /// <param name="team">Команда.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список активных задач команды.</returns>
        Task<IReadOnlyList<ProjectTask>?> GetActiveTasksByTeam(UsersTeam team, CancellationToken ct);

        /// <summary>
        /// Метод получения выполненных задач команды.
        /// </summary>
        /// <param name="team">Команда.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список выполненных задач команды.</returns>
        Task<IReadOnlyList<ProjectTask>?> GetCompleteTasksByTeam(UsersTeam team, CancellationToken ct);

        /// <summary>
        /// Метод получения списка задач определенного пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Задачи пользователя.</returns>
        Task<IReadOnlyList<ProjectTask>?> GetTasksByUser(ProjectUser user, CancellationToken ct);

        /// <summary>
        /// Метод получения задачи по id.
        /// </summary>
        /// <param name="taskId">Id задачи.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Задачи пользователя.</returns>
        Task<ProjectTask?> GetTasksById(Guid taskId, CancellationToken ct);

        /// <summary>
        /// Получение всех активных задач.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список активных задач.</returns>
        Task<IReadOnlyList<ProjectTask>?> GetAllActiveTasks(CancellationToken ct);
    }
}
