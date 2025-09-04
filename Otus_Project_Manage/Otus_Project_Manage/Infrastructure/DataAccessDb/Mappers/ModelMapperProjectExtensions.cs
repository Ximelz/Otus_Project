using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public static class ModelMapperProjectExtensions
    {
        public static Project MapFromModel(this ProjectModel projectModel)
        {
            if (projectModel == null)
                return null;

            return new Project()
            {
                projectId = projectModel.projectId,
                name = projectModel.name,
                description = projectModel.description,
                status = projectModel.status,
                deadline = projectModel.deadline,
                createdAt = projectModel.createdAt,
                projectManager = projectModel.projectManager.MapFromModel()
            };
        }

        public static ProjectModel MapToModel(this Project project)
        {
            if (project == null)
                return null;

            return new ProjectModel()
            {
                projectId = project.projectId,
                name = project.name,
                description = project.description,
                status = project.status,
                deadline = project.deadline,
                createdAt = project.createdAt,
                userId = project.projectManager.userId
            };
        }

        public static Task<List<Project>> MapFromModelListAsync(this Task<List<ProjectModel>> projectsModel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<Project> projects = new List<Project>();
            
            if (projectsModel != null)
                foreach (var projectModel in projectsModel.Result)
                    projects.Add(projectModel.MapFromModel());

            return Task.FromResult(projects);
        }

        public static Task<List<ProjectModel>> MapToModelListAsync(this Task<List<Project>> projects, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            List<ProjectModel> projectsModel = new List<ProjectModel>();

            if (projects != null)
                foreach (var project in projects.Result)
                    projectsModel.Add(project.MapToModel());

            return Task.FromResult(projectsModel);
        }
    }
}
