using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface IUserService
    {
        /// <summary>
        /// Метод получения пользователя по Id. 
        /// </summary>
        /// <param name="id">Id пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденный пользователь.</returns>
        Task<ProjectUser?> GetUserByUserId(Guid id, CancellationToken ct);

        /// <summary>
        /// Метод добавления пользователя.
        /// </summary>
        /// <param name="user">Пользователь для добавления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task AddUser(ProjectUser user, CancellationToken ct);

        /// <summary>
        /// Метод удаления пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        Task DeleteUser(Guid userId, CancellationToken ct);

        /// <summary>
        /// Метод изменения роли пользователя.
        /// </summary>
        /// <param name="telegramUserId">Id пользователя в telegramm.</param>
        /// <param name="role">Новая роль пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        Task ChangeUserRole(long telegramUserId, UserRole role, CancellationToken ct);


        /// <summary>
        /// Метод изменения команды (группы) пользователя.
        /// </summary>
        /// <param name="telegramUserId">Id пользователя в telegramm.</param>
        /// <param name="team">Новая команда (группа) пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        Task ChangeUserTeam(long telegramUserId, UsersTeam team, CancellationToken ct);

        /// <summary>
        /// Метод получения пользователя по telegramm id.
        /// </summary>
        /// <param name="telegramUserId">Id пользователя в telegramm.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Найденный пользователь.</returns>
        Task<ProjectUser?> GetUserByTelegramId(long telegramUserId, CancellationToken ct);

        /// <summary>
        /// Метод получения пользователей определенной роли.
        /// </summary>
        /// <param name="role">Роль пользователей.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список пользователей с определенной ролью.</returns>
        Task<IReadOnlyList<ProjectUser>?> GetUsersByRole(UserRole role, CancellationToken ct);

        /// <summary>
        /// Метод получения пользователей определенной команды (группы).
        /// </summary>
        /// <param name="teamId">Id команды (группы) пользователей.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список пользователей с определенной ролью.</returns>
        Task<IReadOnlyList<ProjectUser>?> GetUsersByTeam(Guid teamId, CancellationToken ct);

        /// <summary>
        /// Метод получения всех пользователей.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список всех пользователей.</returns>
        Task<IReadOnlyList<ProjectUser>?> GetAllUsers(CancellationToken ct);

        /// <summary>
        /// Метод получения всех разегистрированных пользователей.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список зарегистрированных пользователей.</returns>
        Task<IReadOnlyList<ProjectUser>?> GetRegisteredUsers(CancellationToken ct);

        /// <summary>
        /// Метод обновления данных пользователя.
        /// </summary>
        /// <param name="updateUser">Пользователь для обновления</param>
        /// <param name="ct">Токен отмены.</param>
        Task UpdateUser(ProjectUser updateUser, CancellationToken ct);
    }
}
