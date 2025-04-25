namespace inventio.Models.DTO.Efficiency
{
  public class DTOEfficiencyPerShift
  {
    public string Shift { get; set; } = default!;
    public decimal Efficiency { get; set; } = default!;
  }
}