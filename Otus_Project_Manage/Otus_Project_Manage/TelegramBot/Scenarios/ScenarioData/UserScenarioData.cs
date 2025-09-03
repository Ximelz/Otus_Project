using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class UserScenarioData
    {
        public UserScenarioData(long userId, ScenarioTypes scenarioType)
        {
            this.userId = userId;
            scenarioId = Guid.NewGuid();
            currentStep = "Start";
            scenarioStatus = ScenarioStatus.Start;
            this.scenarioType = scenarioType;
            Data = new Dictionary<string, object>();
        }
        public long userId;
        public ScenarioStatus scenarioStatus;
        public ScenarioTypes scenarioType;
        public string currentStep;
        public Guid scenarioId;
        public Dictionary<string, object> Data;
    }
}
