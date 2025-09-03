using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperStageExtensions
    {
        public static TaskStage MapFromModel(this TaskStageModel stageModel)
        {
            return new TaskStage()
            {
                stageId = stageModel.stageId,
                name = stageModel.name,
                comment = stageModel.comment,
                description = stageModel.description,
                status = stageModel.status,
                task = stageModel.task,
                user = stageModel.user,
                nextStage = stageModel.nextStage
            };
        }

        public static TaskStageModel MapToModel(this TaskStage stage)
        {
            return new TaskStageModel()
            {
                stageId = stage.stageId,
                name = stage.name,
                comment = stage.comment,
                description = stage.description,
                status = stage.status,
                task = stage.task,
                user = stage.user,
                nextStage = stage.nextStage
            };
        }

        public static Task<List<TaskStage>> MapFromModelListAsync(this Task<List<TaskStageModel>> stagesModel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<TaskStage> stages = new List<TaskStage>();

            foreach (var stageModel in stagesModel.Result)
                stages.Add(stageModel.MapFromModel());

            return Task.FromResult(stages);
        }

        public static Task<List<TaskStageModel>> MapToModelListAsync(this Task<List<TaskStage>> stages)
        {
            List<TaskStageModel> stagesModel = new List<TaskStageModel>();

            foreach (var stage in stages.Result)
                stagesModel.Add(stage.MapToModel());

            return Task.FromResult(stagesModel);
        }
    }
}
