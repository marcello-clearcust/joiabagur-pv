using Xunit;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Collection definition for API integration tests.
/// All test classes decorated with [Collection(Name)] will share the same ApiWebApplicationFactory instance,
/// meaning they share the same PostgreSQL container, reducing test execution time.
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<ApiWebApplicationFactory>
{
    /// <summary>
    /// The name of the collection. Use this in [Collection] attribute on test classes.
    /// </summary>
    public const string Name = "IntegrationTests";
}

/// <summary>
/// Collection definition for repository integration tests.
/// All test classes decorated with [Collection(Name)] will share the same TestDatabaseFixture instance.
/// </summary>
[CollectionDefinition(RepositoryTestCollection.Name)]
public class RepositoryTestCollection : ICollectionFixture<TestDatabaseFixture>
{
    /// <summary>
    /// The name of the collection. Use this in [Collection] attribute on test classes.
    /// </summary>
    public const string Name = "RepositoryTests";
}

