using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class DeleteTaskScenario : IScenario
    {
        public DeleteTaskScenario(ITaskService taskService)
        {
            this.taskService = taskService;
        }
        readonly ITaskService taskService;
        public ScenarioTypes ScenarioType => ScenarioTypes.DeleteTask;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
            {
                await telegramMessageService.SendMessage("Для работы сценария необходимо получить информацию из кнопки, а не из ввода сообщения!");
                return ScenarioStatus.InProcess;
            }

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);
            Guid taskId;

            switch (userScenario.currentStep)
            {
                case "Start":
                    if (!Guid.TryParse(callbackQueryData.Argument, out taskId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                        return ScenarioStatus.InProcess;
                    }

                    if (!userScenario.Data.ContainsKey("taskId"))
                        userScenario.Data.Add("taskId", callbackQueryData.Argument);
                    else
                        userScenario.Data["taskId"] = callbackQueryData.Argument;

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptDeleteTask|{callbackQueryData.Argument}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelDeleteTask|{callbackQueryData.Argument}")});

                    var task = await taskService.GetTasksById(taskId, telegramMessageService.ct);

                    await telegramMessageService.SendMessageWithKeyboard($"Вы хотите удалить задачу {task.taskName}?", keyboard);

                    userScenario.currentStep = "DeleteTask";
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
                case "DeleteTask":
                    if (callbackQueryData.Command == "AcceptDeleteTask")
                    {
                        Guid.TryParse(userScenario.Data["taskId"].ToString(), out taskId);
                        await taskService.DeleteTask(taskId, telegramMessageService.ct);

                        await telegramMessageService.SendMessageByKeyboardType("Задача удалена!", KeyboardTypes.Admin);
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else if (callbackQueryData.Command == "CancelDeleteTask")
                    {
                        await telegramMessageService.SendMessage("Удаление задачи отменено!");
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Неверная команда!");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    }
                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап удаления задачи.");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    return userScenario.scenarioStatus;
            }
        }
    }
}
