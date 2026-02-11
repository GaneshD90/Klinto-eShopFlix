using AuthService.Application.DTO;
using AuthService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AuthService.Application.Tests;

public class AuthmapperTests
{
    private static UserDTO MapUserToDto(User user)
    {
        return new UserDTO
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = user.Roles.Select(r => r.Name).ToArray()
        };
    }

    [Fact]
    public void MapUserToUserDto_MapsRolesAndIdentifiers()
    {
        var user = new User
        {
            Id = 42,
            Name = "Jane",
            Email = "jane@example.com",
            PhoneNumber = "555-0001",
            Roles = new List<Role> { new() { Name = "Admin" }, new() { Name = "User" } }
        };

        var dto = MapUserToDto(user);

        dto.UserId.Should().Be(user.Id);
        dto.Name.Should().Be(user.Name);
        dto.Email.Should().Be(user.Email);
        dto.PhoneNumber.Should().Be(user.PhoneNumber);
        dto.Roles.Should().BeEquivalentTo(new[] { "Admin", "User" });
    }
}
