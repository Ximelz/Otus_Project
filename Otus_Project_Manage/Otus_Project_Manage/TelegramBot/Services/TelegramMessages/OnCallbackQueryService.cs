using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class OnCallbackQueryService
    {
        public OnCallbackQueryService(IUserService userService, ITeamService teamService, ITaskService taskService, IProjectService projectService)
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

        public async Task ShowUser(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (!Guid.TryParse(callbackData.Argument, out Guid guidShowUser))
                throw new ArgumentException($"Не получилось преобразовать в Guid строку \"{callbackData.Argument}\"");

            var showUser = await userService.GetUserByUserId(guidShowUser, telegramMessageService.ct);

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            if (telegramMessageService.user.isAdmin)
            {
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Удалить", $"deleteUser|{showUser.userId}"));
                if (showUser.role != UserRole.None)
                {
                    keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Изменить роль", $"changeUserRole|{showUser.userId}"));
                    keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Изменить команду", $"changeUserTeam|{showUser.userId}"));
                }
                else
                    keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Зарегистрировать", $"registeredUser|{showUser.userId}"));
            }

            var teamName = showUser.team == null ? "Нет" : showUser.team.name;

            await telegramMessageService.SendMessageWithKeyboard(
                        $"Информация о пользователе {showUser.userName}:\r\n" +
                        $"Команда: {teamName}.\r\n" +
                        $"Роль: {showUser.role}.",
                        keyboard);
        }

        public async Task<UserScenarioData> DeleteUser(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.DeleteUser);
        }

        public async Task<UserScenarioData> ChangeUserTeam(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.ChangeUserTeam);
        }

        public async Task<UserScenarioData> ChageUserRole(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.ChangeUserRole);
        }

        public async Task ShowTeam(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (!Guid.TryParse(callbackData.Argument, out Guid guidShowTeam))
                throw new ArgumentException($"Не получилось преобразовать в Guid строку \"{callbackData.Argument}\"");

            var showTeam = await teamService.GetTeamById(guidShowTeam, telegramMessageService.ct);

            var teamUsers = await userService.GetUsersByTeam(guidShowTeam, telegramMessageService.ct);

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            foreach (var teamUser in teamUsers)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{teamUser.userName}: {teamUser.role}", $"showUser|{teamUser.userId}"));

            if (telegramMessageService.user.isAdmin)
            {
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"Удалить", $"deleteTeam|{guidShowTeam}"));
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"Переименовать", $"renameTeam|{guidShowTeam}"));
             }

            await telegramMessageService.SendMessageWithKeyboard(
                        $"Состав коменды {showTeam.name}:",
                        keyboard);
        }

        public async Task<UserScenarioData> DeleteTeam(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.DeleteTeam);
        }

        public async Task<UserScenarioData> RenameTeam(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.RenameTeam);
        }

        public async Task<UserScenarioData> AddTeam(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.AddTeam);
        }

        public async Task<UserScenarioData> AddTeamTask(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.AddTask);
        }

        public async Task<UserScenarioData> DeleteTeamTask(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.DeleteTask);
        }

        public async Task ShowTask(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (!Guid.TryParse(callbackData.Argument, out Guid taskId))
                throw new ArgumentException($"Не получилось преобразовать в Guid строку \"{callbackData.Argument}\"");

            var showTask = await taskService.GetTasksById(taskId, telegramMessageService.ct);

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData(
                    $"Этап \"{showTask.firstStage.name}\"",
                    $"showStage|{showTask.taskId} 1"));

            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData(
                    $"Этап \"{showTask.firstStage.nextStage.name}\"",
                    $"showStage|{showTask.taskId} 2"));

            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData(
                    $"Этап \"{showTask.firstStage.nextStage.nextStage.name}\"",
                    $"showStage|{showTask.taskId} 3"));

            if (telegramMessageService.user.isAdmin)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Удалить", $"deleteTask|{showTask.taskId}"));

            await telegramMessageService.SendMessageWithKeyboard(
                        $"Информация о задаче {showTask.taskName}:\r\n" +
                        $"Описание: {showTask.description}.\r\n" +
                        $"Дата создания: {showTask.createdAt}.\r\n" +
                        $"Срок выполнения: {showTask.deadline}.\r\n" +
                        $"Текущий этап: {showTask.activeStage.name}",
                        keyboard);
        }

        public async Task ShowStage(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            string[] arguments = callbackData.Argument.Split();

            if (arguments.Length != 2)
                throw new ArgumentException($"Получен неверный аргумент \"{callbackData.Argument}\" для команды \"showStep\".");

            if (!Guid.TryParse(arguments[0], out Guid taskId))
                throw new ArgumentException($"Не получилось преобразовать в Guid строку \"{arguments[0]}\"");

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            var task = await taskService.GetTasksById(taskId, telegramMessageService.ct);

            int currentIndex = int.Parse(arguments[1]);
            TaskStage stage = task.firstStage;

            for (int i = 1; i < currentIndex; i++)
                stage = stage.nextStage;

            Guid stageId = stage.stageId;

            var currentStage = task.firstStage;

            for (int i = 0; i < 3; i++)
            {
                if (stageId == currentStage.stageId)
                    break;
                currentStage = currentStage.nextStage;
            }

            if (task.activeStage.stageId == stageId)
                if (telegramMessageService.user.userId == task.activeStage.user.userId)
                {
                    keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Выполнить", $"completeStage|{task.taskId}"));
                    if (task.activeStage.stageId != task.firstStage.stageId)
                        keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Вернуть на доработку", $"returnStage|{task.taskId}"));
                }

            await telegramMessageService.SendMessageWithKeyboard(
                        $"Информация об этапе {currentStage.name}:\r\n" +
                        $"Описание: {currentStage.description}.\r\n" +
                        $"Комментарий: {currentStage.comment}\r\n" +
                        $"Статус: {currentStage.status}\r\n" +
                        $"Исполнитель: {currentStage.user.userName}",
                        keyboard);
        }

        public async Task<UserScenarioData> CompleteStage(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.CompleteStage);
        }

        public async Task<UserScenarioData> ReturnStage(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.ReturnStage);
        }

        public async Task<UserScenarioData> AddProject(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.AddProject);
        }

        public async Task ShowProject(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (!Guid.TryParse(callbackData.Argument, out Guid projectId))
                throw new ArgumentException($"Не получилось преобразовать в Guid строку \"{callbackData.Argument}\"");

            var project = await projectService.GetProjectById(projectId, telegramMessageService.ct);

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Посмотреть задачи", $"showProjectTasks|{projectId}"));
            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Доабвить задачи", $"addTaskInProject|{projectId}"));
            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Выполнить проект", $"completeProject|{projectId}"));

            if (telegramMessageService.user.isAdmin)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Удалить", $"deleteProject|{projectId}"));

            await telegramMessageService.SendMessageWithKeyboard(
                        $"Информация о проекте {project.name}:\r\n" +
                        $"Описание: {project.description}.\r\n" +
                        $"Руководитель проекта: {project.projectManager.userName}.\r\n" +
                        $"Сдеть до: {project.deadline}.",
                        keyboard);
        }

        public async Task<UserScenarioData> AddTaskInProject(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.AddTaskInProject);
        }

        public async Task<UserScenarioData> CompleteProject(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.CompleteProject);
        }

        public async Task<UserScenarioData> DeleteProject(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.DeleteProject);
        }

        public async Task<UserScenarioData> RegisteredUser(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            await telegramMessageService.SendMessageByKeyboardType(
                            "Если хотите отменить удаление выберите команду \"/cancel\"",
                            KeyboardTypes.CancelScenario);

            return new UserScenarioData(telegramMessageService.user.telegramUserId, ScenarioTypes.RegisteredUser);
        }

        public async Task ShowProjectTasks(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (!Guid.TryParse(callbackData.Argument, out Guid projectId))
                throw new ArgumentException($"Не получилось преобразовать в Guid строку \"{callbackData.Argument}\"");

            var project = await projectService.GetProjectById(projectId, telegramMessageService.ct);
            var tasks = (await taskService.GetAllActiveTasks(telegramMessageService.ct)).Where(x => x.project.projectId == projectId).ToList();

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            if (tasks.Count == 0)
            {
                await telegramMessageService.SendMessage("Активных задач в вашем проекте нет.");
                return;
            }

            foreach (var task in tasks)
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{task.taskName}", $"showTask|{task.taskId}"));

            await telegramMessageService.SendMessageWithKeyboard("Активные задачи вашего проекта:", keyboard);
        }
    }
}
