using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class TeamService : ITeamService
    {
        public TeamService(ITeamRepository teamRep)
        {
            this.teamRep = teamRep;
        }
        private readonly ITeamRepository teamRep;
        public async Task AddTeam(UsersTeam team, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (await GetTeamById(team.teamId, ct) != null)
                throw new ArgumentException("Команда уже существует!");

            await teamRep.AddTeam(team, ct);
        }

        public async Task DeleteTeam(Guid teamId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await teamRep.DeleteTeam(teamId, ct);
        }

        public async Task<IReadOnlyList<UsersTeam>?> GetAllTeams(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await teamRep.GetTeams(x => x.teamId != null, ct);
        }

        public async Task<UsersTeam?> GetTeamById(Guid teamId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await teamRep.GetTeam(x => x.teamId == teamId, ct);
        }

        public async Task RenameTeam(Guid teamId, string newName, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            UsersTeam team = await GetTeamById(teamId, ct);

            if (team == null)
                throw new ArgumentNullException($"Команда с Id\"{teamId}\" не найдена!");

            team.name = newName;
            await teamRep.UpdateTeam(team, ct);
        }

        public async Task<IReadOnlyList<UsersTeam>> GetTeamByEmptyUserRole(UserRole userRole, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await teamRep.GetTeams(x => !x.usersInTeam.ContainsKey(userRole), ct);
        }
    }
}
