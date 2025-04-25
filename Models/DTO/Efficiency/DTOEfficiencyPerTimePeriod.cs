namespace inventio.Models.DTO.Efficiency
{
  public class DTOEfficiencyPerTimePeriod
  {
    public string Time { get; set; } = default!;
    public string Line { get; set; } = default!;
    public decimal Efficiency { get; set; } = default!;
  }
}