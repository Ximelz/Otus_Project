using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    /// <summary>
    /// Сценарий изменения команды пользователя.
    /// </summary>
    public class ChangeUserTeamScenario : IScenario
    {
        public ChangeUserTeamScenario(IUserService userService, ITeamService teamService)
        {
            this.userService = userService;
            this.teamService = teamService;
        }
        private readonly IUserService userService;
        private readonly ITeamService teamService;
        public ScenarioTypes ScenarioType => ScenarioTypes.ChangeUserTeam;

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
            Guid teamId;
            UserRole userRole;
            ProjectUser changeUser;
            UsersTeam chooseTeam;
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
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

                    changeUser = await userService.GetUserByUserId(userId, telegramMessageService.ct);

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"SelectTeamsWithEmptyRole|{changeUser.role}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"SelectTeamsWithCurrentRole|{changeUser.role}")});

                    await telegramMessageService.SendMessageWithKeyboard("Вы хотите поменять роль пользователя при смене команды?", keyboard);

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "SelectTeams";

                    return userScenario.scenarioStatus;
                case "SelectTeams":
                    if (!UserRole.TryParse(callbackQueryData.Argument, out userRole))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (callbackQueryData.Command == "SelectTeamsWithEmptyRole")
                    {
                        List<UsersTeam> teams = new List<UsersTeam>();
                        UsersTeam tempTeam;

                        Guid.TryParse(userScenario.Data["UserId"].ToString(), out userId);
                        changeUser = await userService.GetUserByUserId(userId, telegramMessageService.ct);

                        foreach (var role in Enum.GetValues<UserRole>())
                            if (role != userRole && role != UserRole.None && role != changeUser.role)
                                teams = teams.Union(teamService.GetTeamByEmptyUserRole(role, telegramMessageService.ct).Result, new UsersTeamComparer()).ToList();

                        foreach (var team in teams)
                        {
                            if (team.name == changeUser.team.name)
                                continue;

                            string argumentStr = "";
                            foreach (var role in Enum.GetValues<UserRole>())
                                if (!team.usersInTeam.ContainsKey(role) && role != UserRole.None && role != changeUser.role)
                                    argumentStr += role.ToString() + " ";
                            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{team.name}. Свободные роли:{argumentStr.Trim()}", $"SelectTeamsWithEmptyRole|{team.teamId}"));
                        }
                    }
                    else if (callbackQueryData.Command == "SelectTeamsWithCurrentRole")
                    {
                        var teams = await teamService.GetTeamByEmptyUserRole(userRole, telegramMessageService.ct);

                        foreach (var team in teams)
                            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{team.name}.", $"SelectTeamsWithCurrentRole|{team.teamId}"));
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }
                    await telegramMessageService.SendMessageWithKeyboard("Выберите команду!", keyboard);

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "ChooseTeam";

                    return userScenario.scenarioStatus;
                case "ChooseTeam":
                    if (!Guid.TryParse(callbackQueryData.Argument, out teamId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!userScenario.Data.ContainsKey("TeamId"))
                        userScenario.Data.Add("TeamId", callbackQueryData.Argument);
                    else
                        userScenario.Data["TeamId"] = callbackQueryData.Argument;

                    chooseTeam = await teamService.GetTeamById(teamId, telegramMessageService.ct);
                    string scenarioMessage = "";
                    Guid.TryParse(userScenario.Data["UserId"].ToString(), out userId);
                    changeUser = await userService.GetUserByUserId(userId, telegramMessageService.ct);

                    if (callbackQueryData.Command == "SelectTeamsWithEmptyRole")
                    {
                        var roles = new List<UserRole>();
                        foreach (var role in Enum.GetValues<UserRole>())
                            if (!chooseTeam.usersInTeam.ContainsKey(role) && role != UserRole.None && role != changeUser.role)
                                roles.Add(role);

                        if (roles.Count == 1)
                        {
                            scenarioMessage = $"Вы точно хотите переместить пользователя в команду {chooseTeam.name}?";

                            keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChangeTeamUser|{roles[0]}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChangeTeamUser|{roles[0]}")});

                            userScenario.currentStep = "ChangeTeam";
                        }
                        else
                        {
                            scenarioMessage = $"Выберите новую роль пользователя в команде {chooseTeam.name}";

                            foreach (var role in roles)
                                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{role}", $"ChooseRole|{role}"));

                            userScenario.currentStep = "ChooseRole";
                        }
                    }
                    else if (callbackQueryData.Command == "SelectTeamsWithCurrentRole")
                    {
                        scenarioMessage = $"Вы точно хотите перевести пользователя в команду {chooseTeam.name}?";

                        keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChangeTeamUser|{changeUser.role}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChangeTeamUser|{changeUser.role}")});

                        userScenario.currentStep = "ChangeTeam";
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }
                    await telegramMessageService.SendMessageWithKeyboard(scenarioMessage, keyboard);

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;

                    return userScenario.scenarioStatus;
                case "ChooseRole":
                    if (!Guid.TryParse(userScenario.Data["TeamId"].ToString(), out teamId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }
                    chooseTeam = await teamService.GetTeamById(teamId, telegramMessageService.ct);

                    if (callbackQueryData.Command == "ChooseRole")
                    {
                        keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptChangeTeamUser|{callbackQueryData.Argument}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelChangeTeamUser|{callbackQueryData.Argument}")});

                        await telegramMessageService.SendMessageWithKeyboard($"Вы точно хотите первести пользователя в команду {chooseTeam.name}?", keyboard);

                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        userScenario.currentStep = "ChangeTeam";
                        return userScenario.scenarioStatus;
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }
                case "ChangeTeam":
                    if (!UserRole.TryParse(callbackQueryData.Argument, out userRole))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }
                    Guid.TryParse(userScenario.Data["UserId"].ToString(), out userId);
                    Guid.TryParse(userScenario.Data["TeamId"].ToString(), out teamId);
                    changeUser = await userService.GetUserByUserId(userId, telegramMessageService.ct);
                    chooseTeam = await teamService.GetTeamById(teamId, telegramMessageService.ct);

                    if (callbackQueryData.Command == "AcceptChangeTeamUser")
                    {
                        changeUser.team.usersInTeam.Remove(changeUser.role);
                        changeUser.team = chooseTeam;
                        changeUser.role = userRole;
                        chooseTeam.usersInTeam.Add(userRole, changeUser);
                        userScenario.scenarioStatus = ScenarioStatus.Completed;

                        await userService.UpdateUser(changeUser, telegramMessageService.ct);

                        await telegramMessageService.SendMessageWithDefaultKeyboard($"Пользователь {changeUser.userName} переведен в команду {chooseTeam.name}!");
                    }
                    else if (callbackQueryData.Command == "CancelChangeTeamUser")
                    {
                        keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"SelectTeamsWithEmptyRole|{changeUser.role}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"SelectTeamsWithCurrentRole|{changeUser.role}")});

                        await telegramMessageService.SendMessageWithKeyboard(
                                            "Отмена изменения команды! Вы возвращаетесь на этап выбора команды для изменения.\n" +
                                            "Вы хотите поменять роль пользователя при смене команды?",
                                            keyboard);

                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        userScenario.currentStep = "SelectTeams";
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап для изменения команды пользователя.");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
            }
        }
    }

    public class UsersTeamComparer : IEqualityComparer<UsersTeam>
    {
        public bool Equals(UsersTeam x, UsersTeam y)
        {
            if (x == null || y == null) return false;
            return x.teamId == y.teamId && x.name == y.name;
        }

        public int GetHashCode(UsersTeam obj)
        {
            return HashCode.Combine(obj.teamId, obj.name);
        }
    }
}
