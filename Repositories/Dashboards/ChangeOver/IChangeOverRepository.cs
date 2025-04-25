using inventio.Models.DTO;
using inventio.Models.DTO.ChangeOver;

namespace inventio.Repositories.Dashboards.ChangeOver
{
    public interface IChangeOverRepository
    {
        Task<DTOChangeOverMetrics> GetChangeOverAllMetrics(ChangeOverFilter filters);
        Task<IEnumerable<string>> GetChangeOverLines(ChangeOverFilter filters);
        Task<IEnumerable<string>> GetChangeOverShifts(ChangeOverFilter filters);
        Task<IEnumerable<DTOReactSelect>> GetChangeOverSupervisors(ChangeOverFilter filters);
        Task<IEnumerable<string>> GetChangeOverSubCategory2(ChangeOverFilter filters);
        Task<DTOChangeOverDashboard> GetScrapDashboard(ChangeOverFilter filters);
    }
}