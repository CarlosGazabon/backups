using inventio.Models.DTO;
using inventio.Models.DTO.ProductivitySummary;
using inventio.Models.DTO.ScrapController;

namespace inventio.Repositories.Dashboards.Scrap
{
    public interface IScrapRepository
    {
        Task<DTOScrapDashboard> GetScrapDashboard(ScrapFilter filters);

        Task<IEnumerable<DTOReactDropdown<int>>> GetLines(ScrapFilter filters);

        Task<decimal> GetTotalPlantScrap(ScrapFilter filters);

        Task<List<DTOScrapTable>> GetScrapTableAllLines(ScrapFilter filters);
        Task<DTOScrapType> GetScrapAvailable();

        Task<List<DTOShift>> GetScrapShift(ScrapFilter filters);
    }
}