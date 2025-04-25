namespace Inventio.Models;

public partial class Menu
{
    public string Id { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Route { get; set; }

    public string? ParentId { get; set; }

    public int? IconId { get; set; }

    public int? Sort { get; set; }

    public virtual Icons? Icon { get; set; }

    public virtual Menu? Parent { get; set; }

    public virtual ICollection<Menu> InverseParent { get; set; } = new List<Menu>();

    public virtual ICollection<MenuRights> MenuRights { get; set; } = new List<MenuRights>();


}
