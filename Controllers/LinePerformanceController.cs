using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using inventio.Models.DTO.LinePerformance;
using inventio.Repositories.Dashboards.LinePerformance;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/line-performance")]
    public class LinePerformanceController : ControllerBase
    {
        private readonly ILinePerformanceRepository _repository;

        public LinePerformanceController(ILinePerformanceRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("line-objectives")]
        public async Task<ActionResult<DTOLineObjectives>> GetLineObjectives([FromBody] LinePerformanceFilters filters)
        {
            try
            {
                var objectives = await _repository.GetLineObjectives(filters);

                if (objectives == null)
                {
                    return NotFound(); // Devuelve 404 si no se encuentra ningún resultado
                }

                return Ok(objectives);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("lines")]
        public async Task<ActionResult<IEnumerable<string>>> GetLines([FromBody] LinePerformanceFilters filters)
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

        [HttpPost("dashboard")]
        public async Task<ActionResult<DTOLinePerformanceDashboard>> GetLinePerformanceDashboard([FromBody] LinePerformanceFilters filters)
        {
            try
            {
                var dashboard = await _repository.LinePerformanceDashboard(filters);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
