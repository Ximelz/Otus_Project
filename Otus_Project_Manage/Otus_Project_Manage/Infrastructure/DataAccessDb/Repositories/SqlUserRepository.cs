using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Otus_Project_Manage
{
    public class SqlUserRepository : IUserRepository
    {
        public SqlUserRepository(IDataContextFactory<ProjectTasksDataContext> factory)
        {
            this.factory = factory;
        }
        private readonly IDataContextFactory<ProjectTasksDataContext> factory;
        public async Task AddUser(ProjectUser user, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.InsertAsync(user.MapToModel(), token: ct);
            }
        }

        public async Task DeleteUser(Guid userId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.Users.Where(x => x.userId == userId).DeleteAsync(ct);
            }
        }

        public async Task<IReadOnlyList<ProjectUser>> GetUsers(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                var users = await dbConn.Users
                                        .LoadWith(x => x.team)                                        
                                        .ToListAsync(ct)
                                        .MapFromModelListAsync(ct);
                
                if (users == null)
                    return null;

                foreach (var user in users)
                {
                    var project = (await dbConn.Projects
                                               .LoadWith(x => x.projectManager)
                                               .ToListAsync(ct)
                                               .MapFromModelListAsync(ct))
                                               .Where(x => x.projectManager.userId == user.userId)
                                               .FirstOrDefault();
                    user.project = project;
                }
                return users;
            }
        }

        public async Task UpdateUser(ProjectUser user, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.UpdateAsync(user.MapToModel(), token: ct);
            }
        }

        public async Task<ProjectUser?> GetUser(Func<ProjectUser, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                var user = (await dbConn.Users
                                        .LoadWith(x => x.team)
                                        .ToListAsync(ct)
                                        .MapFromModelListAsync(ct))                             
                                        .Where(predicate)
                                        .FirstOrDefault();

                if (user == null)
                    return null;

                var project = (await dbConn.Projects
                                               .LoadWith(x => x.projectManager)
                                               .ToListAsync(ct)
                                               .MapFromModelListAsync(ct))
                                               .Where(x => x.projectManager.userId == user.userId)
                                               .FirstOrDefault();
                user.project = project;
                return user;
            }
        }
    }
}
