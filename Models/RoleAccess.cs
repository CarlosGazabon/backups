namespace Inventio.Models;

public class RoleAccess
{
    public int Id { get; set; }

    public string? RoleId { get; set; }

    public string? MenuId { get; set; }

    public bool? Active { get; set; }
}
