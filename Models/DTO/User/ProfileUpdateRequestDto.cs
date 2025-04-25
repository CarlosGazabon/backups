
namespace inventio.Models.DTO.User
{
    public class ProfileUpdateRequestDto
    {
        public required string Name { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = "";

    }
}