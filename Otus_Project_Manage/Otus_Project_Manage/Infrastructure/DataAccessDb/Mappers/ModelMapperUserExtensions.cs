using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperUserExtensions
    {
        public static ProjectUser MapFromModel(this ProjectUserModel userModel)
        {
            if (userModel == null)
                return null;

            return new ProjectUser()
            {
                telegramUserId = userModel.telegramUserId,
                userId = userModel.userId,
                userName = userModel.userName,
                team = userModel.team.MapFromModel(),
                role = userModel.role,
                isAdmin = userModel.isAdmin
            };
        }

        public static ProjectUserModel MapToModel(this ProjectUser user)
        {
            if (user == null)
                return null;

            return new ProjectUserModel()
            {
                telegramUserId = user.telegramUserId,
                userId = user.userId,
                userName = user.userName,
                teamId = user.team != null ? user.team.teamId : null,
                role = user.role,
                isAdmin = user.isAdmin
            };
        }

        public static Task<List<ProjectUser>> MapFromModelListAsync(this Task<List<ProjectUserModel>> model, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<ProjectUser> users = new List<ProjectUser>();
            List<ProjectUserModel> usersModel = model.Result;

            if (usersModel != null)
                foreach (var userModel in usersModel)
                    users.Add(userModel.MapFromModel());

            return Task.FromResult(users);
        }

        public static Task<List<ProjectUserModel>> MapToModelListAsync(this Task<List<ProjectUser>> users, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<ProjectUserModel> usersModel = new List<ProjectUserModel>();

            if (users != null)
                foreach (var user in users.Result)
                    usersModel.Add(user.MapToModel());

            return Task.FromResult(usersModel);
        }
    }
}
