namespace inventio.Models.StoredProcedure;
public class SPGetDataEfficiency
{
  public DateTime Date { get; set; }
  public int LineId { get; set; }
  public string Line { get; set; } = default!;
  public int ShiftId { get; set; }
  public string Shift { get; set; } = default!;
  public string Sku { get; set; } = default!;
  public string Flavor { get; set; } = null!;
  public string Packing { get; set; } = default!;
  public string NetContent { get; set; } = default!;
  public decimal Production { get; set; }
  public int StandardSpeed { get; set; }
  public int Scrap { get; set; }
  public int Hrs { get; set; }
  public decimal DownHrs { get; set; }
  public decimal MaxProduction { get; set; }

  // Supervisor metrics Efficiency
  public int ProductId { get; set; }
  public int SupervisorId { get; set; }
  public string Supervisor { get; set; } = null!;
  public int Flow { get; set; }

}