namespace inventio.Models.DTO.Cases
{
  public class DTOCasesPerTimePeriod
  {
    public string Time { get; set; } = default!;
    public string Line { get; set; } = default!;
    public int Production { get; set; } = default!;
  }
}