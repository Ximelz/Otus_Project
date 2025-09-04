using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    [Table("TaskStages")]
    public class TaskStageModel
    {
        /// <summary>
        /// Id этапа.
        /// </summary>
        [Column("stageId"), PrimaryKey]
        public Guid stageId;

        /// <summary>
        /// Наименование этапа.
        /// </summary>
        [Column("name"), NotNull]
        public string name;

        /// <summary>
        /// Комментарий при переходе на другой этап.
        /// </summary>
        [Column("comment"), NotNull]
        public string comment;

        /// <summary>
        /// Описание этапа.
        /// </summary>
        [Column("description"), NotNull]
        public string description;

        /// <summary>
        /// Статус выполнения этапа.
        /// </summary>
        [Column("status"), NotNull]
        public TaskStatus status;

        /// <summary>
        /// Id исполнителя этапа.
        /// </summary>
        [Column("userId"), NotNull]
        public Guid userId;

        /// <summary>
        /// Id следующего этапа.
        /// </summary>
        [Column("nextStageId")]
        public Guid? nextStageId;

        /// <summary>
        /// Исполнитель этапа.
        /// </summary>
        [Association(ThisKey = nameof(userId), OtherKey = nameof(ProjectUserModel.userId))]
        public ProjectUserModel user;

        /// <summary>
        /// Следующий этап.
        /// </summary>
        [Association(ThisKey = nameof(nextStageId), OtherKey = nameof(TaskStageModel.stageId))]
        public TaskStageModel? nextStage;
    }
}
