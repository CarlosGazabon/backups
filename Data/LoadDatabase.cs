using Inventio.Models;
using Microsoft.AspNetCore.Identity;

namespace Inventio.Data
{
    public class LoadDatabase
    {
        public static async Task InsertData(ApplicationDBContext context, UserManager<User> usuarioManager, RoleManager<Role> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Name = "Guest" },
                    new Role { Name = "General User" },
                    new Role { Name = "Administrator" },
                    new Role { Name = "System Administrator" }
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }

            if (!usuarioManager.Users.Any())
            {
                var usuario = new User
                {
                    Name = "admin",
                    LastName = "admin",
                    Email = "admin@ars-combinatoria.com",
                    UserName = "admin",
                    PhoneNumber = "00000000"
                };

                await usuarioManager.CreateAsync(usuario, "ars-combinatoria123Admin$");

                await usuarioManager.AddToRoleAsync(usuario, "System Administrator");
            }

            context.SaveChanges();
        }
    }
}