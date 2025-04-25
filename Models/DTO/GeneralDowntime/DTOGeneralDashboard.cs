namespace Inventio.Models.DTO.GeneralDowntime
{
  public class DTOGeneralDashboard
  {
    public List<DTOGeneralColumnStack> GeneralColumnStack { get; set; } = default!;
    public List<DTOGeneralColumn> GeneralColumn { get; set; } = default!;
  }
}