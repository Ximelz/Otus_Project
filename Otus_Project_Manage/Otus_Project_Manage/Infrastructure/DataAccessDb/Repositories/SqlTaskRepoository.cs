using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class SqlTaskRepoository : ITaskRepository
    {
        public SqlTaskRepoository(IDataContextFactory<ProjectTasksDataContext> factory)
        {
            this.factory = factory;
        }
        private readonly IDataContextFactory<ProjectTasksDataContext> factory;

        public async Task AddTask(ProjectTask task, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                TaskStage stage = task.firstStage;
                while (stage != null)
                {
                    await dbConn.InsertAsync(stage.MapToModel(), token: ct);
                    stage = stage.nextStage;
                }

                await dbConn.InsertAsync(task.MapToModel(), token: ct);

            }
        }

        public async Task DeleteTask(ProjectTask task, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.Stages.Where(x => x.stageId == task.firstStage.stageId).DeleteAsync(ct);
                await dbConn.Stages.Where(x => x.stageId == task.firstStage.nextStage.stageId).DeleteAsync(ct);
                await dbConn.Stages.Where(x => x.stageId == task.firstStage.nextStage.nextStage.stageId).DeleteAsync(ct);
                await dbConn.Tasks.Where(x => x.taskId == task.taskId).DeleteAsync(ct);
            }
        }

        public async Task UpdateTask(ProjectTask task, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                TaskStage stage = task.firstStage;

                while (stage != null)
                {
                    await dbConn.UpdateAsync(stage.MapToModel(), token: ct);
                    stage = stage.nextStage;
                }

                await dbConn.UpdateAsync(task.MapToModel(), token: ct);
            }
        }

        public async Task<ProjectTask?> GetTask(Func<ProjectTask, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                var task = (await dbConn.Tasks
                                        .LoadWith(x => x.project)
                                        .LoadWith(x => x.team)
                                        .LoadWith(x => x.firstStage)
                                        .ThenLoad(x => x.user)
                                        .LoadWith(x => x.firstStage)
                                        .ThenLoad(x => x.nextStage)
                                        .ThenLoad(x => x.user)
                                        .LoadWith(x => x.firstStage)
                                        .ThenLoad(x => x.nextStage)
                                        .ThenLoad(x => x.nextStage)
                                        .ThenLoad(x => x.user)
                                        .ToListAsync()
                                        .MapFromModelListAsync(ct))
                                        .Where(predicate)
                                        .FirstOrDefault();
                if (task == null)
                    return null;

                task.firstStage.task = task;                
                task.firstStage.nextStage.task = task;
                task.firstStage.nextStage.nextStage.task = task;
                
                task.activeStage = task.firstStage.nextStage.nextStage;

                if (task.firstStage.nextStage.status == TaskStatus.Active)
                    task.activeStage = task.firstStage.nextStage;

                if (task.firstStage.status == TaskStatus.Active)
                    task.activeStage = task.firstStage;

                return task;
            }
        }

        public async Task<IReadOnlyList<ProjectTask>?> GetTasks(Func<ProjectTask, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                var tasks = (await dbConn.Tasks
                                         .LoadWith(x => x.project)
                                         .LoadWith(x => x.team)
                                         .LoadWith(x => x.firstStage)
                                         .ThenLoad(x => x.user)
                                         .LoadWith(x => x.firstStage)
                                         .ThenLoad(x => x.nextStage)
                                         .ThenLoad(x => x.user)
                                         .LoadWith(x => x.firstStage)
                                         .ThenLoad(x => x.nextStage)
                                         .ThenLoad(x => x.nextStage)
                                         .ThenLoad(x => x.user)
                                         .ToListAsync()
                                         .MapFromModelListAsync(ct))
                                         .Where(predicate)
                                         .ToList();

                foreach (var task in tasks)
                {
                    task.firstStage.task = task;
                    task.firstStage.nextStage.task = task;
                    task.firstStage.nextStage.nextStage.task = task;

                    task.activeStage = task.firstStage.nextStage.nextStage;

                    if (task.firstStage.nextStage.status == TaskStatus.Active)
                        task.activeStage = task.firstStage.nextStage;

                    if (task.firstStage.status == TaskStatus.Active)
                        task.activeStage = task.firstStage;
                }

                return tasks;
            }
        }
    }
}
