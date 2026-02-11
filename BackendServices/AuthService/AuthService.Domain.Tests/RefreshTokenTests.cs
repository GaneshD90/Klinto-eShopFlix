using AuthService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AuthService.Domain.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_ReturnsTrue_WhenNotRevokedAndNotExpired()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenRevoked()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            RevokedAt = DateTime.UtcNow
        };

        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenExpired()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        token.IsActive.Should().BeFalse();
    }
}
