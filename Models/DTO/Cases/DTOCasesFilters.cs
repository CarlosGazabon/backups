using System.ComponentModel.DataAnnotations;

namespace Inventio.Models.DTO.Cases
{
  public class CasesFilter
  {
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;

    [RegularExpression("^(Day|Week|Month|Quarter|Year)$", ErrorMessage = "The value of 'period' must be 'Day', 'Week', 'Month', 'Quarter', 'Year'.")]
    public string Time { get; set; } = default!;
    public List<string>? Lines { get; set; } = default!;
  }
}