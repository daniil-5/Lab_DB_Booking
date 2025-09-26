using BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.User;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;

    public AuthService(IJwtService jwtService, IUserService userService)
    {
        _jwtService = jwtService;
        _userService = userService;
    }

        public async Task<AuthResponse> Register(RegisterUserDto registerDto)
    {
        // Check if user already exists using UserService
        var existingUser = await _userService.GetUserByEmailAsync(registerDto.Email);
        if (existingUser != null)
            throw new Exception("User already exists");

        // Create user through UserService
        var createUserDto = new CreateUserDto
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            Password = registerDto.Password,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            Role = registerDto.Role
        };
        var userDto = await _userService.CreateUserAsync(createUserDto);

        // Create JWT token
        // Since JWT service likely expects a User entity, create one from the DTO
        var userForToken = new User
        {
            Id = userDto.Id,
            Email = userDto.Email,
            Username = userDto.Username,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Role = (int)registerDto.Role
        };

        return new AuthResponse
        {
            Id = userDto.Id,
            Email = userDto.Email,
            Username = userDto.Username,
            Token = _jwtService.GenerateToken(userForToken)
        };
    }
    public async Task<AuthResponse> Login(LoginDto loginDto)
    {
        // Verify user credentials using the password verification method
        var isPasswordValid = await _userService.VerifyUserPasswordAsync(loginDto.Email, loginDto.Password);
        if (!isPasswordValid)
            throw new Exception("Invalid credentials");
        
        // If password is valid, get the user details
        var userDto = await _userService.GetUserByEmailAsync(loginDto.Email);
        if (userDto == null) // This shouldn't happen if password verification succeeded
            throw new Exception("Invalid credentials");
        
        // Create JWT token
        var userForToken = new User
        {
            Id = userDto.Id,
            Email = userDto.Email,
            Username = userDto.Username,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Role = Enum.Parse<UserRole>(userDto.Role).GetHashCode()
        };
        
        return new AuthResponse
        {
            Id = userDto.Id,
            Email = userDto.Email,
            Username = userDto.Username,
            Token = _jwtService.GenerateToken(userForToken)
        };
    }
    
    public async Task<UserDto> GetUserById(int userId)
    {
        return await _userService.GetUserByIdAsync(userId);
    }
}