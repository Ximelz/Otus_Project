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

                    userScenario.Data.Add($"{UserRole.TeamLead}", null);

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserInAddTeam|{UserRole.TeamLead}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserInAddTeam|{UserRole.TeamLead}")});

                    await telegramMessageService.SendMessageWithKeyboard($"Вы хотите выбрать пользователя на роль {UserRole.TeamLead}?", keyboard);

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
                        case "AcceptChooseUserInAddTeam":
                            users = await userService.GetUsersByRole(UserRole.None, telegramMessageService.ct);

                            foreach (var user in users)
                                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{user.userName}", $"ChooseUsersInAddTeam|{user.telegramUserId}"));

                            await telegramMessageService.SendMessageWithKeyboard($"Выберите пользователя на роль {callbackQueryData.Argument}", keyboard);

                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            userScenario.currentStep = "ChooseUsers";

                            break;
                        case "CancelChooseUserInAddTeam":
                            if (!UserRole.TryParse(callbackQueryData.Argument, out role))
                            {
                                await telegramMessageService.SendMessage("Неверный аргумент у кнопки!");
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                userScenario.currentStep = "ChooseUsers";
                                break;
                            }

                            if (!userScenario.Data.ContainsKey("Developer"))
                            {
                                userScenario.Data.Add($"{UserRole.Developer}", null);
                                keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserInAddTeam|{UserRole.Developer}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserInAddTeam|{UserRole.Developer}")});

                                await telegramMessageService.SendMessageWithKeyboard($"Вы хотите выбрать пользователя на роль {UserRole.Developer}?", keyboard);

                                userScenario.currentStep = "ChooseUsers";
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                break;
                            }

                            if (!userScenario.Data.ContainsKey("Tester"))
                            {
                                userScenario.Data.Add($"{UserRole.Tester}", null);
                                keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserInAddTeam|{UserRole.Tester}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserInAddTeam|{UserRole.Tester}")});

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
                        case "ChooseUsersInAddTeam":
                            if (!long.TryParse(callbackQueryData.Argument, out telegramUserId))
                            {
                                await telegramMessageService.SendMessage("Неверный аргумент у кнопки!");
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                userScenario.currentStep = "ChooseUsers";
                                break;
                            }

                            if (userScenario.Data.ContainsKey("Tester"))
                            {
                                userScenario.Data["Tester"] = callbackQueryData.Argument;

                                keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptAddTeam|{userScenario.Data["TeamName"]}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelAddTeam|{userScenario.Data["TeamName"]}")});

                                await telegramMessageService.SendMessageWithKeyboard($"Вы подтверждаете создание команды {userScenario.Data["TeamName"]}?", keyboard);

                                userScenario.currentStep = "AddTeam";
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                break;
                            }

                            if (userScenario.Data.ContainsKey("Developer"))
                            {
                                userScenario.Data["Developer"] = callbackQueryData.Argument;

                                keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserInAddTeam|{UserRole.Developer}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserInAddTeam|{UserRole.Developer}")});

                                await telegramMessageService.SendMessageWithKeyboard($"Вы хотите выбрать пользователя на роль {UserRole.Developer}?", keyboard);

                                userScenario.currentStep = "ChooseUsers";
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                break;
                            }

                            userScenario.Data["TeamLead"] = callbackQueryData.Argument;

                            keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChooseUserInAddTeam|{UserRole.TeamLead}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChooseUserInAddTeam|{UserRole.TeamLead}")});

                            await telegramMessageService.SendMessageWithKeyboard($"Вы хотите выбрать пользователя на роль {UserRole.TeamLead}?", keyboard);

                            userScenario.currentStep = "ChooseUsers";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
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

                            if (userScenario.Data["TeamLead"] != null)
                            {
                                telegramUserId = (long)userScenario.Data["TeamLead"];
                                await userService.ChangeUserRole(telegramUserId, UserRole.TeamLead, telegramMessageService.ct);
                            }

                            if (userScenario.Data["Developer"] != null)
                            {
                                telegramUserId = (long)userScenario.Data["Developer"];
                                await userService.ChangeUserRole(telegramUserId, UserRole.Developer, telegramMessageService.ct);
                            }

                            if (userScenario.Data["Tester"] != null)
                            {
                                telegramUserId = (long)userScenario.Data["Tester"];
                                await userService.ChangeUserRole(telegramUserId, UserRole.Tester, telegramMessageService.ct);
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
