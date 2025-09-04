using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Otus_Project_Manage
{
    [Table("ProjectTasks")]
    public class ProjectTaskModel
    {
        /// <summary>
        /// Id задачи.
        /// </summary>
        [Column("taskId"), PrimaryKey]
        public Guid taskId;

        /// <summary>
        /// Наименованеи задачи.
        /// </summary>
        [Column("taskName"), NotNull]
        public string taskName;

        /// <summary>
        /// Команда (группа) пользователя.
        /// </summary>
        [Column("teamId"), NotNull]
        public Guid teamId;

        /// <summary>
        /// Id 1 этапа задачи
        /// </summary>
        [Column("startStageId"), NotNull]
        public Guid startStageId;

        /// <summary>
        /// Описание задачи.
        /// </summary>
        [Column("description"), NotNull]
        public string description;

        /// <summary>
        /// Дата создания задачи.
        /// </summary>
        [Column("createdAt"), NotNull]
        public DateTime createdAt;

        /// <summary>
        /// Срок выполнения задачи.
        /// </summary>
        [Column("deadline"), NotNull]
        public DateTime deadline;

        /// <summary>
        /// Статус выполнения задачи.
        /// </summary>
        [Column("status"), NotNull]
        public TaskStatus status;

        /// <summary>
        /// Id проекта.
        /// </summary>
        [Column("projectId")]
        public Guid? projectId;

        /// <summary>
        /// Проект к которому принадлежит задача.
        /// </summary>
        [Association(ThisKey = nameof(projectId), OtherKey = nameof(ProjectModel.projectId))]
        public ProjectModel? project;

        /// <summary>
        /// Команда (группа) пользователя.
        /// </summary>
        [Association(ThisKey = nameof(teamId), OtherKey = nameof(UserTeamModel.teamId))]
        public UserTeamModel team;

        /// <summary>
        /// Команда (группа) пользователя.
        /// </summary>
        [Association(ThisKey = nameof(startStageId), OtherKey = nameof(TaskStageModel.stageId))]
        public TaskStageModel firstStage;
    }
}
