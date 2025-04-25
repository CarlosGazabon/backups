namespace Inventio.Models.DTO.GeneralDowntime
{
  public class DTOGeneralColumnStack
  {
    public string Line { get; set; } = default!;
    public decimal? Minutes { get; set; }
    public string Category { get; set; } = default!;
  }

}