using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApplication;
using ProjectManagementApplication.Data;
using Testcontainers.MsSql;

public class ProjectManagementApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MsSqlContainer? _container;
    private string? _testDbName;
    private string? _testDbConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, conf) =>
        {
            var dict = new Dictionary<string, string> { ["SeedDemoData"] = "false" };
            conf.AddInMemoryCollection(dict);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            if (string.IsNullOrEmpty(_testDbConnectionString))
                throw new InvalidOperationException("Test database connection string not created.");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_testDbConnectionString);
            });

            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                options.DefaultForbidScheme = TestAuthHandler.Scheme;
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
        });
    }

    public async Task InitializeAsync()
    {
            _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Your_strong_password_123!")
            .WithCleanUp(true)
            .Build();

        await _container.StartAsync();

        _testDbName = "testdb_" + Guid.NewGuid().ToString("N");

        var masterBuilder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "master",
            TrustServerCertificate = true
        };

        using (var conn = new SqlConnection(masterBuilder.ConnectionString))
        {
            await conn.OpenAsync();

            var cmdText = $@"
IF DB_ID(N'{_testDbName}') IS NOT NULL
BEGIN
    ALTER DATABASE [{_testDbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{_testDbName}];
END
CREATE DATABASE [{_testDbName}];
";
            using var cmd = new SqlCommand(cmdText, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        var testBuilder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = _testDbName,
            TrustServerCertificate = true
        };
        _testDbConnectionString = testBuilder.ConnectionString;
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_testDbName) && _container != null)
            {
                var masterBuilder = new SqlConnectionStringBuilder(_container.GetConnectionString())
                {
                    InitialCatalog = "master",
                    TrustServerCertificate = true
                };

                using var conn = new SqlConnection(masterBuilder.ConnectionString);
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
IF DB_ID(N'{_testDbName}') IS NOT NULL
BEGIN
    ALTER DATABASE [{_testDbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{_testDbName}];
END
";
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch
        {

        }

        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
            _container = null;
        }
    }
}
