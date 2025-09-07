using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class ReturnStageScenario : IScenario
    {
        public ReturnStageScenario(ITaskService taskService)
        {
            this.taskService = taskService;
        }
        private ITaskService taskService;

        public ScenarioTypes ScenarioType => ScenarioTypes.ReturnStage;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            Guid stageId;
            Guid taskId;
            ProjectTask task;
            TaskStage stage;

            if (telegramMessageService.update.Type == UpdateType.CallbackQuery)
            {
                CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);

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

                        task = await taskService.GetTasksById(taskId, telegramMessageService.ct);

                        if (task.activeStage.stageId == task.firstStage.nextStage.stageId)
                        {
                            if (userScenario.Data.ContainsKey("stageId"))
                                userScenario.Data["stageId"] = task.activeStage.stageId;
                            else
                                userScenario.Data.Add("stageId", task.activeStage.stageId);

                            await telegramMessageService.SendMessage("Введите комментарий к возвращаемому этапу:");

                            userScenario.currentStep = "EnterCommit";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            return userScenario.scenarioStatus;
                        }

                        keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{task.firstStage.name}", $"chooseStage|{task.firstStage.stageId}"));
                        keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{task.firstStage.nextStage.name}", $"chooseStage|{task.firstStage.stageId}"));

                        await telegramMessageService.SendMessageWithKeyboard("На какой этап вернуть задачу?", keyboard);

                        userScenario.currentStep = "ChooseStage";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    case "ChooseStage":
                        if (!Guid.TryParse(callbackQueryData.Argument, out stageId))
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                            return ScenarioStatus.InProcess;
                        }

                        if (userScenario.Data.ContainsKey("stageId"))
                            userScenario.Data["stageId"] = stageId;
                        else
                            userScenario.Data.Add("stageId", stageId);

                        await telegramMessageService.SendMessage("Введите комментарий к возвращаемому этапу:");

                        userScenario.currentStep = "EnterCommit";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    case "ReturnStage":
                        if (callbackQueryData.Command == "AcceptReturnStage")
                        {
                            Guid.TryParse(userScenario.Data["taskId"].ToString(), out taskId);
                            Guid.TryParse(userScenario.Data["stageId"].ToString(), out stageId);
                            string commit = userScenario.Data["commit"].ToString();

                            task = await taskService.GetTasksById(taskId, telegramMessageService.ct);

                            if (task.firstStage.nextStage.stageId == stageId)
                            {
                                task.firstStage.status = TaskStatus.Active;
                                task.firstStage.nextStage.status = TaskStatus.Active;
                                task.activeStage = task.firstStage;
                                task.activeStage.comment = commit;                                
                            }
                            else
                            {
                                task.firstStage.nextStage.status = TaskStatus.Active;
                                task.activeStage = task.firstStage.nextStage;
                                task.activeStage.comment = commit;
                            }

                            await taskService.UpdateTask(task, telegramMessageService.ct);

                            await telegramMessageService.SendMessageWithDefaultKeyboard($"Задача возвращена на этап \"{task.activeStage.name}\".");
                            userScenario.scenarioStatus = ScenarioStatus.Completed;
                        }
                        else if (callbackQueryData.Command == "CancelReturnStage")
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
                        await telegramMessageService.SendMessage("Неверный этап возврата задачи на прошедший этап.");
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

                    Guid.TryParse(userScenario.Data["stageId"].ToString(), out stageId);
                    Guid.TryParse(userScenario.Data["taskId"].ToString(), out taskId);

                    task = await taskService.GetTasksById(taskId, telegramMessageService.ct);
                    stage = task.firstStage;
                    for (int i = 0; i < 3; i++)
                    {
                        if (stageId == stage.stageId)
                            break;
                        stage = stage.nextStage;
                    }

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptReturnStage|{userScenario.Data["taskId"].ToString()}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelReturnStage|{userScenario.Data["taskId"].ToString()}")});

                    await telegramMessageService.SendMessageWithKeyboard($"Вы подтверждаете возврат задачи на этап \"{stage.name}\"?", keyboard);

                    userScenario.currentStep = "ReturnStage";
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
