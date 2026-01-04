using Xunit;

namespace JoiabagurPV.Tests.TestHelpers;

/// <summary>
/// Base class for all unit tests providing common functionality.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        // Setup common test initialization here
    }

    /// <summary>
    /// Cleanup method called after each test.
    /// </summary>
    public virtual void Dispose()
    {
        // Cleanup resources here
    }
}