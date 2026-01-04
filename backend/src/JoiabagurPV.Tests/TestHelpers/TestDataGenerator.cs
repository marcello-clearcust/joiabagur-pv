using Bogus;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;

namespace JoiabagurPV.Tests.TestHelpers;

/// <summary>
/// Test data generator using Bogus for creating fake entities.
/// </summary>
public static class TestDataGenerator
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates a User faker with customizable rules.
    /// </summary>
    public static Faker<User> UserFaker(UserRole? role = null, bool? isActive = null)
    {
        return new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Username, f => f.Internet.UserName())
            .RuleFor(u => u.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("TestPass123!", workFactor: 4))
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Role, f => role ?? f.PickRandom<UserRole>())
            .RuleFor(u => u.IsActive, f => isActive ?? true)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past())
            .RuleFor(u => u.UpdatedAt, f => f.Date.Recent())
            .RuleFor(u => u.LastLoginAt, f => f.Date.Recent().OrNull(f));
    }

    /// <summary>
    /// Creates a single User with optional customization.
    /// </summary>
    public static User CreateUser(UserRole? role = null, bool? isActive = null, string? password = null)
    {
        var user = UserFaker(role, isActive).Generate();
        if (password != null)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        }
        return user;
    }

    /// <summary>
    /// Creates multiple User instances.
    /// </summary>
    public static List<User> CreateUsers(int count, UserRole? role = null, bool? isActive = null)
    {
        return UserFaker(role, isActive).Generate(count);
    }

    /// <summary>
    /// Creates a RefreshToken faker with customizable rules.
    /// </summary>
    public static Faker<RefreshToken> RefreshTokenFaker(Guid? userId = null, bool? isRevoked = null, DateTime? expiresAt = null)
    {
        return new Faker<RefreshToken>()
            .RuleFor(r => r.Id, f => f.Random.Guid())
            .RuleFor(r => r.Token, f => Convert.ToBase64String(f.Random.Bytes(64)))
            .RuleFor(r => r.UserId, f => userId ?? f.Random.Guid())
            .RuleFor(r => r.ExpiresAt, f => expiresAt ?? DateTime.UtcNow.AddHours(8))
            .RuleFor(r => r.IsRevoked, f => isRevoked ?? false)
            .RuleFor(r => r.CreatedAt, f => f.Date.Recent())
            .RuleFor(r => r.CreatedByIp, f => f.Internet.Ip());
    }

    /// <summary>
    /// Creates a single RefreshToken.
    /// </summary>
    public static RefreshToken CreateRefreshToken(Guid? userId = null, bool? isRevoked = null, DateTime? expiresAt = null, User? user = null)
    {
        var token = RefreshTokenFaker(userId, isRevoked, expiresAt).Generate();
        if (user != null)
        {
            token.UserId = user.Id;
            token.User = user;
        }
        return token;
    }

    /// <summary>
    /// Creates a UserPointOfSale faker.
    /// </summary>
    public static Faker<UserPointOfSale> UserPointOfSaleFaker(Guid? userId = null, Guid? pointOfSaleId = null, bool? isActive = null)
    {
        return new Faker<UserPointOfSale>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.UserId, f => userId ?? f.Random.Guid())
            .RuleFor(u => u.PointOfSaleId, f => pointOfSaleId ?? f.Random.Guid())
            .RuleFor(u => u.IsActive, f => isActive ?? true)
            .RuleFor(u => u.AssignedAt, f => f.Date.Past())
            .RuleFor(u => u.CreatedAt, f => f.Date.Past());
    }

    /// <summary>
    /// Creates a single UserPointOfSale assignment.
    /// </summary>
    public static UserPointOfSale CreateUserPointOfSale(Guid? userId = null, Guid? pointOfSaleId = null, bool? isActive = null)
    {
        return UserPointOfSaleFaker(userId, pointOfSaleId, isActive).Generate();
    }

    /// <summary>
    /// Generates a random password that meets requirements.
    /// </summary>
    public static string GeneratePassword(int length = 12)
    {
        return _faker.Internet.Password(length, memorable: false, prefix: "Aa1!");
    }

    /// <summary>
    /// Creates a PointOfSale faker with customizable rules.
    /// </summary>
    public static Faker<PointOfSale> PointOfSaleFaker(bool? isActive = null)
    {
        return new Faker<PointOfSale>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Company.CompanyName())
            .RuleFor(p => p.Code, f => f.Random.AlphaNumeric(6).ToUpperInvariant())
            .RuleFor(p => p.Address, f => f.Address.FullAddress())
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.IsActive, f => isActive ?? true)
            .RuleFor(p => p.CreatedAt, f => f.Date.Past())
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent());
    }

    /// <summary>
    /// Creates a single PointOfSale with optional customization.
    /// </summary>
    public static PointOfSale CreatePointOfSale(bool? isActive = null, string? code = null, string? name = null)
    {
        var pos = PointOfSaleFaker(isActive).Generate();
        if (code != null) pos.Code = code.ToUpperInvariant();
        if (name != null) pos.Name = name;
        return pos;
    }

    /// <summary>
    /// Creates multiple PointOfSale instances.
    /// </summary>
    public static List<PointOfSale> CreatePointOfSales(int count, bool? isActive = null)
    {
        return PointOfSaleFaker(isActive).Generate(count);
    }

    /// <summary>
    /// Creates a BaseEntity for testing (uses User as concrete implementation).
    /// This is for backwards compatibility with existing tests.
    /// </summary>
    public static BaseEntity CreateBaseEntity()
    {
        return CreateUser();
    }

    /// <summary>
    /// Creates multiple BaseEntity instances for testing.
    /// </summary>
    public static IEnumerable<BaseEntity> CreateBaseEntities(int count)
    {
        return CreateUsers(count).Cast<BaseEntity>();
    }
}