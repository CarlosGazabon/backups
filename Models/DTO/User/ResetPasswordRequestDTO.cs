using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.User
{
    public class ResetPasswordRequestDTO
    {
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[$!%*?&])[A-Za-z\d$!%*?&]{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, be at least 8 characters long  , and one special character /!$%&*?/")]
        public string? Password { get; set; }
    }
}