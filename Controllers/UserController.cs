using inventio.Models.DTO.Role;
using inventio.Models.DTO.User;
using inventio.Repositories.Users;
using inventio.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repository;
        private readonly IEmailSender _emailSender;

        public UserController(
            IUserRepository repository,
            IEmailSender emailSender
        )
        {
            _repository = repository;
            _emailSender = emailSender;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserResponseDto>> GetUserSession()
        {
            return await _repository.GetUserSession();
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(
            [FromBody] UserLoginRequestDto request
        )
        {
            return await _repository.Login(request);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(
            [FromBody] UserRegisterRequestDto request
        )
        {
            return await _repository.AddUser(request);
        }


        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _repository.GetUsers();

            return Ok(users);
        }

        [HttpGet("inactive")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetInactiveUsers()
        {
            var users = await _repository.GetInactiveUsers();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(string id)
        {
            var user = await _repository.GetUser(id);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(string id, [FromBody] UserUpdateRequestDto request)
        {
            var updatedUser = await _repository.UpdateUser(id, request);

            return Ok(updatedUser);
        }
        [HttpPut("{id}/UpdateProfile")]
        public async Task<ActionResult<ProfileResponseDto>> UpdateProfile(string id, [FromBody] ProfileUpdateRequestDto request)
        {
            var updatedProfile = await _repository.UpdateProfile(id, request);

            return Ok(updatedProfile);
        }

        [HttpPost("assign-role")]
        public async Task<ActionResult<UserResponseDto>> AssignRoleToUser(
            [FromBody] UserRoleAssignmentDto request
        )
        {

            return await _repository.AssignRoleToUser(request);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<UserResponseDto>> DeleteUser(string id)
        {
            var deletedUser = await _repository.SoftDeleteUser(id);
            return Ok(deletedUser);
        }

        [HttpDelete("permanent/{id}")]
        public async Task<ActionResult<UserResponseDto>> PermaDeleteUser(string id)
        {
            var deletedUser = await _repository.DeleteUser(id);
            return Ok(deletedUser);
        }

        [HttpPost("restore/{id}")]
        public async Task<ActionResult<UserResponseDto>> RestoreSoftDeletedUser(string id)
        {
            var restoredUser = await _repository.RestoreSoftDeletedUser(id);
            return Ok(restoredUser);
        }


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {

            var result = await _repository.ForgotPassword(request);

            return Ok(new { message = result });
        }

        [AllowAnonymous]
        [HttpGet("forgot-password/{token}")]
        public async Task<IActionResult> CheckPasswordToken([FromRoute] string token)
        {

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token parameter is required." });
            }

            try
            {
                bool userExists = await _repository.CheckUserByTokenAsync(token);

                return Ok(new { exists = userExists });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = "Failed to check user by token.", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/{token}")]
        public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromBody] ResetPasswordRequestDTO request)
        {
            try
            {
                bool resetSuccessful = await _repository.ResetPasswordByToken(token, request);

                if (resetSuccessful)
                {
                    return Ok(new { message = "Password reset successful" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to reset password" });
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = "Failed to reset password", error = ex.Message });
            }
        }

    }
}