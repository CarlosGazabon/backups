namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOPackageDonutData
  {
    public int Production { get; set; }
    public string Type { get; set; } = default!;
    public int Total { get; set; }
    public decimal Percent { get; set; }
  }

}