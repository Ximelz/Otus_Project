using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;

namespace Otus_Project_Manage
{
    public class SqlProjectRepository : IProjectRepository
    {
        public SqlProjectRepository(IDataContextFactory<ProjectTasksDataContext> factory)
        {
            this.factory = factory;
        }
        private readonly IDataContextFactory<ProjectTasksDataContext> factory;
        public async Task AddProject(Project project, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.InsertAsync(project.MapToModel(), token: ct);
            }
        }

        public async Task UpdateProject(Project project, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.UpdateAsync(project.MapToModel(), token: ct);
            }
        }

        public async Task DeleteProject(Guid projectId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                await dbConn.Projects.Where(x => x.projectId == projectId).DeleteAsync(ct);
            }
        }

        public async Task<Project?> GetProject(Func<Project, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                var project = (await dbConn.Projects
                                           .LoadWith(u => u.projectManager)
                                           .ToListAsync(ct)
                                           .MapFromModelListAsync(ct))
                                           .Where(predicate)
                                           .FirstOrDefault();

                if (project == null)
                    return null;

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
                                            .Where(x => x.project != null)
                                            .Where(x => x.project.projectId == project.projectId)
                                            .FirstOrDefault();
                if (task == null)
                    return project;

                task.firstStage.task = task;
                task.firstStage.nextStage.task = task;
                task.firstStage.nextStage.nextStage.task = task;

                task.activeStage = task.firstStage.nextStage.nextStage;

                if (task.firstStage.nextStage.status == TaskStatus.Active)
                    task.activeStage = task.firstStage.nextStage;

                if (task.firstStage.status == TaskStatus.Active)
                    task.activeStage = task.firstStage;

                project.tasks.Add(task);

                return project;
            }
        }

        public async Task<IReadOnlyList<Project>?> GetProjects(Func<Project, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                var projects = (await dbConn.Projects
                                            .LoadWith(u => u.projectManager)
                                            .ToListAsync(ct)
                                            .MapFromModelListAsync(ct))
                                            .Where(predicate)
                                            .ToList();

                if (projects == null)
                    return null;

                foreach (var project in projects)
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
                                            .Where(x => x.project != null)
                                            .Where(x => x.project.projectId == project.projectId)
                                            .FirstOrDefault();
                    if (task == null)
                        continue;

                    task.firstStage.task = task;
                    task.firstStage.nextStage.task = task;
                    task.firstStage.nextStage.nextStage.task = task;

                    task.activeStage = task.firstStage.nextStage.nextStage;

                    if (task.firstStage.nextStage.status == TaskStatus.Active)
                        task.activeStage = task.firstStage.nextStage;

                    if (task.firstStage.status == TaskStatus.Active)
                        task.activeStage = task.firstStage;

                    project.tasks.Add(task);
                }

                return projects;
            }
        }
    }
}
