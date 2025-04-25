namespace inventio.Models.DTO.Efficiency
{
  public class DTOEfficiencyPerLine
  {
    public string Line { get; set; } = default!;
    public decimal Efficiency { get; set; } = default!;
    public decimal Goal { get; set; } = default!;
    public decimal Variance { get; set; } = default!;
  }
}