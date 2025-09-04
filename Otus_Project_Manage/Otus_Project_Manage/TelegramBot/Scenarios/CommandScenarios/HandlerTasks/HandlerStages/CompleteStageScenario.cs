using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class CompleteStageScenario : IScenario
    {
        public CompleteStageScenario(ITaskService taskService)
        {
            this.taskService = taskService;
        }
        private ITaskService taskService;

        public ScenarioTypes ScenarioType => ScenarioTypes.CompleteStage;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            if (telegramMessageService.update.Type == UpdateType.CallbackQuery)
            {
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

                        if (userScenario.Data.ContainsKey("taskId"))
                            userScenario.Data["taskId"] = taskId;
                        else
                            userScenario.Data.Add("taskId", taskId);

                        await telegramMessageService.SendMessage("Введите комментарий к пройденному этапу:");

                        userScenario.currentStep = "EnterCommit";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    case "CompleteStage":
                        if (callbackQueryData.Command == "AcceptCompleteStage")
                        {
                            Guid.TryParse(userScenario.Data["taskId"].ToString(), out taskId);
                            string commit = userScenario.Data["commit"].ToString();

                            var task = await taskService.GetTasksById(taskId, telegramMessageService.ct);

                            task.activeStage.comment = commit;
                            task.activeStage.status = TaskStatus.Completed;
                            if (task.activeStage.nextStage != null)
                                task.activeStage = task.activeStage.nextStage;
                            else
                                task.status = TaskStatus.Completed;

                            await taskService.UpdateTask(task, telegramMessageService.ct);

                            await telegramMessageService.SendMessageWithDefaultKeyboard("Этап выполнен!");
                            userScenario.scenarioStatus = ScenarioStatus.Completed;
                        }
                        else if (callbackQueryData.Command == "CancelCompleteStage")
                        {                            
                            await telegramMessageService.SendMessage("Введите комментарий к пройденному этапу:");
                            
                            userScenario.currentStep = "EnterCommit";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        else
                        {
                            await telegramMessageService.SendMessage("Неверная команда!");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }

                        return userScenario.scenarioStatus;
                    default:
                        await telegramMessageService.SendMessage("Неверный этап выполнения этапа задачи.");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                }
            }
            else
            {
                if (userScenario.currentStep == "EnterCommit")
                {
                    string inputMessage = await telegramMessageService.GetMessage();

                    if (userScenario.Data.ContainsKey("commit"))
                        userScenario.Data["commit"] = inputMessage;
                    else
                        userScenario.Data.Add("commit", inputMessage);


                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptCompleteStage|{userScenario.Data["taskId"].ToString()}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelCompleteStage|{userScenario.Data["taskId"].ToString()}")});

                    await telegramMessageService.SendMessageWithKeyboard("Вы подтверждаете выполнение этапа?", keyboard);

                    userScenario.currentStep = "CompleteStage";
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
