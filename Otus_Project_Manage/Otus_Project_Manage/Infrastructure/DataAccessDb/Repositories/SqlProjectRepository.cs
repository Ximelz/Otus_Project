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
                await dbConn.InsertAsync(project.MapToModel, token: ct);
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
                return (await dbConn.Projects
                                    .LoadWith(t => t.tasks)
                                    .LoadWith(u => u.projectManager)
                                    .ToListAsync(ct)
                                    .MapFromModelListAsync(ct))
                                    .Where(predicate)
                                    .FirstOrDefault();
            }
        }

        public async Task<IReadOnlyList<Project>?> GetProjects(Func<Project, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await using (var dbConn = factory.CreateDataContext())
            {
                return (await dbConn.Projects
                                    .LoadWith(t => t.tasks)
                                    .LoadWith(u => u.projectManager)
                                    .ToListAsync(ct)
                                    .MapFromModelListAsync(ct))
                                    .Where(predicate)
                                    .ToList();
            }
        }
    }
}
