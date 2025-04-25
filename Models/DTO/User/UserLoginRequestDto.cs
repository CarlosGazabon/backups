using System.ComponentModel.DataAnnotations;

namespace inventio.Models.DTO.User
{
    public class UserLoginRequestDto
    {
        public string? Email { get; set; }
        
        public string? Password { get; set; }
    }
}