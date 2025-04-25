using Inventio.Data;
using Microsoft.AspNetCore.Mvc;
using inventio.Repositories.Dashboards.TrendAnalysisRepository;
using inventio.Models.DTO;
using inventio.Models.DTO.DowntimeTrend;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/downtime-trend")]
    public class TrendAnalysisController : ControllerBase
    {
        private readonly ITrendAnalysisRepository _repository;
        private readonly ApplicationDBContext _context;

        public TrendAnalysisController(ApplicationDBContext context, ITrendAnalysisRepository repository)
        {
            _context = context;
            _repository = repository;
        }


        [HttpPost("lines")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetLines([FromBody] DTOTrendFilter filters)
        {
            if (_context.VwDowntimeTrend == null)
            {
                return NotFound();
            }

            try
            {
                var lines = await _repository.GetLinesAsync(filters);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPost("categories")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetCategories([FromBody] DTOTrendFilter filters)
        {

            if (_context.VwDowntimeTrend == null)
            {
                return NotFound();
            }

            try
            {
                var lines = await _repository.GetCategoriesAsync(filters);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }



        [HttpPost("subcategory")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetSubCategories([FromBody] DTOTrendFilter filters)
        {

            if (_context.VwDowntimeTrend == null)
            {
                return NotFound();
            }

            try
            {
                var lines = await _repository.GetSubCategoryAsync(filters);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPost("codes")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetCodes([FromBody] DTOTrendFilter filters)
        {

            if (_context.VwDowntimeTrend == null)
            {
                return NotFound();
            }

            try
            {
                var lines = await _repository.GetCodesAsync(filters);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }



        [HttpPost("dashboard")]
        public async Task<ActionResult<DTODowntimeTrendDashboard>> GetDowntimeTrend(DTOTrendFilter filters)
        {

            if (_context.VwDowntimeTrend == null)
            {
                return NotFound();
            }

            try
            {
                var dashboard = await _repository.GetTrendDashBoard(filters);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }



    }
}