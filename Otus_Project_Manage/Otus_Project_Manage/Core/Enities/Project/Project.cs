using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class Project
    {
        /// <summary>
        /// Id проекта.
        /// </summary>
        public Guid projectId = Guid.NewGuid();

        /// <summary>
        /// Имя проекта.
        /// </summary>
        public string name;

        /// <summary>
        /// Описание проекта.
        /// </summary>
        public string description;

        /// <summary>
        /// Статус проекта.
        /// </summary>
        public ProjectStatus status;

        /// <summary>
        /// Срок сдачи проекта.
        /// </summary>
        public DateTime deadline;

        /// <summary>
        /// Дата создания проекта.
        /// </summary>
        public DateTime createdAt = DateTime.Now;

        /// <summary>
        /// Руководитель проекта.
        /// </summary>
        public ProjectUser projectManager;

        /// <summary>
        /// Активные задачи в проекте.
        /// </summary>
        public List<ProjectTask> tasks = new List<ProjectTask>();
    }
}
