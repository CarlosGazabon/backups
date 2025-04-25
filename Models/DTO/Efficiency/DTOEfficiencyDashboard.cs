namespace inventio.Models.DTO.Efficiency
{
  public class DTOEfficiencyDashboard
  {
    public decimal TotalEfficiency { get; set; } = default!;
    public List<DTOEfficiencyPerLine> EfficiencyPerLine { get; set; } = default!;
    public List<DTOEfficiencyPerShift> EfficiencyPerShift { get; set; } = default!;
    public List<DTOEfficiencyPerTimePeriod> EfficiencyPerTimePeriod { get; set; } = default!;
  }
}