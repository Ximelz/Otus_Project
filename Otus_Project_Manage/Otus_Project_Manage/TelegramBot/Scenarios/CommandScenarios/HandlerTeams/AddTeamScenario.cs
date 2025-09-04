using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    /// <summary>
    /// Сценарий добавления команды.
    /// </summary>
    public class AddTeamScenario : IScenario
    {
        public AddTeamScenario(IUserService userService, ITeamService teamService)
        {
            this.userService = userService;
            this.teamService = teamService;
        }
        private readonly IUserService userService;
        private readonly ITeamService teamService;
        public ScenarioTypes ScenarioType => ScenarioTypes.AddTeam;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            CallbackQueryData callbackQueryData;
            IReadOnlyList<ProjectUser> users;
            UserRole role;
            long telegramUserId;

            switch (userScenario.currentStep)
            {
                case "Start":
                    await telegramMessageService.SendMessage("Введите имя команды:");

                    userScenario.currentStep = "ChooseName";
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;

                    return userScenario.scenarioStatus;
                case "ChooseName":
                    string teamName = await telegramMessageService.GetMessage();
                    if (userScenario.Data.ContainsKey("TeamName"))
                        userScenario.Data["TeamName"] = teamName;
                    else
                        userScenario.Data.Add("TeamName", teamName);

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserInAddTeam|{teamName}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserInAddTeam|{teamName}")});

                    await telegramMessageService.SendMessageWithKeyboard($"Вы хотите добавить пользователей?", keyboard);

                    userScenario.currentStep = "ChooseUsers";
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;

                    return userScenario.scenarioStatus;
                case "ChooseUsers":
                    if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
                    {
                        await telegramMessageService.SendMessage("Для работы сценария необходимо получить информацию из кнопки, а не из ввода сообщения!");
                        return ScenarioStatus.InProcess;
                    }

                    callbackQueryData = new CallbackQueryData(telegramMessageService.update);

                    switch (callbackQueryData.Command)
                    {
                        case "CancelChooseUserInAddTeam":
                            keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptAddTeam|{userScenario.Data["TeamName"]}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelAddTeam|{userScenario.Data["TeamName"]}")});

                            await telegramMessageService.SendMessageWithKeyboard($"Вы подтверждаете создание команды {userScenario.Data["TeamName"]}?", keyboard);

                            userScenario.currentStep = "AddTeam";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            break;
                        case "AcceptChooseUserInAddTeam":
                            if (!userScenario.Data.ContainsKey("TeamLead"))
                            {
                                keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserRole|{UserRole.TeamLead}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserRole|{UserRole.TeamLead}")});

                                await telegramMessageService.SendMessageWithKeyboard($"Вы хотите выбрать пользователя на роль {UserRole.TeamLead}?", keyboard);

                                userScenario.currentStep = "ChooseUsers";
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                break;
                            }

                            if (!userScenario.Data.ContainsKey("Developer"))
                            {
                                keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserRole|{UserRole.Developer}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserRole|{UserRole.Developer}")});

                                await telegramMessageService.SendMessageWithKeyboard($"Вы хотите выбрать пользователя на роль {UserRole.Developer}?", keyboard);

                                userScenario.currentStep = "ChooseUsers";
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                break;
                            }

                            if (!userScenario.Data.ContainsKey("Tester"))
                            {
                                keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserRole|{UserRole.Tester}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserRole|{UserRole.Tester}")});

                                await telegramMessageService.SendMessageWithKeyboard($"Вы хотите выбрать пользователя на роль {UserRole.Tester}?", keyboard);

                                userScenario.currentStep = "ChooseUsers";
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                break;
                            }


                            keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptAddTeam|{userScenario.Data["TeamName"]}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelAddTeam|{userScenario.Data["TeamName"]}")});

                            await telegramMessageService.SendMessageWithKeyboard($"Вы подтверждаете создание команды {userScenario.Data["TeamName"]}?", keyboard);

                            userScenario.currentStep = "AddTeam";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            break;
                        case "AcceptChooseUserRole":
                            if (!UserRole.TryParse(callbackQueryData.Argument, out role))
                            {
                                await telegramMessageService.SendMessage("Неверный аргумент у кнопки!");
                                return ScenarioStatus.InProcess;
                            }
                            List<long> ids = new List<long>();

                            if (userScenario.Data.ContainsKey("TeamLead"))
                                ids.Add(long.Parse(userScenario.Data["TeamLead"].ToString()));

                            if (userScenario.Data.ContainsKey("Developer"))
                                ids.Add(long.Parse(userScenario.Data["Developer"].ToString()));

                            if (userScenario.Data.ContainsKey("Tester"))
                                ids.Add(long.Parse(userScenario.Data["Tester"].ToString()));

                            users = await userService.GetUsersByRole(UserRole.None, telegramMessageService.ct);

                            foreach (var user in users)
                                if (!ids.Contains(user.telegramUserId))
                                    keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{user.userName}", $"ChoosenUser|{user.telegramUserId} {role}"));

                            await telegramMessageService.SendMessageWithKeyboard("Выберите пользователя:", keyboard);

                            userScenario.currentStep = "ChooseUsers";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            return userScenario.scenarioStatus;
                        case "ChoosenUser":
                            string[] arguments = callbackQueryData.Argument.Split();
                            if (arguments.Length != 2)
                            {
                                await telegramMessageService.SendMessage("Неверный аргумент у кнопки!");
                                return ScenarioStatus.InProcess;
                            }

                            if (!long.TryParse(arguments[0], out telegramUserId))
                            {
                                await telegramMessageService.SendMessage("Неверный аргумент у кнопки!");
                                return ScenarioStatus.InProcess;
                            }

                            if (!UserRole.TryParse(arguments[1], out role))
                            {
                                await telegramMessageService.SendMessage("Неверный аргумент у кнопки!");
                                return ScenarioStatus.InProcess;
                            }

                            if (userScenario.Data.ContainsKey($"{role}"))
                                userScenario.Data[$"{role}"] = telegramUserId;
                            else
                                userScenario.Data.Add($"{role}", telegramUserId);

                            if (role == UserRole.Tester)
                            {
                                keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptAddTeam|{userScenario.Data["TeamName"]}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelAddTeam|{userScenario.Data["TeamName"]}")});

                                await telegramMessageService.SendMessageWithKeyboard($"Вы подтверждаете создание команды {userScenario.Data["TeamName"]}?", keyboard);

                                userScenario.currentStep = "AddTeam";
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                break;
                            }

                            keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserInAddTeam|{telegramUserId}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserInAddTeam|{telegramUserId}")});

                            await telegramMessageService.SendMessageWithKeyboard($"Пользователь выбран. Хотите добавить еще пользователя?", keyboard);

                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            userScenario.currentStep = "ChooseUsers";
                            break;
                        default:
                            await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            userScenario.currentStep = "ChooseUsers";
                            break;
                    }

                    return userScenario.scenarioStatus;
                case "AddTeam":
                    if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
                    {
                        await telegramMessageService.SendMessage("Для работы сценария необходимо получить информацию из кнопки, а не из ввода сообщения!");
                        return ScenarioStatus.InProcess;
                    }

                    callbackQueryData = new CallbackQueryData(telegramMessageService.update);

                    switch (callbackQueryData.Command)
                    {
                        case "AcceptAddTeam":
                            UsersTeam team = new UsersTeam();
                            team.name = callbackQueryData.Argument;
                            await teamService.AddTeam(team, telegramMessageService.ct);

                            if (userScenario.Data.ContainsKey("TeamLead"))
                            {
                                long.TryParse(userScenario.Data["TeamLead"].ToString(), out telegramUserId);
                                await userService.ChangeUserRole(telegramUserId, UserRole.TeamLead, telegramMessageService.ct);
                                await userService.ChangeUserTeam(telegramUserId, team, telegramMessageService.ct);
                            }

                            if (userScenario.Data.ContainsKey("Developer"))
                            {
                                long.TryParse(userScenario.Data["Developer"].ToString(), out telegramUserId);
                                await userService.ChangeUserRole(telegramUserId, UserRole.Developer, telegramMessageService.ct);
                                await userService.ChangeUserTeam(telegramUserId, team, telegramMessageService.ct);
                            }

                            if (userScenario.Data.ContainsKey("Tester"))
                            {
                                long.TryParse(userScenario.Data["Tester"].ToString(), out telegramUserId);
                                await userService.ChangeUserRole(telegramUserId, UserRole.Tester, telegramMessageService.ct);
                                await userService.ChangeUserTeam(telegramUserId, team, telegramMessageService.ct);
                            }

                            await telegramMessageService.SendMessageByKeyboardType($"Команда {team.name} создана!", KeyboardTypes.Admin);
                            userScenario.scenarioStatus = ScenarioStatus.Completed;

                            break;
                        case "CancelAddTeam":
                            await telegramMessageService.SendMessage("Введите имя команды:");

                            userScenario.Data.Clear();
                            userScenario.currentStep = "ChooseName";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;

                            break;
                        default:
                            await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            userScenario.currentStep = "AddTeam";
                            break;
                    }
                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап добавления команды!");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
            }
        }
    }
}
