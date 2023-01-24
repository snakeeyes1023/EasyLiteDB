using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLiteDB.Mappers.Configurations.DbSetConfigurations
{
    internal class ConfigureField : IConfigureField
    {
        private readonly ILiteDbSet _dbSet;

        internal ConfigureField(ILiteDbSet dbSet)
        {
            _dbSet = dbSet;
        }

        public void UseCustomConnectionString(string connectionString)
        {
            _dbSet.ConnectionStrings = connectionString;
        }
    }
}
