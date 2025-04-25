

using inventio.Models.DTO;
using inventio.Models.DTO.Supervisor;

namespace inventio.Repositories.Dashboards.SupervisorMetrics
{
    public interface ISupervisorRepository
    {
        Task<IEnumerable<DTOReactDropdown<int>>> GetLines(SupervisorFilter filters);
        Task<IEnumerable<DTOReactDropdown<int>>> GetSupervisors(SupervisorFilter filters);
        Task<IEnumerable<DTOReactDropdown<int>>> GetShifts(SupervisorFilter filters);
        Task<DTOMetricsPerSupervisor> GetSupervisorDashBoard(SupervisorFilter filters);
    }
}