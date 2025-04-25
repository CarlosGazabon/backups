namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOSkuColumnData
  {
    public string Package { get; set; } = default!;
    public string Flavor { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string NetContent { get; set; } = default!;
    public decimal? Efficiency { get; set; } = default!;
  }

}