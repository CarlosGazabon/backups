using inventio.Models.DTO;
using inventio.Models.DTO.VwUtilization;

namespace inventio.Repositories.Dashboards.Utilization
{
    public interface IUtilizationRepository
    {
        Task<List<DTOReactDropdown<int>>> GetUtilizationLines(UtilizationFilter filters);
        Task<List<DTOReactDropdown<int>>> GetUtilizationCategories(UtilizationFilter filters);
        Task<DTOUtilization> GetPlantUtilization(UtilizationFilter filters);
        Task<List<DTOUtilizationPerLine>> GetUtilizationPerLine(UtilizationFilter filters);
        Task<List<DTOUtilizationTable>> GetUtilizationTable(UtilizationFilter filters);
        Task<List<DTOUtilizationPerTime>> GetUtilizationPerDay(UtilizationFilter filters);
        Task<List<DTOUtilizationPerTime>> GetUtilizationPerWeek(UtilizationFilter filters);
        Task<List<DTOUtilizationPerTime>> GetUtilizationPerMonth(UtilizationFilter filters);
        Task<List<DTOUtilizationPerTime>> GetUtilizationPerQuarter(UtilizationFilter filters);
        Task<List<DTOUtilizationPerTime>> GetUtilizationPerYear(UtilizationFilter filters);
    }
}