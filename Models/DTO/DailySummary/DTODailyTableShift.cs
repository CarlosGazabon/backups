namespace Inventio.Models.DTO.DailySummary
{
  public class DTODailyTableShift
  {
    public string Line { get; set; } = default!;
    public string Shift { get; set; } = default!;
    public decimal? Production { get; set; }
  }

}