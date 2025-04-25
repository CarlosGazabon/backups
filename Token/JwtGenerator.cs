
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Inventio.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;

namespace inventio.Token
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly UserManager<User> _userManager;

        public JwtGenerator(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<string> CreateToken(User user)
        {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.NameId, user.UserName!),
                new Claim("userId", user.Id)
            };


            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("jz5ing^WNh42hy9ZUY^wNXk2HjMQoQmZ!G9HdB8BL*p%9xo@9z&2hWqfx!KqCTt%"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(30),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescription);

            return tokenHandler.WriteToken(token);
        }
    }
}
