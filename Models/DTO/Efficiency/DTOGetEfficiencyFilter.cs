namespace inventio.Models.DTO.Efficiency
{
  public class DTOGetEfficiencyFilter
  {
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public List<int> Categories { get; set; } = default!;
    public bool DefaultChangeOver { get; set; } = default!;
  }
}