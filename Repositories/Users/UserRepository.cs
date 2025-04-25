using System.Net;
using inventio.Middleware;
using inventio.Models.DTO.Role;
using inventio.Models.DTO.User;
using inventio.Services.Email;
using inventio.Token;
using inventio.Utils;
using Inventio.Data;
using Inventio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Users
{
    public class UserRepository : IUserRepository
    {

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManger;
        private readonly IJwtGenerator _jwtGenerador;
        private readonly ApplicationDBContext _context;
        private readonly IEmailSender _emailSender;

        private readonly IUserSession _userSession;

        public UserRepository(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IJwtGenerator jwtGenerator,
            ApplicationDBContext context,
            IUserSession userSession,
            IEmailSender emailSender
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManger = signInManager;
            _jwtGenerador = jwtGenerator;
            _context = context;
            _userSession = userSession;
            _emailSender = emailSender;
        }

        private async Task<UserResponseDto> TransformUserToUserDto(User user, bool retrieveToken)
        {
            if (retrieveToken)
            {
                var token = await _jwtGenerador.CreateToken(user);

                return new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    UserName = user.UserName,
                    ImagePath = user.ImagePath,
                    IsDeleted = user.IsDeleted,
                    Token = token,

                };
            }

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserName = user.UserName,
                ImagePath = user.ImagePath,
                IsDeleted = user.IsDeleted,

            };

        }


        public async Task<UserResponseDto> GetUserSession()
        {
            var user = await _userManager.FindByNameAsync(_userSession.GetUserSession())
                ?? throw new MiddlewareException(HttpStatusCode.Unauthorized,
                                                 new { message = "User token does not exist" });

            var roleNames = await _userManager.GetRolesAsync(user);
            var role = await _roleManager.FindByNameAsync(roleNames[0]);

            var userDto = await TransformUserToUserDto(user!, false);
            userDto.Roles = roleNames.ToList();
            userDto.RoleId = role?.Id;

            return userDto;
        }

        public async Task<UserResponseDto> Login(UserLoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email!)
                  ?? throw new MiddlewareException(HttpStatusCode.Unauthorized,
                                                 new { message = "User email does not exists" });

            if (user.IsDeleted == true)
            {
                throw new MiddlewareException(HttpStatusCode.Unauthorized, new { message = "User is inactive, please contact your software administrator" });
            }

            var result = await _signInManger.CheckPasswordSignInAsync(user!, request.Password!, false);

            if (result.Succeeded)
            {
                var roleNames = await _userManager.GetRolesAsync(user);
                var role = await _roleManager.FindByNameAsync(roleNames[0]);

                var userDto = await TransformUserToUserDto(user, true);
                userDto.Roles = roleNames.ToList();
                userDto.RoleId = role?.Id;

                return userDto;
            }

            throw new MiddlewareException(HttpStatusCode.Unauthorized, new { message = "Invalid Credentials" });
        }

        public async Task<UserResponseDto> AddUser(UserRegisterRequestDto request)
        {

            var existsEmail = await _context.Users.Where(w => w.Email == request.Email).AnyAsync();

            if (existsEmail)
            {
                throw new MiddlewareException(HttpStatusCode.BadRequest, new { message = "User Email already exists" });
            }


            var existsUserName = await _context.Users.Where(w => w.UserName == request.UserName).AnyAsync();

            if (existsUserName)
            {
                throw new MiddlewareException(HttpStatusCode.BadRequest, new { message = "User Name already exists" });
            }

            var user = new User
            {
                Name = request.Name!,
                LastName = request.LastName!,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                UserName = request.UserName,
                ImagePath = request.ImagePath!
            };

            var result = await _userManager.CreateAsync(user!, request.Password!);

            if (result.Succeeded)
            {
                if (request.Roles != null && request.Roles.Any())
                {
                    var rolesToAdd = request.Roles.Distinct().ToArray();
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                var userDto = await TransformUserToUserDto(user, false);

                userDto.Roles = request.Roles;

                await _emailSender.SendEmailInvitationAsync(user, request.Password!);

                return userDto;
            }

            throw new Exception("Failed to register user");
        }

        public async Task<UserResponseDto> AssignRoleToUser(UserRoleAssignmentDto request)
        {
            if (request.UserId == null)
            {
                throw new ArgumentNullException(nameof(request.UserId), "User Id cannot be null");
            }

            if (request.RoleId == null)
            {
                throw new ArgumentNullException(nameof(request.RoleId), "Role Id cannot be null");
            }

            // Retrieve user and role id
            var user = await _userManager.FindByIdAsync(request.UserId) ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "User not found" });

            var role = await _context.Roles.FindAsync(request.RoleId) ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "Role not found" });

            // Asign Role
            var result = await _userManager.AddToRoleAsync(user, role.Name!);

            if (result.Succeeded)
            {
                return await TransformUserToUserDto(user, false);
            }


            throw new Exception("Failed to assign role to user");
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsers()
        {
            var currentUser = await _userManager.FindByNameAsync(_userSession.GetUserSession());
            if (currentUser == null)
            {
                throw new MiddlewareException(HttpStatusCode.Unauthorized, new { message = "User token does not exist" });
            }

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var currentAdministrator = false;
            foreach (string rol in currentUserRoles)
            {
                if (rol == "System Administrator")
                {
                    currentAdministrator = true;
                }
            }

            var users = await _userManager.Users.ToListAsync();

            List<UserResponseDto> userList = new();

            foreach (var user in users)
            {
                // Filtrar el usuario logueado
                if (user.Id != currentUser.Id)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var userAdministrator = false;
                    foreach (string rol in roles)
                    {
                        if (rol == "System Administrator")
                        {
                            userAdministrator = true;
                        }
                    }
                    var userDto = await TransformUserToUserDto(user, false);
                    userDto.Roles = roles.ToList();
                    if (!currentAdministrator && userAdministrator)
                    {
                        continue;
                    }
                    userList.Add(userDto);
                }
            }

            return userList;
        }

        public async Task<IEnumerable<UserResponseDto>> GetInactiveUsers()
        {
            var users = await _userManager.Users
            .Where(w => w.IsDeleted == true)
            .ToListAsync();

            List<UserResponseDto> userList = new();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = await TransformUserToUserDto(user, false);
                userDto.Roles = roles.ToList();
                userList.Add(userDto);
            }

            return userList;
        }

        public async Task<UserResponseDto> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = await TransformUserToUserDto(user, false);

            userDto.Roles = roles.ToList();

            return userDto;
        }

        public async Task<UserResponseDto> UpdateUser(string id, UserUpdateRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "User not found" });

            user.Name = request.Name;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.Email = request.Email;
            user.UserName = request.UserName;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update user roles
                var existingRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, existingRoles.ToArray());

                var rolesToAdd = request.Roles!.Distinct().ToArray();
                await _userManager.AddToRolesAsync(user, rolesToAdd);

                // Return updated user details
                var updatedUser = await _userManager.FindByIdAsync(id);
                var updatedUserDto = await TransformUserToUserDto(updatedUser!, false);
                updatedUserDto.Roles = rolesToAdd.ToList();
                return updatedUserDto;
            }

            throw new Exception("Failed to update user");
        }

        public async Task<UserResponseDto> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "User not found" });

            // Eliminar el usuario y obtener los roles asignados antes de eliminarlo
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // Devolver detalles del usuario eliminado y los roles asignados
                var deletedUserDto = await TransformUserToUserDto(user, false);
                deletedUserDto.Roles = roles.ToList();
                return deletedUserDto;
            }

            throw new Exception("Failed to delete user");
        }

        public async Task<UserResponseDto> SoftDeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "User not found" });

            // Soft delete user by setting IsDeleted flag to true
            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Remove user from all roles
                var roles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, roles.ToArray());

                // Return deleted user details
                var deletedUserDto = await TransformUserToUserDto(user, false);
                deletedUserDto.Roles = roles.ToList();
                return deletedUserDto;
            }

            throw new Exception("Failed to delete user");
        }
        public async Task<UserResponseDto> RestoreSoftDeletedUser(string id)
        {
            var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id) ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "User not found" });
            if (user.IsDeleted ?? false)
            {
                user.IsDeleted = false;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "General User");
                    var roles = await _userManager.GetRolesAsync(user);
                    var userDto = await TransformUserToUserDto(user, false);
                    userDto.Roles = roles.ToList();
                    return userDto;
                }

                throw new Exception("Failed to restore soft deleted user");
            }
            else
            {
                throw new MiddlewareException(HttpStatusCode.BadRequest, new { message = "User is not soft deleted" });
            }
        }

        public async Task<string> ForgotPassword(ForgotPasswordRequestDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email!)
                  ?? throw new MiddlewareException(HttpStatusCode.NotFound,
                                                 new { message = "User email does not exists" });

            try
            {
                IdGenerator idGenerator = new();
                user.Token = idGenerator.GenerateId();

                await _userManager.UpdateAsync(user);

                await _emailSender.SendEmailForgotPwdAsync(user);
            }
            catch (System.Exception ex)
            {
                var message = "Failed to send the email. " + ex.Message;
                throw new Exception(message);
            }

            return "Instructions have been sent to your email";
        }

        public async Task<bool> CheckUserByTokenAsync(string token)
        {
            return await _context.Users.AnyAsync(u => u.Token == token);
        }

        public async Task<bool> ResetPasswordByToken(string token, ResetPasswordRequestDTO request)
        {

            if (request.Password == null)
            {
                throw new Exception("Password can not be null or empty");
            }

            var user = _userManager.Users.FirstOrDefault(u => u.Token == token)
                ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "User token does not exists" });

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password);
            user.Token = null;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new Exception("Failed to reset password");
            }

            return true;
        }

        public async Task<ProfileResponseDto> UpdateProfile(string id, ProfileUpdateRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw new MiddlewareException(HttpStatusCode.NotFound, new { message = "User not found" });

            user.Name = request.Name;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            if (request.Password != "")
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password);
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var updatedUser = await _userManager.FindByIdAsync(id);

                if (updatedUser != null)
                {

                    ProfileResponseDto updatedProfileDto = new ProfileResponseDto
                    {
                        Name = updatedUser.Name,
                        LastName = updatedUser.LastName,
                        PhoneNumber = updatedUser.PhoneNumber
                    };

                    return updatedProfileDto;
                }
            }

            throw new Exception("Failed to update profile");
        }
    }
}