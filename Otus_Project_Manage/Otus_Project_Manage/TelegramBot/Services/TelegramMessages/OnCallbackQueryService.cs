using System;
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
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Изменить роль", $"changeUserRole|{showUser.userId}"));
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData("Изменить команду", $"changeUserTeam|{showUser.userId}"));
            }

            await telegramMessageService.SendMessageWithKeyboard(
                        $"Информация о пользователе {showUser.userName}:\r\n" +
                        $"Команда: {showUser.team.name}.\r\n" +
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
                keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData($"{teamUser.userName}", $"showUser|{teamUser.userId}"));


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
                    $"Перейти к этапу \"{showTask.firstStage.name}\"",
                    $"showStep|{showTask.taskId} {showTask.firstStage.stageId}"));

            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData(
                    $"Перейти к этапу \"{showTask.firstStage.nextStage.name}\"",
                    $"showStep|{showTask.taskId} {showTask.firstStage.nextStage.stageId}"));

            keyboard.AddNewRow(InlineKeyboardButton.WithCallbackData(
                    $"Перейти к этапу \"{showTask.firstStage.nextStage.nextStage.name}\"",
                    $"showStep|{showTask.taskId} {showTask.firstStage.nextStage.nextStage.stageId}"));

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

        public async Task ShowStep(ITelegramMessageService telegramMessageService, CallbackQueryData callbackData)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            string[] arguments = callbackData.Argument.Split();

            if (arguments.Length != 2)
                throw new ArgumentException($"Получен неверный аргумент \"{callbackData.Argument}\" для команды \"showStep\".");

            if (!Guid.TryParse(arguments[0], out Guid taskId))
                throw new ArgumentException($"Не получилось преобразовать в Guid строку \"{arguments[0]}\"");


            if (!Guid.TryParse(arguments[1], out Guid stageId))
                throw new ArgumentException($"Не получилось преобразовать в Guid строку \"{arguments[1]}\"");

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
            var task = await taskService.GetTasksById(taskId, telegramMessageService.ct);

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
                    if (telegramMessageService.user.role != UserRole.Developer)
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
    }
}
