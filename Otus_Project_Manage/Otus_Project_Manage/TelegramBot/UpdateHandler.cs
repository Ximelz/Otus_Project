using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Otus_Project_Manage
{
    public class UpdateHandler : IUpdateHandler
    {
        public UpdateHandler(IUserService userService, ITeamService teamService, IScenarioService scenarioService, ITaskService taskService, IProjectService projectService)
        {
            this.userService = userService;
            this.teamService = teamService;
            this.taskService = taskService;
            this.projectService = projectService;
            this.scenarioService = scenarioService;
            onMessageService = new OnMessageService(userService, teamService, taskService, projectService);
            onCallbackQueryService = new OnCallbackQueryService(userService, teamService, taskService, projectService);
        }
        private readonly IUserService userService;
        private readonly ITeamService teamService;
        private readonly ITaskService taskService;
        private readonly IProjectService projectService;
        private readonly IScenarioService scenarioService;
        private readonly OnMessageService onMessageService;
        private readonly OnCallbackQueryService onCallbackQueryService;

        /// <summary>
        /// Метод обработки исключений при работе бота.
        /// </summary>
        /// <param name="botClient">Сессия telegram бота.</param>
        /// <param name="exception">Вызванное исключение.</param>
        /// <param name="source">Тип ошибки.</param>
        /// <param name="ct">Токен отмены.</param>
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            Console.WriteLine(exception.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Метод обработки сообщений бота.
        /// </summary>
        /// <param name="botClient">Сессия telegram бота.</param>
        /// <param name="update">Информация о присланном сообщении (тип, пользователь, сам текст и т.д.)</param>
        /// <param name="ct">Токен отмены.</param>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            long userId = update.Type == UpdateType.CallbackQuery ? update.CallbackQuery.From.Id : update.Message.From.Id;

            ProjectUser? user = await userService.GetUserByTelegramId(userId, ct);

            if (user == null)
                user = new ProjectUser();

            ITelegramMessageService telegramMessageService = new TelegramMessageService(botClient, update, user, ct);

            try
            {
                switch (update.Type)
                {
                    case UpdateType.CallbackQuery:
                        await OnCallbackQuery(telegramMessageService);
                        break;
                    default:
                        await OnMessage(telegramMessageService);
                        break;
                }
            }
            catch (Exception ex)
            {
                await telegramMessageService.SendMessage(ex.Message); ;
            }
            finally
            {
                await telegramMessageService.SetUserCommands();
            }
        }

        private async Task OnMessage(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var userScenarioData = await scenarioService.GetScenarioDataByUserId(telegramMessageService.user.telegramUserId, telegramMessageService.ct);

            if (userScenarioData != null)
            {
                if (await telegramMessageService.GetMessage() == "/cancel")
                {
                    await telegramMessageService.SendMessageWithDefaultKeyboard("Сценарий отменен!");

                    await scenarioService.ResetScenarioByUserTelegramId(telegramMessageService.user.telegramUserId, telegramMessageService.ct);
                    return;
                }

                await ScenarioHandler(telegramMessageService, userScenarioData);
                return;
            }

            switch (await telegramMessageService.GetMessage())
            {
                case "/info":
                    await onMessageService.BotInfo(telegramMessageService);
                    break;
                case "/help":
                    await onMessageService.BotHelp(telegramMessageService);
                    break;
                case "/start":
                    await onMessageService.StartBot(telegramMessageService);
                    break;
                case "/showprojects":
                    if (telegramMessageService.user.project != null)
                        await onMessageService.ShowProjects(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("Вы не являетесь руководителем проекта.");
                    break;
                case "/adminconsole":
                    if (telegramMessageService.user.isAdmin)
                        await onMessageService.AdminConsole(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "/showteamtasks":
                    if (telegramMessageService.user.role == UserRole.TeamLead)
                        await onMessageService.ShowTeamTasks(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("Вы не являетесь лидером команды.");
                    break;
                case "/showmytasks":
                    if (telegramMessageService.user.role != UserRole.None)
                        await onMessageService.ShowMyTasks(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("Вы не зарегистрированны.");
                    break;
                case "/showAllprojects":
                    if (telegramMessageService.user.isAdmin)
                        await onMessageService.ShowAllProjects(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "/registerUser":
                    if (telegramMessageService.user.isAdmin)
                        await onMessageService.RegisterUser(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "/showUsers":
                    if (telegramMessageService.user.isAdmin)
                        await onMessageService.ShowUsers(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "/showTeams":
                    if (telegramMessageService.user.isAdmin)
                        await onMessageService.ShowTeams(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "/exitAdminConsole":
                    if (telegramMessageService.user.isAdmin)
                        await onMessageService.ExitAdminConsole(telegramMessageService);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                default:
                    await telegramMessageService.SendMessage("Введена неверная команда!");
                    break;
            }
        }

        private async Task OnCallbackQuery(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (telegramMessageService.user.role == UserRole.None)
            {
                await telegramMessageService.SendMessageByKeyboardType("Вы не зарегистрированны!", KeyboardTypes.None);
                return;
            }    

            CallbackQueryData callbackData = new CallbackQueryData(telegramMessageService.update);
            UserScenarioData? scenarioData = default;
            var userScenarioData = await scenarioService.GetScenarioDataByUserId(telegramMessageService.user.telegramUserId, telegramMessageService.ct);

            if (userScenarioData != null)
            {
                await ScenarioHandler(telegramMessageService, userScenarioData);
                return;
            }

            switch (callbackData.Command)
            {
                case "addTeamTask":
                    if (telegramMessageService.user.role == UserRole.TeamLead)
                        scenarioData = await onCallbackQueryService.AddTeamTask(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "deleteTeamTask":
                    if (telegramMessageService.user.isAdmin)
                        await onCallbackQueryService.DeleteTeamTask(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "showTask":
                    await onCallbackQueryService.ShowTask(telegramMessageService, callbackData);
                    break;
                case "showStage":
                    await onCallbackQueryService.ShowStage(telegramMessageService, callbackData);
                    break;
                case "completeStage":
                    scenarioData = await onCallbackQueryService.CompleteStage(telegramMessageService, callbackData);
                    break;
                case "returnStage":
                    scenarioData = await onCallbackQueryService.ReturnStage(telegramMessageService, callbackData);
                    break;
                case "showUser":
                    await onCallbackQueryService.ShowUser(telegramMessageService, callbackData);
                    break;
                case "deleteUser":
                    if (telegramMessageService.user.isAdmin)
                        scenarioData = await onCallbackQueryService.DeleteUser(telegramMessageService, callbackData);
                    break;
                case "registeredUser":
                    if (telegramMessageService.user.isAdmin)
                        scenarioData = await onCallbackQueryService.RegisteredUser(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "changeUserTeam":
                    if (telegramMessageService.user.isAdmin)
                        scenarioData = await onCallbackQueryService.ChangeUserTeam(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "changeUserRole":
                    if (telegramMessageService.user.isAdmin)
                        scenarioData = await onCallbackQueryService.ChageUserRole(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "showTeam":
                    await onCallbackQueryService.ShowTeam(telegramMessageService, callbackData);
                    break;
                case "deleteTeam":
                    if (telegramMessageService.user.isAdmin)
                        scenarioData = await onCallbackQueryService.DeleteTeam(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "addTeam":
                    scenarioData = await onCallbackQueryService.AddTeam(telegramMessageService, callbackData);
                    break;
                case "renameTeam":
                    if (telegramMessageService.user.isAdmin)
                        scenarioData = await onCallbackQueryService.RenameTeam(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "addProject":
                    if (telegramMessageService.user.isAdmin)
                        scenarioData = await onCallbackQueryService.AddProject(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                case "showProject":
                    if (telegramMessageService.user.project != null)
                        await onCallbackQueryService.ShowProject(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("Вы не являетесь руководителем проекта.");
                    break;
                case "showProjectTasks":
                    if (telegramMessageService.user.project != null)
                        await onCallbackQueryService.ShowProjectTasks(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("Вы не являетесь руководителем проекта.");
                    break;
                case "addTaskInProject":
                    if (telegramMessageService.user.project != null)
                        scenarioData = await onCallbackQueryService.AddTaskInProject(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("Вы не являетесь руководителем проекта.");
                    break;
                case "completeProject":
                    if (telegramMessageService.user.project != null)
                        scenarioData = await onCallbackQueryService.CompleteProject(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("Вы не являетесь руководителем проекта.");
                    break;
                case "deleteProject":
                    if (telegramMessageService.user.isAdmin)
                        scenarioData = await onCallbackQueryService.DeleteProject(telegramMessageService, callbackData);
                    else
                        await telegramMessageService.SendMessage("У вас нет прав администратора.");
                    break;
                default:
                    await telegramMessageService.SendMessage("Введена неверная команда!");
                    break;
            }

            if (scenarioData != null)
            {
                await scenarioService.SaveUserScenarioData(scenarioData, telegramMessageService.ct);
                await ScenarioHandler(telegramMessageService, scenarioData);
            }
        }

        private async Task ScenarioHandler(ITelegramMessageService telegramMessageService, UserScenarioData scenarioData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            IScenario scenario = await scenarioService.GetScenarioByType(scenarioData.scenarioType, telegramMessageService.ct);
            ScenarioStatus currentStatus = await scenario.HandleScanarioAsync(telegramMessageService, scenarioData);

            if (currentStatus == ScenarioStatus.Completed)
                await scenarioService.ResetScenarioByUserTelegramId(scenarioData.userId, telegramMessageService.ct);
        }
    }
}
