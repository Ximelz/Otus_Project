using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperStageExtensions
    {
        public static TaskStage MapFromModel(this TaskStageModel stageModel)
        {
            if (stageModel == null)
                return null;

            return new TaskStage()
            {
                stageId = stageModel.stageId,
                name = stageModel.name,
                comment = stageModel.comment,
                description = stageModel.description,
                status = stageModel.status,
                user = stageModel.user.MapFromModel(),
                nextStage = stageModel.nextStage.MapFromModel()
            };
        }

        public static TaskStageModel MapToModel(this TaskStage stage)
        {
            if (stage == null)
                return null;

            return new TaskStageModel()
            {
                stageId = stage.stageId,
                name = stage.name,
                comment = stage.comment,
                description = stage.description,
                status = stage.status,
                userId = stage.user.userId,
                nextStageId = stage.nextStage != null ? stage.nextStage.stageId : null
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

        public static Task<List<TaskStageModel>> MapToModelListAsync(this Task<List<TaskStage>> stages, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<TaskStageModel> stagesModel = new List<TaskStageModel>();

            foreach (var stage in stages.Result)
                stagesModel.Add(stage.MapToModel());

            return Task.FromResult(stagesModel);
        }
    }
}
