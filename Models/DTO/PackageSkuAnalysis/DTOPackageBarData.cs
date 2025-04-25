namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOPackageBarData
  {
    public int BottleScrap { get; set; }
    public int CanScrap { get; set; }
    public int PreformScrap { get; set; }
    public int UnitsPerPackage { get; set; }
    public decimal? Production { get; set; }
    public string Package { get; set; } = default!;
  }

}