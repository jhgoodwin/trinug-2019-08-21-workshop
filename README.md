# trinug-2019-08-21-workshop

## Fake Your Database

Some of the scariest code to test can sometimes be your database dependency. Rather than let that stop you, let’s walk through some powerful ways to fake your database dependency to quickly and efficiently test code next to it.

In this workshop, we will:

* Create a new ASP .NET Core API project.
* Add an API to get data from a pre-existing database
* Add a service project
* Add a model project
* Add a postgres service project
* Connect to a pre-provisioned postgres database
* Add a test project
* Add provider tests which fake the database.

## Prerequisites

* dotnet core 2.2 SDK or better

## Creating the Database

This was done before the workshop, but for the sake of transparency, these were the resources used:

* http://www.postgresqltutorial.com/postgresql-sample-database/
* http://www.postgresqltutorial.com/load-postgresql-sample-database/
* https://tableplus.io/blog/2018/04/postgresql-how-to-create-read-only-user.html

The exact scripts will be added to [./data](./data)

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
