using BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.User;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Register(RegisterUserDto registerDto)
    {
        if (await _userRepository.GetByEmailAsync(registerDto.Email) != null)
            throw new Exception("User already exists");

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber
        };

        await _userRepository.AddAsync(user);

        return new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Token = _jwtService.GenerateToken(user)
        };
    }

    public async Task<AuthResponse> Login(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        return new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Token = _jwtService.GenerateToken(user)
        };
    }
    public async Task<UserDto> GetUserById(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            PhoneNumber = user.PhoneNumber,
            Username = user.Username,
            LastName = user.LastName,
            Role = ((UserRole)user.Role).ToString()
        };
    }
}