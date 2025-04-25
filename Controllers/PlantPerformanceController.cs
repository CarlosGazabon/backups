using Microsoft.AspNetCore.Mvc;
using Inventio.Data;
using inventio.Models.DTO.VwPlantPerformance;

using inventio.Repositories.Dashboards.PlantPerformance;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/plant-performance")]
    public class PlantPerformanceController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IPlantPerformanceRepository _plantPerformanceRepository;

        public PlantPerformanceController(ApplicationDBContext context, IPlantPerformanceRepository plantPerformanceRepository)
        {
            _context = context;
            _plantPerformanceRepository = plantPerformanceRepository;
        }



        [HttpPost("summary")]
        public async Task<ActionResult<GembaObj>> GetSummaryGemba(GembaFilter filters)
        {
            if (_context.ProductionSummary == null || _context.VwDowntime == null)
            {
                return NotFound();
            }

            try
            {
                var resultGemba = await _plantPerformanceRepository.GetSummaryGemba(filters);
                return Ok(resultGemba);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

    }
}