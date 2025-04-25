namespace inventio.Models.Views;

public class VwDowntimePerSku
{
  public DateTime Date { get; set; }
  public int LineID { get; set; }
  public string Line { get; set; } = null!;
  public int CategoryID { get; set; }
  public string Category { get; set; } = null!;
  public int ShiftID { get; set; }
  public string Shift { get; set; } = null!;
  public string Packing { get; set; } = null!;
  public int SKUID { get; set; }
  public string SKU { get; set; } = null!;
  public string Flavor { get; set; } = null!;
  public string NetContent { get; set; } = null!;
  public decimal Minutes { get; set; }
  public decimal ExtraMinutes { get; set; }
  public string SubCategory2 { get; set; } = null!;
  public string Code { get; set; } = null!;
  public string Failure { get; set; } = null!;
}