using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class TaskService : ITaskService
    {
        public TaskService(ITaskRepository taskRepository)
        {
            taskRepo = taskRepository;
        }
        private readonly ITaskRepository taskRepo;

        public async Task AddTask(ProjectTask task, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await taskRepo.AddTask(task, ct);
        }

        public async Task UpdateTask(ProjectTask task, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await taskRepo.UpdateTask(task, ct);
        }

        public async Task DeleteTask(ProjectTask task, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await taskRepo.DeleteTask(task, ct);
        }

        public async Task<IReadOnlyList<ProjectTask>> GetActiveTasksByTeam(UsersTeam team, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return (await taskRepo.GetTasks(x => x.team.teamId == team.teamId, ct))
                                  .Where(x => x.status == TaskStatus.Active)
                                  .ToList();
        }

        public async Task<IReadOnlyList<ProjectTask>> GetCompleteTasksByTeam(UsersTeam team, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return (await taskRepo.GetTasks(x => x.team.teamId == team.teamId, ct))
                                  .Where(x => x.status == TaskStatus.Completed)
                                  .ToList();
        }

        public async Task<IReadOnlyList<ProjectTask>> GetTasksByUser(ProjectUser user, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return (await taskRepo.GetTasks(x => x.team.teamId == user.team.teamId, ct))
                                  .Where(x => x.activeStage.user.userId == user.userId)
                                  .ToList();
        }

        public async Task<ProjectTask> GetTasksById(Guid taskId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await taskRepo.GetTask(x => x.taskId == taskId, ct);
        }

        public async Task<IReadOnlyList<ProjectTask>?> GetAllActiveTasks(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await taskRepo.GetTasks(x => x.taskId != null, ct);
        }
    }
}
