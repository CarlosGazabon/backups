namespace Inventio.Models.DTO.Form
{
  public class DTOSelectOptions
  {
    public IEnumerable<Line> Lines { get; set; } = default!;
    public IEnumerable<Shift> Shifts { get; set; } = default!;
    public IEnumerable<Hour> Hours { get; set; } = default!;
    public IEnumerable<Supervisor> Supervisors { get; set; } = default!;
  }
}