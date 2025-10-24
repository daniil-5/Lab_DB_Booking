using System.Linq.Expressions;
using BookingSystem.Application.DTOs.User;
using BookingSystem.Application.Interfaces;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Interfaces;

namespace BookingSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserActionAuditService _auditService;

        public UserService(IUserRepository userRepository, IUserActionAuditService auditService)
        {
            _userRepository = userRepository;
            _auditService = auditService;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
        {
            // Check if user with same email or username already exists
            var existingEmail = await _userRepository.GetByEmailAsync(userDto.Email);
            if (existingEmail != null)
                throw new ApplicationException("Email is already in use");

            var existingUsername = await _userRepository.GetByEmailAsync(userDto.Username);
            if (existingUsername != null)
                throw new ApplicationException("Username is already in use");

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                PhoneNumber = userDto.PhoneNumber,
                Role = (int)UserRole.Guest
            };

            await _userRepository.AddAsync(user);
            await _auditService.AuditActionAsync(user.Id, UserActionType.UserCreated, true);
            return MapToDto(user);
        }

        public async Task<UserDto> UpdateUserAsync(UpdateUserDto userDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(userDto.Id);
            if (existingUser == null)
                throw new ApplicationException($"User with ID {userDto.Id} not found");

            // Check if the updated email or username is already in use by another user
            if (userDto.Email != existingUser.Email)
            {
                var existingEmail = await _userRepository.GetByEmailAsync(userDto.Email);
                if (existingEmail != null && existingEmail.Id != userDto.Id)
                    throw new ApplicationException("Email is already in use");
            }

            if (userDto.Username != existingUser.Username)
            {
                var existingUsername = await _userRepository.GetByEmailAsync(userDto.Username);
                if (existingUsername != null && existingUsername.Id != userDto.Id)
                    throw new ApplicationException("Username is already in use");
            }

            existingUser.Username = userDto.Username;
            existingUser.Email = userDto.Email;
            existingUser.FirstName = userDto.FirstName;
            existingUser.LastName = userDto.LastName;
            existingUser.PhoneNumber = userDto.PhoneNumber;
            existingUser.Role = (int)userDto.Role;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(existingUser);
            await _auditService.AuditActionAsync(existingUser.Id, UserActionType.UserUpdated, true);
            return MapToDto(existingUser);
        }

        public async Task DeleteUserAsync(int id)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                throw new ApplicationException($"User with ID {id} not found");

            await _userRepository.DeleteAsync(id);
            await _auditService.AuditActionAsync(id, UserActionType.UserDeleted, true);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return MapToDto(user);
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return MapToDto(user);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByEmailAsync(username);
            return MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto).ToList();
        }
        

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(changePasswordDto.UserId);
            if (user == null)
                throw new ApplicationException($"User with ID {changePasswordDto.UserId} not found");
            
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                return false; 
            
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                throw new ApplicationException("New password and confirmation do not match");
            
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _auditService.AuditActionAsync(user.Id, UserActionType.ChangePassword, true);
            return true;
        }
        
        public async Task<bool> VerifyUserPasswordAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return false;
    
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task<IEnumerable<UserDto>> GetActiveUsersOrderedByRegistrationDateAsync()
        {
            var users = await _userRepository.GetActiveUsersOrderedByRegistrationDateAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<UserDto>> GetUsersWithNoBookingsAsync()
        {
            var users = await _userRepository.GetUsersWithNoBookingsAsync();
            return users.Select(MapToDto).ToList();
        }
        
        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = ((UserRole)user.Role).ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
        
    }
}