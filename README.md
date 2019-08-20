# trinug-2019-08-21-workshop

## Fake Your Database

Some of the scariest code to test can sometimes be your database dependency. Rather than let that stop you, let’s walk through some powerful ways to fake your database dependency to quickly and efficiently test code next to it.

In this workshop, we will:

- Create a new ASP .NET Core API project.
- Add an API to get data from a pre-existing database
- Add a service project
- Add a postgres service project
- Connect to a pre-provisioned postgres database
- Add a service test project
- Add provider tests which fake the database.

## Prerequisites

- dotnet core 2.2 SDK or better

## Creating the Database

This was done before the workshop, but for the sake of transparency, these were the resources used:

- http://www.postgresqltutorial.com/postgresql-sample-database/
- http://www.postgresqltutorial.com/load-postgresql-sample-database/
- https://tableplus.io/blog/2018/04/postgresql-how-to-create-read-only-user.html

To run local w/ docker:

```bash
curl -o ~/Downloads/dvdrental.zip http://www.postgresqltutorial.com/wp-content/uploads/2019/05/dvdrental.zip
docker run --rm -it -p 5432:5432 --name dvdrental -v ~/Downloads/dvdrental.tar:/tmp/dvdrental.tar postgres:10
# in another shell
docker exec -it dvdrental /bin/bash
# inside the container
psql -U postgres -c 'CREATE DATABASE dvdrental;'
pg_restore -U postgres -d dvdrental /tmp/dvdrental.tar
```

## Initialize Project

### Clone Workshop Repo

```shell
git clone https://github.com/jhgoodwin/trinug-2019-08-21-workshop.git
cd trinug-2019-08-21-workshop
```

### Create Solution

```shell
dotnet new sln -n DvdRental
```

### Create Web API

```shell
dotnet new webapi -n DvdRental.WebApi
dotnet sln add DvdRental.WebApi
```

#### Optional

```shell
dotnet run --project DvdRental.WebApi
```

(or run in your IDE)

### Add Swashbuckle

```shell
dotnet add DvdRental.WebApi package Swashbuckle.AspNetCore --version 4.0.1
```

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    
    // Register the Swagger generator, defining 1 or more Swagger documents
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dvd Rental API", Version = "v1" });
    });
}
```

```c#
// Startup.cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    // Enable middleware to serve generated Swagger as a JSON endpoint.
    app.UseSwagger();
    
    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
    // specifying the Swagger JSON endpoint.
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dvd Rental API V1");
    });
    
    app.UseMvc();
}
```

Use editor to fix usings or:
```c#
// Startup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
```

#### Optional

Edit [Properties/launchSettings.json](Properties/launchSettings.json)

Change both instances of:

```json
"launchUrl": "api/values",
```

To this:

```json
"launchUrl": "swagger",
```

This makes your code editor launch the swagger page for testing.

## Add an API

We will add a new `Films` API. This will let us interact with the films portion of the pre-created database.

I think it's more fun to make the output code, then work backwards to fill all the gaps. This let's us type things that don't exist, then fix them later.

In the Controllers folder, add `FilmsController.cs`:
```c#
// usings here
namespace DvdRentals.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller])]
    public class FilmsController : ControllerBase
    {
        public FilmsController(IFilmsRepository filmsRepository)
        {
            _filmsRepository = filmsRepository ?? throw new ArgumentNullException(nameof(filmsRepository));
        }

        private IFilmsRepository _filmsRepository;

        [HttpGet("")]
        public ActionResult<IEnumerable<Film>> List()
            => Ok(_filmsRepository.List());

        [HttpGet("{id}")]
        public ActionResult<Film> Get(int id)
            => Ok(_filmsRepository.Get(id));
    }
}
```

At this point, your editor is probably angry about IFilmsRepository and Film.

Let's fix this.

## Create service project

```shell
dotnet new classlib -n DvdRental.Service
dotnet sln add DvdRental.Service
```

## Add Missing Contracts

### Create IFilmsRepository

```csharp
// IFilmsRepository.cs
public interface IFilmsRepository
{
    IEnumerable<Film> List();
    Film Get(int id);
}
```

### Create DTO - Film

```csharp
// Film.cs
public class Film
{
    public int FilmId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int ReleaseYear { get; set; }
}
```

Okay, run the webapi - whoops - notice the startup now needs to know about IFilmsRepository.

Before we hook up to something with moving parts, let's do a simple stub to prove we didn't break too much

### Create StubFilmsRepository

```csharp
// StubFilmsRepository.cs
public class StubFilmsRepository : IFilmsRepository
{
    private List<Film> _films = new List<Film>()
    {
        new Film { FilmId = 1, Title = "Test Film1", Description = "My Film1", ReleaseYear = 2018 },
        new Film { FilmId = 2, Title = "Test Film2", Description = "My Film2", ReleaseYear = 2019 }
    };
    
    // are you able to finish the stub yourself?
}
```

## Bind Service to WebApi

Add the stub to the startup.

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.TryAddScoped<IFilmsRepository, StubFilmsRepository>();
}
```

Now, launch the swagger page. It should show us our stub films.

## Connect to Real Database

### Add PostgresFilmsRepository

```csharp
// PostgresFilmsRepository.cs
public class PostgresFilmsRepository: IFilmsRepository
{
    private readonly DbConnection _dbConnection;

    public PostgresFilmsRepository(DbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException();
    }

    public IEnumerable<Film> List()
    {
        if (_dbConnection.State != ConnectionState.Open) _dbConnection.Open();
        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = "SELECT film_id, title, description, release_year FROM film";
            command.Prepare();
            var result = new List<Film>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(ToFilm(reader));
                }
                return result;
            }
        }
    }
    /* are you able to infer the Get function and ToFilm functions?
     remember the postgres database uses lowercase fields with underscores by convention
     Hint, the old style of creating parameters:
        var pFilmId = command.CreateParameter();
        pFilmId.ParameterName = "film_id";
        pFilmId.Value = id;
        command.Parameters.Add(pFilmId);
    */
}
```

Let's swap the service binding in Startup.cs. Give it a shot.

Okay, run the webapi - whoops - notice the PostgresFilmsRepository needs a DbConnection.

### Add DbConnection

```csharp
// Startup.cs

private static DbConnection CreatePostgresConnection()
    => new NpgsqlConnection("Host=trinug20190821......;Database=dvdrental;Username=reader;Password=trinug");

public void ConfigureServices(IServiceCollection services)
{
    ...
    services.TryAddScoped<IFilmsRepository, PostgresFilmsRepository>();
    services.TryAddScoped<DbConnection>(_ => CreatePostgresConnection());
}
```

Now, run the webapi - whoo! The service works!

But - is it possible to do any testing of this class without a database? Will we have to keep a clean database up, just for testing?

We can easily do things like:

* Get input can't be zero
* Get input becomes a parameter

Try adding this code, now.

This example is so simple, why would we care about writing tests for this?

Reasons:

* Multiple queries that need to run in order.
* Search/Query object which has discrete use cases with rules
* Testing more of the stack, but without a real database - eg, in process web server with client including serialization.

## Add Test Project

```shell
dotnet new xunit -n DvdRental.Tests
dotnet sln add DvdRental.Tests
```

## Fake Database

Add some test nuget packages and the other projects

```shell
dotnet add DvdRental.Tests package Goodwin.John.Fakes.FakeDbProvider
dotnet add DvdRental.Tests package Microsoft.AspNetCore
dotnet add DvdRental.Tests package Microsoft.AspNetCore.Mvc
dotnet add DvdRental.Tests package Microsoft.AspNetCore.TestHost
dotnet add DvdRental.Tests reference DvdRental.Service
dotnet add DvdRental.Tests reference DvdRental.WebApi
```

Inherit your test class from `AbstractFakeDbTest`

Show how to setup the ExecuteReaderAsync

```csharp
// Find a place to put this
public static readonly string[] FilmTableColumnNames = 
{
    "film_id", "title", "description", "release_year"
};
```

```csharp
private HttpClient CreateClient()
{
    var testServer = new TestServer(
        WebHost.CreateDefaultBuilder()
            .UseStartup<Startup>()
            .ConfigureServices(services =>
            {
                // Add service overrides here
                // In WebApi Startup class, use TryAddScoped to only add if not overridden here
                // Example
                // services.AddScoped<YourAbstractOrInterface>(provider => PrivateFunctionHere);
                services.AddScoped<DbConnection>(_ => Connection);
            })
    );
    return testServer.CreateClient();
}

[Fact]
public async Task FilmsRepository_Get_Valid_Success()
{
    var stubResults = new List<object[]>();
    stubResults.Add(new object[]{1, "test title", "test description", 2018});
    ExecuteReaderAsync = (command, behavior, token)
        => Task.FromResult(new FakeDbDataReader(FilmTableColumnNames, stubResults) as DbDataReader);
    var client = CreateClient();
    var url = $"/api/films/1";
    var result = await client.GetAsync(url);
    // code here to verify you got a film
}

[Fact]
public async Task FilmsRepository_Get_Zero_ThrowsArgumentException()
{
    ExecuteReaderAsync = (command, behavior, token)
        => throw new InvalidOperationException("Get should already have thrown");
    var client = CreateClient();
    var url = $"/api/films/0";
    var result = await client.GetAsync(url);
    // code here to verify you got an exception
}
```

## Benefits

This can be used with Dapper (recently verified with Query/QuerySingle)


## Caveats

Code must be compatible with the Db* base classes. - no db provider specific code

Does not eliminate needing to test the real thing - your queries might still be wrong.
