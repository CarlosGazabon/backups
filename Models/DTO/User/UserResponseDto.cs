namespace inventio.Models.DTO.User
{
    public class UserResponseDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? ImagePath { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? Roles { get; set; }
        public string? RoleId { get; set; }
        public bool? IsDeleted { get; set; }
        public string? Token { get; set; }

    }
}