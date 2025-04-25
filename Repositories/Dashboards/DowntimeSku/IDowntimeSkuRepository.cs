using inventio.Models.DTO.DowntimeSku;

namespace inventio.Repositories.Dashboards.DowntimeSku
{
    public interface IDowntimeSkuRepository
    {
        Task<List<DTODowntimeSkuGetFilters>> GetFilters(DTODowntimeSkuDatesFilter filters);
        Task<DTODowntimeSkuDashboard> GetDowntimeSkuDashboard(DowntimeSkuFilter filters);
    }
}