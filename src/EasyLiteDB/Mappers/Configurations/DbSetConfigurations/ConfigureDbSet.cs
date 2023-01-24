using EasyLiteDB.Mappers.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyLiteDB.Mappers.Configurations.DbSetConfigurations
{
    internal class ConfigureDbSet<T> : IConfigureDbSet<T>
    {
        private readonly LiteDbContext<T> _instance;
        public ConfigureDbSet(LiteDbContext<T> instance)
        {
            _instance = instance;
        }

        public void Configure(Expression<Func<T, ILiteDbSet>> property, Action<IConfigureField> configure)
        {
            // Récupération de la propriété à partir de l'expression lambda
            var propertyInfo = (property.Body as MemberExpression)?.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("L'expression lambda doit être une propriété", nameof(property));

            // get the property as ILiteDbSet
            var dbSet = propertyInfo.GetValue(_instance) as ILiteDbSet;

            if (dbSet == null)
            {
                //instanciate the property as ILiteDbSet
                dbSet = Activator.CreateInstance(propertyInfo.PropertyType) as ILiteDbSet;

                //set the property as ILiteDbSet
                propertyInfo.SetValue(_instance, dbSet);
            }


            ConfigureField config = new ConfigureField(dbSet ?? throw new InvalidOperationException());

            configure.Invoke(config);
        }

        public void Initialise(ILiteDbSet property, string defaultConnectionString)
        {
            property.ConnectionStrings = defaultConnectionString;
        }
    }
}
