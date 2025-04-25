using System.ComponentModel.DataAnnotations;

namespace inventio.Models.DTO.User
{
    public class UserRegisterRequestDto
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? ImagePath { get; set; }
        public List<string>? Roles { get; set; }
        public string? PhoneNumber { get; set; }
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[$!%*?&])[A-Za-z\d$!%*?&]{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, be at least 8 characters long  , and one special character /!$%&*?/")]

        public string? Password { get; set; }

    }
}