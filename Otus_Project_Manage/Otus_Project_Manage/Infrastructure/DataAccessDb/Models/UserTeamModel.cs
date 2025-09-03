using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    [Table("UsersTeam")]
    public class UserTeamModel
    {
        [Column("teamId"), PrimaryKey]
        /// <summary>
        /// Id команды.
        /// </summary>
        public Guid teamId;

        [Column("name"), NotNull]
        /// <summary>
        /// Название команды.
        /// </summary>
        public string name;
    }
}
