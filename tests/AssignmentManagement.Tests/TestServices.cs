using AssignmentManagement.Api.Auth;
using AssignmentManagement.Api.Domain;
using Microsoft.Extensions.Configuration;

namespace AssignmentManagement.Tests;

public static class TestServices
{
    public static CurrentUser As(User user)
    {
        return new CurrentUser
        {
            Id = user.Id,
            Role = user.Role?.Name ?? string.Empty,
            Email = user.Email
        };
    }

    public static JwtTokenService CreateJwtTokenService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-that-is-long-enough-for-hmac-sha256-signing",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        return new JwtTokenService(config);
    }
}
