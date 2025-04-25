namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOSkuBar
  {
    public decimal? Scrap { get; set; }
    public decimal? TotalScrap { get; set; }
    public decimal? ScrapPercentage { get; set; }
    public string Package { get; set; } = default!;
    public string NetContent { get; set; } = default!;
    public string Flavor { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string Type { get; set; } = default!;
  }

}