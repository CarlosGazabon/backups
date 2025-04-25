using inventio.Models.DTO.Role;
using inventio.Models.DTO.User;

namespace inventio.Repositories.Users
{
    public interface IUserRepository
    {
        Task<UserResponseDto> GetUserSession();
        Task<IEnumerable<UserResponseDto>> GetUsers();
        Task<IEnumerable<UserResponseDto>> GetInactiveUsers();
        Task<UserResponseDto> GetUser(string id);
        Task<UserResponseDto> UpdateUser(string id, UserUpdateRequestDto request);
        Task<ProfileResponseDto> UpdateProfile(string id, ProfileUpdateRequestDto request);
        Task<UserResponseDto> Login(UserLoginRequestDto request);
        Task<UserResponseDto> AddUser(UserRegisterRequestDto request);
        Task<UserResponseDto> AssignRoleToUser(UserRoleAssignmentDto request);
        Task<UserResponseDto> DeleteUser(string id);
        Task<UserResponseDto> SoftDeleteUser(string id);
        Task<UserResponseDto> RestoreSoftDeletedUser(string id);
        Task<string> ForgotPassword(ForgotPasswordRequestDTO request);
        Task<bool> CheckUserByTokenAsync(string token);
        Task<bool> ResetPasswordByToken(string token, ResetPasswordRequestDTO request);
    }
}