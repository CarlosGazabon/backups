namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class PackageSkuFilter
  {
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public List<int>? Lines { get; set; } = default!;
    public List<string>? Sku { get; set; } = default!;
    public List<string>? Package { get; set; } = default!;
    public List<string>? NetContent { get; set; } = default!;
    public List<int>? Supervisors { get; set; } = default!;
    public List<int> Categories { get; set; } = default!;
  }
}