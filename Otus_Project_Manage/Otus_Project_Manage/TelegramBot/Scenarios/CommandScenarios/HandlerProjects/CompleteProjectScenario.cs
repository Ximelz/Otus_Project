using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class CompleteProjectScenario : IScenario
    {
        public CompleteProjectScenario(IProjectService projectService, ITaskService taskService)
        {
            this.projectService = projectService;
            this.taskService = taskService;
        }
        private readonly IProjectService projectService;
        private readonly ITaskService taskService;

        public ScenarioTypes ScenarioType => ScenarioTypes.CompleteProject;

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

                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptCompleteProject|{projectId}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelCompleteProject|{projectId}")});

                    await telegramMessageService.SendMessageWithKeyboard("Вы хотите завершить проект и все его незавершенные задачи?", keyboard);
                    userScenario.currentStep = "ChooseProjectManager";
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
                case "CompleteProject":
                    if (callbackQueryData.Command == "AcceptCompleteProject")
                    {
                        Guid.TryParse(userScenario.Data["projectId"].ToString(), out projectId);

                        var project = await projectService.GetProjectById(projectId, telegramMessageService.ct);

                        project.status = ProjectStatus.Complete;

                        foreach (var task in project.tasks)
                        {
                            if (task.status == TaskStatus.Active)
                            {
                                task.status = TaskStatus.Completed;
                                var stage = task.firstStage;
                                stage.status = TaskStatus.Completed;
                                for (int i = 0; i < 2; i++)
                                {
                                    stage = stage.nextStage;
                                    stage.status = TaskStatus.Completed;
                                }
                                await taskService.UpdateTask(task, telegramMessageService.ct);
                            }
                        }

                        await projectService.UpdateProject(project, telegramMessageService.ct);
                        await telegramMessageService.SendMessageWithDefaultKeyboard("Проект завершен");
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else if (callbackQueryData.Command == "CancelCompleteProject")
                    {
                        await telegramMessageService.SendMessage("Завершение проекта отменено!");
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    }
                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап для выполнения проекта.");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
            }
        }
    }
}
