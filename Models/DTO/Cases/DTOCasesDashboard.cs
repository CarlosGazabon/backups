namespace inventio.Models.DTO.Cases
{
  public class DTOCasesDashboard
  {
    public int TotalProduction { get; set; } = default!;
    public List<DTOCasesPerLine> CasesPerLine { get; set; } = default!;
    public List<DTOCasesPerShift> CasesPerShift { get; set; } = default!;
    public List<DTOCasesPerTimePeriod> CasesPerTimePeriod { get; set; } = default!;
  }
}