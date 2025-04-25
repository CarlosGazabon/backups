using Microsoft.AspNetCore.Mvc;
using Inventio.Data;
using inventio.Models.DTO.ProductivitySummary;
using inventio.Repositories.Dashboards.Scrap;
using inventio.Models.DTO.ScrapController;
using inventio.Models.DTO;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/scrap")]
    public class ScrapController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IScrapRepository _scrapRepository;
        public ScrapController(ApplicationDBContext context, IScrapRepository scrapRepository)
        {
            _context = context;
            _scrapRepository = scrapRepository;
        }


        [HttpGet("available")]
        public async Task<ActionResult<DTOScrapType>> GetScrapAvailable()
        {

            if (_context.ProductivitySummary == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _scrapRepository.GetScrapAvailable();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("lines")]
        public async Task<ActionResult<DTOReactDropdown<int>>> GetScrapLines(ScrapFilter filters)
        {

            if (_context.ProductivitySummary == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _scrapRepository.GetLines(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("dashboard")]
        public async Task<ActionResult<DTOScrapDashboard>> GetScrapDashboard(ScrapFilter filters)
        {

            if (_context.ProductivitySummary == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _scrapRepository.GetScrapDashboard(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("shiftanalysis")]
        public async Task<ActionResult<IEnumerable<DTOShift>>> GetScrapShift(ScrapFilter filters)
        {
            try
            {
                var result = await _scrapRepository.GetScrapShift(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

    }
}