using System.ComponentModel.DataAnnotations;

namespace inventio.Models.DTO.Efficiency
{
  public class EfficiencyFilter
  {
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public List<int> Categories { get; set; } = default!;
    public List<int>? Lines { get; set; } = default!;

    [RegularExpression("^(Day|Week|Month|Quarter|Year)$", ErrorMessage = "The value of 'period' must be 'Day', 'Week', 'Month', 'Quarter', 'Year'.")]
    public string Time { get; set; } = default!;
  }
}