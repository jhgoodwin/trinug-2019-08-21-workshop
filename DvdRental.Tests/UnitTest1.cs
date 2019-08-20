using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Http;
using System.Threading.Tasks;
using DvdRental.WebApi;
using Goodwin.John.Fakes.FakeDbProvider;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DvdRental.Tests
{
    public class UnitTest1: AbstractFakeDbTest
    {
        public static readonly string[] FilmTableColumnNames = 
        {
            "film_id", "title", "description", "release_year"
        };
        
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
            stubResults.Add(new object[] {1, "test title", "test description", 2018});
            ExecuteReaderAsync = (command, behavior, token)
                => Task.FromResult(new FakeDbDataReader(FilmTableColumnNames, stubResults) as DbDataReader);
            var client = CreateClient();
            var url = $"/api/Films/1";
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
    }
}
