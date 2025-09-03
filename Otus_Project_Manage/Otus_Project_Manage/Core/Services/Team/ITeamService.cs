using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface ITeamService
    {
        /// <summary>
        /// Метод добавления команды (группы) пользователей.
        /// </summary>
        /// <param name="team">Команда (группа) для доаввления</param>
        /// <param name="ct">Токен отмены.</param>
        Task AddTeam(UsersTeam team, CancellationToken ct);

        /// <summary>
        /// Метод удаления команды (группы) пользователей.
        /// </summary>
        /// <param name="teamId">Id команды (группы) для удаления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task DeleteTeam(Guid teamId, CancellationToken ct);

        /// <summary>
        /// Метод переименования команды (группы) пользователей.
        /// </summary>
        /// <param name="teamId">Id команды (группа) для переименования.</param>
        /// <param name="newName">Новое имя команды (группа).</param>
        /// <param name="ct">Токен отмены.</param>
        Task RenameTeam(Guid teamId, string newName, CancellationToken ct);

        /// <summary>
        /// Метод получения списка команд (групп) пользователей.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список команд (групп) пользователей.</returns>
        Task<IReadOnlyList<UsersTeam>?> GetAllTeams(CancellationToken ct);

        /// <summary>
        /// Метод получения команды (группы) пользователей.
        /// </summary>
        /// <param name="teamId">Id команды (группы).</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Команда (группа) пользователей.</returns>
        Task<UsersTeam?> GetTeamById(Guid teamId, CancellationToken ct);

        /// <summary>
        /// Метод получения команды (группы) пользователей по свободной роли в команде.
        /// </summary>
        /// <param name="userRole">Свободная роль.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Команда (группа) пользователей.</returns>
        Task<IReadOnlyList<UsersTeam>?> GetTeamByEmptyUserRole(UserRole userRole, CancellationToken ct);
    }
}
