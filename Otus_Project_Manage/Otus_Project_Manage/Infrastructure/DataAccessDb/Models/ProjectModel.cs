using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Otus_Project_Manage
{
    [Table("Projects")]
    public class ProjectModel
    {
        /// <summary>
        /// Id проекта.
        /// </summary>
        [Column("projectId"), PrimaryKey]
        public Guid projectId = Guid.NewGuid();

        /// <summary>
        /// Имя проекта.
        /// </summary>
        [Column("name"), NotNull]
        public string name;

        /// <summary>
        /// Описание проекта.
        /// </summary>
        [Column("description"), NotNull]
        public string description;

        /// <summary>
        /// Статус проекта.
        /// </summary>
        [Column("status"), NotNull]
        public ProjectStatus status;

        /// <summary>
        /// Срок сдачи проекта.
        /// </summary>
        [Column("deadline"), NotNull]
        public DateTime deadline;

        /// <summary>
        /// Дата создания проекта.
        /// </summary>
        [Column("createdAt"), NotNull]
        public DateTime createdAt;

        /// <summary>
        /// Id пользователя лидера проекта.
        /// </summary>
        [Column("userId"), NotNull]
        public Guid userId;

        /// <summary>
        /// Руководитель проекта.
        /// </summary>
        [Association(ThisKey = nameof(userId), OtherKey = nameof(ProjectUserModel.userId))]
        public ProjectUser projectManager;

        /// <summary>
        /// Активные задачи в проекте.
        /// </summary>
        [Association(ThisKey = nameof(projectId), OtherKey = nameof(ProjectTaskModel.projectId))]
        public List<ProjectTask> tasks = new List<ProjectTask>();
    }
}
