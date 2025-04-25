
namespace inventio.Models.DTO.User
{
    public class UserUpdateRequestDto
    {
        public required string Name { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public List<string>? Roles { get; set; }
    }
}