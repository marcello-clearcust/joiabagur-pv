using JoiabagurPV.API.Extensions;
using JoiabagurPV.API.Middleware;
using JoiabagurPV.Application.Extensions;
using JoiabagurPV.Infrastructure.Data;
using JoiabagurPV.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// Add layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Add API services
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Apply migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Map OpenAPI specification endpoint
    app.MapOpenApi(); // Available at: /openapi/v1.json
    
    // Map Scalar API reference UI
    app.MapScalarApiReference(); // Available at: /scalar/v1
}

// Add CORS BEFORE HTTPS redirection to handle preflight requests correctly
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");

// HTTPS redirection - skip OPTIONS requests (preflight) and HTTP requests in development to avoid CORS issues
// In development, allow HTTP connections from frontend without redirecting
app.UseWhen(context => 
    context.Request.Method != "OPTIONS" && 
    (app.Environment.IsProduction() || context.Request.IsHttps), 
    appBuilder =>
{
    appBuilder.UseHttpsRedirection();
});

// Add rate limiting
app.UseRateLimiter();

// Add middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();