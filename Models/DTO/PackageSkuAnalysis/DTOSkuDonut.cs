namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOSkuDonut
  {
    public List<DTOSkuDonutData> Data { get; set; } = default!;
    public int SumTotal { get; set; }
  }

}