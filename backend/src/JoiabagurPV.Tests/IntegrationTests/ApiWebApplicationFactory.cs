using JoiabagurPV.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Uses Testcontainers for PostgreSQL database.
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;

    public ApiWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set configuration values BEFORE services are configured
        builder.UseSetting("Jwt:SecretKey", "TestSecretKeyForIntegrationTestingThatIsLongEnough12345!");
        builder.UseSetting("Jwt:Issuer", "TestIssuer");
        builder.UseSetting("Jwt:Audience", "TestAudience");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "60");
        builder.UseSetting("Jwt:RefreshTokenExpirationHours", "8");
        builder.UseSetting("Testing:SkipSwagger", "true");
        
        // Set the connection string for the test database
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration to ensure we use the test connection string
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using test container connection string
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Remove Swashbuckle assemblies from application parts to avoid .NET 10 compatibility issues
            var partManager = services.BuildServiceProvider().GetService<ApplicationPartManager>();
            if (partManager != null)
            {
                var partsToRemove = partManager.ApplicationParts
                    .Where(p => p.Name.Contains("Swashbuckle", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                foreach (var part in partsToRemove)
                {
                    partManager.ApplicationParts.Remove(part);
                }
            }
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgresContainer.StopAsync();
    }

    /// <summary>
    /// Creates a scope and returns the ApplicationDbContext.
    /// </summary>
    public ApplicationDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Resets the database by truncating all tables.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Delete all data and reseed
        await context.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE ""RefreshTokens"" CASCADE;
            TRUNCATE TABLE ""PointOfSalePaymentMethods"" CASCADE;
            TRUNCATE TABLE ""UserPointOfSales"" CASCADE;
            TRUNCATE TABLE ""PaymentMethods"" CASCADE;
            TRUNCATE TABLE ""PointOfSales"" CASCADE;
            TRUNCATE TABLE ""Users"" CASCADE;
        ");

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
