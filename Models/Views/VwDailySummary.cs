namespace Inventio.Models.Views;

public class VwDailySummary
{
  public DateTime Date { get; set; }
  public string Line { get; set; } = null!;
  public string Sku { get; set; } = null!;
  public string Flavor { get; set; } = null!;
  public decimal? Production { get; set; }
  public string Packing { get; set; } = null!;
  public string Shift { get; set; } = null!;
}