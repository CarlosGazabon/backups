namespace inventio.Models.Views;

public class VwCases
{
  public DateTime Date { get; set; }
  public string Line { get; set; } = null!;
  public string Shift { get; set; } = null!;
  public decimal? Production { get; set; }
}