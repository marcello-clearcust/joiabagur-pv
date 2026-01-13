using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Domain.Interfaces.Services;
using JoiabagurPV.Infrastructure.Data;
using JoiabagurPV.Infrastructure.Data.Repositories;
using JoiabagurPV.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace JoiabagurPV.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection with infrastructure services added.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    // Configure connection pooling for free-tier optimization
                    npgsqlOptions.CommandTimeout(30); // 30 second timeout
                });

            // Configure connection string with pooling parameters
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                MaxPoolSize = 10,  // Max 10 connections for free-tier
                MinPoolSize = 1,   // Min 1 connection
                ConnectionIdleLifetime = 60, // 1 minute
                ConnectionPruningInterval = 10 // Check every 10 seconds
            };

            options.UseNpgsql(builder.ConnectionString);

            // Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserPointOfSaleRepository, UserPointOfSaleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPointOfSaleRepository, PointOfSaleRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IPointOfSalePaymentMethodRepository, PointOfSalePaymentMethodRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductPhotoRepository, ProductPhotoRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISalePhotoRepository, SalePhotoRepository>();
        services.AddScoped<IModelMetadataRepository, ModelMetadataRepository>();
        services.AddScoped<IModelTrainingJobRepository, ModelTrainingJobRepository>();

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register database seeder
        services.AddScoped<DatabaseSeeder>();

        // Register file storage service based on configuration
        var storageProvider = configuration["FileStorage:Provider"]?.ToLowerInvariant() ?? "local";
        if (storageProvider == "cloud")
        {
            services.AddScoped<IFileStorageService, CloudFileStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        return services;
    }
}