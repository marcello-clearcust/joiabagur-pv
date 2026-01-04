using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Infrastructure.Data;
using JoiabagurPV.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Database fixture for integration tests using Testcontainers.
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private IServiceProvider? _serviceProvider;
    private IServiceScopeFactory? _scopeFactory;
    private string? _connectionString;

    public TestDatabaseFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString => _connectionString!;

    public IUnitOfWork UnitOfWork => _serviceProvider!.GetRequiredService<IUnitOfWork>();

    public ApplicationDbContext DbContext => _serviceProvider!.GetRequiredService<ApplicationDbContext>();

    public IServiceScopeFactory ScopeFactory => _scopeFactory!;

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        _connectionString = _postgresContainer.GetConnectionString();

        // Setup DI container for tests
        var services = new ServiceCollection();

        // Configure test configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString
            })
            .Build();

        services.AddInfrastructure(configuration);

        _serviceProvider = services.BuildServiceProvider();
        _scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        // Apply migrations
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}