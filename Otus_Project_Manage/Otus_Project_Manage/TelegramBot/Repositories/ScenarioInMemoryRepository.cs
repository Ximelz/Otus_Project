using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class ScenarioInMemoryRepository : IScenarioRepository
    {
        public ScenarioInMemoryRepository()
        {
            scenarios = new List<IScenario>();
            userScenarioDatas = new List<UserScenarioData>();
        }
        private readonly List<IScenario> scenarios;
        private readonly List<UserScenarioData> userScenarioDatas;

        public Task<IScenario> GetScenario(Func<IScenario, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return Task.FromResult(scenarios.Where(predicate).FirstOrDefault());
        }

        public Task AddScenario(IScenario scenario, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (scenarios.Where(x => x.ScenarioType == scenario.ScenarioType).Any())
                throw new ArgumentException($"Сценарий {scenario.ScenarioType} уже добавлен!");

            scenarios.Add(scenario);

            return Task.CompletedTask;
        }

        public Task<UserScenarioData> GetUserScenarioData(Func<UserScenarioData, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return Task.FromResult(userScenarioDatas.Where(predicate).FirstOrDefault());
        }

        public Task SaveUserScenarioData(UserScenarioData userScenarioData, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (userScenarioDatas.Where(x => x.userId == userScenarioData.userId).Any())
                throw new ArgumentException($"Пользователь с telegram id {userScenarioData.userId} уже выполняет сценарий команды!");

            userScenarioDatas.Add(userScenarioData);

            return Task.CompletedTask;
        }

        public Task DeleteUserScenarioData(Func<UserScenarioData, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var userScenarioData = userScenarioDatas.Where(predicate).FirstOrDefault();

            if (userScenarioData == null)
                throw new ArgumentException("Данные о данных сценария пользователя не найдены по указанной выборке!");

            userScenarioDatas.Remove(userScenarioData);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<UserScenarioData>?> GetUserScenarioDatas(Func<UserScenarioData, bool> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return Task.FromResult((IReadOnlyList<UserScenarioData>)userScenarioDatas.Where(predicate).ToList());
        }
    }
}
