using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus_Project_Manage
{
    public interface IDataContextFactory<TDataContext> where TDataContext : DataConnection
    {
        TDataContext CreateDataContext();
    }
}
