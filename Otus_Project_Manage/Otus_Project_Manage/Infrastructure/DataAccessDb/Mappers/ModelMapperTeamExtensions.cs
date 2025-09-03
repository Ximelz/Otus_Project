using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperTeamExtensions
    {
        public static UsersTeam MapFromModel(this UserTeamModel userModel)
        {
            return new UsersTeam()
            {
                teamId = userModel.teamId,
                name = userModel.name
            };
        }

        public static UserTeamModel MapToModel(this UsersTeam team)
        {
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

            foreach (var teamModel in teamsModel.Result)
                teams.Add(teamModel.MapFromModel());

            return Task.FromResult(teams);
        }

        public static Task<List<UserTeamModel>> MapToModelListAsync(this Task<List<UsersTeam>> teams)
        {
            List<UserTeamModel> teamsModel = new List<UserTeamModel>();

            foreach (var team in teams.Result)
                teamsModel.Add(team.MapToModel());

            return Task.FromResult(teamsModel);
        }
    }
}
