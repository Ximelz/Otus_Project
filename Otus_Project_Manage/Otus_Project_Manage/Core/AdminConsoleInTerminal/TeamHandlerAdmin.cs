using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Otus_Project_Manage
{
    public class TeamHandlerAdmin
    {
        public TeamHandlerAdmin(ITeamService teamService, IUserService userService)
        {
            this.teamService = teamService;
            this.userService = userService;
        }
        private readonly ITeamService teamService;
        private readonly IUserService userService;

        /// <summary>
        /// Метод удаления команд в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        public async Task DeleteTeam(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                var team = ChooseTeam(await teamService.GetAllTeams(ct), out bool cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Удаление команд закончено.");
                    break;
                }

                if (team == null)
                {
                    Console.WriteLine("Команда не была выбрана или такой команды нет.");
                    break;
                }

                Console.WriteLine($"Вы подтверждаете удаление команды {team.name}?\r\n" +
                                  $"Введите \"Да\" для удаления и любую другую строку для отмены.");

                string inputStr = Console.ReadLine();

                if (inputStr == "Да")
                {
                    var usersInTeam = await userService.GetUsersByTeam(team.teamId, ct);

                    foreach (var user in usersInTeam)
                    {
                        user.team = null;
                        user.role = UserRole.None;
                        await userService.UpdateUser(user, ct);
                    }

                    await teamService.DeleteTeam(team.teamId, ct);
                }
                else
                    continue;

                Console.WriteLine("Хотите удалить еще одну команду?\r\n" +
                                  "Введите \"Да\" для удаления еще одну команду и любую другую строку для выхода из режима удаления.");

                inputStr = Console.ReadLine();

                if (inputStr != "Да")
                    break;
            }
        }


        /// <summary>
        /// Метод добавления команд в админской консоли.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        public async Task AddTeam(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            string inputStr;
            UsersTeam team;

            while (true)
            {

                Console.WriteLine("Введите имя команды.\r\n" +
                                  "Для выхода из режима создания команды введите \"Cancel\":");

                inputStr = Console.ReadLine();

                if (inputStr == "Cancel")
                {
                    Console.WriteLine("Создание команды отменено.");
                    return;
                }

                team = new UsersTeam(inputStr);

                var users = await userService.GetUsersByRole(UserRole.None, ct);

                if (users != null)
                {
                    Console.WriteLine("Хотите добавить пользователей в команду?\r\n" +
                                      "Введите \"Да\" для добавления пользователей. Любая другая строка пропустает данный этап.");

                    inputStr = Console.ReadLine();

                    if (inputStr == "Да")
                        await AddUsersInTeam((List<ProjectUser>)users, team, ct);
                }

                await teamService.AddTeam(team, ct);

                Console.WriteLine("Хотите добавить еще одну команду?\r\n" +
                                      "Введите \"Да\" для добавления еще одной команды. Любая другая строка завершает режим создания команд.");

                inputStr = Console.ReadLine();

                if (inputStr != "Да")
                    return;
            }
        }


        /// <summary>
        /// Метод изменения команд в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        public async Task ChangeTeam(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            string inputStr;

            while (true)
            {
                UsersTeam? team = ChooseTeam(await teamService.GetAllTeams(ct), out bool cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Изменение команд закончено.");
                    return;
                }

                if (team == null)
                {
                    Console.WriteLine("Команда не была выбрана или такой команды нет.");
                    continue;
                }

                int changeParams = ChooseChangeTeamParams(out cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Изменение команд закончено.");
                    return;
                }

                switch (changeParams)
                {
                    case 0:
                        Console.WriteLine("Не выбраны атрибуты для изменения команды.");
                        break;
                    case 2:
                        await UpdateTeamParams(team, true, false, false, ct);
                        break;
                    case 4:
                        await UpdateTeamParams(team, true, false, false, ct);
                        break;
                    case 6:
                        await UpdateTeamParams(team, true, false, false, ct);
                        break;
                    case 8:
                        await UpdateTeamParams(team, true, false, false, ct);
                        break;
                    case 10:
                        await UpdateTeamParams(team, true, false, false, ct);
                        break;
                    case 12:
                        await UpdateTeamParams(team, true, false, false, ct);
                        break;
                    case 14:
                        await UpdateTeamParams(team, true, false, false, ct);
                        break;
                }

                Console.WriteLine("Хотите изменить еще 1 команду?\r\n" +
                                  "Введите \"Да\" для выбора еще одной команды пользователей и любую другую строку для выхода из модуля изменения команд пользователей.");
                inputStr = Console.ReadLine();

                if (inputStr != "Да")
                    return;
            }
        }

        /// <summary>
        /// Метод выбора команды из списка команд.
        /// </summary>
        /// <param name="teams">Список команд.</param>
        /// <param name="cancelFlag">Флаг отмены операции.</param>
        /// <returns>Выбранная команда.</returns>
        private UsersTeam? ChooseTeam(IReadOnlyList<UsersTeam> teams, out bool cancelFlag)
        {
            cancelFlag = false;
            string inputStr;
            int countTeams = teams.Count;
            Console.WriteLine("Выберите команду:\r\n" +
                              "Для выхода наберите \"Сancel\"");

            while (true)
            {
                for (int i = 0; i < countTeams; i++)
                    Console.WriteLine($"{i + 1}.{teams[i].name}");

                inputStr = Console.ReadLine();

                if (inputStr == "Cancel")
                {
                    cancelFlag = true;
                    break;
                }

                if (!int.TryParse(inputStr, out int teamIndex))
                {
                    Console.WriteLine("Было введено не число!");
                    continue;
                }

                if (teamIndex > countTeams && teamIndex < 1)
                {
                    Console.WriteLine("Введено число вне диапазона индексов команд!\r\n" +
                                      "Выберите команду заново или отмените операцию написав \"Cancel\".");
                    continue;
                }
                return teams[teamIndex - 1];
            }
            return null;
        }

        /// <summary>
        /// Метод получения выбранных функция для изменеия данных о команде.
        /// </summary>
        /// <param name="cancelFlag">Флаг отмены.</param>
        /// <returns>Id операций (id каждой операции является степень двойки, их сумма отвечает за комбинацию функций для изменения).</returns>
        private int ChooseChangeTeamParams(out bool cancelFlag)
        {
            cancelFlag = false;

            List<string> teamChangeParams = new List<string>
            {
                { "Переименование команды" },
                { "Удаление пользователей в команде" },
                { "Добавление пользователей в команду" }
            };

            List<string> chooseParams = new List<string>();
            List<int> indexParams = new List<int>();

            int paramsNumbs = 0;
            string inputStr;

            while (true)
            {
                if (chooseParams.Count == 3)
                    break;

                indexParams.Clear();

                Console.WriteLine("Выберите изменяемый параметр команды.\r\n" +
                                  "Для выхода из режима выбора команды команд (групп) пользователей наберите \"Сancel\":");

                int index = 1;
                for (int i = 0; i < 3; i++)
                {
                    if (!chooseParams.Contains(teamChangeParams[i]))
                    {
                        indexParams.Add(i);
                        Console.WriteLine($"{index}.{teamChangeParams[i]}.");
                        index++;
                    }
                }

                inputStr = Console.ReadLine();
                if (inputStr == "Cancel")
                {
                    cancelFlag = true;
                    return 0;
                }

                if (!int.TryParse(inputStr, out int parametrIndex))
                {
                    Console.WriteLine("Было введено не число!");
                    continue;
                }

                if (parametrIndex < 1 && parametrIndex > index)
                {
                    Console.WriteLine("Введено число вне диапазона индексов команд!\r\n" +
                                     "Выберите команду заново или отмените операцию написав \"Cancel\".");
                    continue;
                }

                chooseParams.Add(teamChangeParams[indexParams[parametrIndex - 1]]);
            }

            for (int i = 0; i < 3; i++)
                if (chooseParams.Contains(teamChangeParams[i]))
                    paramsNumbs += (int)Math.Pow(2, (i + 1));

            return paramsNumbs;
        }

        /// <summary>
        /// Метод обновления данных о командах.
        /// </summary>
        /// <param name="team">Изменяемая команда.</param>
        /// <param name="renameFlag">Флаг необходимости переименования команды.</param>
        /// <param name="deleteUsersFromTeamFlag">Флаг необходимости удаления пользователей из команды.</param>
        /// <param name="addUsersInTeamFlag">Флаг необходимости добавления пользователей в команду.</param>
        /// <param name="ct">Токен отмены.</param>
        private async Task UpdateTeamParams(UsersTeam team, bool renameFlag, bool deleteUsersFromTeamFlag, bool addUsersInTeamFlag, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            string inputStr;
            
            if (renameFlag)
            {
                Console.WriteLine("Введите новое имя команды или введите \"Cancel\" для выхода из режима изменения команд:");

                inputStr = Console.ReadLine();
                
                if (inputStr == "Cancel")
                {
                    Console.WriteLine("Изменение команд пользователей закончено.");
                    return;
                }

                team.name = inputStr;
            }

            if (deleteUsersFromTeamFlag)
            {
                if (team.usersInTeam.Count != 0)
                    await DeleteUsersFromTeam(team, ct);
                else
                    Console.WriteLine("В данной команде нет пользователей!");
            }

            if (addUsersInTeamFlag)
            {
                if (team.usersInTeam.Count == 4)
                {
                    Console.WriteLine("В данной команде все роли заняты!");
                    return;
                }

                var usersWithoutTeam = await userService.GetUsersByRole(UserRole.None, ct);
                if (usersWithoutTeam != null)
                    await AddUsersInTeam((List<ProjectUser>)usersWithoutTeam, team, ct);
                else
                    Console.WriteLine("Пользователей без команды нет!");
            }
        }

        /// <summary>
        /// Метод удаления пользователей из команды.
        /// </summary>
        /// <param name="team">Команда из которой удаляются пользователи.</param>
        /// <param name="ct">Токен отмены.</param>
        private async Task DeleteUsersFromTeam(UsersTeam team, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            string inputStr;
            int usersCount = team.usersInTeam.Count;
            ProjectUser user;
            List<ProjectUser> users = new List<ProjectUser>();
            foreach (KeyValuePair<UserRole, ProjectUser> keyValuePair in team.usersInTeam)
                users.Add(keyValuePair.Value);

            while (true)
            {
                Console.WriteLine("Выберите пользователя для удаления из команды или введите \"Cancel\" для выхода из режима удаления пользователя из команды");
                for (int i = 0; i < usersCount; i++)
                    Console.WriteLine($"{i + 1}.{users[i].userName}");

                inputStr = Console.ReadLine();

                if (inputStr == "Cancel")
                {
                    Console.WriteLine("Удаление пользователей из команды завершено!");
                    return;
                }

                if (!int.TryParse(inputStr, out int userIndex))
                {
                    Console.WriteLine("Было введено не число!");
                    continue;
                }

                if (userIndex < 1 && userIndex > usersCount)
                {
                    Console.WriteLine("Введено число вне диапазона индексов пользователей!\r\n" +
                                     "Выберите команду заново или отмените операцию написав \"Cancel\".");
                    continue;
                }
                user = users[userIndex - 1];
                team.usersInTeam.Remove(user.role);
                user.team = null;
                user.role = UserRole.None;
                await userService.UpdateUser(user, ct);
                users.Remove(user);
                usersCount = users.Count;

                if (usersCount == 0)
                    break;

                Console.WriteLine($"Пользователь {user.userName} удален из команды {team.name}. Хотите удалить еще одного пользователя из команды?\r\n" +
                                  $"Введимте \"Да\" для удаления пользователя из команды, любая другая введенная строка заканчивает удаления пользователей из команды.");

                inputStr = Console.ReadLine();

                if (inputStr != "Да")
                    break;
            }
        }

        /// <summary>
        /// Метод добавления пользователей в команду.
        /// </summary>
        /// <param name="users">Список пользователей, откуда нужно брать пользователей для добавления.</param>
        /// <param name="team">Команда в которую добавляются пользователи.</param>
        /// <param name="ct">Токен отмены.</param>
        private async Task AddUsersInTeam(List<ProjectUser> users, UsersTeam team, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            string inputStr;
            int usersCount = users.Count;
            ProjectUser? user;
            int roleIndex;
            List<UserRole> roles = new List<UserRole>();

            while (true)
            {
                roles.Clear();
                roleIndex = 1;
                Console.WriteLine("Выберите роль для добавления пользователя в команду или введите \"Cancel\" для выхода из режима удаления пользователя из команды");

                if (!team.usersInTeam.ContainsKey(UserRole.TeamLead))
                {
                    Console.WriteLine($"{roleIndex}.Руководитель команды.");
                    roleIndex++;
                    roles.Add(UserRole.TeamLead);
                }

                if (!team.usersInTeam.ContainsKey(UserRole.Developer))
                {
                    Console.WriteLine($"{roleIndex}.Разработчик.");
                    roleIndex++;
                    roles.Add(UserRole.Developer);
                }

                if (!team.usersInTeam.ContainsKey(UserRole.Tester))
                {
                    Console.WriteLine($"{roleIndex}.Испытатель.");
                    roleIndex++;
                    roles.Add(UserRole.Tester);
                }                              


                inputStr = Console.ReadLine();

                if (inputStr == "Cancel")
                {
                    Console.WriteLine("Добавление пользователей в команду завершено!");
                    return;
                }

                if (!int.TryParse(inputStr, out int inputRoleIndex))
                {
                    Console.WriteLine("Было введено не число!");
                    continue;
                }

                if (inputRoleIndex < 1 && inputRoleIndex > roleIndex)
                {
                    Console.WriteLine("Введено число вне диапазона индексов ролей!\n" +
                                     "Выберите команду заново или отмените операцию написав \"Cancel\".");
                    continue;
                }

                user = ChooseUser(users, out bool cancelFlag);

                if (inputStr == "Cancel")
                {
                    Console.WriteLine("Добавление пользователей в команду завершено!");
                    return;
                }

                if (user == null)
                {
                    Console.WriteLine("Пользователь не выбран или такого пользователя нет!");
                    continue;
                }

                user.role = roles[inputRoleIndex - 1];
                await userService.UpdateUser(user, ct);
                team.usersInTeam.Add(user.role, user);
                users.Remove(user);

                if (team.usersInTeam.Count == 4)
                {
                    Console.WriteLine($"Пользователь {user.userName} добавлен в команду {team.name}, теперь она укомплектована.");
                    return;
                }    

                Console.WriteLine($"Пользователь {user.userName} добавлен в команду {team.name}. Хотите добавить еще одного пользователя в команду?\r\n" +
                                  $"Введимте \"Да\" для добавления пользователя в команду, любая другая введенная строка заканчивает добавление пользователей в команду.");

                inputStr = Console.ReadLine();

                if (inputStr != "Да")
                    break;
            }
        }

        /// <summary>
        /// Метод выбора пользователя из списка пользователей.
        /// </summary>
        /// <param name="users">Список пользователей.</param>
        /// <param name="cancelFlag">Флаг отмены.</param>
        /// <returns>Выбранный пользователей.</returns>
        private ProjectUser? ChooseUser(IReadOnlyList<ProjectUser> users, out bool cancelFlag)
        {
            cancelFlag = false;
            string inputStr;
            int countUsers = users.Count;
            Console.WriteLine("Выберите пользователя\r\n" +
                              "Для выхода наберите \"Сancel\"");

            while (true)
            {
                for (int i = 0; i < countUsers; i++)
                    Console.WriteLine($"{i + 1}.{users[i].userName}");

                inputStr = Console.ReadLine();

                if (inputStr == "Cancel")
                {
                    cancelFlag = true;
                    break;
                }

                if (!int.TryParse(inputStr, out int userIndex))
                {
                    Console.WriteLine("Было введено не число!");
                    continue;
                }

                if (userIndex > countUsers && userIndex < 1)
                {
                    Console.WriteLine("Введено число вне диапазона индексов пользователей!\r\n" +
                                      "Выберите пользователя заново или отмените операцию написав \"Cancel\".");
                    continue;
                }
                return users[userIndex - 1];
            }
            return null;
        }
    }
}
