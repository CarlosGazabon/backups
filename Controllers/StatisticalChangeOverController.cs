using inventio.Models.DTO.StatisticalChangeOver;
using inventio.Repositories.Dashboards.StatisticalChangeOver;
using Microsoft.AspNetCore.Mvc;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/statistical-changeover")]
    public class StatisticalChangeOverController : ControllerBase
    {
        private readonly IStatisticalChangeOverRepository _repository;

        public StatisticalChangeOverController(IStatisticalChangeOverRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("lines")]
        public async Task<ActionResult<IEnumerable<string>>> GetLines([FromBody] DTOStatisticalChangeOverFilter filters)
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

        [HttpPost("shifts")]
        public async Task<ActionResult<IEnumerable<string>>> GetShifts([FromBody] DTOStatisticalChangeOverFilter filters)
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

        [HttpPost("supervisors")]
        public async Task<ActionResult<IEnumerable<string>>> GetSupervisors([FromBody] DTOStatisticalChangeOverFilter filters)
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

        [HttpPost("subcategories")]
        public async Task<ActionResult<IEnumerable<string>>> GetSubCategories([FromBody] DTOStatisticalChangeOverFilter filters)
        {
            try
            {
                var subcategories = await _repository.GetSubCategories(filters);
                return Ok(subcategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("codes")]
        public async Task<ActionResult<IEnumerable<string>>> GetCodes([FromBody] DTOStatisticalChangeOverFilter filters)
        {
            try
            {
                var codes = await _repository.GetCodes(filters);
                return Ok(codes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("dashboard")]
        public async Task<ActionResult<DTOStatisticalChangeOverDashboard>> GetStatisticalChangeOverDashboard([FromBody] DTOStatisticalChangeOverFilter filters)
        {
            try
            {
                var dashboard = await _repository.GetStatisticalChangeOverDashboard(filters);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}