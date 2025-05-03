using BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.User;

namespace BookingSystem.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> Register(RegisterUserDto registerDto);
    Task<AuthResponse> Login(LoginDto loginDto);
}