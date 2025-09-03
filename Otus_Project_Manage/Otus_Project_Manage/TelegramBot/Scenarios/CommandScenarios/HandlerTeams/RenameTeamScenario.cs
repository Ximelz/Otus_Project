using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    /// <summary>
    /// Сценарий изменение имени команды.
    /// </summary>
    public class RenameTeamScenario : IScenario
    {
        public RenameTeamScenario(ITeamService teamService)
        {
            this.teamService = teamService;
        }
        private readonly ITeamService teamService;
        public ScenarioTypes ScenarioType => ScenarioTypes.RenameTeam;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            UsersTeam team;
            Guid teamId;
            string newNameTeam;

            if (telegramMessageService.update.Type == UpdateType.CallbackQuery)
            {
                CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);

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

                        team = await teamService.GetTeamById(teamId, telegramMessageService.ct);

                        await telegramMessageService.SendMessage($"Введите новое имя для команды {team.name}:");

                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        userScenario.currentStep = "EnteringName";
                        return userScenario.scenarioStatus;
                    case "RenameTeam":

                        teamId = (Guid)userScenario.Data["TeamId"];
                        newNameTeam = userScenario.Data["NewName"].ToString();
                        if (callbackQueryData.Command == "AcceptRenameTeam")
                        {
                            await teamService.RenameTeam(teamId, newNameTeam, telegramMessageService.ct);
                            await telegramMessageService.SendMessageByKeyboardType("Команда переименована!", KeyboardTypes.Admin);
                            userScenario.scenarioStatus = ScenarioStatus.Completed;
                        }
                        else if (callbackQueryData.Command == "CancelRenameTeam")
                        {
                            team = await teamService.GetTeamById(teamId, telegramMessageService.ct);
                            await telegramMessageService.SendMessage($"Введите новое имя для команды {team.name}:");
                            userScenario.currentStep = "EnteringName";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        else
                        {
                            await telegramMessageService.SendMessage("Неверная команда!");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }

                        return userScenario.scenarioStatus;
                    default:
                        await telegramMessageService.SendMessage("Неверный этап переименования команды.");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                }
            }
            else
            {
                if (userScenario.currentStep == "EnteringName")
                {
                    newNameTeam = await telegramMessageService.GetMessage();

                    if (userScenario.Data.ContainsKey("NewName"))
                        userScenario.Data["NewName"] = newNameTeam;
                    else
                        userScenario.Data.Add("NewName", newNameTeam);

                    teamId = (Guid)userScenario.Data["TeamId"];

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptRenameTeam|{teamId}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelRenameTeam|{teamId}")});

                    team = await teamService.GetTeamById(teamId, telegramMessageService.ct);

                    await telegramMessageService.SendMessageWithKeyboard($"Вы хотите изменить имя команда {team.name} на \"{newNameTeam}\"?", keyboard);

                    userScenario.currentStep = "RenameTeam";
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
                }

                await telegramMessageService.SendMessage("Получен неверный тип входящего сообщения! Ожидаются данные из кнопки!");
                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                return userScenario.scenarioStatus;
            }
        }
    }
}
