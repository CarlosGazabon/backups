namespace Inventio.Models.DTO
{
  public class DTOCreateFeature
  {
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool Inactive { get; set; }
  }
}