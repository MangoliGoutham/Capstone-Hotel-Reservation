using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HotelReservationSystem.Models;
using HotelReservationSystem.Repositories;
using HotelReservationSystem.Services;
using HotelReservationSystem.DTOs;

namespace HotelReservationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("staff")]
    public async Task<ActionResult<IEnumerable<User>>> GetStaff()
    {
        var staff = await _userRepository.GetStaffUsersAsync();
        return Ok(staff);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(users);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.Email = userDto.Email;
        user.Role = userDto.Role;
        

        await _userRepository.UpdateAsync(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userRepository.DeleteAsync(id);
        return NoContent();
    }
}
