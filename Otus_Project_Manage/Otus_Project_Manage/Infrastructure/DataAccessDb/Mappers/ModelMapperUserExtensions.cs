using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperUserExtensions
    {
        public static Task<ProjectUser> MapFromModel(this ProjectUserModel userModel)
        {
            return Task.FromResult(new ProjectUser()
            {
                telegramUserId = userModel.telegramUserId,
                userId = userModel.userId,
                userName = userModel.userName,
                team = userModel.team,
                role = userModel.role,
                isAdmin = userModel.isAdmin,
                project = userModel.project
            });
        }

        public static Task<ProjectUserModel> MapToModel(this ProjectUser user)
        {
            return Task.FromResult(new ProjectUserModel()
            {
                telegramUserId = user.telegramUserId,
                userId = user.userId,
                userName = user.userName,
                teamId = user.team != null ? user.team.teamId : null,
                role = user.role,
                isAdmin = user.isAdmin,
                projectId = user.project != null ? user.project.projectId : Guid.Empty
            });
        }

        public async static Task<List<ProjectUser>> MapFromModelListAsync(this Task<List<ProjectUserModel>> usersModel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<ProjectUser> users = new List<ProjectUser>();

            foreach (var userModel in usersModel.Result)
                users.Add(await userModel.MapFromModel());

            return users;
        }

        public async static Task<List<ProjectUserModel>> MapToModelListAsync(this Task<List<ProjectUser>> users, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<ProjectUserModel> usersModel = new List<ProjectUserModel>();

            foreach (var user in users.Result)
                usersModel.Add(await user.MapToModel());

            return usersModel;
        }
    }
}
