using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public class DataContextFactory : IDataContextFactory<ProjectTasksDataContext>
    {
        public DataContextFactory(string SqlConnStr)
        {
            SqlConnectionString = SqlConnStr;
        }
        private readonly string SqlConnectionString;
        public ProjectTasksDataContext CreateDataContext() => new ProjectTasksDataContext(SqlConnectionString);
    }
}
