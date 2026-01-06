using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using HotelReservationSystem.Services;
using HotelReservationSystem.Repositories;
using HotelReservationSystem.Models;
using HotelReservationSystem.DTOs;
using Microsoft.Extensions.Configuration;
using HotelReservationSystem.Helpers;

namespace HotelReservationSystem.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Mock JWT settings
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyForTestingPurposes123!");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            var jwtHelper = new JwtHelper(_mockConfiguration.Object);
            _authService = new AuthService(_mockUserRepository.Object, jwtHelper);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Id = 1,
                Email = email,
                PasswordHash = hashedPassword,
                Role = "Guest",
                FirstName = "Test",
                LastName = "User"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync(user);

            var loginDto = new LoginDto { Email = email, Password = password };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal("Guest", result.Role);
        }

        [Fact]
        public async Task Login_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "nonexistent@example.com", Password = "password" };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Login_ShouldReturnNull_WhenPasswordIsInvalid()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Id = 1,
                Email = email,
                PasswordHash = hashedPassword,
                Role = "Guest",
                FirstName = "Test",
                LastName = "User"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
            
            var loginDto = new LoginDto { Email = email, Password = "wrongpassword" };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Register_ShouldCreateUser_WhenEmailIsUnique()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "new@example.com",
                Password = "password123",
                Role = "Guest"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null); // No existing user

            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => { u.Id = 1; return u; });

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(registerDto.Email, result.Email);
            _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldReturnNull_WhenEmailExists()
        {
             // Arrange
            var registerDto = new RegisterDto
            {
                Email = "existing@example.com",
                Password = "password"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync(new User()); // Existing user

            // Act
            var result = await _authService.RegisterAsync(registerDto);

             // Assert
             Assert.Null(result);
        }
    }
}
