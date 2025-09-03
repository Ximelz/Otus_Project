using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class TaskStage
    {
        public TaskStage() { }
        public TaskStage(string name, ProjectUser user)
        {
            this.name = name;
            this.user = user;
        }
        /// <summary>
        /// Id этапа.
        /// </summary>
        public Guid stageId = Guid.NewGuid();

        /// <summary>
        /// Наименование этапа.
        /// </summary>
        public string name;

        /// <summary>
        /// Комментарий при переходе на другой этап.
        /// </summary>
        public string comment;

        /// <summary>
        /// Описание этапа.
        /// </summary>
        public string description;

        /// <summary>
        /// Статус выполнения этапа.
        /// </summary>
        public TaskStatus status;

        /// <summary>
        /// Задача этого этапа.
        /// </summary>
        public ProjectTask task;

        /// <summary>
        /// Исполнитель этапа.
        /// </summary>
        public ProjectUser user;

        /// <summary>
        /// Следующий этап.
        /// </summary>
        public TaskStage? nextStage;
    }
}
