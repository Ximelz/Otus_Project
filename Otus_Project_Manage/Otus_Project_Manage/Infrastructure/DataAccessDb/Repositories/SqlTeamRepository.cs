using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class SqlTeamRepository : ITeamRepository
    {
        public SqlTeamRepository(IUserRepository userRep, IDataContextFactory<ProjectTasksDataContext> factory)
        {
            this.userRep = userRep;
            this.factory = factory;
        }

        private readonly IUserRepository userRep;
        private readonly IDataContextFactory<ProjectTasksDataContext> factory;

        public async Task AddTeam(UsersTeam team, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.InsertAsync(team.MapToModel(), token: ct);
            }
        }

        public async Task DeleteTeam(Guid teamId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.Teams.Where(x => x.teamId == teamId).DeleteAsync(ct);
            }
        }

        public async Task<IReadOnlyList<UsersTeam>> GetTeams(Func<UsersTeam, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                var teams = await dbConn.Teams
                                         .ToListAsync(ct)
                                         .MapFromModelListAsync(ct);

                foreach (var team in teams)
                {
                    var users = (await dbConn.Users
                                             .LoadWith(x => x.team)
                                             .ToListAsync(ct)
                                             .MapFromModelListAsync(ct))
                                             .Where(x => x.team != null)
                                             .Where(x => x.team.teamId == team.teamId)
                                             .ToList();

                    foreach (var user in users)
                        team.usersInTeam.Add(user.role, user);
                }

                return teams.Where(predicate).ToList(); ;
            }
        }

        public async Task UpdateTeam(UsersTeam team, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.UpdateAsync(team.MapToModel(), token: ct);
            }
        }

        public async Task<UsersTeam?> GetTeam(Func<UsersTeam, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                var teams = await dbConn.Teams
                                         .ToListAsync(ct)
                                         .MapFromModelListAsync(ct);

                foreach (var team in teams)
                {
                    var users = (await dbConn.Users
                                             .LoadWith(x => x.team)
                                             .ToListAsync(ct)
                                             .MapFromModelListAsync(ct))
                                             .Where(x => x.team != null)
                                             .Where(x => x.team.teamId == team.teamId)
                                             .ToList();

                    foreach (var user in users)
                        team.usersInTeam.Add(user.role, user);
                }

                return teams.Where(predicate).FirstOrDefault();
            }
        }
    }
}
