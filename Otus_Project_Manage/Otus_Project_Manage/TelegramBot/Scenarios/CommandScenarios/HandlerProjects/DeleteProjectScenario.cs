using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class DeleteProjectScenario : IScenario
    {
        public DeleteProjectScenario(IProjectService projectService)
        {
            this.projectService = projectService;
        }
        public readonly IProjectService projectService;

        public ScenarioTypes ScenarioType => ScenarioTypes.DeleteProject;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
            {
                await telegramMessageService.SendMessage("Для работы сценария необходимо получить информацию из кнопки, а не из ввода сообщения!");
                return ScenarioStatus.InProcess;
            }

            CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);
            Guid projectId;

            switch (userScenario.currentStep)
            {
                case "Start":
                    if (!Guid.TryParse(callbackQueryData.Argument, out projectId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    var project = await projectService.GetProjectById(projectId, telegramMessageService.ct);

                    if (project.tasks.Count != 0)
                    {
                        await telegramMessageService.SendMessage("У данного проекта есть задачи. Удалите все задачи, даже завершенные, и попробьуйте удалить проект еще раз.");
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                        return userScenario.scenarioStatus;
                    }

                    if (!userScenario.Data.ContainsKey("projectId"))
                        userScenario.Data.Add("projectId", projectId);
                    else
                        userScenario.Data["projectId"] = projectId;

                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptDeleteProject|{projectId}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelDeleteProject|{projectId}")});

                    await telegramMessageService.SendMessageWithKeyboard("Вы хотите удалить проект?", keyboard);

                    userScenario.currentStep = "DeleteProject";
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
                case "DeleteProject":
                    if (callbackQueryData.Command == "AcceptDeleteProject")
                    {
                        Guid.TryParse(userScenario.Data["projectId"].ToString(), out projectId);

                        await projectService.DeleteProject(projectId, telegramMessageService.ct);
                        await telegramMessageService.SendMessageByKeyboardType("Проект удален", KeyboardTypes.Admin);

                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else if (callbackQueryData.Command == "CancelDeleteProject")
                    {
                        await telegramMessageService.SendMessage("Удаление отменено!");
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    }
                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап для удаления проекта.");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
            }
        }
    }
}
