using inventio.Models.DTO.Supervisor;
using Microsoft.AspNetCore.Mvc;
using inventio.Repositories.Dashboards.SupervisorMetrics;
using inventio.Models.DTO;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/supervisor-metrics")]
    public class SupervisorMetricsController : ControllerBase
    {
        private readonly ISupervisorRepository _repository;

        public SupervisorMetricsController(ISupervisorRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("lines")]
        public async Task<ActionResult<IEnumerable<DTOReactSelect>>> GetLines([FromBody] SupervisorFilter filters)
        {
            try
            {
                var lines = await _repository.GetLines(filters);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("supervisors")]
        public async Task<ActionResult<IEnumerable<DTOReactSelect>>> GetSupervisors([FromBody] SupervisorFilter filters)
        {
            try
            {
                var supervisors = await _repository.GetSupervisors(filters);
                return Ok(supervisors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("shifts")]
        public async Task<ActionResult<IEnumerable<DTOReactSelect>>> GetShifts([FromBody] SupervisorFilter filters)
        {
            try
            {
                var shifts = await _repository.GetShifts(filters);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("dashboard")]
        public async Task<ActionResult<DTOMetricsPerSupervisor>> GetSupervisorDashboard([FromBody] SupervisorFilter filters)
        {
            try
            {
                var dashboard = await _repository.GetSupervisorDashBoard(filters);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}