namespace Inventio.Models.DTO.DailySummary
{
  public class DTODailyTableWarehouse
  {
    public string Line { get; set; } = default!;
    public string Category { get; set; } = default!;
    public decimal? Hours { get; set; }
  }

}