using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    /// <summary>
    /// Сценарий регистрации пользователя.
    /// </summary>
    public class RegisteredUserScenario : IScenario
    {
        public RegisteredUserScenario(IUserService userService, ITeamService teamService)
        {
            this.userService = userService;
            this.teamService = teamService;
        }
        private readonly IUserService userService;
        private readonly ITeamService teamService;
        public ScenarioTypes ScenarioType => ScenarioTypes.RegisteredUser;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
            {
                await telegramMessageService.SendMessage("Для работы сценария необходимо получить информацию из кнопки, а не из ввода сообщения!");
                return ScenarioStatus.InProcess;
            }

            CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            var user = await userService.GetUserByTelegramId(userScenario.userId, telegramMessageService.ct);

            switch (userScenario.currentStep)
            {
                case "Start":
                    if (!Guid.TryParse(callbackQueryData.Argument, out Guid userId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!userScenario.Data.ContainsKey("UserId"))
                        userScenario.Data.Add("UserId", callbackQueryData.Argument);
                    else
                        userScenario.Data["UserId"] = callbackQueryData.Argument;

                    keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Лидер команды", $"ChooseRole|{UserRole.TeamLead}"));
                    keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Разработчик", $"ChooseRole|{UserRole.Developer}"));
                    keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Испытатель", $"ChooseRole|{UserRole.Tester}"));

                    await telegramMessageService.SendMessageWithKeyboard("Выберите роль регистрируемого пользователя.", keyboard);

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "ChooseRole";
                    return userScenario.scenarioStatus;
                case "ChooseRole":
                    if (callbackQueryData.Command != "ChooseRole")
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!UserRole.TryParse(callbackQueryData.Argument, out UserRole userRole))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!userScenario.Data.ContainsKey("UserRole"))
                        userScenario.Data.Add("UserRole", callbackQueryData.Argument);
                    else
                        userScenario.Data["UserId"] = callbackQueryData.Argument;

                    var teams = await teamService.GetTeamByEmptyUserRole(userRole, telegramMessageService.ct);

                    foreach (var team in teams)
                        keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{team.name}", $"ChooseTeam|{team.teamId}"));

                    await telegramMessageService.SendMessageWithKeyboard("Выберите команду для регистрируемого пользователя.", keyboard);

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "ChooseTeam";

                    return userScenario.scenarioStatus;
                case "ChooseTeam":
                    if (callbackQueryData.Command != "ChooseTeam")
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!Guid.TryParse(callbackQueryData.Argument, out Guid teamId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!userScenario.Data.ContainsKey("TeamId"))
                        userScenario.Data.Add("TeamId", callbackQueryData.Argument);
                    else
                        userScenario.Data["TeamId"] = callbackQueryData.Argument;

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptRegisteredUser|{callbackQueryData.Argument}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelRegisteredUser|{callbackQueryData.Argument}")});

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "RegisteredUser";

                    return userScenario.scenarioStatus;
                case "RegisteredUser":
                    ProjectUser changeUser = await userService.GetUserByUserId((Guid)userScenario.Data["UserId"], telegramMessageService.ct);
                    UserRole chooseUserRole = (UserRole)userScenario.Data["UserRole"];
                    UsersTeam chooseTeam = await teamService.GetTeamById((Guid)userScenario.Data["TeamId"], telegramMessageService.ct);

                    if (callbackQueryData.Command == "AcceptRegisteredUser")
                    {
                        changeUser.team = chooseTeam;
                        changeUser.role = chooseUserRole;
                        chooseTeam.usersInTeam.Add(chooseUserRole, changeUser);
                        userScenario.scenarioStatus = ScenarioStatus.Completed;

                        await userService.UpdateUser(changeUser, telegramMessageService.ct);

                        KeyboardTypes keyboardType = KeyboardCommands.GetKeyoardTypeByRole(chooseUserRole);

                        await telegramMessageService.SendMessageByKeyboardType($"Пользователь {changeUser.userName} зарегистрирован!", KeyboardTypes.Admin);
                    }
                    else if (callbackQueryData.Command == "CancelRegisteredUser")
                    {
                        keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Лидер команды", $"ChooseRole|{UserRole.TeamLead}"));
                        keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Разработчик", $"ChooseRole|{UserRole.Developer}"));
                        keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Испытатель", $"ChooseRole|{UserRole.Tester}"));

                        await telegramMessageService.SendMessageWithKeyboard("Отмена регистрации пользователя! Вы возвращаетесь на этап выбора роли пользователя.", keyboard);

                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        userScenario.currentStep = "ChooseRole";
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }
                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап для регистрации пользователя.");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
            }
        }
    }
}
