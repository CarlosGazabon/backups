using inventio.Models.DTO;
using inventio.Models.DTO.DowntimeXSubcat;
using inventio.Repositories.Dashboards.DowntimeXSubCat;
using Microsoft.AspNetCore.Mvc;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/downtime-subcategory")]
    public class DowntimeXSubCategoryController : ControllerBase
    {
        private readonly IDowntimeXSubcatRepository _repository;

        public DowntimeXSubCategoryController(IDowntimeXSubcatRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("lines")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetLines([FromBody] DTODowntimeFilters filters)
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

        [HttpPost("categories")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetCategories([FromBody] DTODowntimeFilters filters)
        {
            try
            {
                var categories = await _repository.GetCategories(filters);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("shifts")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetShifts([FromBody] DTODowntimeFilters filters)
        {
            try
            {
                var shifts = await _repository.GetShift(filters);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("subcategories")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetSubCategories([FromBody] DTODowntimeFilters filters)
        {
            try
            {
                var subcategories = await _repository.GetSubCategory(filters);
                return Ok(subcategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("dashboard")]
        public async Task<ActionResult<DTODowntimeDashboard>> GetDashboard([FromBody] DTODowntimeFilters filters)
        {
            try
            {
                var dashboard = await _repository.GetDowntimeDashBoard(filters);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}