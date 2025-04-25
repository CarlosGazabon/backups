using Microsoft.AspNetCore.Identity;

namespace Inventio.Models
{
    public class Role : IdentityRole
    {
        public Nullable<bool> IsDelete { get; set; }
    }
}