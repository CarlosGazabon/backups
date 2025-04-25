namespace inventio.Models.DTO.Cases
{
  public class DTOCasesPerLine
  {
    public string Line { get; set; } = default!;
    public int Production { get; set; } = default!;
    public decimal? Percent { get; set; } = default!;
  }
}