using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HotelReservationSystem.Controllers;
using HotelReservationSystem.Repositories;
using HotelReservationSystem.Models;
using HotelReservationSystem.DTOs;

namespace HotelReservationSystem.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _controller = new UsersController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnOkWithList()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, FirstName = "Test", Email = "test@test.com" }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            // Act
            var actionResult = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
            Assert.Single(returnUsers);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNoContent_WhenUserExists()
        {
            // Arrange
            var id = 1;
            var existingUser = new User { Id = id, FirstName = "Old", Role = "Guest" };
            var updateDto = new UserUpdateDto { FirstName = "New", LastName = "Name", Email = "new@test.com", Role = "Admin" };

            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingUser);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(existingUser);

            // Act
            var result = await _controller.UpdateUser(id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal("New", existingUser.FirstName);
            Assert.Equal("Admin", existingUser.Role);
            _mockRepo.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var id = 1;
            var updateDto = new UserUpdateDto();
            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.UpdateUser(id, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent()
        {
            // Arrange
            var id = 1;
            _mockRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteUser(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepo.Verify(r => r.DeleteAsync(id), Times.Once);
        }
    }
}
