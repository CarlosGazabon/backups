using inventio.Models.DTO;
using inventio.Models.Views;
using Inventio.Models.DTO.PackageSkuAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace inventio.Repositories.Dashboards.PackageSku
{
  public interface IPackageSkuRepository
  {
    Task<IEnumerable<DTOReactDropdown<int>>> GetLine(PackageSkuFilter filters);
    Task<IEnumerable<DTOReactDropdown<string>>> GetSKU(PackageSkuFilter filters);
    Task<IEnumerable<DTOReactDropdown<string>>> GetPacking(PackageSkuFilter filters);
    Task<IEnumerable<DTOReactDropdown<string>>> GetNetContent(PackageSkuFilter filters);
    Task<IEnumerable<DTOReactDropdown<int>>> GetSupervisors(PackageSkuFilter filters);

    Task<DTOPackageSkuDashboard> GetPackageDashboard(PackageSkuFilter filters);

    DTOPackageDonut DataDonutPackage(List<VwPackageSku> data, PackageSkuFilter filters);
  }

}