namespace inventio.Models.Views;

public partial class VwPackageSku
{
  public DateTime Date { get; set; }
  public int LineID { get; set; }
  public string Line { get; set; } = null!;
  public string Packing { get; set; } = null!;
  public string Sku { get; set; } = null!;
  public string Flavor { get; set; } = null!;
  public string NetContent { get; set; } = null!;
  public int UnitsPerPackage { get; set; }
  public int SupervisorID { get; set; }
  public string Supervisor { get; set; } = null!;
  public int ScrapUnits { get; set; }
  public int CanScrap { get; set; }
  public int BottleScrap { get; set; }
  public int PreformScrap { get; set; }
  public decimal Production { get; set; }
  public int Hrs { get; set; }
  public decimal DownHrs { get; set; }
  public decimal NetHrs { get; set; }
  public int StandardSpeed { get; set; }
  public decimal MaxProduction { get; set; }
}