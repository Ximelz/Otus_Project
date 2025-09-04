using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    [Table("ProjectUsers")]
    public class ProjectUserModel
    {
        /// <summary>
        /// Id пользователя.
        /// </summary>
        [Column("userId"), PrimaryKey]
        public Guid userId;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        [Column("userName"), NotNull]
        public string userName;

        /// <summary>
        /// Id пользователя в tеlegramm.
        /// </summary>
        [Column("telegramUserId"), NotNull]
        public long telegramUserId;

        /// <summary>
        /// Команда (группа) пользователя.
        /// </summary>
        [Column("teamId")]
        public Guid? teamId;

        /// <summary>
        /// Роль пользователя.
        /// </summary>
        [Column("role"), NotNull]
        public UserRole role;

        /// <summary>
        /// Флаг указывающий административные права пользователя.
        /// </summary>
        [Column("isAdmin"), NotNull]
        public bool isAdmin = false;


        /// <summary>
        /// Команда (группа) пользователя.
        /// </summary>
        [Association(ThisKey = nameof(teamId), OtherKey = nameof(UserTeamModel.teamId))]
        public UserTeamModel? team;
    }
}
