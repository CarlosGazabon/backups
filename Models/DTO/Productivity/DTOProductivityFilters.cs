namespace inventio.Models.DTO.Productivity
{
  public class ProductivityFilter
  {
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public List<int> Lines { get; set; } = default!;
    public List<int> Supervisor { get; set; } = default!;
    public List<int> Sku { get; set; } = default!;
    public List<int> Shift { get; set; } = default!;
    public int Skip { get; set; } = default!;
    public int Take { get; set; } = default!;
  }
}