using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class AddTaskScenario : IScenario
    {
        public AddTaskScenario(ITaskService taskService)
        {
            this.taskService = taskService;
        }
        readonly ITaskService taskService;
        public ScenarioTypes ScenarioType => ScenarioTypes.AddTask;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (userScenario.currentStep == "Start")
            {
                await telegramMessageService.SendMessage("Введите имя задачи!");
                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                userScenario.currentStep = "EnterNameTask";
                return userScenario.scenarioStatus;
            }

            if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
            {
                string inputMessage = await telegramMessageService.GetMessage();
                switch (userScenario.currentStep)
                {
                    case "EnterNameTask":
                        if (!userScenario.Data.ContainsKey("taskName"))
                            userScenario.Data.Add("taskName", inputMessage);
                        else
                            userScenario.Data["taskName"] = inputMessage;

                        await telegramMessageService.SendMessage("Введите описание задачи!");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        userScenario.currentStep = "EnterDescriptionTask";
                        return userScenario.scenarioStatus;
                    case "EnterDescriptionTask":
                        if (!userScenario.Data.ContainsKey("taskDescription"))
                            userScenario.Data.Add("taskDescription", inputMessage);
                        else
                            userScenario.Data["taskDescription"] = inputMessage;

                        await telegramMessageService.SendMessage("Введите описание 1 этапа!");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        userScenario.currentStep = "EnterDescriptionStages";
                        return userScenario.scenarioStatus;
                    case "EnterDescriptionStages":
                        if (!userScenario.Data.ContainsKey("stageFirstDescription"))
                        {
                            userScenario.Data.Add("stageFirstDescription", inputMessage);
                            await telegramMessageService.SendMessage("Введите описание 2 этапа!");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            userScenario.currentStep = "EnterDescriptionStages";
                            return userScenario.scenarioStatus;
                        }

                        if (!userScenario.Data.ContainsKey("stageSecondDescription"))
                        {
                            userScenario.Data.Add("stageSecondDescription", inputMessage);
                            await telegramMessageService.SendMessage("Введите описание 3 этапа!");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            userScenario.currentStep = "EnterDescriptionStages";
                            return userScenario.scenarioStatus;
                        }
                        
                        userScenario.Data.Add("stageThirdDescription", inputMessage);
                        await telegramMessageService.SendMessage("Введите крайнюю дату выполнения задачи:");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        userScenario.currentStep = "EnterDeadline";
                        return userScenario.scenarioStatus;
                    case "EnterDeadline":
                        string format = "dd.MM.yyyy";
                        if (DateTime.TryParseExact(inputMessage, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadline))
                        {
                            await telegramMessageService.SendMessage("Введен неверный формат даты!");
                            return ScenarioStatus.InProcess;
                        }

                        if (!userScenario.Data.ContainsKey("taskDeadline"))
                            userScenario.Data.Add("taskDeadline", deadline);
                        else
                            userScenario.Data["taskDeadline"] = deadline;

                        InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

                        keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptAddTask|{deadline}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelAddTask|{deadline}")});

                        await telegramMessageService.SendMessageWithKeyboard($"Вы хотите добавить задачу {userScenario.Data["taskName"].ToString()}?", keyboard);

                        userScenario.currentStep = "AddTask";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    default:
                        await telegramMessageService.SendMessage("Неверный этап для добавления задачи.");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                }
            }
            else
            {
                CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);

                if (callbackQueryData.Command == "AcceptAddTask")
                {
                    string taskName = userScenario.Data["taskName"].ToString();
                    string taskDescription = userScenario.Data["taskDescription"].ToString();
                    DateTime.TryParseExact(userScenario.Data["taskDeadline"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime taskDeadline);
                    string stageFirstDescription = userScenario.Data["stageFirstDescription"].ToString();
                    string stageSecondDescription = userScenario.Data["stageSecondDescription"].ToString();
                    string stageThirdDescription = userScenario.Data["stageThirdDescription"].ToString();

                    ProjectTask newTask = new ProjectTask(taskName, taskDeadline, taskDescription);
                    newTask.status = TaskStatus.Active;
                    newTask.team = telegramMessageService.user.team;
                    newTask.activeStage = newTask.firstStage;

                    newTask.firstStage = new TaskStage()
                    {
                        name = "Разработка",
                        description = stageFirstDescription,
                        status = TaskStatus.Active,
                        user = telegramMessageService.user.team.usersInTeam[UserRole.Developer],
                        task = newTask
                    };

                    newTask.firstStage.nextStage = new TaskStage()
                    {
                        name = "Испытание",
                        description = stageSecondDescription,
                        status = TaskStatus.Active,
                        user = telegramMessageService.user.team.usersInTeam[UserRole.Tester],
                        task = newTask
                    };

                    newTask.firstStage.nextStage.nextStage = new TaskStage()
                    {
                        name = "Проверка",
                        description = stageThirdDescription,
                        status = TaskStatus.Active,
                        user = telegramMessageService.user.team.usersInTeam[UserRole.TeamLead],
                        task = newTask
                    };

                    await taskService.AddTask(newTask, telegramMessageService.ct);
                    userScenario.scenarioStatus = ScenarioStatus.Completed;
                }
                else if (callbackQueryData.Command == "CancelAddTask")
                {
                    await telegramMessageService.SendMessage("Отмена создания задачи и возвращение на стартовый этап.");
                    userScenario.Data.Clear();
                    userScenario.currentStep = "Start";
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                }
                else
                {
                    await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                }
                return userScenario.scenarioStatus;
            }
        }
    }
}
