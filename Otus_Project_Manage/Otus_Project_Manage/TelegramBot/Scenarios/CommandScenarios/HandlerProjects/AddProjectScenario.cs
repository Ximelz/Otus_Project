using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class AddProjectScenario : IScenario
    {
        public AddProjectScenario(IProjectService projectService, IUserService userService)
        {
            this.projectService = projectService;
            this.userService = userService;
        }
        private readonly IProjectService projectService;
        private readonly IUserService userService;
        public ScenarioTypes ScenarioType => ScenarioTypes.AddProject;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (userScenario.currentStep == "Start")
            {
                await telegramMessageService.SendMessage("Введите имя проекта:");
                userScenario.currentStep = "EnterName";
                userScenario.scenarioStatus = ScenarioStatus.InProcess;
                return userScenario.scenarioStatus;
            }

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
            {
                string inputMessage = await telegramMessageService.GetMessage();

                switch (userScenario.currentStep)
                {
                    case "EnterName":
                        if (!userScenario.Data.ContainsKey("projectName"))
                            userScenario.Data.Add("projectName", inputMessage);
                        else
                            userScenario.Data["projectName"] = inputMessage;

                        await telegramMessageService.SendMessage("Введите описание проекта:");
                        userScenario.currentStep = "EnterDescription";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    case "EnterDescription":
                        if (!userScenario.Data.ContainsKey("projectDescription"))
                            userScenario.Data.Add("projectDescription", inputMessage);
                        else
                            userScenario.Data["projectDescription"] = inputMessage;

                        await telegramMessageService.SendMessage("Введите срок выполнения проекта:");
                        userScenario.currentStep = "EnterDeadline";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    case "EnterDeadline":
                        string format = "dd.MM.yyyy";
                        if (!DateTime.TryParseExact(inputMessage, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadline))
                        {
                            await telegramMessageService.SendMessage("Введен неверный формат даты!");
                            return ScenarioStatus.InProcess;
                        }

                        if (!userScenario.Data.ContainsKey("projectDeadline"))
                            userScenario.Data.Add("projectDeadline", deadline);
                        else
                            userScenario.Data["projectDeadline"] = deadline;

                        var users = (await userService.GetAllUsers(telegramMessageService.ct)).Where(x => x.project == null).Where(x => x.role == UserRole.TeamLead).ToList();

                        foreach (var user in users)
                            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{user.userName}",$"ChooseProjectManager|{user.userId}"));

                        await telegramMessageService.SendMessageWithKeyboard("Выберите руководителя проекта:", keyboard);
                        userScenario.currentStep = "ChooseProjectManager";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    default:
                        await telegramMessageService.SendMessage("Неверный этап для добавления проекта.");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                }
            }
            else
            {
                CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);
                Guid userId;

                switch (userScenario.currentStep)
                {
                    case "ChooseProjectManager":
                        if (!Guid.TryParse(callbackQueryData.Argument, out userId))
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                            return userScenario.scenarioStatus;
                        }

                        if (!userScenario.Data.ContainsKey("userId"))
                            userScenario.Data.Add("userId", userId);
                        else
                            userScenario.Data["userId"] = userId;

                        keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptAddProject|{userId}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelAddProject|{userId}")});

                        await telegramMessageService.SendMessageWithKeyboard("Вы подтверждаете создание проекта?", keyboard);
                        userScenario.currentStep = "AddProject";
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                    case "AddProject":
                        if (callbackQueryData.Command == "AcceptAddProject")
                        {
                            Guid.TryParse(userScenario.Data["userId"].ToString(), out userId);
                            string projectName = userScenario.Data["projectName"].ToString();
                            string projectDescription = userScenario.Data["projectDescription"].ToString();
                            DateTime.TryParseExact(userScenario.Data["projectDeadline"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime projectDeadline);

                            var user = await userService.GetUserByUserId(userId, telegramMessageService.ct);

                            Project project = new Project()
                            {
                                name = projectName,
                                description = projectDescription,
                                deadline = projectDeadline,
                                status = ProjectStatus.Active,
                                projectManager = user
                            };

                            await projectService.AddProject(project, telegramMessageService.ct);

                            await telegramMessageService.SendMessageByKeyboardType($"Проект \"{projectName}\" создан!", KeyboardTypes.Admin);
                            userScenario.scenarioStatus = ScenarioStatus.Completed;
                        }
                        else if (callbackQueryData.Command == "CancelAddProject")
                        {
                            userScenario.Data.Clear();
                            await telegramMessageService.SendMessage("Вы отменили создание проекта и возвращаетесь на этап ввода имени проекта.\r\nВведите имя проекта:!");
                            userScenario.currentStep = "EnterName";
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        else
                        {
                            await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                            userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        }
                        return userScenario.scenarioStatus;
                    default:
                        await telegramMessageService.SendMessage("Неверный этап для добавления проекта.");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                        return userScenario.scenarioStatus;
                }
            }
        }
    }
}
