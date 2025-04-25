namespace inventio.Models.DTO.DowntimeSku
{
  public class DTODowntimeSkuGetFilters
  {
    public string Line { get; set; } = default!;
    public int LineID { get; set; } = default!;
    public string Category { get; set; } = default!;
    public int CategoryID { get; set; } = default!;
    public string Shift { get; set; } = default!;
    public int ShiftID { get; set; } = default!;
    public string Packing { get; set; } = default!;
    public string SKU { get; set; } = default!;
    public int SkuID { get; set; } = default!;
  }
}