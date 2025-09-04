using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface ITaskRepository
    {
        /// <summary>
        /// Сохранение задачи в репозиторий.
        /// </summary>
        /// <param name="task">Задача.</param>
        /// <param name="ct">Токен отмены.</param>
        Task AddTask(ProjectTask task, CancellationToken ct);

        /// <summary>
        /// Удаление задачи из репозитория.
        /// </summary>
        /// <param name="task">Задача для удаления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task DeleteTask(ProjectTask task, CancellationToken ct);

        /// <summary>
        /// Обновление задачи в репозитории.
        /// </summary>
        /// <param name="task">Задача.</param>
        /// <param name="ct">Токен отмены.</param>
        Task UpdateTask(ProjectTask task, CancellationToken ct);

        /// <summary>
        /// Получение задачи из репозитория.
        /// </summary>
        /// <param name="predicate">Условие выборки.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденная задача.</returns>
        Task<ProjectTask?> GetTask(Func<ProjectTask, bool> predicate, CancellationToken ct);

        /// <summary>
        /// Получение списка задач.
        /// </summary>
        /// <param name="predicate">Условие выборки.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список задач.</returns>
        Task<IReadOnlyList<ProjectTask>?> GetTasks(Func<ProjectTask, bool> predicate, CancellationToken ct);
    }
}
