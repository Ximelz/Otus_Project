using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    /// <summary>
    /// Сценарий изменение роли пользователя.
    /// </summary>
    public class ChangeUserRoleScenario : IScenario
    {
        public ChangeUserRoleScenario(IUserService userService)
        {
            this.userService = userService;
        }
        private readonly IUserService userService;
        public ScenarioTypes ScenarioType => ScenarioTypes.ChangeUserRole;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
            {
                await telegramMessageService.SendMessage("Для работы сценария необходимо получить информацию из кнопки, а не из ввода сообщения!");
                return ScenarioStatus.InProcess;
            }

            CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);
            Guid userId;
            UserRole newRole;
            InlineKeyboardMarkup keyboard;
            List<UserRole> roles;
            var user = await userService.GetUserByTelegramId(userScenario.userId, telegramMessageService.ct);

            switch (userScenario.currentStep)
            {
                case "Start":
                    if (!Guid.TryParse(callbackQueryData.Argument, out userId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!userScenario.Data.ContainsKey("UserId"))
                        userScenario.Data.Add("UserId", callbackQueryData.Argument);
                    else
                        userScenario.Data["UserId"] = callbackQueryData.Argument;

                    keyboard = new InlineKeyboardMarkup();

                    roles = new List<UserRole>();

                    foreach (var userRole in Enum.GetValues(typeof(UserRole)))
                        if (!user.team.usersInTeam.ContainsKey((UserRole)userRole))
                            roles.Add((UserRole)userRole);

                    if (roles.Count == 0)
                    {
                        await telegramMessageService.SendMessage($"Свободных ролей в команде {user.team.name} нет!");
                        return ScenarioStatus.Completed;
                    }

                    foreach (var role in roles)
                        keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{role}", $"chooseRole|{role}"));

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "ChooseRole";

                    await telegramMessageService.SendMessageWithKeyboard("Выберите новую роль пользователя.", keyboard);
                    return userScenario.scenarioStatus;
                case "ChooseRole":
                    if (!UserRole.TryParse(callbackQueryData.Argument, out newRole))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }
                    string userIdStr = userScenario.Data["UserId"].ToString();
                    keyboard = new InlineKeyboardMarkup();

                    if (!userScenario.Data.ContainsKey("newRole"))
                        userScenario.Data.Add("newRole", callbackQueryData.Argument);
                    else
                        userScenario.Data["newRole"] = callbackQueryData.Argument;

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChangeRoleUser|{userIdStr}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChangeRoleUser|{userIdStr}")});

                    await telegramMessageService.SendMessageWithKeyboard("Вы хоиите изменить роль пользователя?", keyboard);

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "ChangeRole";
                    return userScenario.scenarioStatus;
                case "ChangeRole":
                    if (callbackQueryData.Command == "AcceptChangeRoleUser")
                    {
                        Guid.TryParse(callbackQueryData.Argument, out userId);
                        UserRole.TryParse(userScenario.Data["newRole"].ToString(), out newRole);

                        var changeUser = await userService.GetUserByUserId(userId, telegramMessageService.ct);
                        changeUser.team.usersInTeam.Add(newRole, changeUser);
                        changeUser.team.usersInTeam.Remove(changeUser.role);

                        await userService.ChangeUserRole(changeUser.telegramUserId, newRole, telegramMessageService.ct);

                        await telegramMessageService.SendMessageByKeyboardType($"Роль пользователя {changeUser.userName} изменена", KeyboardTypes.Admin);

                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else if (callbackQueryData.Command == "CancelChangeRoleUser")
                    {
                        keyboard = new InlineKeyboardMarkup();

                        foreach (var userRole in Enum.GetValues<UserRole>())
                            if (!user.team.usersInTeam.ContainsKey(userRole) && userRole != UserRole.None)
                                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{userRole}", $"chooseRole|{userRole}"));

                        await telegramMessageService.SendMessageWithKeyboard("Выберите новую роль пользователя.", keyboard);

                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        userScenario.currentStep = "ChooseRole";
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    }
                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап для изменения роли пользователя.");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
            }
        }
    }
}
