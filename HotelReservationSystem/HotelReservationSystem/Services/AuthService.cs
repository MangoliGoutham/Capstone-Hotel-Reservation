using HotelReservationSystem.DTOs;
using HotelReservationSystem.Models;
using HotelReservationSystem.Repositories;
using HotelReservationSystem.Helpers;
using BCrypt.Net;

namespace HotelReservationSystem.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IUserRepository userRepository, JwtHelper jwtHelper)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtHelper.GenerateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return null;
        }

        var user = new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = !string.IsNullOrEmpty(registerDto.Role) ? registerDto.Role : "Guest",
            PhoneNumber = registerDto.PhoneNumber
        };

        await _userRepository.AddAsync(user);

        var token = _jwtHelper.GenerateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }
}