
namespace inventio.Models.DTO.NonConformance
{
  public class NonConformanceFilter
  {
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public List<string>? Lines { get; set; } = default!;
  }
}