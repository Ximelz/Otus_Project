using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperTaskExtensions
    {
        public static ProjectTask MapFromModel(this ProjectTaskModel taskModel)
        {
            return new ProjectTask()
            {
                taskId = taskModel.taskId,
                taskName = taskModel.taskName,
                team = taskModel.team,
                description = taskModel.description,
                status = taskModel.status,
                firstStage = taskModel.firstStage,
                deadline = taskModel.deadline,
                createdAt = taskModel.createdAt,
                project = taskModel.project
            };
        }

        public static ProjectTaskModel MapToModel(this ProjectTask task)
        {
            return new ProjectTaskModel()
            {
                taskId = task.taskId,
                taskName = task.taskName,
                team = task.team,
                description = task.description,
                status = task.status,
                firstStage = task.firstStage,
                deadline = task.deadline,
                createdAt = task.createdAt,
                projectId = task.project != null ? task.project.projectId : Guid.Empty
            };
        }

        public static Task<List<ProjectTask>> MapFromModelListAsync(this Task<List<ProjectTaskModel>> tasksModel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<ProjectTask> tasks = new List<ProjectTask>();

            foreach (var taskModel in tasksModel.Result)
                tasks.Add(taskModel.MapFromModel());

            return Task.FromResult(tasks);
        }

        public static Task<List<ProjectTaskModel>> MapToModelListAsync(this Task<List<ProjectTask>> tasks)
        {
            List<ProjectTaskModel> tasksModel = new List<ProjectTaskModel>();

            foreach (var task in tasks.Result)
                tasksModel.Add(task.MapToModel());

            return Task.FromResult(tasksModel);
        }
    }
}
