namespace Inventio.Models.DTO.GeneralDowntime
{
  public class GeneralDowntimeFilter
  {
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public List<string>? Lines { get; set; } = default!;
    public List<string>? Categories { get; set; } = default!;
    public List<string>? Shifts { get; set; } = default!;
  }

}