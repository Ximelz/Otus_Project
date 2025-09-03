using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface IUserRepository
    {
        /// <summary>
        /// Метод добавления пользователя.
        /// </summary>
        /// <param name="user">Пользователь для добавления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task AddUser(ProjectUser user, CancellationToken ct);

        /// <summary>
        /// Метод удаления пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя для удаления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task DeleteUser(Guid userId, CancellationToken ct);

        /// <summary>
        /// Метод обновления свойств пользователя.
        /// </summary>
        /// <param name="user">Пользователь для обновления.</param>
        /// <param name="ct">Токен отмены.</param>
        Task UpdateUser(ProjectUser user, CancellationToken ct);

        /// <summary>
        /// Метод получения списка пользователей.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="ct">Токен отмены.</param>
        Task<IReadOnlyList<ProjectUser>?> GetUsers(Func<ProjectUser, bool> predicate, CancellationToken ct);


        /// <summary>
        /// Метод получения пользователя.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="ct">Токен отмены.</param>
        Task<ProjectUser?> GetUser(Func<ProjectUser, bool> predicate, CancellationToken ct);
    }
}
