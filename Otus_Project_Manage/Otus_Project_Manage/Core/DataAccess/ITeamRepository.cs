using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface ITeamRepository
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
        /// Метод обноваления параментов команды (группы) пользователей.
        /// </summary>
        /// <param name="team">Команда (группа) для обновления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task UpdateTeam(UsersTeam team, CancellationToken ct);

        /// <summary>
        /// Метод получения списка команд (групп) пользователей.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список команд (групп) пользователей.</returns>
        Task<IReadOnlyList<UsersTeam>?> GetTeams(Func<UsersTeam, bool> predicate, CancellationToken ct);

        /// <summary>
        /// Метод получения команды (группы) пользователей.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список команд (групп) пользователей.</returns>
        Task<UsersTeam?> GetTeam(Func<UsersTeam, bool> predicate, CancellationToken ct);
    }
}
