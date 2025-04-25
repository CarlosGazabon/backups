namespace Inventio.Models;

public class Supervisor
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required bool Inactive { get; set; }

    // public virtual List<Productivity>? Productivities { get; set; }

}
