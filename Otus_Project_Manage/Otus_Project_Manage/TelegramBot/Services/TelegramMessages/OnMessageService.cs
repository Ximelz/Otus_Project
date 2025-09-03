using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class OnMessageService
    {
        public OnMessageService(IUserService userService, ITeamService teamService, ITaskService taskService, IProjectService projectService)
        {
            this.userService = userService;
            this.teamService = teamService;
            this.taskService = taskService;
            this.projectService = projectService;
        }
        private readonly IUserService userService;
        private readonly ITeamService teamService;
        private readonly ITaskService taskService;
        private readonly IProjectService projectService;

        public async Task ShowTeamTasks(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var tasks = await taskService.GetActiveTasksByTeam(telegramMessageService.user.team, telegramMessageService.ct);
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            foreach (var task in tasks)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{task.taskName}", $"showTask|{task.taskId}"));

            await telegramMessageService.SendMessageWithKeyboard($"Задачи команды {telegramMessageService.user.team.name}:", keyboard);
        }

        public async Task ShowMyTasks(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var tasks = await taskService.GetTasksByUser(telegramMessageService.user, telegramMessageService.ct);
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            foreach (var task in tasks)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{task.taskName}", $"showTask|{task.taskId}"));

            await telegramMessageService.SendMessageWithKeyboard($"Мои задачи:", keyboard);
        }

        public async Task AdminConsole(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Вы вошли в консоль администратора. Для управления используйте кнопки под строкой ввода!",
                            KeyboardTypes.Admin);
        }

        public async Task RegisterUser(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var users = await userService.GetUsersByRole(UserRole.None, telegramMessageService.ct);
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            foreach (var user in users)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{user.userName}", $"showUser|{user.userId}"));

            await telegramMessageService.SendMessageWithKeyboard("Незарегистрированные пользователи:", keyboard);
        }

        public async Task ShowUsers(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var users = await userService.GetRegisteredUsers(telegramMessageService.ct);
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            foreach (var user in users)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{user.userName}", $"showUser|{user.userId}"));

            await telegramMessageService.SendMessageWithKeyboard("Зарегистрированные пользователи:", keyboard);
        }

        public async Task ShowTeams(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var teams = await teamService.GetAllTeams(telegramMessageService.ct);
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            foreach (var team in teams)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{team.name}", $"showTeam|{team.teamId}"));

            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Добавить команду", $"addTeam|{telegramMessageService.user.userName}"));

            await telegramMessageService.SendMessageWithKeyboard("Активные команды разработчиков:", keyboard);
        }

        public async Task ExitAdminConsole(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.bot.SetMyCommands(
                            KeyboardCommands.GetUserCommands(telegramMessageService.user),
                            BotCommandScope.Chat(telegramMessageService.update.Message.Chat.Id));
        }

        public async Task StartBot(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            telegramMessageService.user.telegramUserId = telegramMessageService.update.Message.From.Id;
            telegramMessageService.user.userName = telegramMessageService.update.Message.From.Username;

            await userService.AddUser(telegramMessageService.user, telegramMessageService.ct);
            await telegramMessageService.SendMessageWithDefaultKeyboard("Вы авторизовались! Попросите администратора вас зарегистировать.");
        }

        public async Task BotInfo(ITelegramMessageService telegramMessageService)
        {
            await telegramMessageService.SendMessageWithDefaultKeyboard("Info");
        }

        public async Task BotHelp(ITelegramMessageService telegramMessageService)
        {
            await telegramMessageService.SendMessageWithDefaultKeyboard("Help");
        }

        public async Task ShowProjects(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var projects = await projectService.GetActiveProjects(telegramMessageService.ct);

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            foreach (var project in projects)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{project.name}", $"showProject|{project.projectId}"));

            if (telegramMessageService.user.isAdmin)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Добавить проект", $"addProject|{telegramMessageService.user.userId}"));

            await telegramMessageService.SendMessageWithKeyboard("Активные проекты:", keyboard);
        }
    }
}
