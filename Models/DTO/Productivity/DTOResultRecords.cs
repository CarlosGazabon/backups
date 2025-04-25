namespace Inventio.Models.DTO.Productivity;
using Inventio.Models;

public class DTOResultRecords
{
  public List<Productivity> List { get; set; } = default!;
  public int Count { get; set; } = default!;
}
