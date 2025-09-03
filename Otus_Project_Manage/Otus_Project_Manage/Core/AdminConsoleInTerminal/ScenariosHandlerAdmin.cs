using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class ScenariosHandlerAdmin
    {
        public ScenariosHandlerAdmin(IScenarioService scenarioService, IUserService userService)
        {
            this.scenarioService = scenarioService;
            this.userService = userService;
        }
        private readonly IScenarioService scenarioService;
        private readonly IUserService userService;

        /// <summary>
        /// Метод удаления данных о сценариях пользователей в админской консоли в терминале.
        /// </summary>
        /// <param name="ct">Токен отмены.</param>
        public async Task DeleteScenarioUserData(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var users = await userService.GetAllUsers(ct);
            var scenarios = await scenarioService.GetAllScenatiosData(ct);

            var userScenario = ChooseUserScenarioData(scenarios, users, out bool cancelFlag);

            if (cancelFlag)
            {
                Console.WriteLine("Удаление данных о сценариях завершено.");
                return;
            }

            await scenarioService.ResetScenarioByUserTelegramId(userScenario.userId, ct);

            Console.WriteLine("Хотите удалить еще одни данные о сценарии пользователя?\r\n" +
                                 "Введите \"Да\" для выбора еще одни данные о сценарии пользователя и любую другую строку для выхода из модуля удаления данных сценариев пользователей.");
            string inputStr = Console.ReadLine();

            if (inputStr != "Да")
                return;
        }

        /// <summary>
        /// Метод получения данных о сценарии конкретного пользователя.
        /// </summary>
        /// <param name="userScenarioDatas">Список данных о сценариях пользователей.</param>
        /// <param name="users">Список пользователей.</param>
        /// <param name="cancelFlag">Флаг отмены операции.</param>
        /// <returns>Данные о сценарии пользователя.</returns>
        private UserScenarioData ChooseUserScenarioData(IReadOnlyList<UserScenarioData> userScenarioDatas, IReadOnlyList<ProjectUser> users, out bool cancelFlag)
        {
            cancelFlag = false;
            string inputStr;
            int countUsers = userScenarioDatas.Count;
            Console.WriteLine("Выберите пользователя:\r\n" +
                              "Для выхода введите \"Сancel\"");

            while (true)
            {
                for (int i = 0; i < countUsers; i++)
                    Console.WriteLine($"{i + 1}.{userScenarioDatas[i].scenarioType} - пользователь {users.
                                                                                                    Where(x => x.telegramUserId == userScenarioDatas[i].userId).
                                                                                                    First().
                                                                                                    userName}");

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
                    Console.WriteLine("Введено число вне диапазона индексов данных о сценарии!\r\n" +
                                      "Выберите данные о сценарии заново или отмените операцию написав \"Cancel\".");
                    continue;
                }
                return userScenarioDatas[userIndex - 1];
            }
            return null;
        }
    }
}
