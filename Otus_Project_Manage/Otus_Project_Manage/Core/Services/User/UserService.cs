using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class UserService : IUserService
    {
        public UserService(IUserRepository userRep)
        {
            this.userRep = userRep;
        }
        private readonly IUserRepository userRep;

        public async Task AddUser(ProjectUser user, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (await GetUserByTelegramId(user.telegramUserId, ct) != null)
                throw new Exception("Пользователь уже зарегистирован!");

            await userRep.AddUser(user, ct);
        }

        public async Task ChangeUserRole(long telegramUserId, UserRole role, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            ProjectUser user = await GetUserByTelegramId(telegramUserId, ct);

            if (user == null)
                throw new ArgumentNullException($"Пользователь с telegramUserId \"{telegramUserId}\" не найден!");

            user.role = role;

            await userRep.UpdateUser(user, ct);
        }

        public async Task ChangeUserTeam(long telegramUserId, UsersTeam team, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            ProjectUser user = await GetUserByTelegramId(telegramUserId, ct);

            if (user == null)
                throw new ArgumentNullException($"Пользователь с telegramUserId \"{telegramUserId}\" не найден!");

            user.team = team;

            await userRep.UpdateUser(user, ct);
        }

        public async Task DeleteUser(Guid userId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await userRep.DeleteUser(userId, ct);
        }

        public async Task<ProjectUser?> GetUserByTelegramId(long telegramUserId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await userRep.GetUser(x => x.telegramUserId == telegramUserId, ct);
        }

        public async Task<ProjectUser?> GetUserByUserId(Guid id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            return await userRep.GetUser(x => x.userId == id, ct);
        }

        public async Task<IReadOnlyList<ProjectUser>?> GetUsersByRole(UserRole role, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await userRep.GetUsers(x => x.role == role, ct);
        }

        public async Task<IReadOnlyList<ProjectUser>?> GetUsersByTeam(Guid teamId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await userRep.GetUsers(x => x.team.teamId == teamId, ct);
        }

        public async Task<IReadOnlyList<ProjectUser>?> GetRegisteredUsers(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await userRep.GetUsers(x => x.role != UserRole.None, ct);
        }

        public async Task<IReadOnlyList<ProjectUser>?> GetAllUsers(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await userRep.GetUsers(x => x.userId != null, ct);
        }

        public async Task UpdateUser(ProjectUser updateUser, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await userRep.UpdateUser(updateUser, ct);
        }
    }
}
