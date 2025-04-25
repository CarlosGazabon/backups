namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOPackageDonut
  {
    public List<DTOPackageDonutData> Data { get; set; } = default!;
    public int SumTotal { get; set; }
  }

}