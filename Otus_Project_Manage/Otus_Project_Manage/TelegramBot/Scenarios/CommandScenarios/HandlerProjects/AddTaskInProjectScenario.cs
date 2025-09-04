using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class AddTaskInProjectScenario : IScenario
    {
        public AddTaskInProjectScenario(IProjectService projectService, ITaskService taskService, ITeamService teamService)
        {
            this.projectService = projectService;
            this.taskService = taskService;
            this.teamService = teamService;
        }
        private readonly IProjectService projectService;
        private readonly ITaskService taskService;
        private readonly ITeamService teamService;
        public ScenarioTypes ScenarioType => ScenarioTypes.AddTaskInProject;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            Guid projectId;
            Guid taskId;
            Guid teamId;

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
                        if (!DateTime.TryParseExact(inputMessage, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadline))
                        {
                            await telegramMessageService.SendMessage("Введен неверный формат даты!");
                            return ScenarioStatus.InProcess;
                        }

                        if (!userScenario.Data.ContainsKey("taskDeadline"))
                            userScenario.Data.Add("taskDeadline", deadline);
                        else
                            userScenario.Data["taskDeadline"] = deadline;

                        var teams = (await teamService.GetAllTeams(telegramMessageService.ct)).Where(x => x.usersInTeam.Count == 3).ToList();

                        if (teams.Count == 0)
                        {
                            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Добавить задачу в проект.", $"addTaskInProject|{userScenario.Data["projectId"].ToString()}"));
                            await telegramMessageService.SendMessageWithKeyboard("Полных команд не найдено! Возврат на стартовый этап.", keyboard);
                            userScenario.Data.Clear();
                            userScenario.currentStep = "Start";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                            return userScenario.scenarioStatus;
                        }

                        foreach (var team in teams)
                            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{team.name}", $"ChooseTeam|{team.teamId}"));

                        await telegramMessageService.SendMessageWithKeyboard($"Выберите команду для выполнения задачи.", keyboard);

                        userScenario.currentStep = "ChooseTeam";
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
                switch (userScenario.currentStep)
                {
                    case "Start":
                        if (!Guid.TryParse(callbackQueryData.Argument, out projectId))
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                            return ScenarioStatus.InProcess;
                        }

                        if (!userScenario.Data.ContainsKey("projectId"))
                            userScenario.Data.Add("projectId", projectId);
                        else
                            userScenario.Data["projectId"] = projectId;

                        keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Добавить задачу", $"AddTaskInProject|{projectId}"),
                                           InlineKeyboardButton.WithCallbackData("Создать задачу", $"CreateNewTask|{projectId}")});

                        await telegramMessageService.SendMessageWithKeyboard("Вы хотите добавить уже созданную задачу или создать новую?", keyboard);
                        userScenario.currentStep = "ChooseMode";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    case "ChooseMode":
                        if (callbackQueryData.Command == "AddTaskInProject")
                        {
                            var tasks = (await taskService.GetAllActiveTasks(telegramMessageService.ct)).Where(x => x.project == null).ToList();

                            if (tasks.Count == 0)
                            {
                                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Добавить задачу в проект.", $"addTaskInProject|{userScenario.Data["projectId"].ToString()}"));
                                await telegramMessageService.SendMessageWithKeyboard("Задач для добавления нет. Возврат на стартовый этап.", keyboard);
                                userScenario.Data.Clear();
                                userScenario.currentStep = "Start";
                                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                                return userScenario.scenarioStatus;
                            }

                            foreach (var task in tasks)
                                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{task.taskName}.", $"chooseTaskInProject|{task.taskId}"));

                            await telegramMessageService.SendMessageWithKeyboard("Список доступных задач!", keyboard);
                            userScenario.currentStep = "ChooseTask";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        else if (callbackQueryData.Command == "CreateNewTask")
                        {
                            await telegramMessageService.SendMessage("Введите имя задачи!");
                            userScenario.currentStep = "EnterNameTask";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        else
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка.");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        return userScenario.scenarioStatus;
                    case "ChooseTask":
                        if (!Guid.TryParse(callbackQueryData.Argument, out taskId))
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                            return ScenarioStatus.InProcess;
                        }

                        if (!userScenario.Data.ContainsKey("taskId"))
                            userScenario.Data.Add("taskId", taskId);
                        else
                            userScenario.Data["taskId"] = taskId;


                        keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AccaptAddChoosenTask|{taskId}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelAddChoosenTask|{taskId}")});

                        await telegramMessageService.SendMessageWithKeyboard("Вы хотите добавить выбранную задачу?", keyboard);
                        userScenario.currentStep = "AddChoosenTask";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;

                        return userScenario.scenarioStatus;
                    case "AddChoosenTask":
                        if (callbackQueryData.Command == "AccaptAddChoosenTask")
                        {
                            Guid.TryParse(userScenario.Data["taskId"].ToString(), out taskId);
                            Guid.TryParse(userScenario.Data["projectId"].ToString(), out projectId);

                            var project = await projectService.GetProjectById(projectId, telegramMessageService.ct);
                            var task = await taskService.GetTasksById(taskId, telegramMessageService.ct);

                            project.tasks.Add(task);
                            task.project = project;
                            await projectService.UpdateProject(project, telegramMessageService.ct);
                            await taskService.UpdateTask(task, telegramMessageService.ct);

                            await telegramMessageService.SendMessage($"Задача {task.taskName} была добавлена в проект {project.name}.");

                            userScenario.scenarioStatus = ScenarioStatus.Completed;
                        }
                        else if (callbackQueryData.Command == "CancelAddChoosenTask")
                        {
                            keyboard.AddNewRow(InlineKeyboardButton.WithSwitchInlineQuery("Добавить задачу в проект.", $"addTaskInProject|{userScenario.Data["projectId"]}"));
                            await telegramMessageService.SendMessageWithKeyboard("Отмена добавления задачи в проект и возвращение на стартовый этап.", keyboard);
                            userScenario.Data.Clear();
                            userScenario.currentStep = "Start";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        else
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка.");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        return userScenario.scenarioStatus;
                    case "ChooseTeam":
                        if (!Guid.TryParse(callbackQueryData.Argument, out teamId))
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                            return ScenarioStatus.InProcess;
                        }

                        if (!userScenario.Data.ContainsKey("teamId"))
                            userScenario.Data.Add("teamId", teamId);
                        else
                            userScenario.Data["teamId"] = teamId;

                        keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptAddNewTask|{teamId}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelAddNewTask|{teamId}")});

                        await telegramMessageService.SendMessageWithKeyboard($"Вы хотите добавить задачу {userScenario.Data["taskName"].ToString()}?", keyboard);

                        userScenario.currentStep = "AddNewTask";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                        return userScenario.scenarioStatus;
                    case "AddNewTask":
                        if (callbackQueryData.Command == "AcceptAddNewTask")
                        {
                            string taskName = userScenario.Data["taskName"].ToString();
                            string taskDescription = userScenario.Data["taskDescription"].ToString();
                            DateTime.TryParseExact(userScenario.Data["taskDeadline"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime taskDeadline);
                            string stageFirstDescription = userScenario.Data["stageFirstDescription"].ToString();
                            string stageSecondDescription = userScenario.Data["stageSecondDescription"].ToString();
                            string stageThirdDescription = userScenario.Data["stageThirdDescription"].ToString();
                            Guid.TryParse(userScenario.Data["projectId"].ToString(), out projectId);
                            Guid.TryParse(userScenario.Data["teamId"].ToString(), out teamId);

                            var currentProject = await projectService.GetProjectById(projectId, telegramMessageService.ct);

                            ProjectTask newTask = new ProjectTask(taskName, taskDeadline, taskDescription);
                            newTask.status = TaskStatus.Active;
                            var team = await teamService.GetTeamById(teamId, telegramMessageService.ct);
                            newTask.activeStage = newTask.firstStage;
                            newTask.project = currentProject;

                            newTask.firstStage = new TaskStage()
                            {
                                name = "Разработка",
                                description = stageFirstDescription,
                                status = TaskStatus.Active,
                                user = team.usersInTeam[UserRole.Developer],
                                task = newTask                                
                            };

                            newTask.firstStage.nextStage = new TaskStage()
                            {
                                name = "Испытание",
                                description = stageSecondDescription,
                                status = TaskStatus.Active,
                                user = team.usersInTeam[UserRole.Tester],
                                task = newTask
                            };

                            newTask.firstStage.nextStage.nextStage = new TaskStage()
                            {
                                name = "Проверка",
                                description = stageThirdDescription,
                                status = TaskStatus.Active,
                                user = team.usersInTeam[UserRole.TeamLead],
                                task = newTask
                            };

                            await telegramMessageService.SendMessage($"Задача {newTask.taskName} была добавлена в проект {currentProject.name}.");

                            await projectService.UpdateProject(currentProject, telegramMessageService.ct);
                            await taskService.AddTask(newTask, telegramMessageService.ct);
                            userScenario.scenarioStatus = ScenarioStatus.Completed;
                        }
                        else if (callbackQueryData.Command == "CancelAddNewTask")
                        {
                            keyboard.AddNewRow(InlineKeyboardButton.WithSwitchInlineQuery("Добавить задачу в проект.", $"addTaskInProject|{userScenario.Data["projectId"]}"));
                            await telegramMessageService.SendMessageWithKeyboard("Отмена добавления задачи в проект и возвращение на стартовый этап.", keyboard);
                            userScenario.Data.Clear();
                            userScenario.currentStep = "Start";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        else
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка.");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        return userScenario.scenarioStatus;
                    default:
                        await telegramMessageService.SendMessage("Неверный этап для добавления задачи.");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                }
            }
        }
    }
}
