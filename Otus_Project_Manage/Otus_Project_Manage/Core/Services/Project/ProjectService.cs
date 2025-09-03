using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class ProjectService : IProjectService
    {
        public ProjectService(IProjectRepository projectRepo)
        {
            this.projectRepo = projectRepo;
        }
        private readonly IProjectRepository projectRepo;

        public async Task AddProject(Project project, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await projectRepo.AddProject(project, ct);
        }

        public async Task UpdateProject(Project project, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await projectRepo.UpdateProject(project, ct);
        }

        public async Task DeleteProject(Guid projectId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await projectRepo.DeleteProject(projectId, ct);
        }

        public async Task<IReadOnlyList<Project>?> GetActiveProjects(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await projectRepo.GetProjects(x => x.status == ProjectStatus.Active, ct);
        }

        public async Task<Project?> GetProjectById(Guid projectId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await projectRepo.GetProject(x => x.projectId == projectId, ct);
        }

        public async Task<Project?> GetProjectByTeamLead(Guid userId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await projectRepo.GetProject(x => x.projectManager.userId == userId, ct);
        }
    }
}
