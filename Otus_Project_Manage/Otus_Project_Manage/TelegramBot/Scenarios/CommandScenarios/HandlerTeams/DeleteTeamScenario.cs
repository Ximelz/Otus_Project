using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    /// <summary>
    /// Сценарий удаление команды.
    /// </summary>
    public class DeleteTeamScenario : IScenario
    {
        public DeleteTeamScenario(IUserService userService, ITeamService teamService)
        {
            this.userService = userService;
            this.teamService = teamService;
        }
        private readonly IUserService userService;
        private readonly ITeamService teamService;
        public ScenarioTypes ScenarioType => ScenarioTypes.DeleteTeam;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();


            if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
            {
                await telegramMessageService.SendMessage("Для работы сценария необходимо получить информацию из кнопки, а не из ввода сообщения!");
                return ScenarioStatus.InProcess;
            }

            CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);
            Guid teamId;
            UsersTeam team;

            switch (userScenario.currentStep)
            {
                case "Start":
                    if (!Guid.TryParse(callbackQueryData.Argument, out teamId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!userScenario.Data.ContainsKey("TeamId"))
                        userScenario.Data.Add("TeamId", callbackQueryData.Argument);
                    else
                        userScenario.Data["TeamId"] = callbackQueryData.Argument;

                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptDeleteTeam|{callbackQueryData.Argument}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelDeleteTeam|{callbackQueryData.Argument}")});

                    team = await teamService.GetTeamById(teamId, telegramMessageService.ct);
                    await telegramMessageService.SendMessageWithKeyboard($"Вы хотите удалить команду {team.name}?", keyboard);

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "DeleteTeam";

                    return userScenario.scenarioStatus;
                case "DeleteTeam":
                    if (callbackQueryData.Command == "AcceptDeleteTeam")
                    {
                        Guid.TryParse(userScenario.Data["TeamId"].ToString(), out teamId);
                        team = await teamService.GetTeamById(teamId, telegramMessageService.ct);
                        ProjectUser user;
                        foreach (KeyValuePair<UserRole, ProjectUser> keyValuePair in team.usersInTeam)
                        {
                            user = keyValuePair.Value;
                            user.role = UserRole.None;
                            user.team = null;
                            await userService.UpdateUser(user, telegramMessageService.ct);
                        }

                        await teamService.DeleteTeam(teamId, telegramMessageService.ct);
                        await telegramMessageService.SendMessageByKeyboardType("Команда удалена!", KeyboardTypes.Admin);
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else if (callbackQueryData.Command == "CancelDeleteTeam")
                    {
                        await telegramMessageService.SendMessage("Удаление команды отменено!");
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Неверная команда!");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    }
                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап для удаления команды.");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
            }
        }
    }
}
