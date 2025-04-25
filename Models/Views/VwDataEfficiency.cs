namespace Inventio.Models.Views;

public partial class VwDataEfficiency
{
  public int Id { get; set; }
  public DateTime Date { get; set; }
  public string Line { get; set; } = null!;
  public int LineId { get; set; }
  public string Shift { get; set; } = null!;
  public int ShiftId { get; set; }
  public int ProductId { get; set; }
  public string Sku { get; set; } = null!;
  public string Flavor { get; set; } = null!;
  public string Packing { get; set; } = null!;
  public string NetContent { get; set; } = null!;
  public decimal Production { get; set; }
  public int StandardSpeed { get; set; }
  public int Scrap { get; set; }
  public int? ProductivityID { get; set; }
  public decimal? Minutes { get; set; }
  public int? DowntimeCategoryId { get; set; }
  public int? FlowIndex { get; set; }
}