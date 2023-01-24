# Easy-LiteDB

Easy-LiteDB C# is a simple wrapper for LiteDB. Allow you to track changes and save changes to the database. It is designed to be easy to use and easy to understand. It is not designed to be the fastest or most efficient. User with enitity framework experience will find this library very easy to use.

## Installation

Use the package manager to install [Easy-LiteDB](https://www.nuget.org/packages/LiteDB)

```bash
dotnet add package LiteDB --version 5.0.15
```

## Getting started

1. Create a class that inherits from LiteDBEntity. This class will be used to represent the data in the database.

```csharp
public class Person : LiteDBEntity
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

2. Create a class that inherits from LiteDBContext. This class will be used to represent the database.

```csharp
public class MyContext : LiteDBContext<Mycontext>
{
    public MyContext() : base("Filename=mycontext.db;Connection=shared"){}

    public LiteDBSet<Person> Peoples { get; private set; }
}
```

3. Create an instance of the context and use it to access the database.

```csharp
MyContext context = new MyContext();
var peoples = context.Peoples.ToList();
```

### Basic Usage (Crud)

#### Create

```csharp
MyContext context = new MyContext();

// Create
Person person = new Person { Name = "John", Age = 30 };
context.Peoples.Add(person);

// save changes
context.SaveChanges();
```

#### Read

```csharp
MyContext context = new MyContext();

// Read
List<Person> peoples = context.Peoples.ToList();
```

#### Update

```csharp
MyContext context = new MyContext();

// Update
Person person = context.Peoples.FirstOrDefault();
person.Name = "John Doe";
person.SetDirty();

// save changes
context.Peoples.SaveChanges();
```

#### Delete

```csharp
// not implemented yet
```

### Advanced Usage

#### Configuration

Use custom connection string for each DbSet. This is useful when you want to use different database file for each DbSet. default connection string will be used if not specified.

```csharp
public class MyContext : LiteDBContext<Mycontext>
{
    public MyContext() : base("Filename=mycontext.db;Connection=shared"){}

    public LiteDBSet<Person> Peoples { get; private set; }

    protected override void Configure(IConfigureDbSet<ShopDbContext> configurer)
    {
        base.Configure(configurer);

        configurer.Configure(x => x.Peoples, x =>
        {
            x.UseCustomConnectionString("Filename=peoples.db;Connection=shared");
        });
    }
}
```

#### Query

```csharp
MyContext context = new MyContext();

// Client side query (Linq)
List<Person> peoples = context.Peoples.Where(x => x.Age > 30)
.ToList();

// Server side query (LiteDb query)
List<Person> peoples = context.Peoples
    .UseQuery(query =>
    {
        query = query.Where(x => x._id != null);
    }).ToList()
```


## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

[MIT](https://choosealicense.com/licenses/mit/)

