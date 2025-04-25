namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOSkuDonutData
  {
    public decimal? Production { get; set; }
    public string Package { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string Flavour { get; set; } = default!;
    public string NetContent { get; set; } = default!;
    public int Total { get; set; }
    public decimal Percent { get; set; }
  }

}