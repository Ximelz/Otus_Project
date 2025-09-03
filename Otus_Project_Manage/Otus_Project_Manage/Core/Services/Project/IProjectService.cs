using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface IProjectService
    {
        /// <summary>
        /// Метод добавления проекта.
        /// </summary>
        /// <param name="project">Добавляемый проект.</param>
        /// <param name="ct">Токен отмены.</param>
        Task AddProject(Project project, CancellationToken ct);


        /// <summary>
        /// Метод обновления проекта.
        /// </summary>
        /// <param name="project">Обновляемый проект.</param>
        /// <param name="ct">Токен отмены.</param>
        Task UpdateProject(Project project, CancellationToken ct);


        /// <summary>
        /// Метод удаления проекта.
        /// </summary>
        /// <param name="projectId">Id удаляемого проекта.</param>
        /// <param name="ct">Токен отмены.</param>
        Task DeleteProject(Guid projectId, CancellationToken ct);

        /// <summary>
        /// Метод получения активных проектов.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список активных проектов.</returns>
        Task<IReadOnlyList<Project>?> GetActiveProjects(CancellationToken ct);

        /// <summary>
        /// Метод получения проекта по Id.
        /// </summary>
        /// <param name="projectId">Id проект.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденный проект.</returns>
        Task<Project?> GetProjectById(Guid projectId, CancellationToken ct);

        /// <summary>
        /// Метод получения проекта по id руководителя проекта.
        /// </summary>
        /// <param name="project">Id пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденный проект</returns>
        Task<Project?> GetProjectByTeamLead(Guid userId, CancellationToken ct);
    }
}
