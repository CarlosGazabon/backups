using Inventio.Models.DTO.GeneralDowntime;

namespace inventio.Repositories.Dashboards.GeneralDowntime
{
  public interface IGeneralDowntimeRepository
  {
    Task<IEnumerable<string>> GetLine(GeneralDowntimeFilter filters);
    Task<IEnumerable<string>> GetCategories(GeneralDowntimeFilter filters);
    Task<IEnumerable<string>> GetShifts(GeneralDowntimeFilter filters);
    Task<DTOGeneralDashboard> GetDashboard(GeneralDowntimeFilter filters, bool allFilters);

  }

}