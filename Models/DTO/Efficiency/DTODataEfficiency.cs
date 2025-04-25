namespace inventio.Models.DTO.Efficiency
{
  public class DTODataEfficiency
  {
    public DateTime Date { get; set; }
    public string Line { get; set; } = default!;
    public int LineId { get; set; }
    public string Shift { get; set; } = default!;
    public int ShiftId { get; set; }
    public string Sku { get; set; } = default!;
    public string Flavor { get; set; } = default!;
    public int ProductId { get; set; }
    public string Packing { get; set; } = default!;
    public string NetContent { get; set; } = default!;
    public decimal Production { get; set; }
    public int StandardSpeed { get; set; }
    //public string Supervisor { get; set; } = default!;
    public int Scrap { get; set; }
    public int Hrs { get; set; }
    public decimal DownHrs { get; set; }
    public decimal NetHrs { get; set; }
    public decimal MaxProduction { get; set; }
  }
}