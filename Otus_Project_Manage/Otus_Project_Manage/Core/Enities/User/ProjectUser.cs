using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class ProjectUser
    {
        public ProjectUser() { }
        public ProjectUser(long telegramUserId, string name)
        {
            this.telegramUserId = telegramUserId;
            this.userName = name;
        }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string userName;

        /// <summary>
        /// Id пользователя в tеlegramm.
        /// </summary>
        public long telegramUserId = 0;

        /// <summary>
        /// Id пользователя.
        /// </summary>
        public Guid userId = Guid.NewGuid();

        /// <summary>
        /// Команда (группа) пользователя.
        /// </summary>
        public UsersTeam? team = null;

        /// <summary>
        /// Роль пользователя.
        /// </summary>
        public UserRole role = UserRole.None;

        /// <summary>
        /// Флаг указывающий административные права пользователя.
        /// </summary>
        public bool isAdmin = false;

        
        public Project? project = null;
    }
}
