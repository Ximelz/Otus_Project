using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface IProjectRepository
    {
        /// <summary>
        /// Метод добавления проекта в репозмторий.
        /// </summary>
        /// <param name="project">Проект для добавления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task AddProject(Project project, CancellationToken ct);

        /// <summary>
        /// Метод обновления проекта в репозмтории.
        /// </summary>
        /// <param name="project">Проект для обновления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task UpdateProject(Project project, CancellationToken ct);

        /// <summary>
        /// Метод удаления проекта из репозмтория.
        /// </summary>
        /// <param name="projectId">Проект для удаления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task DeleteProject(Guid projectId, CancellationToken ct);

        /// <summary>
        /// Метод получения проекта из репозмтория.
        /// </summary>
        /// <param name="predicate">Условие выборки.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденный проект.</returns>
        Task<Project?> GetProject(Func<Project, bool> predicate, CancellationToken ct);

        /// <summary>
        /// Метод получения проектов из репозмтория.
        /// </summary>
        /// <param name="predicate">Условие выборки.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденные проекты.</returns>
        Task<IReadOnlyList<Project>?> GetProjects(Func<Project, bool> predicate, CancellationToken ct);
    }
}
