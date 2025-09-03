using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    /// <summary>
    /// Класс для подключения и выгрузки данных из таблиц БД.
    /// </summary>
    public class ProjectTasksDataContext : DataConnection
    {
        public ProjectTasksDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) { }

        /// <summary>
        /// Выгруженная таблица пользователей.
        /// </summary>
        public ITable<ProjectUserModel> Users => this.GetTable<ProjectUserModel>();

        /// <summary>
        /// Выгруженная таблица команд (групп) пользователей.
        /// </summary>
        public ITable<UserTeamModel> Teams => this.GetTable<UserTeamModel>();

        /// <summary>
        /// Выгруженная таблица задач пользователей.
        /// </summary>
        public ITable<ProjectTaskModel> Tasks => this.GetTable<ProjectTaskModel>();

        /// <summary>
        /// Выгруженная таблица этапов задач пользователей.
        /// </summary>
        public ITable<TaskStageModel> Stages => this.GetTable<TaskStageModel>();

        /// <summary>
        /// Выгруженная таблица проектов.
        /// </summary>
        public ITable<ProjectModel> Projects => this.GetTable<ProjectModel>();
    }
}
