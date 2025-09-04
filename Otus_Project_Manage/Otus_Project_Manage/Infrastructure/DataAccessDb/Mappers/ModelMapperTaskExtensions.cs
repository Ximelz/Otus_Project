using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperTaskExtensions
    {
        public static ProjectTask MapFromModel(this ProjectTaskModel taskModel)
        {
            if (taskModel == null)
                return null;

            return new ProjectTask()
            {
                taskId = taskModel.taskId,
                taskName = taskModel.taskName,
                team = taskModel.team.MapFromModel(),
                description = taskModel.description,
                status = taskModel.status,
                firstStage = taskModel.firstStage.MapFromModel(),
                deadline = taskModel.deadline,
                createdAt = taskModel.createdAt,
                project = taskModel.project.MapFromModel()
            };
        }

        public static ProjectTaskModel MapToModel(this ProjectTask task)
        {
            if (task == null)
                return null;

            return new ProjectTaskModel()
            {
                taskId = task.taskId,
                taskName = task.taskName,
                teamId = task.team.teamId,
                description = task.description,
                status = task.status,
                startStageId = task.firstStage.stageId,
                deadline = task.deadline,
                createdAt = task.createdAt,
                projectId = task.project != null ? task.project.projectId : null
            };
        }

        public static Task<List<ProjectTask>> MapFromModelListAsync(this Task<List<ProjectTaskModel>> model, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<ProjectTask> tasks = new List<ProjectTask>();
            List<ProjectTaskModel> tasksModel = model.Result;

            foreach (var taskModel in tasksModel)
                tasks.Add(taskModel.MapFromModel());

            return Task.FromResult(tasks);
        }

        public static Task<List<ProjectTaskModel>> MapToModelListAsync(this Task<List<ProjectTask>> tasks, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<ProjectTaskModel> tasksModel = new List<ProjectTaskModel>();

            foreach (var task in tasks.Result)
                tasksModel.Add(task.MapToModel());

            return Task.FromResult(tasksModel);
        }
    }
}
