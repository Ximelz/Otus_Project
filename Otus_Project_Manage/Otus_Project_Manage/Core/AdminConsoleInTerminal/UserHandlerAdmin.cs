using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Otus_Project_Manage
{
    public class UserHandlerAdmin
    {
        public UserHandlerAdmin(IUserService userService, ITeamService teamService)
        {
            this.userService = userService;
            this.teamService = teamService;
        }
        private readonly IUserService userService;
        private readonly ITeamService teamService;

        /// <summary>
        /// Метод удаления пользователей в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        public async Task DeleteUser(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                var user = ChooseUser(await userService.GetAllUsers(ct), out bool cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Удаление пользователей закончено.");
                    break;
                }

                if (user == null)
                {
                    Console.WriteLine("Пользователь не был выбран или такого пользователя нет.");
                    break;
                }

                Console.WriteLine($"Вы подтверждаете удаление пользователя {user.userName}?\r\n" +
                                  $"Введите \"Да\" для удаления и любую другую строку для отмены.");

                string inputStr = Console.ReadLine();

                if (inputStr == "Да")
                    await userService.DeleteUser(user.userId, ct);
                else
                    continue;

                Console.WriteLine("Хотите удалить еще одного пользователя?\r\n" +
                                  "Введите \"Да\" для удаления еще одного пользователя и любую другую строку для выхода из режима удаления.");

                inputStr = Console.ReadLine();

                if (inputStr != "Да")
                    break;
            }
        }

        /// <summary>
        /// Метод регистрации пользователей в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        public async Task RegisteredUser(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            string inputStr;

            while (true)
            {
                var user = ChooseUser(await userService.GetUsersByRole(UserRole.None, ct), out bool cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Регистрация пользователя закончена.");
                    return;
                }

                if (user == null)
                {
                    Console.WriteLine("Пользователь не был выбран или такого пользователя нет.");
                    continue;
                }

                UserRole userRole = ChooseUserRole(out cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Регистрация пользователя отменена.");
                    break;
                }

                if (userRole == UserRole.None)
                {
                    Console.WriteLine("Роль не была выбрана!");
                    continue;
                }

                UsersTeam userTeam = ChooseUserTeam(await teamService.GetTeamByEmptyUserRole(userRole, ct), out cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Регистрация пользователя закончена.");
                    return;
                }

                if (userTeam == null)
                {
                    Console.WriteLine("Команда не была выбрана!\r\n");
                    continue;
                }

                Console.WriteLine("Хотите выдать права администратора пользователю?\r\n" +
                                  "Введите \"Да\" для выдачи прав администратора," +
                                  "любая другая строка не дает пользователю прав администратора.");

                bool isAdmin = false;
                inputStr = Console.ReadLine();
                
                if (inputStr == "Да")
                    isAdmin = true;

                user.role = userRole;
                user.team = userTeam;
                user.isAdmin = isAdmin;

                await userService.UpdateUser(user, ct);
            }
        }

        /// <summary>
        /// Метод изменения пользователей в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        public async Task ChangeUser(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            string inputStr;

            while (true)
            {
                ProjectUser? user = ChooseUser((await userService.GetAllUsers(ct)).Where(x => x.role != UserRole.None).ToList(), out bool cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Изменение пользователей закончено.");
                    return;
                }

                if (user == null)
                {
                    Console.WriteLine("Пользователь не был выбран или такого пользователя нет.");
                    continue;
                }

                int changeParams = ChooseChangeUserParams(out cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Изменение пользователей закончено.");
                    return;
                }

                switch (changeParams)
                {
                    case 0:
                        Console.WriteLine("Не выбраны атрибуты для изменения пользователя.");
                        break;
                    case 2:
                        await UpdateUserParams(user, true, false, false, ct);
                        break;
                    case 4:
                        await UpdateUserParams(user, false, true, false, ct);
                        break;
                    case 6:
                        await UpdateUserParams(user, true, true, false, ct);
                        break;
                    case 8:
                        await UpdateUserParams(user, false, false, true, ct);
                        break;
                    case 10:
                        await UpdateUserParams(user, true, false, true, ct);
                        break;
                    case 12:
                        await UpdateUserParams(user, false, true, true, ct);
                        break;
                    case 14:
                        await UpdateUserParams(user, true, true, true, ct);
                        break;
                }
                
                Console.WriteLine("Хотите изменить еще 1 пользователя?\r\n" +
                                  "Введите \"Да\" для выбора еще одного пользователя и любую другую строку для выхода из модуля изменения пользователей.");
                inputStr = Console.ReadLine();
                
                if (inputStr != "Да")
                    return;
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
            Console.WriteLine("Выберите пользователя:\r\n" +
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

        /// <summary>
        /// Метод выбора роли пользователя.
        /// </summary>
        /// <param name="cancelFlag">Флаг отмены.</param>
        /// <returns>Выбранная роль.</returns>
        private UserRole ChooseUserRole(out bool cancelFlag)
        {
            cancelFlag = false;

            while (true)
            {
                Console.WriteLine("Выберите роль пользователя:\r\n" +
                                 "Для выхода из режима регистрации пользователей наберите \"Сancel\":\r\n" +
                                 "1.Лидер команды.\r\n" +
                                 "2.Разработчик.\r\n" +
                                 "3.Испытатель.");


                string inputStr = Console.ReadLine();

                if (inputStr == "Cancel")
                {
                    cancelFlag = true;
                    break;
                }

                if (!int.TryParse(inputStr, out int roleIndex))
                {
                    Console.WriteLine("Было введено не число!");
                    continue;
                }

                if (roleIndex < 1 && roleIndex > 4)
                {
                    Console.WriteLine("Введено число вне диапазона выбора роли!\r\n" +
                                      "Выберите роль заново или отмените операцию написав \"Cancel\".");
                    continue;
                }

                switch (roleIndex)
                {
                    case 1:
                        return UserRole.TeamLead;
                    case 2:
                        return UserRole.Developer;
                    case 3:
                        return UserRole.Tester;
                }
            }
            return UserRole.None;
        }

        /// <summary>
        /// Метод выбора команды пользователей.
        /// </summary>
        /// <param name="teams">Список команд.</param>
        /// <param name="cancelFlag">Флаг отмены.</param>
        /// <returns>Выбранная команда.</returns>
        private UsersTeam? ChooseUserTeam(IReadOnlyList<UsersTeam> teams, out bool cancelFlag)
        {
                
            cancelFlag = false;
            string inputStr;
            int countTeams = teams.Count;
            Console.WriteLine("Выберите команду пользователей.\r\n" +
                              "Для выхода из режима выбора команды пользователей наберите \"Сancel\":");

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
        /// Метод получения выбранных функция для изменеия данных о пользователе.
        /// </summary>
        /// <param name="cancelFlag">Флаг отмены.</param>
        /// <returns>Id операций (id каждой операции является степень двойки, их сумма отвечает за комбинацию функций для изменения).</returns>
        private int ChooseChangeUserParams(out bool cancelFlag)
        {
            cancelFlag = false;

            List<string> userChangeParams= new List<string>
            {
                { "Изменение роли" },
                { "Изменение команды" },
                { "Измение прав администратора" }
            };

            List<string> chooseParams = new List<string>();
            List<int> indexParams = new List<int>();

            int paramsNumbs = 0;
            string inputStr;

            while (true)
            {
                indexParams.Clear();

                Console.WriteLine("Выберите изменяемый параметр пользователя.\r\n" +
                                  "Для выхода из режима выбора команды пользователей наберите \"Сancel\":");

                int index = 1;
                for (int i = 0; i < 3; i++)
                {
                    if (!chooseParams.Contains(userChangeParams[i]))
                    {
                        indexParams.Add(i);
                        Console.WriteLine($"{index}.{userChangeParams[i]}.");
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

                chooseParams.Add(userChangeParams[indexParams[parametrIndex - 1]]);

                if (chooseParams.Count == 3)
                    break;

                Console.WriteLine("Хотите выбрать еще 1 функцию?\r\n" +
                                  "Введите \"Да\" для выбора еще одной команды и любую другую строку для перехода к обработке выбранных функций.");


                inputStr = Console.ReadLine();

                if (inputStr != "Да")
                    break;
            }

            for (int i = 0; i < 3; i++)
                if (chooseParams.Contains(userChangeParams[i]))
                    paramsNumbs += (int)Math.Pow(2,(i + 1));

            return paramsNumbs;
        }

        /// <summary>
        /// Метод обновления данных о командах.
        /// </summary>
        /// <param name="user">Изменяемый пользователь.</param>
        /// <param name="roleFChangeFlag">Флаг необходимости изменение роли пользователя.</param>
        /// <param name="teamChangeFlag">Флаг необходимости изменение команды пользователя.</param>
        /// <param name="AdminChangeFlag">Флаг необходимости изменения прав администратора у пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        private async Task UpdateUserParams(ProjectUser user, bool roleFChangeFlag, bool teamChangeFlag, bool AdminChangeFlag, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            bool cancelFlag;
            string inputStr;

            if (roleFChangeFlag)
            {                
                UserRole userRole = ChooseUserRole(out cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Изменение пользователя закончено.");
                    return;
                }

                user.role = userRole;
            }

            if (teamChangeFlag)
            {
                UsersTeam userTeam = ChooseUserTeam(await teamService.GetTeamByEmptyUserRole(user.role, ct), out cancelFlag);

                if (cancelFlag)
                {
                    Console.WriteLine("Изменение пользователя закончено.");
                    return;
                }

                if (userTeam == null)
                {
                    Console.WriteLine("Команда не была выбрана!\r\n" +
                                      "Хотите изменить пользователя без команды?" +
                                      "Введите \"Да\" для удаления пользователя из команды,");

                    inputStr = Console.ReadLine();

                    if (inputStr != "Да")
                        user.team = userTeam;
                }
                else
                    user.team = userTeam;
            }
            if (AdminChangeFlag)
            {
                string adminStatus;
                if (user.isAdmin)
                    adminStatus = "является админом";
                else
                    adminStatus = "не является админом";
                Console.WriteLine($"Данный пользователь {adminStatus}. Хотите это изменить?\r\n" +
                                  $"Введимте \"Да\" для изменения привелигированных прав у пользователя, любая другая введенная строка не изменяет этого.");

                inputStr = Console.ReadLine();

                if (inputStr == "Да")
                    user.isAdmin = user.isAdmin ? false : true;
            }
            await userService.UpdateUser(user, ct);
        }
    }
}
