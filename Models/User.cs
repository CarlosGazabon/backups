
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Inventio.Models
{
    public class User : IdentityUser
    {

        [Column(TypeName = "varchar(30)")]
        [StringLength(30, ErrorMessage = "Name cannot be longer than 30 characters.")]
        public string? Name { get; set; }

        [Column(TypeName = "varchar(30)")]
        [StringLength(30, ErrorMessage = "Name cannot be longer than 30 characters.")]
        public string? LastName { get; set; }
        [Required]
        public string ImagePath { get; set; } = "";
        public Nullable<DateTime> PasswordExpiryDate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }

        public string? Token { get; set; }

    }
}