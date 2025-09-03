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
            return new Project()
            {
                projectId = projectModel.projectId,
                name = projectModel.name,
                description = projectModel.description,
                status = projectModel.status,
                deadline = projectModel.deadline,
                createdAt = projectModel.createdAt,
                projectManager = projectModel.projectManager
            };
        }

        public static ProjectModel MapToModel(this Project project)
        {
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

            foreach (var taskModel in projectsModel.Result)
                projects.Add(taskModel.MapFromModel());

            return Task.FromResult(projects);
        }

        public static Task<List<ProjectModel>> MapToModelListAsync(this Task<List<Project>> projects)
        {
            List<ProjectModel> projectsModel = new List<ProjectModel>();

            foreach (var project in projects.Result)
                projectsModel.Add(project.MapToModel());

            return Task.FromResult(projectsModel);
        }
    }
}
