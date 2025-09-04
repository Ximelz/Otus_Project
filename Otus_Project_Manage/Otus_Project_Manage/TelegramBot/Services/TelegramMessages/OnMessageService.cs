using System;
using System.Linq;
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

            
            if (telegramMessageService.user.role == UserRole.TeamLead)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"Создать задачу", $"addTeamTask|addTeamTask"));

            await telegramMessageService.SendMessageWithKeyboard($"Задачи команды {telegramMessageService.user.team.name}:", keyboard);
        }

        public async Task ShowMyTasks(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var tasks = await taskService.GetTasksByUser(telegramMessageService.user, telegramMessageService.ct);
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            if (tasks.Count == 0)
            {
                await telegramMessageService.SendMessage("У вас нет активных задач.");
                return;
            }

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

            await telegramMessageService.SendMessageWithDefaultKeyboard("Вы вышли из консоли администратора.");
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
            await telegramMessageService.SendMessageWithDefaultKeyboard("Данный бот позволяет управлять проектами и его задачами. Каждая задача закрепляется за конкретной командой.");
        }

        public async Task BotHelp(ITelegramMessageService telegramMessageService)
        {
            string help = "Вам доступны следующие команды:\r\n";
            int i = 1;
            switch (telegramMessageService.user.role)
            {
                case UserRole.Tester:
                    help += "/showmytasks - вывод ваших задач.\r\n" +
                            "Кнопка \"Перейти к этапу\" - переход к этапу задачи. Доступна после открытия вашей задачи.\r\n" +
                            "Кнопка \"Выполнить этап\" - выполнение этапа, исполнителем которого вы являетесь. Доступна после открытия вашего этапа.\r\n" +
                            "Кнопка \"Вернуть на доработку\" - возврат задачи на этап для доработки. Доступна после открытия вашего этапа.\r\n";
                    break;
                case UserRole.Developer:
                    help += "/showmytasks - вывод ваших задач.\r\n" +
                            "Кнопка \"Выполнить этап\" - выполнение этапа, исполнителем которого вы являетесь. Доступна после открытия вашего этапа.\r\n";
                    break;
                case UserRole.TeamLead:
                    help += "/showmytasks - вывод ваших задач.\r\n" +
                            "/showteamtasks - вывод задач вашей команды.\r\n" +
                            "Кнопка \"Создать задачу\" - создание задачи для вашей команды.\r\n" +
                            "Кнопка \"Перейти к этапу\" - переход к этапу задачи. Доступна после открытия вашей задачи.\r\n" +
                            "Кнопка \"Выполнить этап\" - выполнение этапа, исполнителем которого вы являетесь. Доступна после открытия вашего этапа. При выполнении последнего этапа задача считается выполненной.\r\n" +
                            "Кнопка \"Вернуть на доработку\" - возврат задачи на этап для доработки. Доступна после открытия вашего этапа.\r\n";
                    break;
                default:
                    help += "/start - начало работы с ботом. Для регистрации обратитесь к администратору бота.\r\n";
                    break;
            }

            if (telegramMessageService.user.project != null)
                help += "/showprojects - вывод активных проектов.\r\n" +
                        "Кнопка \"Доабвить задачи\" - добавление задач или создание новых в проекте.\r\n" +
                        "Кнопка \"Выполнить проект\" - закрытие проекта в связи с его выполнением, а так же входящие в него задачи.\r\n";

            if (telegramMessageService.user.isAdmin)
                help += "/showprojects - вывод активных проектов.\r\n" +
                        "/adminconsole - переход в админскую консоль.\r\n" +
                        "/registerUser - вывод незарегистрированных пользователей.\r\n" +
                        "/showUsers - вывод всех пользователей.\r\n" +
                        "/showTeams - вывод всех команд пользователей.\r\n" +
                        "/exitAdminConsole - выход из админской консоли.\r\n" +
                        "Кнопка \"Добавить команду\" при открытии команд пользователей - добавление новых команд пользователей.\r\n" +
                        "Кнопка \"Удалить\" при открытии команды - удаление команды пользователей.\r\n" +
                        "Кнопка \"Переименовать\" при открытии команды - переименование команды пользователей.\r\n" +
                        "Кнопка \"Удалить\" при открытии пользователя - удаление пользователя.\r\n" +
                        "Кнопка \"Изменить роль\" при открытии пользователя - изменение роли пользователя в команде.\r\n" +
                        "Кнопка \"Изменить команду\" при открытии пользователя - перенос пользователя в другую команду.\r\n" +
                        "Кнопка \"Зарегистрировать\" при открытии пользователя - регистрация нового пользователя.\r\n" +
                        "Кнопка \"Удалить\" при открытии пользователя - удаление пользователя.\r\n" +
                        "Кнопка \"Удалить\" при открытии задачи - удаление задачи проекта.\r\n" + 
                        "Кнопка \"Удалить\" при открытии проекта - удаление проекта без задач.\r\n";

            help += "/help - информация о доступных командах бота.\r\n" +
                    "/info - информация о боте.";

            await telegramMessageService.SendMessageWithDefaultKeyboard(help);
        }

        public async Task ShowProjects(ITelegramMessageService telegramMessageService)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            var projects = (await projectService.GetActiveProjects(telegramMessageService.ct)).Where(x => x.projectManager.userId == telegramMessageService.user.userId).ToList();

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            foreach (var project in projects)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{project.name}", $"showProject|{project.projectId}"));

            if (telegramMessageService.user.isAdmin)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Добавить проект", $"addProject|{telegramMessageService.user.userId}"));

            await telegramMessageService.SendMessageWithKeyboard("Активные проекты:", keyboard);
        }

        public async Task ShowAllProjects(ITelegramMessageService telegramMessageService)
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
