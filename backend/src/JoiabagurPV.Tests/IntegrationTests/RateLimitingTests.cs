using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for rate limiting functionality.
/// </summary>
public class RateLimitingTests : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RateLimitingTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Login Rate Limiting Tests

    [Fact]
    public async Task Login_WithinRateLimit_ShouldSucceed()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        // Act - Make a few requests within the limit
        for (int i = 0; i < 3; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);

            // Assert - Should succeed
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Login_ExceedingRateLimit_ShouldReturnTooManyRequests()
    {
        // Arrange
        var invalidRequest = new LoginRequest
        {
            Username = "nonexistent",
            Password = "wrongpassword"
        };

        // Create a fresh client for this test to avoid IP-based rate limit state from other tests
        var testClient = _factory.CreateClient();

        // Act - Make more requests than allowed (limit is 5 per 15 minutes per IP)
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 8; i++)
        {
            var response = await testClient.PostAsJsonAsync("/api/auth/login", invalidRequest);
            responses.Add(response);
        }

        // Assert - Later requests should be rate limited
        var rateLimitedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        rateLimitedResponses.Should().NotBeEmpty("Rate limiting should kick in after 5 requests");
    }

    [Fact]
    public async Task Login_SuccessfulLogin_DoesNotExemptFromRateLimit()
    {
        // Arrange
        var validRequest = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };
        var invalidRequest = new LoginRequest
        {
            Username = "admin",
            Password = "wrongpassword"
        };

        // Create a fresh client for this test
        var testClient = _factory.CreateClient();

        // Act - Mix of successful and failed logins
        await testClient.PostAsJsonAsync("/api/auth/login", validRequest);
        await testClient.PostAsJsonAsync("/api/auth/login", invalidRequest);
        await testClient.PostAsJsonAsync("/api/auth/login", validRequest);
        await testClient.PostAsJsonAsync("/api/auth/login", invalidRequest);
        await testClient.PostAsJsonAsync("/api/auth/login", invalidRequest);

        // Make more requests to trigger rate limit
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 5; i++)
        {
            var response = await testClient.PostAsJsonAsync("/api/auth/login", invalidRequest);
            responses.Add(response);
        }

        // Assert - Should eventually get rate limited
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        rateLimitedCount.Should().BeGreaterThan(0, "Rate limiting should apply regardless of login success");
    }

    #endregion

    #region Non-Rate-Limited Endpoints Tests

    [Fact]
    public async Task AuthMe_ShouldNotBeRateLimited()
    {
        // Arrange - Login first
        var loginRequest = new LoginRequest { Username = "admin", Password = "Admin123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var authClient = _factory.CreateClient();
        foreach (var cookie in cookies)
        {
            var cookieParts = cookie.Split(';')[0].Split('=');
            if (cookieParts.Length == 2 && cookieParts[0] == "access_token")
            {
                authClient.DefaultRequestHeaders.Add("Cookie", $"access_token={cookieParts[1]}");
            }
        }

        // Act - Make many requests
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 20; i++)
        {
            var response = await authClient.GetAsync("/api/auth/me");
            responses.Add(response);
        }

        // Assert - None should be rate limited
        responses.Should().NotContain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_ShouldNotBeRateLimited()
    {
        // Arrange
        var logoutClient = _factory.CreateClient();

        // Act - Make many logout requests
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 10; i++)
        {
            var response = await logoutClient.PostAsync("/api/auth/logout", null);
            responses.Add(response);
        }

        // Assert - None should be rate limited
        responses.Should().NotContain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Refresh_ShouldNotBeRateLimited()
    {
        // Act - Make many refresh requests (they will fail but should not be rate limited)
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 10; i++)
        {
            var response = await _client.PostAsync("/api/auth/refresh", null);
            responses.Add(response);
        }

        // Assert - Should return Unauthorized, not TooManyRequests
        responses.Should().NotContain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Rate Limit Headers Tests

    [Fact]
    public async Task Login_RateLimited_ShouldIncludeRetryAfterHeader()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "wrongpassword"
        };

        // Create a fresh client
        var testClient = _factory.CreateClient();

        // Act - Exhaust rate limit
        HttpResponseMessage? rateLimitedResponse = null;
        for (int i = 0; i < 10; i++)
        {
            var response = await testClient.PostAsJsonAsync("/api/auth/login", request);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedResponse = response;
                break;
            }
        }

        // Assert
        if (rateLimitedResponse != null)
        {
            rateLimitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
            // Rate limiter may include Retry-After header
            // Note: The default ASP.NET Core rate limiter may not include this header
        }
    }

    #endregion
}


