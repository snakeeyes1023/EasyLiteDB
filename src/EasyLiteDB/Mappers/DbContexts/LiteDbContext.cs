using EasyLiteDB.Mappers.Configurations.DbSetConfigurations;
using EasyLiteDB.Models;

namespace EasyLiteDB.Mappers.DbContexts
{
    public abstract class LiteDbContext<T>
    {
        protected readonly string _defaultConnectionString;

        public LiteDbContext(string defaultConnectionString)
        {
            _defaultConnectionString = defaultConnectionString;

            Initialise();
        }

        public void Initialise()
        {
            var configuration = new ConfigureDbSet<T>(this);
            Configure(configuration);
        }


        protected virtual void Configure(IConfigureDbSet<T> configurer)
        {
            var properties = GetType().GetProperties().Where(p => p.PropertyType.GetInterfaces().Any(i => i == typeof(ILiteDbSet)));

            foreach (var prop in properties)
            {
                // signature :       void Initialise(Expression<Func<T, ILiteDbSet>> property, string defaultConnectionString);

                var instance = Activator.CreateInstance(prop.PropertyType) as ILiteDbSet;

                // assign the instance to the property
                prop.SetValue(this, instance);

                // call the Initialise method
                configurer.Initialise(instance!, _defaultConnectionString);
            }
        }

        public LiteDbSet<C> GetCollection<C>() where C : LiteDBEntity
        {
            var properties = GetType().GetProperties().Where(p => p.PropertyType.GetInterfaces().Any(i => i == typeof(ILiteDbSet)));

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(LiteDbSet<C>))
                {
                    return prop.GetValue(this) as LiteDbSet<C> ?? throw new NullReferenceException();
                }
            }

            throw new NullReferenceException();
        }
    }
}
