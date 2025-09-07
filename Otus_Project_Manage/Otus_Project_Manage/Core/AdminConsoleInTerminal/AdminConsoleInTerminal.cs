using System;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class AdminConsoleInTerminal
    {
        public AdminConsoleInTerminal(IUserService userService, ITeamService teamService, IScenarioService scenarioService)
        {
            userHandlerAdmin = new UserHandlerAdmin(userService, teamService);
            teamHandlerAdmin = new TeamHandlerAdmin(teamService, userService);
            scenarioHandlerAdmin = new ScenariosHandlerAdmin(scenarioService, userService);
        }
        private readonly UserHandlerAdmin userHandlerAdmin;
        private readonly TeamHandlerAdmin teamHandlerAdmin;
        private readonly ScenariosHandlerAdmin scenarioHandlerAdmin;

        /// <summary>
        /// Метод запуска админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        public async Task StartAdminConsoleInTerminal(CancellationTokenSource ct)
        {
            ct.Token.ThrowIfCancellationRequested();
            while (true)
            {
                Console.WriteLine("Для выбота команды введите её номер:\n" +
                                  "1.Управление пользователями.\n" +
                                  "2.Управление командами.\n" +
                                  "3.Управление активными пользовательскими сценариями.\n" +
                                  "4.Выход из консоли аднимистратора.\n" +
                                  "5.Остановка работы бота.");

                string inputStr = Console.ReadLine();

                switch (inputStr)
                {
                    case "1":
                        await StartAdminUsers(ct.Token);
                        break;
                    case "2":
                        await StartAdminTeams(ct.Token);
                        break;
                    case "3":
                        await StartAdminScenarios(ct.Token);
                        break;
                    case "4":
                        Console.Clear();
                        return;
                    case "5":
                        Console.WriteLine("Остановка работы бота.");
                        ct.Cancel();
                        return;
                    default:
                        Console.WriteLine("Введена неверная команда!");
                        break;
                }
            }
        }

        /// <summary>
        /// Метод запуска управления пользователями в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        private async Task StartAdminUsers(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            while (true)
            {
                Console.WriteLine("Для выбота команды введите её номер:\n" +
                                  "1.Удаление пользователя.\n" +
                                  "2.Регистрация пользователя.\n" +
                                  "3.Изменение данных пользователя.\n" +
                                  "4.Возврат в главное меню.");

                string inputStr = Console.ReadLine();

                switch (inputStr)
                {
                    case "1":
                        await userHandlerAdmin.DeleteUser(ct);
                        break;
                    case "2":
                        await userHandlerAdmin.RegisteredUser(ct);
                        break;
                    case "3":
                        await userHandlerAdmin.ChangeUser(ct);
                        break;
                    case "4":
                        Console.WriteLine("Выход из режима изменения пользователей.");
                        return;
                    default:
                        Console.WriteLine("Введена неверная команда!");
                        break;
                }
            }
        }

        /// <summary>
        /// Метод запуска управления командами в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        private async Task StartAdminTeams(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                Console.WriteLine("Для выбота команды введите её номер:\n" +
                                  "1.Удаление команды.\n" +
                                  "2.Создание команды.\n" +
                                  "3.Изменение данных команды.\n" +
                                  "4.Возврат в главное меню.");

                string inputStr = Console.ReadLine();

                switch (inputStr)
                {
                    case "1":
                        await teamHandlerAdmin.DeleteTeam(ct);
                        break;
                    case "2":
                        await teamHandlerAdmin.AddTeam(ct);
                        break;
                    case "3":
                        await teamHandlerAdmin.ChangeTeam(ct);
                        break;
                    case "4":
                        Console.WriteLine("Выход из режима изменения команд.");
                        return;
                    default:
                        Console.WriteLine("Введена неверная команда!");
                        break;
                }
            }
        }

        /// <summary>
        /// Метод запуска управления данными о сценариях пользователей в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        private async Task StartAdminScenarios(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                Console.WriteLine("Для выбота команды введите её номер:\n" +
                                  "1.Удаление активного сценария пользователя.\n" +
                                  "2.Возврат в главное меню.");

                string inputStr = Console.ReadLine();

                switch (inputStr)
                {
                    case "1":
                        await scenarioHandlerAdmin.DeleteScenarioUserData(ct);
                        break;
                    case "2":
                        Console.WriteLine("Выход из режима изменения данных о сценарии пользователей.");
                        return;
                    default:
                        Console.WriteLine("Введена неверная команда!");
                        break;
                }
            }
        }
    }
}
