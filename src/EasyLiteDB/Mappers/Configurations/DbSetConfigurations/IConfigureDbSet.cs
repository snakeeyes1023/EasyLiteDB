using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasyLiteDB.Mappers.Configurations.DbSetConfigurations
{
    public interface IConfigureDbSet<T>
    {
        void Configure(Expression<Func<T, ILiteDbSet>> property, Action<IConfigureField> configure);

        void Initialise(ILiteDbSet property, string defaultConnectionString);
    }
}
