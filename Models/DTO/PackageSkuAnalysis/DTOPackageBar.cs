namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOPackageBar
  {
    public decimal? Scrap { get; set; }
    public decimal? TotalScrap { get; set; }
    public decimal? ScrapPercentage { get; set; }
    public string Package { get; set; } = default!;
    public string Type { get; set; } = default!;
  }

}