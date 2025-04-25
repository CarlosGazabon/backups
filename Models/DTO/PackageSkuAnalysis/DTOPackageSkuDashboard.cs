namespace Inventio.Models.DTO.PackageSkuAnalysis
{
  public class DTOPackageSkuDashboard
  {
    public DTOPackageDonut PackageDonut { get; set; } = default!;
    public List<DTOPackageBar> PackageBar { get; set; } = default!;
    public List<DTOPackageColumnData> PackageColumn { get; set; } = default!;
    public List<DTOSkuBar> SkuBar { get; set; } = default!;
    public DTOSkuDonut SkuDonut { get; set; } = default!;
    public List<DTOSkuColumnData> SkuColumn { get; set; } = default!;

  }

}