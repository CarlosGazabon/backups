namespace Inventio.Models
{
  public class FeatureFlags
  {
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool Inactive { get; set; }
  }

}