using Inventio.Models.DTO.DailySummary;

namespace inventio.Repositories.Dashboards.DailySummary
{
  public interface IDailySummaryRepository
  {
    Task<DTODailyDashboard> GetDashboard(DailySummaryFilter filter);
  }
}