using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class ScenarioService : IScenarioService
    {
        public ScenarioService(IScenarioRepository scenarioRep)
        {
            this.scenarioRep = scenarioRep;
        }
        private readonly IScenarioRepository scenarioRep;

        public async Task<IScenario> GetScenarioByType(ScenarioTypes scenarioType, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await scenarioRep.GetScenario(x => x.ScenarioType == scenarioType, ct);
        }

        public async Task<UserScenarioData> GetScenarioDataByUserId(long telegramUserId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await scenarioRep.GetUserScenarioData(x => x.userId == telegramUserId, ct);
        }

        public async Task SaveUserScenarioData(UserScenarioData userScenarioData, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await scenarioRep.SaveUserScenarioData(userScenarioData, ct);
        }

        public async Task ResetScenarioByUserTelegramId(long telegramUserId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await scenarioRep.DeleteUserScenarioData(x => x.userId == telegramUserId, ct);
        }

        public async Task<IReadOnlyList<UserScenarioData>> GetAllScenatiosData(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return await scenarioRep.GetUserScenarioDatas(x => x.userId != null, ct);
        }
    }
}
