namespace Inventio.Models;

public partial class MenuRights
{
    public int Id { get; set; }

    public string? MenuId { get; set; }

    public string? RoleId { get; set; }

    public bool? CanView { get; set; }

    public bool? CanAdd { get; set; }

    public bool? CanDelete { get; set; }

    public bool? CanModify { get; set; }

    public virtual Menu? Menu { get; set; }

    public virtual Role? Role { get; set; }
}
