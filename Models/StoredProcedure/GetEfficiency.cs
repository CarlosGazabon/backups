namespace inventio.Models.StoredProcedure;
public class GetEfficiency
{
  public DateTime Date { get; set; }
  public string Line { get; set; } = default!;
  public string Shift { get; set; } = default!;
  public string Sku { get; set; } = default!;
  public string Packing { get; set; } = default!;
  public string NetContent { get; set; } = default!;
  public decimal? Production { get; set; }
  public int StandardSpeed { get; set; }
  public int Scrap { get; set; }
  public int Hrs { get; set; }
  public decimal? DownHrs { get; set; }
  public decimal? MaxProduction { get; set; }
}