namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOSkuBarData
  {
    public string Sku { get; set; } = default!;
    public decimal? Production { get; set; }
    public int UnitsPerPackage { get; set; }
    public int BottleScrap { get; set; }
    public int CanScrap { get; set; }
    public int PreformScrap { get; set; }
    public string Flavor { get; set; } = default!;
    public string Package { get; set; } = default!;
    public string NetContent { get; set; } = default!;
  }

}
