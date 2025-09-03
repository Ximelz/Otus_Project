using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class UsersTeam
    {
        public UsersTeam() { }
        public UsersTeam(string name)
        {
            this.name = name;
            teamId = Guid.NewGuid();
        }

        /// <summary>
        /// Id команды.
        /// </summary>
        public Guid teamId = Guid.NewGuid();
        
        /// <summary>
        /// Название команды.
        /// </summary>
        public string name;

        /// <summary>
        /// Пользователи в команде по ролям.
        /// </summary>
        public readonly Dictionary<UserRole, ProjectUser> usersInTeam = new Dictionary<UserRole, ProjectUser>();
    }
}
