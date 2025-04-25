using inventio.Models.DTO.VwPlantPerformance;

namespace inventio.Repositories.Dashboards.PlantPerformance
{
    public interface IPlantPerformanceRepository
    {
        Task<GembaObj> GetSummaryGemba(GembaFilter filters);
    }
}