namespace Inventio.Models.DTO.DailySummary
{
  public class DTODailyTablePackage
  {
    public string Line { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string Flavor { get; set; } = default!;
    public string Package { get; set; } = default!;
    public decimal? Production { get; set; }
  }

}