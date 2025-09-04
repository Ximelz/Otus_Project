using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperTeamExtensions
    {
        public static UsersTeam MapFromModel(this UserTeamModel userModel)
        {
            if (userModel == null)
                return null;

            return new UsersTeam()
            {
                teamId = userModel.teamId,
                name = userModel.name
            };
        }

        public static UserTeamModel MapToModel(this UsersTeam team)
        {
            if (team == null)
                return null;

            return new UserTeamModel()
            {
                teamId = team.teamId,
                name = team.name
            };
        }

        public static Task<List<UsersTeam>> MapFromModelListAsync(this Task<List<UserTeamModel>> teamsModel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<UsersTeam> teams = new List<UsersTeam>();

            if (teamsModel != null)
                foreach (var teamModel in teamsModel.Result)
                    teams.Add(teamModel.MapFromModel());

            return Task.FromResult(teams);
        }

        public static Task<List<UserTeamModel>> MapToModelListAsync(this Task<List<UsersTeam>> teams, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<UserTeamModel> teamsModel = new List<UserTeamModel>();

            if (teams != null)
                foreach (var team in teams.Result)
                    teamsModel.Add(team.MapToModel());

            return Task.FromResult(teamsModel);
        }
    }
}
