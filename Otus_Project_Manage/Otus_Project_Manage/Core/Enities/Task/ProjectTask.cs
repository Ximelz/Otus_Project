using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class ProjectTask
    {
        public ProjectTask() { }
        public ProjectTask(string name, DateTime deadline, string description)
        {
            taskName = name;
            this.deadline = deadline;
            this.description = description;
        }

        /// <summary>
        /// Id задачи.
        /// </summary>
        public Guid taskId = Guid.NewGuid();

        /// <summary>
        /// Наименованеи задачи.
        /// </summary>
        public string taskName;

        /// <summary>
        /// Описание задачи.
        /// </summary>
        public string description;

        /// <summary>
        /// Дата создания задачи.
        /// </summary>
        public DateTime createdAt = DateTime.UtcNow;

        /// <summary>
        /// Срок выполнения задачи.
        /// </summary>
        public DateTime deadline;

        /// <summary>
        /// Статус выполнения задачи.
        /// </summary>
        public TaskStatus status = TaskStatus.Active;


        /// <summary>
        /// Команда (группа) пользователя.
        /// </summary>
        public UsersTeam team;

        /// <summary>
        /// Первый этап задачи.
        /// </summary>
        public TaskStage firstStage;

        /// <summary>
        /// Активный этап задачи.
        /// </summary>
        public TaskStage activeStage;

        /// <summary>
        /// Проект к которому принадлежит задача.
        /// </summary>
        public Project project;
    }
}
