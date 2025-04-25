


namespace Inventio.Models
{
    public class RoleRights
    {
        public int Id { get; set; }

        public string? RoleId { get; set; }

        public string? MenuId { get; set; }
        public string? MenuName { get; set; }
        public bool? AllowView { get; set; }

        public bool? AllowInsert { get; set; }

        public bool? AllowEdit { get; set; }

        public bool? AllowDelete { get; set; }
    }
}