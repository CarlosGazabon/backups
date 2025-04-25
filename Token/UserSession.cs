using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Token
{
    public class UserSession : IUserSession
    {
        private readonly IHttpContextAccessor _httpContextAccesor;

        public UserSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccesor = httpContextAccessor;
        }

        public string GetUserSession()
        {
            var userName = _httpContextAccesor.HttpContext!.User?.Claims?
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            return userName!;
        }
    }
}