using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Otus_Project_Manage
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Введите пароль от БД:");
            string inputStr = Console.ReadLine();
            string sqlConn = $"Host=localhost;Database=TelegramBotProjectManager;Username=postgres;Password={inputStr};Port=5432";

            var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User) ?? throw new ArgumentException("Отсутствует токен телеграмм бота!");
            var bot = new TelegramBotClient(token);

            IDataContextFactory<ProjectTasksDataContext> factory = new DataContextFactory(sqlConn);
            ITaskRepository taskRepository = new SqlTaskRepoository(factory);
            IUserRepository userRepository = new SqlUserRepository(factory);
            ITeamRepository teamRepository = new SqlTeamRepository(userRepository, factory);
            IProjectRepository projectRepository = new SqlProjectRepository(factory);
            IScenarioRepository scenarioRepository = new ScenarioInMemoryRepository();

            ITaskService taskService = new TaskService(taskRepository);
            IUserService userService = new UserService(userRepository);
            ITeamService teamService = new TeamService(teamRepository);
            IProjectService projectService = new ProjectService(projectRepository);
            IScenarioService scenarioService = new ScenarioService(scenarioRepository);
            

            var handle = new UpdateHandler(userService, teamService, scenarioService, taskService, projectService);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                DropPendingUpdates = true
            };

            using (CancellationTokenSource ct = new CancellationTokenSource())
            {
                await scenarioRepository.AddScenario(new AddTeamScenario(userService, teamService), ct.Token);
                await scenarioRepository.AddScenario(new DeleteTeamScenario(userService, teamService), ct.Token);
                await scenarioRepository.AddScenario(new RenameTeamScenario(teamService), ct.Token);

                await scenarioRepository.AddScenario(new ChangeUserRoleScenario(userService), ct.Token);
                await scenarioRepository.AddScenario(new ChangeUserTeamScenario(userService, teamService), ct.Token);
                await scenarioRepository.AddScenario(new DeleteUserScenario(userService), ct.Token);
                await scenarioRepository.AddScenario(new RegisteredUserScenario(userService, teamService), ct.Token);

                await scenarioRepository.AddScenario(new AddTaskScenario(taskService), ct.Token);
                await scenarioRepository.AddScenario(new DeleteTaskScenario(taskService), ct.Token);
                await scenarioRepository.AddScenario(new CompleteStageScenario(taskService), ct.Token);
                await scenarioRepository.AddScenario(new ReturnStageScenario(taskService), ct.Token);

                await scenarioRepository.AddScenario(new AddProjectScenario(projectService, userService), ct.Token);
                await scenarioRepository.AddScenario(new DeleteProjectScenario(projectService), ct.Token);
                await scenarioRepository.AddScenario(new AddTaskInProjectScenario(projectService, taskService), ct.Token);
                await scenarioRepository.AddScenario(new CompleteProjectScenario(projectService, taskService), ct.Token);
                var me = await bot.GetMe();
                bot.StartReceiving(handle, receiverOptions, ct.Token);

                while (!ct.Token.IsCancellationRequested)
                {                    
                    Console.WriteLine("Нажмите \"Del\" чтобы открыть консоль администратора.");
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Delete)
                    {
                        var adminConsole = new AdminConsoleInTerminal(userService, teamService, scenarioService);
                        await adminConsole.StartAdminConsoleInTerminal(ct);
                    }
                    else
                        Console.WriteLine($"\nБот {me.FirstName} запущен");
                }
            }
        }
    }
}