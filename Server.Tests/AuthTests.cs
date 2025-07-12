using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared.Common;
using Server.Contexts;
using Server.Controllers;
using Shared.DTOs;
using Server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class AuthControllerTests
{
    private static UserService CreateMockUserService()
    {
        var dbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        var mapper = new Mock<IMapper>();
        return new UserService(dbContext.Object, mapper.Object);
    }

    [Fact]
    public void HashPassword_And_CheckPassword_ShouldWork()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Key"]).Returns("test_secret_key_12345678901234567890");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test_issuer");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test_audience");

        var userService = CreateMockUserService();
        var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

        var authService = new AuthService(mockConfig.Object, userService, mockDbContext.Object);
        var password = "MySecurePassword123!";

        // Act
        var hash = authService.HashPassword(password);
        var result = authService.CheckPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CheckPassword_ShouldReturnFalseForWrongPassword()
    {
        // Arrange
        var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Key"]).Returns("test_secret_key_12345678901234567890");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test_issuer");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test_audience");

        var userService = CreateMockUserService();
        var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

        var authService = new AuthService(mockConfig.Object, userService, mockDbContext.Object);
        var password = "MySecurePassword123!";
        var wrongPassword = "WrongPassword!";
        var hash = authService.HashPassword(password);

        // Act
        var result = authService.CheckPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_ShouldProduceDifferentHashesForDifferentPasswords()
    {
        // Arrange
        var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Key"]).Returns("test_secret_key_12345678901234567890");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test_issuer");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test_audience");

        var userService = CreateMockUserService();
        var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

        var authService = new AuthService(mockConfig.Object, userService, mockDbContext.Object);
        var password1 = "Password1!";
        var password2 = "Password2!";

        // Act
        var hash1 = authService.HashPassword(password1);
        var hash2 = authService.HashPassword(password2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public async void GenerateToken_ShouldReturnJwtWithUserIdAndRole()
    {
        // Arrange: mock config
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Key"]).Returns("test_secret_key_12345678901234567890");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test_issuer");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test_audience");

        // Clear default mappings to avoid auto-remap issues
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var userService = CreateMockUserService();
        var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

        var authService = new AuthService(mockConfig.Object, userService, mockDbContext.Object);
        var controller = new AuthController(authService);

        var userId = GuidUtils.GenerateLittleGuid();
        var role = UserRole.Admin;

        var request = new AuthDto { Username = "bingchilling", Password = "skibidi" };

        // Act
        var result = await controller.Create(request) as OkObjectResult;
        Assert.NotNull(result);

        var token = result.Value as string;
        Assert.False(string.IsNullOrWhiteSpace(token));

        // Decode the JWT
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert claims
        Assert.Contains(jwtToken.Claims, c => c.Type == "UserId" && c.Value == userId.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == role.ToString());
    }
}