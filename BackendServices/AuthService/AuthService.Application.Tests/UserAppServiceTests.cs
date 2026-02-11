using AuthService.Application.DTO;
using AuthService.Application.Repositories;
using AuthService.Application.Services.Implementations;
using AuthService.Domain.Entities;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using BC = BCrypt.Net.BCrypt;

namespace AuthService.Application.Tests;

public class UserAppServiceTests
{
    private static IConfiguration BuildConfiguration()
    {
        var config = new ConfigurationManager
        {
            ["Jwt:Key"] = "test-signing-key-1234567890",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:ExpireMinutes"] = "60",
            ["Jwt:RefreshExpireDays"] = "7"
        };

        return config;
    }

    private static IMapper BuildMapper()
    {
        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
            .Returns((User source) => new UserDTO
            {
                UserId = source.Id,
                Name = source.Name,
                Email = source.Email,
                PhoneNumber = source.PhoneNumber,
                Roles = source.Roles?.Select(r => r.Name).ToArray()
            });

        mapper.Setup(m => m.Map<User>(It.IsAny<SignUpDTO>()))
            .Returns((SignUpDTO source) => new User
            {
                Name = source.Name,
                Email = source.Email,
                PhoneNumber = source.PhoneNumber,
                Password = source.Password
            });

        return mapper.Object;
    }

    private static UserAppService CreateSut(
        Mock<IUserRepository> userRepository,
        Mock<IRefreshTokenRepository> refreshTokenRepository,
        IConfiguration? configuration = null,
        IMapper? mapper = null)
    {
        return new UserAppService(
            userRepository.Object,
            refreshTokenRepository.Object,
            configuration ?? BuildConfiguration(),
            mapper ?? BuildMapper());
    }

    [Fact]
    public void LoginUser_WhenUserNotFound_Throws()
    {
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        userRepository.Setup(r => r.GetUserByEmail("missing@example.com")).Returns((User?)null);
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var action = () => sut.LoginUser(new LoginDTO { Email = "missing@example.com", Password = "secret" });

        action.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void LoginUser_WhenPasswordInvalid_Throws()
    {
        var user = new User { Id = 1, Email = "user@example.com", Password = BC.HashPassword("correct") };
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        userRepository.Setup(r => r.GetUserByEmail(user.Email)).Returns(user);
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var action = () => sut.LoginUser(new LoginDTO { Email = user.Email, Password = "wrong" });

        action.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void LoginUser_WhenValid_ReturnsTokensAndSavesRefreshToken()
    {
        var user = new User
        {
            Id = 7,
            Name = "Ava",
            Email = "ava@example.com",
            Password = BC.HashPassword("secret"),
            Roles = new List<Role> { new() { Name = "User" } }
        };
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        userRepository.Setup(r => r.GetUserByEmail(user.Email)).Returns(user);
        refreshTokenRepository.Setup(r => r.Save(It.IsAny<RefreshToken>())).Returns((RefreshToken token) => token);
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var result = sut.LoginUser(new LoginDTO { Email = user.Email, Password = "secret" });

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        result.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        refreshTokenRepository.Verify(r => r.Save(It.Is<RefreshToken>(t => t.UserId == user.Id)), Times.Once);
    }

    [Fact]
    public void RefreshToken_WhenRequestMissing_Throws()
    {
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var action = () => sut.RefreshToken(new RefreshTokenRequestDTO { RefreshToken = "" });

        action.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void RefreshToken_WhenTokenInvalid_Throws()
    {
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByToken("bad-token")).Returns((RefreshToken?)null);
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var action = () => sut.RefreshToken(new RefreshTokenRequestDTO { RefreshToken = "bad-token" });

        action.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void RefreshToken_WhenUserMissing_Throws()
    {
        var storedToken = new RefreshToken
        {
            UserId = 10,
            Token = "stored",
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByToken("stored")).Returns(storedToken);
        userRepository.Setup(r => r.GetUserById(storedToken.UserId)).Returns((User?)null);
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var action = () => sut.RefreshToken(new RefreshTokenRequestDTO { RefreshToken = "stored" });

        action.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void RefreshToken_WhenValid_ReturnsNewTokensAndRevokes()
    {
        var user = new User
        {
            Id = 5,
            Name = "Liam",
            Email = "liam@example.com",
            Roles = new List<Role> { new() { Name = "Admin" } }
        };
        var storedToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "stored-token",
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            User = user
        };
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByToken(storedToken.Token)).Returns(storedToken);
        refreshTokenRepository.Setup(r => r.Save(It.IsAny<RefreshToken>())).Returns((RefreshToken token) => token);
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var result = sut.RefreshToken(new RefreshTokenRequestDTO { RefreshToken = storedToken.Token });

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshTokenRepository.Verify(r => r.Revoke(storedToken, It.IsAny<string>()), Times.Once);
        refreshTokenRepository.Verify(r => r.Save(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WhenTokenMissing_NoOp()
    {
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var sut = CreateSut(userRepository, refreshTokenRepository);

        await sut.RevokeRefreshTokenAsync(new RefreshTokenRequestDTO());

        refreshTokenRepository.Verify(r => r.Revoke(It.IsAny<RefreshToken>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public void SignUpUser_WhenUserExists_Throws()
    {
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        userRepository.Setup(r => r.GetUserByEmail("dupe@example.com")).Returns(new User());
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var action = () => sut.SignUpUser(new SignUpDTO { Email = "dupe@example.com", Password = "secret" }, "User");

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SignUpUser_WhenValid_RegistersHashedPassword()
    {
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        userRepository.Setup(r => r.GetUserByEmail("new@example.com")).Returns((User?)null);

        User? capturedUser = null;
        userRepository.Setup(r => r.RegisterUser(It.IsAny<User>(), "User"))
            .Callback<User, string>((user, _) => capturedUser = user)
            .Returns(true);

        var sut = CreateSut(userRepository, refreshTokenRepository);
        var signUp = new SignUpDTO
        {
            Name = "New",
            Email = "new@example.com",
            Password = "secret",
            PhoneNumber = "555-1010"
        };

        var result = sut.SignUpUser(signUp, "User");

        result.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        BC.Verify(signUp.Password, capturedUser!.Password).Should().BeTrue();
    }

    [Fact]
    public void GetAllUsers_WhenRepositoryReturnsNull_ReturnsEmpty()
    {
        var userRepository = new Mock<IUserRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        userRepository.Setup(r => r.GetAll()).Returns((IEnumerable<User>?)null);
        var sut = CreateSut(userRepository, refreshTokenRepository);

        var result = sut.GetAllUsers();

        result.Should().BeEmpty();
    }
}
